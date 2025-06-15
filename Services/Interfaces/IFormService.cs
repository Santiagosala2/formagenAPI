using DTOs;
using Microsoft.Azure.Cosmos;
using Models;

namespace FormagenAPI.Services
{
    public interface IFormService
    {
        Task<Form> GetFormByIdAsync(string id);

        Task<List<Form>> GetFormsAsync();

        Task<CreateFormResponse> CreateFormAsync(CreateFormRequest createFormRequest);

        Task<ItemResponse<Form>> UpdateFormAsync(UpdateFormRequest updateFormRequest);

        Task<ItemResponse<Form>> DeleteFormByIdAsync(string id);
    }
}
