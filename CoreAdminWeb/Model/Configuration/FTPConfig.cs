namespace CoreAdminWeb.Model.Configuration
{
    public class FTPConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UsePassive { get; set; }
    }
} 