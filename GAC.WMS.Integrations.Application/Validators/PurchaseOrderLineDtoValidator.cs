using FluentValidation;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;

namespace GAC.WMS.Integrations.Application.Validators
{
    public class PurchaseOrderLineDtoValidator : AbstractValidator<PurchaseOrderLineDto>
    {
        public PurchaseOrderLineDtoValidator()
        {
            RuleFor(x => x.LineNumber)
                .GreaterThan(0).WithMessage("Line number must be greater than 0");

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required")
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0");

            RuleFor(x => x.TotalPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Total price must be greater than or equal to 0");
        }
    }
}
