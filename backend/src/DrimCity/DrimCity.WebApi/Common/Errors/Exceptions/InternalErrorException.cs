using DrimCity.WebApi.Common.Errors.Exceptions.Base;

namespace DrimCity.WebApi.Common.Errors.Exceptions;

public class InternalErrorException : ErrorException
{
    public InternalErrorException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
