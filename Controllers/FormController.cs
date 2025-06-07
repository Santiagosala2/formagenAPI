using System.ComponentModel.DataAnnotations;
using System.Net;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
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
        var (formNameIsUnique, formNameIsUniqueStatusCode) = await _formsService.CheckFormExistsByNameAsync(formRequest.Name);

        if (formNameIsUniqueStatusCode != HttpStatusCode.OK)
        {
            return BadRequest(new ErrorMessage("Something went wrong", HttpStatusCode.BadRequest));
        }

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
        var (formResponse, statusCode) = await _formsService.GetFormByIdAsync(id);

        if (formResponse is null)
        {
            if (statusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new ErrorMessage("Form not found", statusCode));
            }
            else
            {
                return BadRequest(new ErrorMessage("Something went wrong", statusCode));
            }
        }

        return Ok(formResponse);
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> UpdateForm(UpdateFormRequest formRequest)
    {

        var (formResponse, formResponseStatusCode) = await _formsService.GetFormByIdAsync(formRequest.Id);

        if (formResponse is null)
        {
            if (formResponseStatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new ErrorMessage("Form not found", formResponseStatusCode));
            }
            else
            {
                return BadRequest(new ErrorMessage("Something went wrong", formResponseStatusCode));
            }
        }

        if (formRequest.Name != formResponse.Name)
        {
            var (formNameIsUnique, formNameIsUniqueStatusCode) = await _formsService.CheckFormExistsByNameAsync(formRequest.Name);

            if (formNameIsUniqueStatusCode != HttpStatusCode.OK)
            {
                return BadRequest(new ErrorMessage("Something went wrong", HttpStatusCode.BadRequest));
            }

            if (!formNameIsUnique)
            {
                return BadRequest(new ErrorMessage("Form name is not unique", HttpStatusCode.BadRequest));
            }
        }



        try
        {

            Form updatedForm = new()
            {
                Id = formRequest.Id,
                Name = formRequest.Name,
                Title = formRequest.Title,
                Description = formRequest.Description,
                Questions = formRequest.Questions,
                LastUpdated = DateTime.UtcNow
            };

            var response = await _formsService.UpdateFormAsync(updatedForm, formResponse.Created);

            return Ok(response);
        }
        catch (System.Exception)
        {
            return BadRequest(new ErrorMessage("Something went wrong", HttpStatusCode.BadRequest));
        }

    }



    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteForm(string id)
    {

        var (formResponse, statusCode) = await _formsService.GetFormByIdAsync(id);

        if (formResponse is null)
        {
            if (statusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new ErrorMessage("Form not found", statusCode));
            }
            else
            {
                return BadRequest(new ErrorMessage("Something went wrong", statusCode));
            }
        }

        Form form = formResponse;
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
