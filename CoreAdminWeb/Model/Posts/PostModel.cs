using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class PostModel : BaseModel<int>
    {
        public string? title { get; set; }
        public string? sort_description { get; set; }
        public int? post_catgory { get; set; }
        public int? view_count { get; set; }
        public FileModel? post_images { get; set; }
    }
}