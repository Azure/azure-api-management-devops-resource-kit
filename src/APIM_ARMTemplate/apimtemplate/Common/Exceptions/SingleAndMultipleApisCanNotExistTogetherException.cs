using System;

namespace apimtemplate.Common.Exceptions
{
    public class SingleAndMultipleApisCanNotExistTogetherException : Exception
    {
        public SingleAndMultipleApisCanNotExistTogetherException(string message) : base(message)
        {
        }
    }
}
