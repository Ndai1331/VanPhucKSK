namespace CoreAdminWeb.Commons
{
    public static class GlobalConstant
    {
        public static string BaseUrl { get; set; } = "https://localhost:5001/"; // Default base URL, can be overridden

        public static readonly IReadOnlyList<string> PefaultChartColors = new List<string>()
        {
            "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF", "#FF9F40",
            "#C9CBCF", "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF",
            "#FF9F40", "#C9CBCF", "#B2FF66", "#66FFB2", "#66B2FF", "#B266FF",
            "#FF66B2", "#FFB266", "#A2EB36", "#6384FF", "#CE56FF", "#C0C04B",
            "#66FF99", "#FF6666", "#66FF66", "#6666FF", "#FF66FF", "#66FFFF",
            "#FFFF66"
        };
    }
}
