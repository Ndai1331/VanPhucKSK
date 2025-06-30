namespace CoreAdminWeb.Model.Dashboard.IndexModel
{
    public class TopPerformingPagesModel
    {
        public string? PageName { get; set; }
        public int Clicks { get; set; }
        public int Sessions { get; set; }
        public string? Change { get; set; }
        public string? ChangePercentage { get; set; }
    }
}
