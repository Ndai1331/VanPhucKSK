using System.Text.Json;

namespace CoreAdminWeb.Helpers
{
    public static class ObjectCloner
    {
        public static T DeepClone<T>(this T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json)!;
        }
    }
}