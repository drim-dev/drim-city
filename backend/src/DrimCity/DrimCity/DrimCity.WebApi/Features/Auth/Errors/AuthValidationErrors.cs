namespace DrimCity.WebApi.Features.Auth.Errors;

public static class AuthValidationErrors
{
    private const string Prefix = "auth:validation:";

    public const string LoginMustNotBeEmpty = Prefix + "login_must_not_be_empty";
    public const string LoginMustBeGreaterOrEqualMinLength = Prefix + "login_must_be_greater_or_equal_min_length";
    public const string LoginMustBeLessOrEqualMaxLength = Prefix + "login_must_be_less_or_equal_max_length";
    public const string LoginMustContainSpecificSymbols = Prefix + "login_must_contain_specific_symbols";

    public const string PasswordMustNotBeEmpty = Prefix + "password_must_not_be_empty";
    public const string PasswordMustBeGreaterOrEqualMinLength = Prefix + "password_must_be_greater_or_equal_min_length";
    public const string PasswordMustBeLessOrEqualMaxLength = Prefix + "password_must_be_less_or_equal_max_length";
    public const string PasswordMustContainUppercaseLetter = Prefix + "password_must_contain_uppercase_letter";
    public const string PasswordMustContainLowercaseLetter = Prefix + "password_must_contain_lowercase_letter";
    public const string PasswordMustContainNumber = Prefix + "password_must_contain_number";
    public const string PasswordMustContainSpecialSymbol = Prefix + "password_must_contain_special_symbol";
}
