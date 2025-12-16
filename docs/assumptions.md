# Assumptions

This document lists all assumptions made during the implementation of the OneStop Basket Calculator API.

## Data & Input Assumptions

### 1. Unit Price in Request
**Issue:** Original sample had UnitPrice in request, but Product table has prices.  
**Assumption:** Remove UnitPrice from request. Use Product table prices (single source of truth). This prevents price manipulation by clients.

### 2. Sample Data Inconsistencies
**Issue:** Sample request/response values don't match calculations.  
**Assumption:** Sample is illustrative only. Use Product table prices for actual calculations.

### 3. Duplicate Entry in Junction Table
**Issue:** DP001 → PRD02 appears twice in the discount promotion products junction table.  
**Assumption:** Data entry error. Treat as single entry.

### 4. Missing Product Mappings (DP002)
**Issue:** DP002 "Happy Promo" has no products in junction table.  
**Assumption:** If no products in junction table, discount applies to NO products. Junction table is definitive.

### 5. Transaction Date Format
**Decision:** Accept `dd-MMM-yyyy` format as shown in sample (e.g., `"10-Jan-2020"`).

## Promotion Rules

### 6. Overlapping Points Promotions
**Issue:** What if multiple points promotions active on same date?  
**Assumption:** System validates before creation. Only one active at any time (per requirement).

### 7. Overlapping Discount Promotions
**Issue:** What if same product in multiple active discount promotions?  
**Assumption:** System validates before creation. Same product cannot be in multiple active discount promotions simultaneously. Different products can overlap.  
**Runtime Behavior:** If overlapping discounts are detected at runtime (data integrity issue), a `DomainException` is thrown. Alternative approach would be to apply highest discount, but we follow the validation assumption as a requirement.

### 8. Points & Discount Together
**Issue:** Can both promotions run simultaneously?  
**Assumption:** Yes. Customer gets both benefits when applicable.

### 9. Points Promotion to Product Relationship
**Issue:** No junction table for points promotions.  
**Assumption:** Category field determines eligibility:
- `"Any"` (represented as `null` in code) = all products
- `"Fuel"` = fuel products only
- `"Shop"` = shop products only

### 10. Points Calculation Basis
**Issue:** Pre-discount or post-discount amount?  
**Assumption:** Post-discount (customer-friendly, common retail practice).

## Business Logic

### 11. Rounding Rules
**Assumption:** 
- Money = 2 decimal places (rounded using standard rounding)
- Points = floor (no partial points)

### 12. Product Not Found
**Assumption:** Return 400 Bad Request with product ID(s) in error message.

### 13. Empty Basket
**Assumption:** Return 400 Bad Request with validation error.

### 14. Loyalty Card Validation
**Assumption:** Presence check only (non-empty = has card). Actual card validation happens upstream. If empty/null/whitespace = guest = no points.

### 15. Currency
**Assumption:** Single currency. Payment conversion handled externally by bank/payment gateway.

### 16. Negative Quantity/Price
**Assumption:** Reject with validation error.

## Technical & Scale

### 17. Scale & Performance
**Assumption:** Small stores, limited products (<1000), one operator per store processing sequentially. No need for micro-optimization. Prioritize readability (use LINQ over manual loops).

### 18. Testing
**Issue:** Exercise states "Automated testing is not required."  
**Assumption:** Refers to CI/CD pipelines. Unit/integration tests included to demonstrate approach and ensure correctness.

### 19. Date Range Inclusivity
**Assumption:** Promotion start and end dates are inclusive. A transaction on the start date or end date qualifies for the promotion.
