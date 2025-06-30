namespace CoreAdminWeb.Model.Tables
{
    public class CaptionsWithIcon
    {
        public string? Name { get; set; }
        public List<TeamMember> AssignedTo { get; set; } = new List<TeamMember>();
        public int TeamOverflow { get; set; }
        public string? TimeSpent { get; set; }
        public string? Status { get; set; }
        public string? StatusColor { get; set; }
    }

    public class TeamMember
    {
        public string? ImageUrl { get; set; }
    }
}
