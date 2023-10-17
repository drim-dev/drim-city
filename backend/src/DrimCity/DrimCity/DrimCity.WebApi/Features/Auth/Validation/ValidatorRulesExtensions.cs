using DrimCity.WebApi.Domain;
using FluentValidation;
using static DrimCity.WebApi.Features.Auth.Errors.AuthValidationErrors;

namespace DrimCity.WebApi.Features.Auth.Validation;

public static class ValidatorRulesExtensions
{
    public static IRuleBuilderOptions<T, string> LoginLength<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
                .WithMessage("Login cannot be empty")
                .WithErrorCode(LoginRequired)
            .MinimumLength(Account.LoginMinLength)
                .WithMessage($"Login length must be greater or equal than {Account.LoginMinLength}")
                .WithErrorCode(LoginMustBeGreaterOrEqualMinLength)
            .MaximumLength(Account.LoginMaxLength)
                .WithMessage($"Login length must be less or equal than {Account.LoginMaxLength}")
                .WithErrorCode(LoginMustBeLessOrEqualMaxLength);

    public static IRuleBuilderOptions<T, string> PasswordLength<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
                .WithMessage("Password cannot be empty")
                .WithErrorCode(PasswordRequired)
            .MinimumLength(Account.PasswordMinLength)
                .WithMessage($"Password length must be greater or equal than {Account.PasswordMinLength}")
                .WithErrorCode(PasswordMustBeGreaterOrEqualMinLength)
            .MaximumLength(Account.PasswordMaxLength)
                .WithMessage($"Password length must be less or equal {Account.PasswordMaxLength}")
                .WithErrorCode(PasswordMustBeLessOrEqualMaxLength);

}
