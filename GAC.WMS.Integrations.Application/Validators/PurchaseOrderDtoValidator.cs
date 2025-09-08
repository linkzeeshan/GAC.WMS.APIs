using FluentValidation;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;

namespace GAC.WMS.Integrations.Application.Validators
{
    public class PurchaseOrderDtoValidator : AbstractValidator<PurchaseOrderDto>
    {
        public PurchaseOrderDtoValidator()
        {
            RuleFor(x => x.PONumber)
                .NotEmpty().WithMessage("Purchase order number is required")
                .MaximumLength(50).WithMessage("Purchase order number cannot exceed 50 characters");

            RuleFor(x => x.VendorId)
                .NotEmpty().WithMessage("Vendor ID is required")
                .MaximumLength(50).WithMessage("Vendor ID cannot exceed 50 characters");

            RuleFor(x => x.OrderDate)
                .NotEmpty().WithMessage("Order date is required");

            RuleFor(x => x.Status)
                .MaximumLength(20).WithMessage("Status cannot exceed 20 characters");

            RuleFor(x => x.Currency)
                .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters");

            RuleFor(x => x.TotalAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Total amount must be greater than or equal to 0");

            RuleFor(x => x.CustomerId)
                .GreaterThanOrEqualTo(0).WithMessage("Weight must be greater than or equal to 0");

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

            RuleFor(x => x.POLines)
                .NotNull().WithMessage("Purchase order lines cannot be null");

            RuleForEach(x => x.POLines)
                .SetValidator(new PurchaseOrderLineDtoValidator());
        }
    }
}
