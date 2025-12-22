---
name: Fix thermal printer formatting
overview: Fix CSS print media queries and print service timing to ensure proper 80mm thermal printer formatting. The current implementation has CSS specificity issues that break the layout, and timing issues that can interrupt the print dialog.
todos: []
---

# Fix Thermal Printer Formatting for 80mm Receipts

## Problems Identified

1. **CSS Specificity Conflict**: The `body * { display: none !important; }` rule is too aggressive and conflicts with showing `.thermal-receipt`, causing layout issues
2. **Insufficient Timing**: 200ms delay may not be enough for Angular change detection and rendering
3. **Navigation Interference**: Immediate navigation to `/orders` after calling print can interrupt the print dialog
4. **Missing CSS Properties**: Need better width constraints, word wrapping, and print-specific styling for thermal printers

## Solution

### 1. Fix CSS Print Media Queries
**File**: `frontend/src/app/shared/components/thermal-receipt/thermal-receipt.component.scss`

- Replace aggressive `body *` hiding with targeted selectors that exclude `.thermal-receipt`
- Add explicit width constraints (58mm content width is standard for 80mm paper)
- Improve text wrapping and overflow handling for item names
- Add print-specific font sizing and spacing
- Use `max-width` instead of fixed width for better compatibility
- Add proper page break handling

### 2. Improve Print Service Timing
**File**: `frontend/src/app/core/services/print.service.ts`

- Increase delay to 300-500ms to ensure Angular renders the component
- Use `ChangeDetectorRef` if possible, or ensure signal updates are detected
- Add proper error handling if print dialog doesn't open

### 3. Fix Navigation Timing
**File**: `frontend/src/app/features/orders/order-create/order-create.component.ts`

- Delay navigation until after print dialog is dismissed (use `window.onafterprint`)
- Or keep the user on the current page until printing completes
- Ensure order data persists during print operation

### 4. Enhance Thermal Receipt Template
**File**: `frontend/src/app/shared/components/thermal-receipt/thermal-receipt.component.html`

- Ensure proper text truncation for long item names
- Add RM currency symbol consistently
- Verify all order data fields are properly displayed
- Add fallback for missing optional fields

### 5. Test Print Preview
- Verify receipt fits within 58-72mm width (printable area for 80mm paper)
- Ensure text doesn't overflow or wrap incorrectly
- Check that dividers and spacing are appropriate
- Verify all data displays correctly

## Technical Details

**80mm Thermal Printer Specifications**:
- Paper width: 80mm
- Printable width: ~58-72mm (depends on printer)
- Standard receipt width: 58mm is safest for compatibility
- Font: Monospace (Courier New) for consistent character width
- Font size: 10-12pt for readability

**CSS Changes**:
- Use `@page { size: 80mm auto; margin: 0; }` for page size
- Set `.thermal-receipt` width to `58mm` (safer than 72mm)
- Use `max-width: 100%` as fallback
- Add `word-wrap: break-word` for long item names
- Ensure all text uses monospace font

**Print Service Changes**:
- Increase setTimeout delay to 500ms
- Ensure signal is set before delay starts
- Handle edge cases (browser blocking print dialog)

## Files to Modify

1. `frontend/src/app/shared/components/thermal-receipt/thermal-receipt.component.scss` - Fix CSS print media queries
2. `frontend/src/app/core/services/print.service.ts` - Improve timing and error handling
3. `frontend/src/app/features/orders/order-create/order-create.component.ts` - Fix navigation timing (