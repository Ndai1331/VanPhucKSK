using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class PostModel 
    {
        public string? title { get; set; }
        public string? sort_description { get; set; }
        public string? description { get; set; }
        public int? post_catgory { get; set; }
        public int? view_count { get; set; }
        public FileModel? post_images { get; set; }
        public string? status { get; set; }
        public string? user_created { get; set; }
        public string? user_updated { get; set; }
        public int id { get; set; }
        public DateTime date_created { get; set; } = DateTime.Now;
        public DateTime? date_updated { get; set; } = DateTime.Now;
    }
}