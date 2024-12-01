using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using NetDaemon.AppModel;
using Microsoft.Extensions.Hosting;
using NetDaemon.Runtime;
using NetDaemon.Extensions.Logging;
using HomeNetDaemon.Access;
using NetDaemon.HassModel;

namespace FFMPEGReader;

[NetDaemonApp]
class Program
{
  static IHaContext HaContext { get; set; }
  public Program(IHaContext haContext)
  {
    HaContext = haContext;
  }
  //const int _imageBitSize = 15360000;

  private const int BufferSize = 4096;

  private static readonly Queue<byte[]> ImageQueue = new();
  
  static void Main(string[] args)
  {
    ConfigureHomeAssistantConnection(args);
    
    var config = GetConfiguration();

    VideoStreamManager videoProcess = StartVideoProcessing(config);
    Thread.Sleep(5000);
    PythonManager pyManager = StartPython(config);
    
    videoProcess.VideoStreamProcess.WaitForExit();
  }

  private static void ConfigureHomeAssistantConnection(string[] args)
  {
    Host.CreateDefaultBuilder(args)
      .UseNetDaemonAppSettings()
      .UseNetDaemonDefaultLogging()
      .UseNetDaemonRuntime()
      .ConfigureServices((_, services) =>
        services
          .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
          .AddNetDaemonStateManager()
          .AddHomeAssistantGenerated()
      )
      .Build()
      .RunAsync()
      .ConfigureAwait(false);
  }

  private static Configuration GetConfiguration()
  {
    var xDoc = XDocument.Load("Configuration.xml");
    var xmlSerializer = new XmlSerializer(typeof(Configuration));
    var config = xmlSerializer.Deserialize(xDoc.CreateReader()) as Configuration;
    if (config == null)
    {
      throw new Exception("Configuration did not load correctly");
    }
    return config;
  }


  private static PythonManager StartPython(Configuration config)
  {
    PythonManager pythonManager = new PythonManager(config.PythonPath, HaContext, ImageQueue);
    
    return pythonManager;
  }

  private static VideoStreamManager StartVideoProcessing(Configuration config)
  {
    VideoStreamManager videoStreamManager = new VideoStreamManager(config, BufferSize, ImageQueue);
    
    return videoStreamManager;
  }
}