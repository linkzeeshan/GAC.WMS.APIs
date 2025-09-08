using FluentValidation;
using GAC.WMS.Integrations.Application.DTOs.SalesOrders;

namespace GAC.WMS.Integrations.Application.Validators
{
    public class SalesOrderLineDtoValidator : AbstractValidator<SalesOrderLineDto>
    {
        public SalesOrderLineDtoValidator()
        {
            RuleFor(x => x.LineNumber)
                .GreaterThan(0).WithMessage("Line number must be greater than 0");

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Product ID must be greater than 0");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0");

            RuleFor(x => x.TotalPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Total price must be greater than or equal to 0");
        }
    }
}
