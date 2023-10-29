using Common.Web.Validation.Extensions;
using DrimCity.WebApi.Domain;
using FluentValidation;
using static DrimCity.WebApi.Features.Auth.Errors.AuthValidationErrors;

namespace DrimCity.WebApi.Features.Auth.Validation;

public static class ValidatorRulesExtensions
{
    public static IRuleBuilderOptions<T, string> LoginLength<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty(LoginMustNotBeEmpty)
            .MinimumLength(Account.LoginMinLength, LoginMustBeGreaterOrEqualMinLength)
            .MaximumLength(Account.LoginMaxLength, LoginMustBeLessOrEqualMaxLength);

    public static IRuleBuilderOptions<T, string> PasswordLength<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty(PasswordMustNotBeEmpty)
            .MinimumLength(Account.PasswordMinLength, PasswordMustBeGreaterOrEqualMinLength)
            .MaximumLength(Account.PasswordMaxLength, PasswordMustBeLessOrEqualMaxLength);
}
