using CoreAdminWeb.Model.User;
using System.ComponentModel;

namespace CoreAdminWeb.Model.Base
{
    public enum Status
    {
        [Description("Bản nháp")]
        draft,
        [Description("Xuất bản")]
        published,
        [Description("Đã xóa")]
        removed
    }
    public class BaseModel<T> : BaseDetailModel
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
        public Status status { get; set; } = Status.published;
        public int? sort { get; set; } = 0;
        public bool? active { get; set; } = true;
    }
}
