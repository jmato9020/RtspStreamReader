using System.Diagnostics;

namespace FFMPEGReader;

public class PythonManager
{
  public Process PythonProcess { get; }

  public PythonManager(string pythonPath)
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
    }
    else
    {
      throw new Exception("Python process could not start correctly");
    }
  }

  public void WriteImageToStdin(byte[] imageBytes)
  {
    using (BinaryWriter writer = new BinaryWriter(PythonProcess.StandardInput.BaseStream))
    {
      writer.Write(imageBytes);
    }
  }

  public void StopProcess()
  {
    PythonProcess.Close();
  }
  
}