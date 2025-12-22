---
name: UI Workflow Verification and Gap Analysis
overview: Verify UI workflows match required business flows for dine-in, takeaway, and delivery orders, identify gaps, and document findings.
todos:
  - id: verify-dinein-serve
    content: Verify if waitstaff interface exists for marking orders as Served in dine-in workflow
    status: completed
  - id: verify-takeaway-pickup
    content: Check if takeaway pickup confirmation step exists in UI
    status: completed
  - id: verify-delivery-workflow
    content: Verify if delivery dispatch, deliver, and confirm steps exist in UI
    status: completed
  - id: check-status-enum
    content: Review if backend OrderStatus enum needs delivery-specific statuses
    status: completed
  - id: verify-payment-timing
    content: Check if takeaway orders enforce payment before kitchen preparation
    status: completed
  - id: document-findings
    content: Document all gaps and create recommendations for UI workflow fixes
    status: completed
---

# UI Workflow Verification and Gap Analysis

## Current State Analysis

### Backend Support

- ✅ OrderStatus enum includes: `Pending`, `Preparing`, `Ready`, `Served`, `Completed`, `Cancelled`, `OnHold`
- ✅ Order entity has `ServedTime` field for tracking
- ✅ OrderService supports status updates via `UpdateOrderStatusAsync`
- ✅ PaymentService sets order to `Completed` after payment

### UI Components Found

1. **POS Layout** ([UI/src/app/features/pos/components/pos-layout](UI/src/app/features/pos/components/pos-layout)) - Handles order creation and payment
2. **KDS Layout** ([UI/src/app/features/kds/components/kds-layout](UI/src/app/features/kds/components/kds-layout)) - Handles kitchen status: pending → preparing → ready
3. **Order Service** ([UI/src/app/core/services/order.service.ts](UI/src/app/core/services/order.service.ts)) - Has `updateOrderStatus` method

## Gap Analysis

### 1. Dine-In Workflow: `seat → order → kitchen → serve → pay → close`

**Required Flow:**

```
seat → order → kitchen (preparing → ready) → serve → pay → close
```

**Current Implementation:**

- ✅ Seat: Table selection in POS (line 47-56 in pos-layout.component.html)
- ✅ Order: Order creation supported
- ✅ Kitchen: KDS handles pending → preparing → ready (kds-layout.component.ts:176-196)
- ❌ **MISSING: Serve step** - No UI to mark orders as "Served" for dine-in orders
- ✅ Pay: Payment dialog exists (payment-dialog.component.ts)
- ⚠️ **ISSUE: Close** - Order goes directly to Completed after payment, but "serve" step is skipped

**Gap:** Waitstaff interface to mark ready orders as "Served" is missing. Orders jump from Ready → Completed after payment, skipping the Served status.

---

### 2. Takeaway Workflow: `order → pay → prepare → pickup`

**Required Flow:**

```
order → pay (must happen first) → prepare (kitchen) → pickup (confirmation)
```

**Current Implementation:**

- ✅ Order: Order creation supported
- ⚠️ **ISSUE: Payment timing** - Payment can be done at any time, not enforced before kitchen preparation
- ✅ Prepare: KDS handles preparing → ready
- ❌ **MISSING: Pickup confirmation** - No UI to mark takeaway orders as "Picked up" after customer collects

**Gaps:**

1. No enforcement that payment must occur before kitchen preparation for takeaway
2. No pickup confirmation step in UI

---

### 3. Delivery Workflow: `order → dispatch → deliver → confirm`

**Required Flow:**

```
order → dispatch (assign delivery partner) → deliver (out for delivery) → confirm (delivered)
```

**Current Implementation:**

- ✅ Order: Order creation supported
- ❌ **MISSING: Dispatch step** - No UI to assign delivery orders to delivery partners
- ❌ **MISSING: Deliver step** - No UI to mark orders as "Out for Delivery"
- ❌ **MISSING: Confirm step** - No UI to confirm delivery completion

**Gap:** Entire delivery workflow is missing from UI. Backend has OrderStatus enum but no delivery-specific statuses are tracked in UI.

---

## Backend Status Mapping

The backend `OrderStatus` enum (`API/RestaurantOrderManagement.Domain/Enums/OrderStatus.cs`) has:

- `Pending = 1`
- `Preparing = 2`
- `Ready = 3`
- `Served = 4` ← Exists but not used in UI
- `Completed = 5`
- `Cancelled = 6`
- `OnHold = 7`

**Issue:** Delivery-specific statuses (dispatch, out_for_delivery, delivered) are not in the enum. Need to check if these should be:

1. Added as new enum values, OR
2. Handled through a separate delivery tracking entity/status

---

## Recommendations

### Immediate Actions Needed

1. **Add Waitstaff/Serve Interface**

   - Create component to display ready dine-in orders
   - Allow marking orders as "Served" (status = Served)
   - Integrate with existing order service

2. **Add Takeaway Pickup Confirmation**

   - Add UI to mark takeaway orders as collected/picked up
   - Could use existing "Completed" status or add pickup timestamp

3. **Implement Delivery Workflow**

   - Determine if delivery statuses should extend OrderStatus enum
   - Create dispatch interface for assigning delivery orders
   - Create delivery partner interface for tracking delivery status
   - Add delivery confirmation step

4. **Enforce Takeaway Payment Order**

   - Ensure payment happens before kitchen receives takeaway orders
   - Add validation in order creation flow

### Files to Review/Create

1. **Waitstaff Interface** (NEW)

   - Component: `UI/src/app/features/waitstaff/` or integrate into existing dashboard
   - Service methods: Use existing `orderService.updateOrderStatus(orderId, 'Served')`

2. **Takeaway Pickup** (NEW)

   - Component: `UI/src/app/features/pos/components/pickup-confirmation/`
   - Update order status or add pickup timestamp

3. **Delivery Management** (NEW)

   - Components: `UI/src/app/features/delivery/`
   - Dispatch, tracking, and confirmation interfaces

4. **Backend Enum Updates** (If needed)

   - `API/RestaurantOrderManagement.Domain/Enums/OrderStatus.cs` - Add delivery statuses if required

---

## Status Transition Validation

Verify status transitions match workflows:

**Dine-in:** `Pending → Preparing → Ready → Served → Completed`

**Takeaway:** `Pending → Preparing → Ready → Completed` (with pickup confirmation)

**Delivery:** `Pending → Preparing → Ready → [Dispatch] → [OutForDelivery] → [Delivered] → Completed`

Current backend only supports the standard flow up to Ready, with Served available but unused.