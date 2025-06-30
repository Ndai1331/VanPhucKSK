using System;

namespace CoreAdminWeb.Exceptions
{
    public class FailedOperation : Exception
    {
        public FailedOperation(string message) : base(message)
        {
            
        }
    }
}