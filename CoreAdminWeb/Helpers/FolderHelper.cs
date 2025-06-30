using  CoreAdminWeb.Model;
namespace CoreAdminWeb.Helpers
{
    public class FolderHelper
    {
        public static List<FolderModel> CreateSubMenus(List<FolderModel> folders, Guid? parentId = null)
        {
            var result = new List<FolderModel>();
            foreach(var folder in folders)
            {
                if((parentId == null && folder.parent == null) || 
                   (parentId != null && folder.parent == parentId))
                {
                    result.Add(folder);
                }
            }

            foreach(var folder in result)
            {
                folder.sub_folders = CreateSubMenus(folders, folder.id);
            }

            return result;
        }
    }
}