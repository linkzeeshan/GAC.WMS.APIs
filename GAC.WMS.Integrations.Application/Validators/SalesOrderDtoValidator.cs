using FluentValidation;
using GAC.WMS.Integrations.Application.DTOs.SalesOrders;

namespace GAC.WMS.Integrations.Application.Validators
{
    public class SalesOrderDtoValidator : AbstractValidator<SalesOrderDto>
    {
        public SalesOrderDtoValidator()
        {
            RuleFor(x => x.SONumber)
                .NotEmpty().WithMessage("Sales order number is required")
                .MaximumLength(50).WithMessage("Sales order number cannot exceed 50 characters");

            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required")
                .MaximumLength(50).WithMessage("Customer ID cannot exceed 50 characters");

            RuleFor(x => x.OrderDate)
                .NotEmpty().WithMessage("Order date is required");

            RuleFor(x => x.Status)
                .MaximumLength(20).WithMessage("Status cannot exceed 20 characters");

            RuleFor(x => x.Currency)
                .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters");

            RuleFor(x => x.TotalAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Total amount must be greater than or equal to 0");

            RuleFor(x => x.ShippingStreet)
                .MaximumLength(100).WithMessage("Shipping street cannot exceed 100 characters");

            RuleFor(x => x.ShippingCity)
                .MaximumLength(50).WithMessage("Shipping city cannot exceed 50 characters");

            RuleFor(x => x.ShippingStateProvince)
                .MaximumLength(50).WithMessage("Shipping state/province cannot exceed 50 characters");

            RuleFor(x => x.ShippingPostalCode)
                .MaximumLength(20).WithMessage("Shipping postal code cannot exceed 20 characters");

            RuleFor(x => x.ShippingCountry)
                .MaximumLength(50).WithMessage("Shipping country cannot exceed 50 characters");

            RuleFor(x => x.BillingStreet)
                .MaximumLength(100).WithMessage("Billing street cannot exceed 100 characters");

            RuleFor(x => x.BillingCity)
                .MaximumLength(50).WithMessage("Billing city cannot exceed 50 characters");

            RuleFor(x => x.BillingStateProvince)
                .MaximumLength(50).WithMessage("Billing state/province cannot exceed 50 characters");

            RuleFor(x => x.BillingPostalCode)
                .MaximumLength(20).WithMessage("Billing postal code cannot exceed 20 characters");

            RuleFor(x => x.BillingCountry)
                .MaximumLength(50).WithMessage("Billing country cannot exceed 50 characters");

            RuleFor(x => x.SOLines)
                .NotNull().WithMessage("Sales order lines cannot be null");

            RuleForEach(x => x.SOLines)
                .SetValidator(new SalesOrderLineDtoValidator());
        }
    }
}
