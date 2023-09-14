using DrimCity.WebApi.Common.Errors.Exceptions.Base;

namespace DrimCity.WebApi.Common.Errors.Exceptions;

public class LogicConflictException : ErrorException
{
    public LogicConflictException(string message, string code) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
