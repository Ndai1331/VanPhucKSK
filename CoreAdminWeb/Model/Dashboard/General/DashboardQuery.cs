namespace CoreAdminWeb.Model.Dashboard.General
{
    public class DashboardQuery
    {
        public bool IsCountQuery { get; set; }
        public string Sql { get; set; } = string.Empty;
        public Action<object?>? Action { get; set; }
    }
}
