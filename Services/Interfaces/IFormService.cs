using DTOs.Form;
using Microsoft.Azure.Cosmos;
using Models.Form;

namespace FormagenAPI.Services
{
    public interface IFormService
    {
        Task<Form> GetFormByIdAsync(string id);

        Task<List<Form>> GetFormsAsync();

        Task<CreateFormResponse> CreateFormAsync(CreateFormRequest createFormRequest);

        Task<ItemResponse<Form>> UpdateFormAsync(SaveFormRequest updateFormRequest);

        Task<ItemResponse<Form>> DeleteFormByIdAsync(string id);

        Task<ItemResponse<Form>> ShareFormAsync(ShareFormRequest shareFormRequest);
    }
}
