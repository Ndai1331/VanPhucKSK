using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.Extensions
{
    public static class RequestHttpResponseExtensions
    {
        public static string GetErrorMessage(this List<ErrorResponse> errors)
        {
            if (errors == null || !errors.Any())
                return "Đã xảy ra lỗi không xác định";

            var error = errors[0];
            
            return error.Code switch
            {
                "RECORD_NOT_UNIQUE" => "Mã này đã tồn tại trong hệ thống",
                _ => error.Message ?? "Đã xảy ra lỗi không xác định"
            };
        }
    }
}