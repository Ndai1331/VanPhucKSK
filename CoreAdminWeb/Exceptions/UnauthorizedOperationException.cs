using System;

namespace CoreAdminWeb.Exceptions
{
    public class UnauthorizedOperationException : Exception
    {
        public UnauthorizedOperationException(string message) : base(message)
        {
            
        }
    }
}