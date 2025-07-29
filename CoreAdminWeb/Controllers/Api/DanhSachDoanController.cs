
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;



namespace CoreAdminWeb.Controllers.Api;

/// <summary>
/// 
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CodeController : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="client"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> Get([FromQuery] string fileName,
     [FromServices] HttpClient client,
      [FromServices] IOptionsMonitor<WebsiteOptions> options)
    {
        var ret = "";
        client.BaseAddress = new Uri(options.CurrentValue.SourceUrl);
        try
        {
            ret = await client.GetStringAsync(fileName);
        }
        catch (HttpRequestException ex) { ret = ex.StatusCode == HttpStatusCode.NotFound ? "无" : ex.StatusCode.ToString() ?? "网络错误"; }
        catch (Exception) { }
        return ret;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpOptions]
    public string Options() => string.Empty;
}