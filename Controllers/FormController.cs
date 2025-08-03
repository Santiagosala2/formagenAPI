using System.Net;
using DTOs.Form;
using FormagenAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.User;

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

    [AuthorizeSession]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetForm(string id)
    {
        var session = HttpContext.Items["Session"] as Session;
        var formResponse = await _formService.GetFormByIdAsync(id, session);
        return Ok(formResponse);
    }

    [AdminAuthorizeSession]
    [HttpPost("{id}")]
    public async Task<IActionResult> UpdateForm(SaveFormRequest SaveFormRequest)
    {

        var formResponse = await _formService.UpdateFormAsync(SaveFormRequest);
        return Ok(formResponse);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteForm(string id)
    {
        var formResponse = await _formService.DeleteFormByIdAsync(id);
        return Ok(formResponse);
    }

    [AdminAuthorizeSession]
    [HttpPost("share/")]
    public async Task<IActionResult> ShareForm(ShareFormRequest shareFormRequest)
    {
        var formResponse = await _formService.ShareFormAsync(shareFormRequest);
        return Ok(formResponse);
    }


    [AdminAuthorizeSession]
    [HttpPost("removeAccess/")]
    public async Task<IActionResult> RemoveAccessForm(RemoveAccessFormRequest removeAccessFormRequest)
    {
        var formResponse = await _formService.RemoveAccessFormAsync(removeAccessFormRequest);
        return Ok(formResponse);
    }


    [AdminAuthorizeSession]
    [HttpPost("submit/")]
    public async Task<IActionResult> SubmitForm(SubmitFormRequest submitFormRequest)
    {
        var formResponse = await _formService.SubmitFormAsync(submitFormRequest);
        return Ok(formResponse);
    }

    [AdminAuthorizeSession]
    [HttpGet("{id}/responses")]
    public async Task<IActionResult> GetFormResponses(string id)
    {
        var formResponse = await _formService.GetFormResponsesAsync(id);
        return Ok(formResponse);
    }

}
