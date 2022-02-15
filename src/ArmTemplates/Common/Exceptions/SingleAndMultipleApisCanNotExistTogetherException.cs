using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions
{
    public class SingleAndMultipleApisCanNotExistTogetherException : Exception
    {
        public SingleAndMultipleApisCanNotExistTogetherException(string message) : base(message)
        {
        }
    }
}
