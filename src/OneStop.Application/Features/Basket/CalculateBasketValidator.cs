using FluentValidation;

namespace OneStop.Application.Features.Basket;

/// <summary>
/// Validator for CalculateBasketCommand.
/// </summary>
public class CalculateBasketValidator : AbstractValidator<CalculateBasketCommand>
{
    public CalculateBasketValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .WithMessage("Transaction date is required.");

        RuleFor(x => x.Basket)
            .NotNull()
            .WithMessage("Basket is required.")
            .NotEmpty()
            .WithMessage("Basket cannot be empty.");

        RuleForEach(x => x.Basket).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        });
    }
}
