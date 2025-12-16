using MediatR;
using OneStop.Application.Exceptions;
using OneStop.Application.Interfaces;
using OneStop.Domain.Services;
using OneStop.Domain.ValueObjects;

namespace OneStop.Application.Features.Basket;

/// <summary>
/// Command to calculate basket totals.
/// </summary>
public sealed record CalculateBasketCommand : IRequest<CalculateBasketResult>
{
    public required Guid CustomerId { get; init; }
    public required string? LoyaltyCard { get; init; }
    public required DateTime TransactionDate { get; init; }
    public required IReadOnlyList<BasketItemDto> Basket { get; init; }

    public sealed record BasketItemDto
    {
        public required string ProductId { get; init; }
        public required int Quantity { get; init; }
    }
}

/// <summary>
/// Result of basket calculation.
/// </summary>
public sealed record CalculateBasketResult
{
    public required Guid CustomerId { get; init; }
    public required string? LoyaltyCard { get; init; }
    public required DateTime TransactionDate { get; init; }
    public required decimal TotalAmount { get; init; }
    public required decimal DiscountApplied { get; init; }
    public required decimal GrandTotal { get; init; }
    public required int PointsEarned { get; init; }
}

/// <summary>
/// Handler for basket calculation.
/// Orchestrates data fetching and delegates calculation to domain service.
/// </summary>
public class CalculateBasketHandler : IRequestHandler<CalculateBasketCommand, CalculateBasketResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IDiscountPromotionRepository _discountPromotionRepository;
    private readonly IPointsPromotionRepository _pointsPromotionRepository;
    private readonly IBasketCalculatorService _basketCalculatorService;

    public CalculateBasketHandler(
        IProductRepository productRepository,
        IDiscountPromotionRepository discountPromotionRepository,
        IPointsPromotionRepository pointsPromotionRepository,
        IBasketCalculatorService basketCalculatorService)
    {
        _productRepository = productRepository;
        _discountPromotionRepository = discountPromotionRepository;
        _pointsPromotionRepository = pointsPromotionRepository;
        _basketCalculatorService = basketCalculatorService;
    }

    public async Task<CalculateBasketResult> Handle(
        CalculateBasketCommand request, 
        CancellationToken cancellationToken)
    {
        // Fetch products for all basket items
        var productIds = request.Basket.Select(b => b.ProductId).Distinct();
        var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);

        // Validate all products exist
        var missingProductIds = request.Basket
            .Select(b => b.ProductId)
            .Where(id => !products.ContainsKey(id))
            .Distinct()
            .ToList();

        if (missingProductIds.Count > 0)
        {
            throw new NotFoundException("Product", string.Join(", ", missingProductIds));
        }

        // Build line items with resolved products
        var lineItems = request.Basket
            .Select(b => (
                LineItem: new BasketLineItem(b.ProductId, b.Quantity),
                Product: products[b.ProductId]))
            .ToList();

        // Fetch active promotions
        var activeDiscountPromotions = await _discountPromotionRepository
            .GetActiveOnDateAsync(request.TransactionDate, cancellationToken);
        
        var activePointsPromotion = await _pointsPromotionRepository
            .GetActiveOnDateAsync(request.TransactionDate, cancellationToken);

        // Determine if customer has loyalty card
        var hasLoyaltyCard = !string.IsNullOrWhiteSpace(request.LoyaltyCard);

        // Delegate calculation to domain service
        var result = _basketCalculatorService.Calculate(
            lineItems,
            activeDiscountPromotions,
            activePointsPromotion,
            hasLoyaltyCard);

        return new CalculateBasketResult
        {
            CustomerId = request.CustomerId,
            LoyaltyCard = request.LoyaltyCard,
            TransactionDate = request.TransactionDate,
            TotalAmount = result.TotalAmount,
            DiscountApplied = result.DiscountApplied,
            GrandTotal = result.GrandTotal,
            PointsEarned = result.PointsEarned
        };
    }
}
