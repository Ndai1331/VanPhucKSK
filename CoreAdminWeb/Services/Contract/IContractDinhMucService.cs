using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;

namespace CoreAdminWeb.Services.Contract
{
    public interface IContractDinhMucService : IBaseService<ContractDinhMucModel>
    {
        Task<RequestHttpResponse<List<ContractDinhMucModel>>> CreateAsync(List<ContractDinhMucModel> model);
        Task<RequestHttpResponse<bool>> UpdateAsync(List<ContractDinhMucModel> model);
        Task<RequestHttpResponse<bool>> DeleteAsync(List<ContractDinhMucModel> model);
    }
}
