namespace CoreAdminWeb.Commons.Validations
{
    public static class EmailValidation
    {
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
        }
    }
}
