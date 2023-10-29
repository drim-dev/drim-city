using Common.Web.Errors.Exceptions.Base;

namespace Common.Web.Errors.Exceptions;

public class UnauthorizedException : ErrorException
{
    public UnauthorizedException() : base("Request is unauthorized")
    {
    }
}
