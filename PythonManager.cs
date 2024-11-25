using System.Diagnostics;

namespace FFMPEGReader;

public class PythonManager
{
  private Process PythonProcess { get; }
  private BinaryWriter PythonWriter { get; }
  
  

  public PythonManager(string pythonPath, Queue<Byte[]> imageQueue)
  {
    ProcessStartInfo start = new ProcessStartInfo();
    start.FileName = pythonPath;
    var cmd = "PythonTools/cvpy.py";
    var args = "-u";
    start.Arguments = $"{cmd} {args}";
    start.UseShellExecute = false;
    start.RedirectStandardOutput = false;
    start.RedirectStandardInput = true;

    
    var process = Process.Start(start);
    
    if (process!=null)
    {
      PythonProcess = process;
      PythonWriter = new BinaryWriter(PythonProcess.StandardInput.BaseStream);
    }
    else
    {
      throw new Exception("Python process could not start correctly");
    }
    
    Task.Factory.StartNew(() =>
    {
      while (true)
      {
        try
        {
          WriteImageToStdin(imageQueue.Dequeue());
        }
        catch{ }
      }
    });

    // Task.Factory.StartNew(() =>
    // {
    //   while (true)
    //   {
    //     var stream = process.StandardOutput;
    //     var results = stream.ReadToEndAsync();
    //     results.Wait();
    //     if (results != null)
    //     {
    //       Console.WriteLine(results.Result);
    //     }
    //   }
    //   
    // });
  }

  public void WriteImageToStdin(byte[] imageBytes)
  {
    PythonWriter.Write(imageBytes);
  }

  public void StopProcess()
  {
    PythonProcess.Close();
  }
  
}