using System.Data.Common;

namespace CoreAdminWeb.Model.Dashboard.General
{
    public class DashboardQuery
    {
        public string Sql { get; set; } = string.Empty;
        public Action<DbDataReader>? Action { get; set; }
    }
}
