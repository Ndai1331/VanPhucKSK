namespace CoreAdminWeb.Commons.Utils
{
    public static class DeltaSpanUtil
    {
        public static string RenderDeltaSpan(this decimal? value, string format = "N0")
        {
            if (value is null)
            {
                return "<span style='background-color:#D1D5DB;padding:2px 6px;border-radius:4px;'>—</span>";
            }

            string pos = "#51f046";
            string neg = "#e05a5a";
            string neu = "#dadee6";

            string bg = value switch
            {
                > 0m => pos,
                < 0m => neg,
                _ => neu
            };

            return $"<span style='background-color:{bg};padding:2px 6px;border-radius:4px;'>{value?.ToString(format)}</span>";
        }
    }
}
