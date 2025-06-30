using System.ComponentModel;
using CoreAdminWeb.Exceptions;
using CoreAdminWeb.Model.User;

namespace CoreAdminWeb.Model.Base
{
    public enum Status
    {
        [Description("Đang hoạt động")]
        active,
        [Description("Ngưng hoạt động")]
        deactive,
        [Description("Đã xóa")]
        removed
    }
    public class BaseModel<T> :  BaseDetailModel
    {
        public T id { get; set; }
        public UserModel? user_created { get; set; }
        public DateTime date_created { get; set; } = DateTime.Now;
        public UserModel? user_updated { get; set; }
        public DateTime? date_updated { get; set; }
    }

    public class BaseDetailModel
    {
        public string? code { get; set; }
        public string? name { get; set; }
        public bool? deleted { get; set; } = false;
        public string? description { get; set; }
        public Status status { get; set; } = Status.active;
        public int? sort { get; set; } = 0;
    }
}   
