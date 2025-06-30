using System;

namespace CoreAdminWeb.Exceptions
{
    public class TooManyRequests : Exception
    {
        public TooManyRequests(string message) : base(message)
        {
            
        }
    }
}