namespace CoreAdminWeb.Extensions
{
    public static class StringExtension
    {
        public static (string FirstName, string LastName) SplitName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (string.Empty, string.Empty);
            }
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string lastName = "";
            string firstName = "";

            if (parts.Length > 0)
            {
                firstName = parts[^1]; // phần tử cuối là tên
                lastName = string.Join(" ", parts[..^1]); // ghép các phần còn lại thành họ
            }

            return (firstName, lastName);
        }
    }
}
