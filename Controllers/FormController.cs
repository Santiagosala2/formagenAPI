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

    [AdminAuthorizeSession]
    [HttpGet]
    public async Task<IActionResult> GetForms()
    {
        var forms = await _formService.GetFormsAsync();
        return Ok(forms);
    }

    [AdminAuthorizeSession]
    [HttpPost]
    public async Task<IActionResult> CreateForm(CreateFormRequest formRequest)
    {
        var formResponse = await _formService.CreateFormAsync(formRequest);
        return Ok(formResponse);
    }

    [AdminAuthorizeSession]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetForm(string id)
    {
        var formResponse = await _formService.GetFormByIdAsync(id);
        return Ok(formResponse);
    }

    [AdminAuthorizeSession]
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
