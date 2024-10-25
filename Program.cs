using System.Diagnostics;
using System.Text;

namespace FFMPEGReader;

class Program
{
  const int imageBitSize = 15360000;
  const int imageByteSize = imageBitSize / 8;
  const int bufferSize = 4096;
  
  static void Main(string[] args)
  {
    
    var psi = CreateProcessStartInfo();
    Process process = new Process();
    process.StartInfo = psi;
    process.EnableRaisingEvents = true;
    process.Start();

    Stream baseStream = process.StandardOutput.BaseStream;


    Task.Factory.StartNew(() =>
    {
      Thread.Sleep(10000);
      process.Kill();
    });

    HandleVideoStream(bufferSize, baseStream);


    process.WaitForExit();
  }

  private static ProcessStartInfo CreateProcessStartInfo()
  {
    ProcessStartInfo psi = new ProcessStartInfo();
    psi.CreateNoWindow = true;
    psi.RedirectStandardInput = false;
    psi.RedirectStandardOutput = true;
    psi.Arguments =
      "-i rtsps://192.168.1.1:7441/yBxm0eHAoO0Dz2uf -rtsp_transport tcp -f rawvideo -r 1/6 -vf scale=800:800 -pix_fmt bgr24 pipe:";
    psi.FileName = "./ffmpeg";
    return psi;
  }

  private static void HandleVideoStream(int bufferSize, Stream baseStream)
  {
    int lastRead;
    List<byte[]> imageBytes = new List<byte[]>();
    
    using (MemoryStream ms = new MemoryStream())
    {
      byte[] buffer = new byte[bufferSize];
      var bufferCount = 0;
      do
      {
        lastRead = baseStream.Read(buffer, 0, buffer.Length);
        ms.Write(buffer, 0, lastRead);
        
        bufferCount += lastRead;
        
        if (bufferCount == bufferSize)
        {
          imageBytes.Add(ms.ToArray());
        }
      } while (lastRead > 0);

      
      Console.WriteLine($"Buffer Count: {bufferCount}");

       
    }
  }
}