using FluentValidation;
using MediatR;
using WebApi.Common.Errors.Exceptions;

namespace WebApi.Common.Pipeline.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IValidator<TRequest>? _validator;

    public ValidationBehavior(IValidator<TRequest>? validator = null)
    {
        _validator = validator;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validator is not null)
        {
            var result = await _validator.ValidateAsync(request, cancellationToken);

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => new RequestValidationError(x.PropertyName, x.ErrorMessage, x.ErrorCode));
                throw new ValidationErrorsException(errors);
            }
        }

        return await next();
    }
}
