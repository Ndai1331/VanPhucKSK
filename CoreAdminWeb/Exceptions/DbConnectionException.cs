using System;

namespace CoreAdminWeb.Exceptions
{
    public class DbConnectionException : Exception
    {
        public DbConnectionException(string message) : base(message)
        {
            
        }
    }
}