using System.Globalization;

namespace CoreAdminWeb.Helpers
{
    public static class NumberHelper
    {
        public static string ConvertToShortText(decimal amount)
        {
            // Xử lý số âm
            bool isNegative = amount < 0;
            amount = Math.Abs(amount);

            // Nếu số nhỏ hơn 1000, trả về định dạng số nguyên
            if (amount < 1000)
            {
                return (isNegative ? "-" : "") + amount.ToString("N0", CultureInfo.InvariantCulture);
            }

            decimal billion = 1_000_000_000;
            decimal million = 1_000_000;
            decimal thousand = 1_000;

            // Xử lý số từ 1 tỷ trở lên
            if (amount >= billion)
            {
                decimal billions = Math.Floor(amount / billion);
                decimal remainingMillions = (amount % billion) / million;

                string result = $"{billions:N0}";
                if (remainingMillions > 0)
                {
                    result += $" tỷ {(remainingMillions):N2}".Replace(".00", "").TrimEnd('0').TrimEnd(',');
                }
                else
                {
                    result += " tỷ";
                }

                return (isNegative ? "-" : "") + result;
            }

            // Xử lý số từ 1 triệu đến dưới 1 tỷ
            if (amount >= million)
            {
                decimal millions = amount / million;
                return (isNegative ? "-" : "") + $"{millions:N2} triệu".Replace(".00", "").TrimEnd('0').TrimEnd(',');
            }

            // Xử lý số từ 1000 đến dưới 1 triệu
            if (amount >= thousand)
            {
                decimal thousands = amount / thousand;
                return (isNegative ? "-" : "") + $"{thousands:N2} nghìn".Replace(".00", "").TrimEnd('0').TrimEnd(',');
            }

            // Trường hợp còn lại
            return (isNegative ? "-" : "") + amount.ToString("N2", CultureInfo.InvariantCulture).Replace(".00", "").TrimEnd('0').TrimEnd(',');
        }
    }
}
