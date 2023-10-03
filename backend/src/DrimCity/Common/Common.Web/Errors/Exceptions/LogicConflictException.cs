using Common.Web.Errors.Exceptions.Base;

namespace Common.Web.Errors.Exceptions;

public class LogicConflictException : ErrorException
{
    public LogicConflictException(string message, string code) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
