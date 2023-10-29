using FluentValidation;

namespace Common.Web.Validation.Extensions;

public static class ValidationRulesExtensions
{
    public static IRuleBuilderOptions<T, string> NotEmpty<T>(this IRuleBuilder<T, string> ruleBuilder, string errorCode)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("{PropertyName} must not be empty")
            .WithErrorCode(errorCode);
    }

    public static IRuleBuilderOptions<T, string> MinimumLength<T>(this IRuleBuilder<T, string> ruleBuilder,
        int minimumLength, string errorCode) => ruleBuilder
        .MinimumLength(minimumLength)
            .WithMessage($"{{PropertyName}} length must be greater or equal {minimumLength}")
            .WithErrorCode(errorCode);

    public static IRuleBuilderOptions<T, string> MaximumLength<T>(this IRuleBuilder<T, string> ruleBuilder,
        int maximumLength, string errorCode) => ruleBuilder
        .MaximumLength(maximumLength)
            .WithMessage($"{{PropertyName}} length length must be less or equal {maximumLength}")
            .WithErrorCode(errorCode);
}
