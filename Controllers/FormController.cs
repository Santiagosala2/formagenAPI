using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace FormagenAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class FormController : ControllerBase
{
    private readonly FormsService _formsService;

    public FormController(FormsService formsService)
    {
        _formsService = formsService;
    }


    [HttpPost]
    public async Task<IActionResult> Post(FormRequest formRequest)
    {
        Form form = new Form()
        {
            Id = Guid.NewGuid().ToString(),
            Name = formRequest.Name,
            Title = formRequest.Title,
            Description = formRequest.Description,
            Questions = formRequest.Questions
        };



        await _formsService.CreateAsync(form);

        FormResponse formResponse = new FormResponse()
        {
            Id = form.Id,
            Name = form.Name
        };

        return Ok(formResponse);
    }

}
