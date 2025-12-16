using System.Globalization;
using MediatR;
using OneStop.Application.Features.Basket;
using OneStop.Presentation.Api.Contracts;

namespace OneStop.Presentation.Api.Endpoints;

/// <summary>
/// Endpoints for basket operations.
/// </summary>
public static class BasketEndpoints
{
    private const string DateFormat = "dd-MMM-yyyy";

    /// <summary>
    /// Maps basket-related endpoints.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The route group builder for chaining.</returns>
    public static RouteGroupBuilder MapBasketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/basket")
            .WithTags("Basket");

        group.MapPost("/calculate", CalculateBasket)
            .WithName("CalculateBasket")
            .WithSummary("Calculate basket totals")
            .WithDescription("Calculates basket totals including discounts and loyalty points based on active promotions.")
            .Produces<CalculateBasketResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        return group;
    }

    /// <summary>
    /// Calculates basket totals including discounts and loyalty points.
    /// </summary>
    private static async Task<IResult> CalculateBasket(
        CalculateBasketRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Parse transaction date
        if (!DateTime.TryParseExact(
                request.TransactionDate, 
                DateFormat, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var transactionDate))
        {
            return Results.BadRequest(new ErrorResponse
            {
                Type = "ValidationError",
                Message = "One or more validation errors occurred.",
                Errors = new Dictionary<string, string[]>
                {
                    ["TransactionDate"] = [$"Invalid date format. Expected format: {DateFormat} (e.g., 10-Jan-2020)"]
                }
            });
        }

        // Map to command
        var command = new CalculateBasketCommand
        {
            CustomerId = request.CustomerId,
            LoyaltyCard = request.LoyaltyCard,
            TransactionDate = transactionDate,
            Basket = request.Basket
                .Select(b => new CalculateBasketCommand.BasketItemDto
                {
                    ProductId = b.ProductId,
                    Quantity = b.Quantity
                })
                .ToList()
        };

        // Execute
        var result = await mediator.Send(command, cancellationToken);

        // Map to response
        var response = new CalculateBasketResponse
        {
            CustomerId = result.CustomerId,
            LoyaltyCard = result.LoyaltyCard,
            TransactionDate = result.TransactionDate.ToString(DateFormat, CultureInfo.InvariantCulture),
            TotalAmount = result.TotalAmount,
            DiscountApplied = result.DiscountApplied,
            GrandTotal = result.GrandTotal,
            PointsEarned = result.PointsEarned
        };

        return Results.Ok(response);
    }
}
