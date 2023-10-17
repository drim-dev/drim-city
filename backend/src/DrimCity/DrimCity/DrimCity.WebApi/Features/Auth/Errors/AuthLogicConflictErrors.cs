namespace DrimCity.WebApi.Features.Auth.Errors;

public static class AuthLogicConflictErrors
{
    private const string Prefix = "auth:logic:";

    public const string AccountAlreadyExists = Prefix + "account_already_exists";
}
