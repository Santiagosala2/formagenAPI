using System.Net;
using DTOs;
using FormagenAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FormagenAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormController : ControllerBase
{
    private readonly IFormService _formService;

    public FormController(IFormService formService)
    {
        _formService = formService;
    }

    public record ErrorMessage(string message, HttpStatusCode statusCode);

    [HttpGet]
    public async Task<IActionResult> GetForms()
    {
        var forms = await _formService.GetFormsAsync();
        return Ok(forms);
    }

    [HttpPost]
    public async Task<IActionResult> CreateForm(CreateFormRequest formRequest)
    {
        var formResponse = await _formService.CreateFormAsync(formRequest);
        return Ok(formResponse);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetForm(string id)
    {
        var formResponse = await _formService.GetFormByIdAsync(id);
        return Ok(formResponse);
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> UpdateForm(UpdateFormRequest UpdateFormRequest)
    {
        var formResponse = await _formService.UpdateFormAsync(UpdateFormRequest);
        return Ok(formResponse);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteForm(string id)
    {
        var formResponse = await _formService.DeleteFormByIdAsync(id);
        return Ok(formResponse);
    }
}
