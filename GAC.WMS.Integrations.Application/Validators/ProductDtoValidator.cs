using FluentValidation;
using GAC.WMS.Integrations.Application.DTOs.Products;

namespace GAC.WMS.Integrations.Application.Validators
{
    public class ProductDtoValidator : AbstractValidator<ProductDto>
    {
        public ProductDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required")
                .MaximumLength(50).WithMessage("Product ID cannot exceed 50 characters");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Category)
                .MaximumLength(50).WithMessage("Category cannot exceed 50 characters");

            RuleFor(x => x.UnitOfMeasure)
                .MaximumLength(20).WithMessage("Unit of measure cannot exceed 20 characters");

            RuleFor(x => x.Weight)
                .GreaterThanOrEqualTo(0).WithMessage("Weight must be greater than or equal to 0");

            RuleFor(x => x.Length)
                .GreaterThanOrEqualTo(0).WithMessage("Length must be greater than or equal to 0");

            RuleFor(x => x.Width)
                .GreaterThanOrEqualTo(0).WithMessage("Width must be greater than or equal to 0");

            RuleFor(x => x.Height)
                .GreaterThanOrEqualTo(0).WithMessage("Height must be greater than or equal to 0");

            RuleFor(x => x.Barcode)
                .MaximumLength(50).WithMessage("Barcode cannot exceed 50 characters");
        }
    }
}
