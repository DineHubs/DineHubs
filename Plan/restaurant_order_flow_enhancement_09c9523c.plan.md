---
name: Restaurant Order Flow Enhancement
overview: Implement missing order flow logic for dine-in and takeaway processes, including exception handling (cancellation, void/refund, item unavailable), receipt/reprint functionality, payment processing, and KPI tracking infrastructure.
todos:
  - id: process-docs
    content: Create process maps documentation (As-Is and To-Be) with swimlanes and exception flows
    status: completed
  - id: domain-enhance
    content: Enhance Order domain model with cancellation validation, line modification methods, and payment timing
    status: completed
  - id: order-cancel-api
    content: Implement order cancellation API with validation (only before kitchen prep)
    status: completed
    dependencies:
      - domain-enhance
  - id: order-line-modify-api
    content: Implement order line modification APIs (remove items, update quantities)
    status: completed
    dependencies:
      - domain-enhance
  - id: payment-service
    content: Create payment service with process payment, refund, and void methods
    status: completed
  - id: payment-endpoints
    content: Create payment API endpoints for processing payments and refunds
    status: completed
    dependencies:
      - payment-service
  - id: receipt-service
    content: Create receipt service for generating and reprinting receipts with audit trail
    status: completed
  - id: receipt-endpoints
    content: Create receipt API endpoints for viewing and reprinting receipts
    status: completed
    dependencies:
      - receipt-service
  - id: kpi-service
    content: Create KPI service for tracking prep time, accuracy, table turn time, refund frequency, and reprint count
    status: completed
  - id: kpi-endpoints
    content: Create KPI API endpoints for retrieving metrics
    status: completed
    dependencies:
      - kpi-service
  - id: order-cancel-ui
    content: Add order cancellation UI with confirmation dialog and reason input
    status: completed
    dependencies:
      - order-cancel-api
  - id: payment-ui
    content: Create payment processing UI component
    status: completed
    dependencies:
      - payment-endpoints
  - id: receipt-ui
    content: Add receipt display and reprint functionality to order details
    status: completed
    dependencies:
      - receipt-endpoints
  - id: order-line-modify-ui
    content: Add order line modification UI (remove items, update quantities)
    status: completed
    dependencies:
      - order-line-modify-api
---

# Restaurant Order Flow Enhancement Plan

## Current State Analysis

### Existing Functionality

- Basic order creation (dine-in and takeaway) in `OrderManagement.Infrastructure/Ordering/OrderService.cs`
- Order status management (Draft → Submitted → InPreparation → Ready → Delivered/Cancelled/Paid)
- Kitchen queue display in `frontend/src/app/features/kitchen/kitchen-display/`
- Menu item availability toggle
- Payment transaction entity exists but no full payment workflow
- Order listing and details views

### Missing Functionality

1. **Order Cancellation**: No validation to prevent cancellation after kitchen prep starts
2. **Void/Refund Workflow**: PaymentTransaction has Refund() method but no API/UI integration
3. **Receipt/Reprint**: No receipt generation or reprint functionality
4. **Payment Processing**: Payment gateway exists but no order payment endpoints
5. **Order Line Modifications**: Cannot remove items or update quantities after order creation
6. **Exception Handling**: No standardized handling for unavailable items, payment failures
7. **KPI Tracking**: No metrics collection for prep time, accuracy, table turn time, etc.
8. **Table Management**: No table status tracking for dine-in orders

## Implementation Plan

### Phase 1: Process Documentation & Domain Model Enhancement

#### 1.1 Create Process Maps Documentation

- **File**: `Plan/ORDER_FLOW_PROCESS_MAPS.md`
- Document As-Is and To-Be process flows with swimlanes
- Include exception handling paths
- Document bottlenecks and risks

#### 1.2 Enhance Order Domain Model

- **File**: `OrderManagement.Domain/Entities/Order.cs`
- Add cancellation validation logic (can only cancel if status < InPreparation)
- Add method to remove order lines
- Add method to update line quantities
- Add payment timing flag (PayBeforeKitchen vs PayAfterReady)

#### 1.3 Create Order Exception Entities

- **File**: `OrderManagement.Domain/Entities/OrderException.cs` (new)
- Track: ItemUnavailable, PaymentFailure, KitchenDelay, CustomerRequest
- Link to orders and order lines

### Phase 2: Order Management API Enhancements

#### 2.1 Add Order Cancellation Endpoint

- **File**: `OrderManagement.Application/Ordering/IOrderService.cs`
- Add `CancelOrderAsync(Guid orderId, string reason, ...)`
- **File**: `OrderManagement.Infrastructure/Ordering/OrderService.cs`
- Implement cancellation with validation (only if status < InPreparation)
- Update order status to Cancelled
- **File**: `OrderManagement.Api/Controllers/OrdersController.cs`
- Add `[HttpPost("{id:guid}/cancel")]` endpoint
- Accept cancellation reason
- Return appropriate error if cancellation not allowed

#### 2.2 Add Order Line Modification Endpoints

- **File**: `OrderManagement.Application/Ordering/IOrderService.cs`
- Add `RemoveOrderLineAsync(Guid orderId, Guid lineId, ...)`
- Add `UpdateOrderLineQuantityAsync(Guid orderId, Guid lineId, int quantity, ...)`
- **File**: `OrderManagement.Infrastructure/Ordering/OrderService.cs`
- Implement line removal (only if order not in preparation)
- Implement quantity updates (recalculate totals)
- **File**: `OrderManagement.Api/Controllers/OrdersController.cs`
- Add endpoints for line modifications

#### 2.3 Enhance Order Creation with Validation

- **File**: `OrderManagement.Infrastructure/Ordering/OrderService.cs`
- Validate menu item availability before adding to order
- Throw exception if item unavailable with suggestion to check menu
- Log unavailable item attempts

### Phase 3: Payment Processing Integration

#### 3.1 Create Payment Service

- **File**: `OrderManagement.Application/Payments/IPaymentService.cs` (new)
- `ProcessPaymentAsync(Guid orderId, decimal amount, string provider, ...)`
- `RefundPaymentAsync(Guid paymentId, decimal amount, string reason, ...)`
- `GetPaymentByOrderIdAsync(Guid orderId, ...)`
- **File**: `OrderManagement.Infrastructure/Payments/PaymentService.cs` (new)
- Integrate with existing IPaymentGateway
- Handle payment authorization and capture
- Update order status to Paid after successful payment
- Link PaymentTransaction to Order

#### 3.2 Add Payment Endpoints

- **File**: `OrderManagement.Api/Controllers/PaymentsController.cs` (new)
- `[HttpPost("orders/{orderId:guid}/pay")]` - Process payment
- `[HttpPost("{paymentId:guid}/refund")]` - Process refund
- `[HttpGet("orders/{orderId:guid}")]` - Get payment status

#### 3.3 Update Order Status Flow

- **File**: `OrderManagement.Domain/Entities/Order.cs`
- Add payment timing configuration
- Update status transitions to include payment states

### Phase 4: Receipt Generation & Reprint

#### 4.1 Create Receipt Service

- **File**: `OrderManagement.Application/Receipts/IReceiptService.cs` (new)
- `GenerateReceiptAsync(Guid orderId, ...)` - Generate receipt PDF/HTML
- `ReprintReceiptAsync(Guid orderId, string reason, ...)` - Log reprint with audit trail
- **File**: `OrderManagement.Infrastructure/Receipts/ReceiptService.cs` (new)
- Generate receipt using order and payment data
- Store receipt URL in PaymentTransaction
- Log reprint events for audit

#### 4.2 Add Receipt Endpoints

- **File**: `OrderManagement.Api/Controllers/ReceiptsController.cs` (new)
- `[HttpGet("orders/{orderId:guid}/receipt")]` - Get receipt
- `[HttpPost("orders/{orderId:guid}/receipt/reprint")]` - Reprint receipt

#### 4.3 Create Receipt Entity for Audit

- **File**: `OrderManagement.Domain/Entities/ReceiptPrint.cs` (new)
- Track: OrderId, PrintedAt, PrintedBy, Reason, IsReprint

### Phase 5: Exception Handling & Workflows

#### 5.1 Item Unavailable Handling

- **File**: `OrderManagement.Infrastructure/Ordering/OrderService.cs`
- Check item availability during order creation
- Return clear error messages
- Log unavailable item attempts

#### 5.2 Void/Refund Workflow

- **File**: `OrderManagement.Application/Payments/IPaymentService.cs`
- Add `VoidPaymentAsync(Guid paymentId, string reason, ...)`
- **File**: `OrderManagement.Infrastructure/Payments/PaymentService.cs`
- Implement void (before capture) and refund (after capture) logic
- Update order status appropriately
- Create audit trail

#### 5.3 Kitchen Delay Notification

- **File**: `OrderManagement.Application/Notifications/IOrderNotificationService.cs` (new)
- `NotifyKitchenDelayAsync(Guid orderId, TimeSpan delay, ...)`
- Log delays for KPI tracking

### Phase 6: KPI Tracking Infrastructure

#### 6.1 Create KPI Metrics Entities

- **File**: `OrderManagement.Domain/Entities/OrderMetrics.cs` (new)
- Track: PrepTime, OrderAccuracy, TableTurnTime, RefundCount, ReprintCount
- Link to orders and timestamps

#### 6.2 Create KPI Service

- **File**: `OrderManagement.Application/KPIs/IKpiService.cs` (new)
- `CalculatePrepTimeAsync(Guid orderId, ...)`
- `CalculateTableTurnTimeAsync(Guid orderId, ...)`
- `GetOrderAccuracyAsync(Guid orderId, ...)`
- `GetRefundFrequencyAsync(DateTime from, DateTime to, ...)`
- `GetReprintCountAsync(DateTime from, DateTime to, ...)`
- **File**: `OrderManagement.Infrastructure/KPIs/KpiService.cs` (new)
- Calculate metrics from order timestamps and status changes
- Store metrics in database for reporting

#### 6.3 Add KPI Tracking to Order Service

- **File**: `OrderManagement.Infrastructure/Ordering/OrderService.cs`
- Record timestamps at each status change
- Calculate and store prep time when order moves to Ready
- Calculate table turn time when order moves to Paid/Closed

#### 6.4 Create KPI Endpoints

- **File**: `OrderManagement.Api/Controllers/KPIsController.cs` (new)
- `[HttpGet("prep-time")]` - Average prep time
- `[HttpGet("order-accuracy")]` - Order accuracy rate
- `[HttpGet("table-turn-time")]` - Average table turn time
- `[HttpGet("refund-frequency")]` - Refund/void frequency
- `[HttpGet("reprint-count")]` - Reprint count

### Phase 7: Frontend Implementation

#### 7.1 Order Cancellation UI

- **File**: `frontend/src/app/features/orders/order-details/order-details.component.ts`
- Add cancel button (only show if order can be cancelled)
- Cancel confirmation dialog with reason input
- Call cancellation API

#### 7.2 Payment Processing UI

- **File**: `frontend/src/app/features/orders/order-payment/order-payment.component.ts` (new)
- Payment form component
- Integration with payment API
- Payment status display

#### 7.3 Receipt Display & Reprint

- **File**: `frontend/src/app/features/orders/order-details/order-details.component.ts`
- Add "View Receipt" button
- Add "Reprint Receipt" button with reason input
- Display receipt in modal or new window

#### 7.4 Exception Handling UI

- **File**: `frontend/src/app/features/orders/order-create/order-create.component.ts`
- Show error messages for unavailable items
- Prevent adding unavailable items to cart (already implemented, verify)

#### 7.5 Order Line Modifications UI

- **File**: `frontend/src/app/features/orders/order-details/order-details.component.ts`
- Add "Remove Item" button for each line (if order not in preparation)
- Add quantity update controls
- Show warning if modifications not allowed

### Phase 8: Testing & Validation

#### 8.1 Unit Tests

- Test order cancellation validation
- Test payment processing
- Test receipt generation
- Test KPI calculations

#### 8.2 Integration Tests

- Test complete order flows (dine-in and takeaway)
- Test exception scenarios
- Test payment workflows

## Files to Create/Modify

### New Files

1. `Plan/ORDER_FLOW_PROCESS_MAPS.md` - Process documentation
2. `OrderManagement.Domain/Entities/OrderException.cs` - Exception tracking
3. `OrderManagement.Domain/Entities/ReceiptPrint.cs` - Receipt audit
4. `OrderManagement.Domain/Entities/OrderMetrics.cs` - KPI metrics
5. `OrderManagement.Application/Payments/IPaymentService.cs` - Payment service interface
6. `OrderManagement.Infrastructure/Payments/PaymentService.cs` - Payment service implementation
7. `OrderManagement.Application/Receipts/IReceiptService.cs` - Receipt service interface
8. `OrderManagement.Infrastructure/Receipts/ReceiptService.cs` - Receipt service implementation
9. `OrderManagement.Application/KPIs/IKpiService.cs` - KPI service interface
10. `OrderManagement.Infrastructure/KPIs/KpiService.cs` - KPI service implementation
11. `OrderManagement.Api/Controllers/PaymentsController.cs` - Payment endpoints
12. `OrderManagement.Api/Controllers/ReceiptsController.cs` - Receipt endpoints
13. `OrderManagement.Api/Controllers/KPIsController.cs` - KPI endpoints
14. `frontend/src/app/features/orders/order-payment/order-payment.component.ts` - Payment UI

### Modified Files

1. `OrderManagement.Domain/Entities/Order.cs` - Add cancellation, line modification methods
2. `OrderManagement.Application/Ordering/IOrderService.cs` - Add cancellation, line modification methods
3. `OrderManagement.Infrastructure/Ordering/OrderService.cs` - Implement new methods, add validation
4. `OrderManagement.Api/Controllers/OrdersController.cs` - Add cancellation, line modification endpoints
5. `frontend/src/app/features/orders/order-details/order-details.component.ts` - Add cancellation, receipt, line modification UI
6. `frontend/src/app/core/models/order.model.ts` - Add new interfaces for payment, receipt, metrics

## Risk Mitigation

1. **Kitchen Delays**: Track prep time metrics, implement notifications
2. **Payment Failures**: Implement retry logic, clear error messages
3. **Exception Handling**: Standardize error responses, log all exceptions
4. **Reprint Fraud**: Audit trail for all reprints, require manager approval for multiple reprints
5. **Order Cancellation Abuse**: Validate cancellation rules, log all cancellations

## Success Criteria

1. All exception scenarios handled with clear user feedback
2. Payment processing integrated with order flow
3. Receipt generation and reprint with audit trail
4. KPI metrics collected and accessible via API
5. Order cancellation respects business rules
6. Order line modifications work correctly with validation