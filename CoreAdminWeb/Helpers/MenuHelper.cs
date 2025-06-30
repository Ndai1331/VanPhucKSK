using CoreAdminWeb.Model.Menus;

namespace CoreAdminWeb.Helpers
{
    public class MenuHelper
    {
        public static List<MenuResponse> CreateSubMenus(List<MenuResponse> menus, int? parentId = null)
        {
            var result = new List<MenuResponse>();
            foreach(var menu in menus)
            {
                if((parentId == null && menu.parent_id == null) || 
                   (parentId != null && menu.parent_id == parentId))
                {
                    result.Add(menu);
                }
            }

            foreach(var menu in result)
            {
                menu.sub_menus = CreateSubMenus(menus, menu.id);
            }

            return result;
        }

        public static string CreateUrlByName(string name, int? id = null)
        {
            if(string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }   

            if(id != null && (id == 110 || id == 111))
            {
                name = $"danh-muc-vbqppl";
                return name;
            }


            //chuyển thành chữ thường
            name = name.ToLower();

            //xoá dấu cách và chuyển thành chữ thường
            name = name.Replace(" ", "-").ToLower();

            //xoá ký tự tiếng việt
            name = name.Replace("à", "a").Replace("á", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a");
            name = name.Replace("ă", "a").Replace("ằ", "a").Replace("ắ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a");
            name = name.Replace("â", "a").Replace("ầ", "a").Replace("ấ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a");
            name = name.Replace("è", "e").Replace("é", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e");
            name = name.Replace("ê", "e").Replace("ề", "e").Replace("ế", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e");
            name = name.Replace("ì", "i").Replace("í", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i");
            name = name.Replace("ò", "o").Replace("ó", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o");
            name = name.Replace("ô", "o").Replace("ồ", "o").Replace("ố", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o");
            name = name.Replace("ơ", "o").Replace("ờ", "o").Replace("ớ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o");
            name = name.Replace("ù", "u").Replace("ú", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u");
            name = name.Replace("ư", "u").Replace("ừ", "u").Replace("ứ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u");
            name = name.Replace("ỳ", "y").Replace("ý", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y");
            name = name.Replace("đ", "d");

            // Xóa các ký tự đặc biệt
            name = System.Text.RegularExpressions.Regex.Replace(name, @"[^a-z0-9\-]", "");

            // Xóa dấu gạch ngang liên tiếp
            name = System.Text.RegularExpressions.Regex.Replace(name, @"-+", "-");

            // Xóa dấu gạch ngang ở đầu và cuối
            name = name.Trim('-');

            return name;
        }
    }
}