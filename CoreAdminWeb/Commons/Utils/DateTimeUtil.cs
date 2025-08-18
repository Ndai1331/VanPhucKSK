namespace CoreAdminWeb.Commons.Utils
{
    public static class DateTimeUtil
    {
        public static string CalculateAge(DateTime? birthDate, DateOnly? asOf = null)
        {
            if (birthDate == null)
            {
                return string.Empty;
            }

            DateOnly birthDateOnly = DateOnly.FromDateTime(birthDate.Value);
            var today = asOf ?? DateOnly.FromDateTime(DateTime.Now);
            if (birthDateOnly > today)
            {
                return string.Empty;
            }

            // Xác định "sinh nhật năm nay", quy ước: nếu sinh 29/02 thì năm không nhuận -> tính sang 01/03
            var isLeapBirth = birthDateOnly.Month == 2 && birthDateOnly.Day == 29;
            var birthdayThisYear = isLeapBirth && !DateTime.IsLeapYear(today.Year)
                ? new DateOnly(today.Year, 3, 1)
                : new DateOnly(today.Year, birthDateOnly.Month, birthDateOnly.Day);

            var age = today.Year - birthDateOnly.Year;
            if (today < birthdayThisYear)
            {
                age--;
            }

            return age.ToString();
        }
    }
}
