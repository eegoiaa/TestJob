using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TestJob.Api.Models;
using TestJob.Api.Services;

namespace TestJob.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly IProcessingService _processingService;
    private readonly IValidator<ProcessRequest> _validator;

    public ProcessingController(IProcessingService processingService, IValidator<ProcessRequest> validator)
    {
        _processingService = processingService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<ActionResult<ProcessResponse>> Process([FromBody] ProcessRequest request, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Ok(ProcessResponse.Error("VALIDATION_ERROR", errors));
        }

        var result = await _processingService.ProcessAsync(request, ct);
        return Ok(result);
    }
}
