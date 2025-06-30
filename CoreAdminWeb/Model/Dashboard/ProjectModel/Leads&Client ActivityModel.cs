namespace CoreAdminWeb.Model.Dashboard.ProjectModel
{
    public class Leads_Client_ActivityModel
    {
        public string? ProjectName { get; set; }
        public string? WorkTimeline { get; set; }
        public List<TeamMember> ProjectTeam { get; set; } = new List<TeamMember>();
        public int TeamOverflow { get; set; }
        public string? ProjectType { get; set; }
        public string? Status { get; set; }
    }

    public class TeamMember
    {
        public string? ImageUrl { get; set; }
    }
}
