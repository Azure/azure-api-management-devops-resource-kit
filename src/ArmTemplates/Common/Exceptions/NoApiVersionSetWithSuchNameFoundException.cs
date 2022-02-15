using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions
{
    public class NoApiVersionSetWithSuchNameFoundException : Exception
    {
        public NoApiVersionSetWithSuchNameFoundException(string message) : base(message)
        {

        }
    }
}
