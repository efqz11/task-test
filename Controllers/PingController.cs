using Microsoft.AspNetCore.Mvc;
using TestTask;

namespace task_test.Controllers;

[ApiController]
[ApiVersion("1.0")]
public class PingController : ControllerBase
{

    private readonly ILogger<PingController> _logger;
    private readonly SearchProviderOneService searchProviderOneService;
    private readonly SearchProviderTwoService searchProviderTwoService;

    public PingController(ILogger<PingController> logger, SearchProviderOneService searchProviderOneService, SearchProviderTwoService searchProviderTwoService)
    {
        _logger = logger;
        this.searchProviderOneService = searchProviderOneService;
        this.searchProviderTwoService = searchProviderTwoService;
    }



    [Route("provider-one/api/v{version:apiVersion}/[controller]")]
    [MapToApiVersion("1.0")]
    [HttpGet]
    public async Task<IActionResult> ProviderOneGet(CancellationToken cancellationToken) => await searchProviderOneService.IsAvailableAsync(cancellationToken) ? StatusCode(200) : StatusCode(500);


    [Route("provider-two/api/v{version:apiVersion}/[controller]")]
    [MapToApiVersion("1.0")]
    [HttpGet]
    public async Task<IActionResult> ProviderTwoGet(CancellationToken cancellationToken) => await searchProviderTwoService.IsAvailableAsync(cancellationToken) ? StatusCode(200) : StatusCode(500);


}

