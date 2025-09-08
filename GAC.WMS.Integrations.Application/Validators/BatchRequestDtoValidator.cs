using FluentValidation;
using GAC.WMS.Integrations.Application.DTOs.Common;

namespace GAC.WMS.Integrations.Application.Validators
{
    public class BatchRequestDtoValidator<T> : AbstractValidator<BatchRequestDto<T>> where T : class
    {
        public BatchRequestDtoValidator(IValidator<T> itemValidator)
        {
            RuleFor(x => x.RequestId)
                .NotEmpty().WithMessage("Request ID is required");

            RuleFor(x => x.Items)
                .NotNull().WithMessage("Items cannot be null")
                .NotEmpty().WithMessage("Items cannot be empty");

            RuleForEach(x => x.Items)
                .SetValidator(itemValidator);
        }
    }
}
