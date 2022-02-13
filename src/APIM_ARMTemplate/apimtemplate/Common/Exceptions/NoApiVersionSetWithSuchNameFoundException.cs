using System;

namespace apimtemplate.Common.Exceptions
{
    public class NoApiVersionSetWithSuchNameFoundException : Exception
    {
        public NoApiVersionSetWithSuchNameFoundException(string message) : base(message)
        {

        }
    }
}
