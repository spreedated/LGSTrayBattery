using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System.Reflection;

namespace LGSTrayCore.HttpServer;

public class HttpControllerFactory
{
    private readonly ILogiDeviceCollection _logiDeviceCollection;

    public HttpControllerFactory(ILogiDeviceCollection logiDeviceCollection)
    {
        _logiDeviceCollection = logiDeviceCollection;
    }

    public HttpController CreateController()
    {
        return new HttpController(_logiDeviceCollection);
    }
}

public class HttpController : WebApiController
{
    private static readonly string _assemblyVersion = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion!;
    private readonly ILogiDeviceCollection _logiDeviceCollection;

    public HttpController(ILogiDeviceCollection logiDeviceCollection)
    {
        _logiDeviceCollection = logiDeviceCollection;
    }

    private void DefaultResponse(string contentType = "text/html")
    {
        this.Response.ContentType = contentType;
        this.Response.DisableCaching();
        this.Response.KeepAlive = false;
        this.Response.Headers.Add("Access-Control-Allow-Origin", "*");
    }

    [Route(HttpVerbs.Get, "/")]
    [Route(HttpVerbs.Get, "/devices")]
    public void GetDevices()
    {
        this.DefaultResponse();

        using var tw = this.HttpContext.OpenResponseText();
        tw.Write("<html>");

        tw.Write("<b>By Device ID</b><br>");
        foreach (string? logiDeviceId in _logiDeviceCollection.GetDevices().Select(x => x.DeviceId))
        {
            tw.Write($"{logiDeviceId} : <a href=\"/device/{logiDeviceId}\">{logiDeviceId}</a><br>");
        }

        tw.Write("<br><b>By Device Name</b><br>");
        foreach (string? logiDeviceName in _logiDeviceCollection.GetDevices().Select(x => x.DeviceName))
        {
            tw.Write($"<a href=\"/device/{Uri.EscapeDataString(logiDeviceName)}\">{logiDeviceName}</a><br>");
        }

        tw.Write("<br><hr>");
        tw.Write($"<i>LGSTray version: {_assemblyVersion}</i><br>");
        tw.Write("</html>");
    }

    [Route(HttpVerbs.Get, "/device/{deviceIden}")]
    public void GetDevice(string deviceIden)
    {
        var logiDevice = _logiDeviceCollection.GetDevices().FirstOrDefault(x => x.DeviceId == deviceIden);
        logiDevice ??= _logiDeviceCollection.GetDevices().FirstOrDefault(x => x.DeviceName == deviceIden);

        using var tw = this.HttpContext.OpenResponseText();
        if (logiDevice == null)
        {
            this.HttpContext.Response.StatusCode = 404;
            tw.Write($"{deviceIden} not found.");
            return;
        }

        this.DefaultResponse("text/xml");

        tw.Write(logiDevice.GetXmlData());
    }
}
