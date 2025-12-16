# Testing Strategy

This document describes the testing approach for the OneStop Basket Calculator API.

## Test Classification

### Unit Tests (`OneStop.Tests/Unit/`)

**Target:** Domain layer (pure business logic)  
**Characteristics:**
- No mocks required
- Fast execution
- Test business rules in isolation
- High coverage of edge cases

**Why no mocks?**  
The `BasketCalculatorService` is a pure domain service with no dependencies. It receives domain objects as parameters and returns results. This design makes it trivially testable.

## Test Case Table

### Unit Tests: `BasketCalculatorServiceTests`

| Scenario | Test Method | Description |
|----------|-------------|-------------|
| Single item, no promotions | `Calculate_SingleItem_NoPromotions_ReturnsCorrectTotal` | Basic calculation without any promotions |
| Multiple items, no promotions | `Calculate_MultipleItems_NoPromotions_ReturnsSumOfLineTotals` | Sum of multiple line totals |
| Discount applied | `Calculate_WithDiscount_AppliesDiscountToEligibleProduct` | 20% discount on eligible product |
| Empty junction table | `Calculate_DiscountPromotion_NoProductsInJunctionTable_NoDiscountApplied` | DP002 with no products |
| Multiple discounts, different products | `Calculate_MultipleDiscounts_DifferentProducts_BothApplied` | Two promotions, two products |
| Points, no loyalty card | `Calculate_WithPointsPromotion_NoLoyaltyCard_NoPointsEarned` | Guest checkout |
| Points, with loyalty card | `Calculate_WithPointsPromotion_WithLoyaltyCard_PointsEarned` | Points earned |
| Points on post-discount | `Calculate_PointsOnPostDiscountAmount` | Points calculated after discount |
| Category filter - no match | `Calculate_PointsPromotion_CategoryFilter_OnlyMatchingCategoryEarnsPoints` | Fuel promo, shop product |
| Category filter - Any | `Calculate_PointsPromotion_AnyCategory_AllProductsEarnPoints` | All products qualify |
| Category filter - mixed | `Calculate_PointsPromotion_MixedCategories_OnlyMatchingEarnsPoints` | Mixed basket, category promo |
| Discount + Points | `Calculate_DiscountAndPoints_BothApplied` | Sample scenario validation |
| Empty basket | `Calculate_EmptyBasket_ThrowsArgumentException` | Validation error |
| Null line items | `Calculate_NullLineItems_ThrowsArgumentNullException` | Null check |
| Overlapping discounts | `Calculate_OverlappingDiscounts_SameProduct_ThrowsDomainException` | Data integrity violation |
| No active promotions | `Calculate_NoActivePromotions_ReturnsZeroDiscountAndPoints` | Date with no promos |
| Points floored | `Calculate_PointsFloored_NoPartialPoints` | No decimal points |
| Rounding | `Calculate_Rounding_TwoDecimalPlaces` | Money rounding |

## Coverage Goals

| Layer | Target | Notes |
|-------|--------|-------|
| Domain | 95%+ | Critical business logic |