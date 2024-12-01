using System.Diagnostics;
using HomeNetDaemon.Access;
using NetDaemon.HassModel;

namespace FFMPEGReader;

public class PythonManager
{
  private Process PythonProcess { get; }
  private BinaryWriter PythonWriter { get; }

  private System.Timers.Timer? PersonDetectedTimer = null;
  private bool TimerIsRunning = false;
  
  private Services Services { get; set; }
    
  public PythonManager(string pythonPath, IHaContext haContext, Queue<Byte[]> imageQueue)
  {
    Services = new HomeNetDaemon.Access.Services(haContext);
    
    ProcessStartInfo start = new ProcessStartInfo();
    start.FileName = pythonPath;
    var cmd = "PythonTools/cvpy.py";
    var args = "-u";
    start.Arguments = $"{cmd} {args}";
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
    
    Task.Factory.StartNew(() =>
    {
      while (true)
      {
        var stream = process.StandardOutput;
        var results = stream.ReadLine();
       
        if (results != null)
        {
          ProcessOutputDataReceived(results);
        }
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

  private void ProcessOutputDataReceived(string line)
  {
    var results = line.Split(",");

    if (results.Any(entry => entry.Contains("person"))&&!TimerIsRunning)
    {
      TriggerPersonDetectedTimer();
      Services.Notify.Notify("Person detected in the driveway");
    }
    
  }

  private void TriggerPersonDetectedTimer()
  {
    PersonDetectedTimer = new System.Timers.Timer(5000);
    PersonDetectedTimer.Enabled = true;
    TimerIsRunning = true;
    PersonDetectedTimer.Elapsed += ElapsedFunc;
  }

  private void ElapsedFunc(Object source, System.Timers.ElapsedEventArgs e)
  {
    TimerIsRunning = false;
  }
  
  
}