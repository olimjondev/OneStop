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

### Integration Tests (`OneStop.Tests/Integration/`)

**Target:** Application layer (handlers with real repositories)  
**Characteristics:**
- Use real in-memory repositories
- Test end-to-end handler logic
- Verify data fetching and mapping
- Test error scenarios (not found, validation)

**Why real repositories?**  
The in-memory repositories are simple enough that mocking adds complexity without value. Using real repos tests the actual data access patterns.

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

### Integration Tests: `CalculateBasketHandlerTests`

| Scenario | Test Method | Description |
|----------|-------------|-------------|
| Sample scenario | `Handle_SampleScenario_ReturnsCorrectCalculation` | Requirements sample validation |
| Date before promo | `Handle_DateBeforePromotion_NoDiscountApplied` | Dec 31, 2019 |
| Date after promo | `Handle_DateAfterPromotion_NoDiscountApplied` | Feb 16, 2020 |
| Fuel promo date | `Handle_FuelPromoDate_OnlyFuelProductsEarnPoints` | PP002 validation |
| Shop promo date | `Handle_ShopPromoDate_OnlyShopProductsEarnPoints` | PP003 validation |
| No loyalty card | `Handle_NoLoyaltyCard_NoPointsEarned` | Null card |
| Empty loyalty card | `Handle_EmptyLoyaltyCard_NoPointsEarned` | Empty string |
| Whitespace loyalty card | `Handle_WhitespaceLoyaltyCard_NoPointsEarned` | Spaces only |
| Unknown product | `Handle_UnknownProductId_ThrowsNotFoundException` | Single unknown |
| Multiple unknown products | `Handle_MultipleUnknownProducts_ThrowsWithAllIds` | All IDs in error |
| Mixed known/unknown | `Handle_MixedKnownAndUnknown_ThrowsForUnknown` | Partial match |
| Response mapping | `Handle_ReturnsCorrectCustomerInfo` | Customer ID, card, date preserved |
| No active promotions | `Handle_DateWithNoActivePromotions_ReturnsZeroDiscountAndPoints` | April 2020 |
| Product not in discount | `Handle_ProductNotInDiscountPromotion_NoDiscount` | PRD01 not in DP001 |
| Case insensitive | `Handle_CaseInsensitiveProductId` | Lowercase product ID |

## Running Tests

```bash
# Run all tests
dotnet test
```

## Coverage Goals

| Layer | Target | Notes |
|-------|--------|-------|
| Domain | 95%+ | Critical business logic |
| Application | 80%+ | Handler orchestration |