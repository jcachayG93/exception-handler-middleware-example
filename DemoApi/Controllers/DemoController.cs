using DemoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoApi.Controllers;

[ApiController]
public class DemoController
: ControllerBase
{
    private readonly PingService _someService;

    public DemoController(
        PingService someService)
    {
        _someService = someService;
    }
    
    [HttpGet("ping")]
    public ActionResult<string> Ping()
    {
       return Ok(_someService.Ping());
    }
}