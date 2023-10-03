using Common.Web.Errors.Exceptions.Base;

namespace Common.Web.Errors.Exceptions;

public class InternalErrorException : ErrorException
{
    public InternalErrorException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
