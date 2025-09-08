using FluentValidation;
using GAC.WMS.Integrations.Application.DTOs.Customers;

namespace GAC.WMS.Integrations.Application.Validators
{
    public class CustomerDtoValidator : AbstractValidator<CustomerDto>
    {
        public CustomerDtoValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required")
                .MaximumLength(50).WithMessage("Customer ID cannot exceed 50 characters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("A valid email address is required");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

            RuleFor(x => x.Street)
                .MaximumLength(100).WithMessage("Street cannot exceed 100 characters");

            RuleFor(x => x.City)
                .MaximumLength(50).WithMessage("City cannot exceed 50 characters");

            RuleFor(x => x.StateProvince)
                .MaximumLength(50).WithMessage("State/Province cannot exceed 50 characters");

            RuleFor(x => x.PostalCode)
                .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters");

            RuleFor(x => x.Country)
                .MaximumLength(50).WithMessage("Country cannot exceed 50 characters");

            RuleFor(x => x.CustomerType)
                .MaximumLength(50).WithMessage("Customer type cannot exceed 50 characters");

            RuleFor(x => x.TaxIdentifier)
                .MaximumLength(50).WithMessage("Tax identifier cannot exceed 50 characters");
        }
    }
}
