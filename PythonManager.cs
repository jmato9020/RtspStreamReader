using System.Diagnostics;

namespace FFMPEGReader;

public class PythonManager
{
  public Process PythonProcess { get; }
  public BinaryWriter PythonWriter { get; }

  public PythonManager(string pythonPath, Queue<Byte[]> imageQueue)
  {
    ProcessStartInfo start = new ProcessStartInfo();
    start.FileName = pythonPath;
    var cmd = "PythonTools/cvpy.py";
    var args = "-u";
    start.Arguments = string.Format("{0} {1}", cmd, args);
    start.UseShellExecute = false;
    start.RedirectStandardOutput = true;
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