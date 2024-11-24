using System.Diagnostics;
using SkiaSharp;

namespace FFMPEGReader;

class VideoStreamManager
{
  private const int width = 2048;
  private const int height = 1080;
  private const int bitsPerPixel = 32;
  private const int _imageBitSize = width*height *bitsPerPixel;
  private const int _imageByteSize = _imageBitSize / 8;

  public Process VideoStreamProcess { get; }
  
  public VideoStreamManager(Configuration config, int bufferSize, Queue<Byte[]> imageQueue)
  {
    var psi = VideoStreamManager.CreateProcessStartInfo(config);
    Process process = new Process();
    process.StartInfo = psi;
    process.EnableRaisingEvents = true;
    process.Start();

    Stream baseStream = process.StandardOutput.BaseStream;

    Task.Factory.StartNew(() =>
    {
      VideoStreamManager.HandleVideoStream(bufferSize, baseStream, imageQueue);
    });
    
    VideoStreamProcess = process;
    
  }
  private static ProcessStartInfo CreateProcessStartInfo(Configuration config)
  {
    
    ProcessStartInfo psi = new ProcessStartInfo();
    psi.CreateNoWindow = true;
    psi.RedirectStandardInput = false;
    psi.RedirectStandardOutput = true;
    psi.Arguments =
      $"-i {config?.ConnectionString} -rtsp_transport tcp -f rawvideo -r 1/6 -vf scale={width}:{height} -pix_fmt rgba pipe:";
    psi.FileName = "./ffmpeg";
    return psi;
  }

  private static void HandleVideoStream(int bufferSize, Stream baseStream, Queue<Byte[]> imageQueue)
  {
    List<byte[]> imageBytes = new List<byte[]>();
    int lastRead;

    using var ms = new MemoryStream();
    var buffer = new byte[bufferSize];
    var bufferCount = 0;
    do
    {
      var realTimeBufferSize = buffer.Length;
        
      var bufferDifference = _imageByteSize - bufferCount;
        
      if (bufferDifference < buffer.Length)
      {
        realTimeBufferSize = bufferDifference;
      }
        
      lastRead = baseStream.Read(buffer, 0, realTimeBufferSize);
      ms.Write(buffer, 0, lastRead);
        
      bufferCount += lastRead;
        
      if (bufferCount == _imageByteSize)
      {
        var bytes = ms.ToArray();
        imageBytes.Add(bytes);
        imageQueue.Enqueue(bytes);
        bufferCount = 0;
        ms.Seek(0, SeekOrigin.Begin);
      }
    } while (lastRead > 0);

    Console.WriteLine(Environment.NewLine);
    Console.WriteLine($"Buffer Count: {bufferCount}");
    Console.WriteLine($"Images Parsed: {imageBytes.Count}");
  }

  private static void WriteImageOut(List<byte[]> imageBytes)
  {
    int counter = 0;
    foreach (var image in imageBytes)
    {
      using (var fs = System.IO.File.Create($"image_{counter}.png"))
      {
        using (var ms = new MemoryStream(image))
        {
          var skImageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888);
          
          using var skData = SKData.Create(ms);
          using var codec = SKCodec.Create(skData);
          
          SKImage im = SKImage.FromPixels(skImageInfo,skData);
          
          using var encodedImage = im.Encode(); 
          encodedImage.SaveTo(fs);
          counter++;
          
        }
      }
    }
  }
}