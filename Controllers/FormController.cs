using System.ComponentModel.DataAnnotations;
using System.Net;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace FormagenAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormController : ControllerBase
{
    private readonly FormsService _formsService;

    public FormController(FormsService formsService)
    {
        _formsService = formsService;
    }

    public record ErrorMessage(string message, HttpStatusCode statusCode);

    [HttpGet]
    public async Task<IActionResult> GetForms()
    {
        var forms = await _formsService.GetFormsAsync();

        return Ok(forms);
    }

    [HttpPost]
    public async Task<IActionResult> CreateForm(CreateFormRequest formRequest)
    {
        // Verify that the form name is unique
        bool formNameIsUnique = await _formsService.CheckFormExistsByNameAsync(formRequest.Name);

        if (!formNameIsUnique)
        {
            return BadRequest(new ErrorMessage("Form name is not unique", HttpStatusCode.BadRequest));
        }

        Form form = new()
        {
            Id = Guid.NewGuid().ToString(),
            Name = formRequest.Name,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        await _formsService.CreateAsync(form);

        CreateFormResponse formResponse = new()
        {
            Id = form.Id,
            Name = form.Name
        };

        return Ok(formResponse);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetForm(string id)
    {
        var form = await _formsService.GetFormByIdAsync(id);

        if (form == null)
        {
            return NotFound(new ErrorMessage("Form not found", HttpStatusCode.NotFound));
        }

        return Ok(form);
    }





    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteForm(string id)
    {
        var form = await _formsService.GetFormByIdAsync(id);

        if (form == null)
        {
            return NotFound(new ErrorMessage("Form not found", HttpStatusCode.NotFound));
        }

        try
        {
            var response = await _formsService.DeleteFormByIdAsync(id);
        }
        catch (System.Exception)
        {
            return BadRequest(new ErrorMessage("Something went wrong", HttpStatusCode.BadRequest));
        }

        return Ok();

    }


}
