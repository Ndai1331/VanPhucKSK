using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using System.Net;
using CoreAdminWeb.Http;
using Microsoft.Extensions.Options;
using CoreAdminWeb.Model.Configuration;
using System.Runtime.CompilerServices;

namespace CoreAdminWeb.Services.Posts
{
    public class ElectronicInvoiceService : IBaseGetService<HoaDonDienTuModel>
    {
        public ElectronicInvoiceService( )
        {
        }

        public async Task<RequestHttpResponse<List<HoaDonDienTuModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"HoaDonDienTu/electronic-invoice?{query}";
                
                var result = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<List<HoaDonDienTuModel>>>(url) ;

                var response =  result.IsSuccess
                    ? new RequestHttpResponse<List<HoaDonDienTuModel>> { Data = result.Data.Data, Meta = result.Data.Meta }
                    : new RequestHttpResponse<List<HoaDonDienTuModel>> { Errors = result.Errors };

                return response;
            }
            catch (Exception ex)
            {
                return IBaseGetService<HoaDonDienTuModel>.CreateErrorResponse<List<HoaDonDienTuModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<HoaDonDienTuModel>> GetByIdAsync(string id)
        {
           throw new NotImplementedException();
        }
    }
}