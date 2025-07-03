using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Helpers;
using System.Text.Json.Serialization;


namespace CoreAdminWeb.Model  
{

    public class FolderModel 
    {
        public Guid id { get; set; } = Guid.Empty;
        public string? name { set; get; }
        public Guid? parent { set; get; }
        [JsonIgnore]
        public List<FolderModel> sub_folders { get; set; } = new List<FolderModel>();
        [JsonIgnore]
        public bool isOpen { get; set; } = false;
        [JsonIgnore]
        public bool isSelected { get; set; } = false;
    }
    public class FolderCRUDModel 
    {
        public Guid? parent { set; get; }
        public string? name { set; get; }
    }


    public class FileModel  : BaseModel<Guid>
    {
        public string? storage { set; get; }
        public string? filename_disk { set; get; }
        public string? filename_download { set; get; }
        public string? title { set; get; }
        public string? type { set; get; }
        public Guid? folder { set; get; }
        public Guid? uploaded_by { set; get; }
        public DateTime? created_on { set; get; }
        public DateTime? modified_on { set; get; }
        public string? charset { set; get; }
        public string? filesize { set; get; }
        public int? width { set; get; }
        public int? height { set; get; }
        public string? duration { set; get; }
        public string? embed { set; get; }
        public string? description { set; get; }
        public string? location { set; get; }
        public string? tags { set; get; }
        //public string? metadata { set; get; }
        public int? focal_point_x { set; get; }
        public int? focal_point_y { set; get; }
        public string? tus_id { set; get; }
        public string? tus_data { set; get; }
        public DateTime? uploaded_on { set; get; }
        [JsonIgnore]    
        public string? icon_file { set; get; }
        [JsonIgnore]
        public string? type_file { set; get; }
        [JsonIgnore]
        public string? url_download { set; get; }
        
        

        public (string, string) GetFileType()
        {
            var fileType = FileHelpers.GetFileType(filename_disk);
            icon_file = fileType.Item2;
            type_file = fileType.Item1;
            return fileType;
        }
        
        public string? GetFileSize()
        {
            return FileHelpers.ConvertSize(long.Parse(filesize));
        }

        public string? GetMinifiedFileName()
        {
            return FileHelpers.MinifyFileName(title ?? filename_disk);
        }

    }
    public class FileCRUDModel  
    {
        public string? icon_file { set; get; }
        public string? type_file { set; get; }
        public Guid? folder { set; get; } = null;

        public string? storage { set; get; }
        public string? filename_disk { set; get; }
        public string? filename_download { set; get; }
        public string? title { set; get; }
        public string? type { set; get; }
        public string? charset { set; get; }
        public string? filesize { set; get; }
        public int? width { set; get; }
        public int? height { set; get; }
        public string? duration { set; get; }
        public string? embed { set; get; }
        public string? description { set; get; }
        public string? location { set; get; }
        public string? tags { set; get; }
        //public string? metadata { set; get; }
        public int? focal_point_x { set; get; }
        public int? focal_point_y { set; get; }
        public string? tus_id { set; get; }
        public string? tus_data { set; get; }
        
    }
}