using IRoute.Commerce.Api.Contracts;
using IRoute.Commerce.Application.Commerces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRoute.Commerce.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/commerce")]
public class CommerceController : ControllerBase
{
    private const long MaxFileSize = 50 * 1024 * 1024;

    private readonly CommerceService _service;

    public CommerceController(CommerceService service)
    {
        _service = service;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return Problem(title: "El archivo se encuentra vacío.", statusCode: StatusCodes.Status400BadRequest);

        await using var stream = file.OpenReadStream();
        var result = await _service.UploadAsync(file.FileName, file.Length, stream);

        if (!result.Success)
        {
            var problem = new ProblemDetails { Title = result.Error, Status = StatusCodes.Status400BadRequest };
            problem.Extensions["errors"] = result.Errors;
            return BadRequest(problem);
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            fileName = result.FileName,
            inserted = result.Inserted,
            processDates = result.ProcessDates
        });
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process(ProcessRequest request)
    {
        var result = await _service.ProcessAsync(request.ProcessDate!.Value);
        return Ok(result);
    }

    [HttpGet("quarantine")]
    public async Task<IActionResult> GetQuarantine([FromQuery] DateOnly? processDate)
    {
        var records = await _service.GetQuarantinedAsync(processDate);
        return Ok(records);
    }

    [HttpGet("process-dates")]
    public async Task<IActionResult> GetProcessDates()
    {
        var dates = await _service.GetProcessDatesAsync();
        return Ok(dates);
    }
}
