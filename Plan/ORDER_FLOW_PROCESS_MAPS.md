# Order Flow Process Maps

## As-Is Process Maps

### Dine-In Flow (Current)

```
Seat → Order → Kitchen → Serve → Pay → Close
```

**Current Implementation:**
- Order creation with table number
- Order status: Draft → Submitted → InPreparation → Ready → Delivered → Paid
- Kitchen display shows orders in queue
- No payment integration yet
- No table management

### Takeaway Flow (Current)

```
Order → Pay → Prepare → Pickup
```

**Current Implementation:**
- Order creation with IsTakeAway flag
- Same status flow as dine-in
- No payment integration yet
- No pickup tracking

## To-Be Process Maps (Enhanced)

### Dine-In Flow (Enhanced)

```
Seat → Order → [Payment?] → Kitchen → Serve → [Payment?] → Close
```

**Enhanced Flow with Exceptions:**

1. **Normal Flow:**
   - Customer seated at table
   - Waiter takes order (creates order with table number)
   - Order submitted to kitchen
   - Kitchen prepares items
   - Order marked ready
   - Waiter serves to table
   - Customer pays (before or after service, configurable)
   - Order closed/paid

2. **Exception: Item Unavailable**
   - During order creation, if item unavailable:
     - System prevents adding to cart
     - Waiter informed: "Item unavailable, check menu"
     - Suggest alternative items if configured
     - Customer can choose alternative or remove item

3. **Exception: Order Cancellation**
   - Before kitchen prep (status < InPreparation):
     - Allow cancellation
     - Log cancellation reason
     - Update order status to Cancelled
   - After kitchen prep started:
     - Prevent cancellation
     - Show error: "Order already in preparation, cannot cancel"

4. **Exception: Void/Refund**
   - If payment not yet captured:
     - Allow void payment
     - Update payment status to Voided
   - If payment captured:
     - Allow refund
     - Process refund through payment gateway
     - Update payment status to Refunded
     - Log refund reason

5. **Exception: Receipt Reprint**
   - After payment completed:
     - Allow receipt reprint
     - Log reprint with reason and user
     - Audit trail for fraud prevention
     - Manager approval required for multiple reprints

### Takeaway Flow (Enhanced)

```
Order → [Payment?] → Prepare → Pickup → [Payment?] → Close
```

**Enhanced Flow with Exceptions:**

1. **Normal Flow:**
   - Customer places order (takeaway)
   - Payment processed (before or after preparation, configurable)
   - Kitchen prepares items
   - Order marked ready
   - Customer picks up
   - Order closed/paid

2. **Exception: Item Unavailable**
   - Same as dine-in flow
   - Prevent adding unavailable items
   - Suggest alternatives

3. **Exception: Order Cancellation**
   - Same rules as dine-in
   - Can cancel before preparation starts

4. **Exception: Void/Refund**
   - Same as dine-in flow
   - Process refund if payment captured

5. **Exception: Receipt Reprint**
   - Same as dine-in flow
   - Audit trail required

## Swimlanes (Roles)

### Customer
- **Dine-In:** Seat, Order (via waiter or QR), Pay, Leave
- **Takeaway:** Order, Pay, Pickup

### Waiter/Cashier
- **Order Management:**
  - Take order (create order)
  - Process payment
  - Handle exceptions (void/refund, reprint)
  - Cancel orders (before kitchen prep)
  
- **Exception Handling:**
  - Inform customer of unavailable items
  - Suggest alternatives
  - Process refunds/voids
  - Reprint receipts with reason

### Kitchen Staff
- **Order Preparation:**
  - View kitchen queue
  - Start preparation (update status to InPreparation)
  - Mark items ready (update status to Ready)
  - Notify if item unavailable during prep

- **Exception Handling:**
  - Mark items as unavailable if discovered during prep
  - Notify waiter/cashier of delays

### System
- **Transaction Logging:**
  - Log all order status changes
  - Log payment transactions
  - Log refunds/voids
  - Log receipt reprints
  
- **Workflow Triggers:**
  - Trigger payment processing
  - Trigger refund workflows
  - Trigger notifications (delays, ready orders)
  
- **Receipt Generation:**
  - Generate receipts on payment
  - Store receipt URLs
  - Track reprint history

## Bottlenecks & Risks

### Identified Bottlenecks

1. **Kitchen Delays**
   - **Impact:** Increased prep time, customer dissatisfaction
   - **Mitigation:** 
     - Track prep time metrics
     - Implement delay notifications
     - Alert management if delays exceed threshold

2. **Payment Failures**
   - **Impact:** Slow checkout, risk of abandoned orders
   - **Mitigation:**
     - Implement retry logic
     - Clear error messages
     - Support multiple payment methods
     - Allow payment retry without recreating order

3. **Exception Handling Not Standardized**
   - **Impact:** Inconsistent customer experience
   - **Mitigation:**
     - Standardize error responses
     - Create exception handling workflows
     - Train staff on exception procedures

4. **Manual Reprints**
   - **Impact:** Risk of fraud or duplicate charges
   - **Mitigation:**
     - Audit trail for all reprints
     - Require reason for reprint
     - Manager approval for multiple reprints
     - Limit reprints per order

### Additional Risks

5. **Order Cancellation Abuse**
   - **Risk:** Customers canceling orders after kitchen prep
   - **Mitigation:**
     - Validate cancellation rules
     - Log all cancellations
     - Charge cancellation fee if configured

6. **Item Availability Sync**
   - **Risk:** Items marked available but out of stock
   - **Mitigation:**
     - Real-time inventory checks
     - Kitchen staff can mark items unavailable
     - Automatic updates when items run out

## Proposed KPIs

### Average Prep Time (Kitchen Efficiency)
- **Metric:** Time from Submitted to Ready
- **Target:** < 15 minutes for standard items
- **Calculation:** Average of (Ready timestamp - Submitted timestamp) for all orders
- **Tracking:** Per order, aggregated by day/week/month

### Order Accuracy Rate
- **Metric:** Correct items served vs ordered
- **Target:** > 95% accuracy
- **Calculation:** (Orders with no modifications) / (Total orders) * 100
- **Tracking:** Compare order lines at creation vs final delivery

### Table Turn Time (Dine-In Only)
- **Metric:** Seat-to-close duration
- **Target:** < 60 minutes for standard service
- **Calculation:** Average of (Paid/Closed timestamp - Created timestamp) for dine-in orders
- **Tracking:** Per table, aggregated by day/week/month

### Refund/Void Frequency
- **Metric:** Number of refunds/voids per period
- **Target:** < 5% of total orders
- **Calculation:** Count of refunded/voided payments / Total payments
- **Tracking:** Daily/weekly/monthly frequency
- **Alert:** If frequency exceeds threshold, investigate root cause

### Reprint Count
- **Metric:** Number of receipt reprints per period
- **Target:** < 2% of total receipts
- **Calculation:** Count of reprints / Total receipts
- **Tracking:** Daily/weekly/monthly count
- **Alert:** If reprint count high, investigate for fraud patterns

## Process Flow Diagrams

### Dine-In Order Flow

```
[Customer Seated]
    ↓
[Waiter Takes Order]
    ↓
[Order Created - Status: Submitted]
    ↓
[Payment?] ← Configurable: Before or After
    ↓
[Kitchen Receives Order]
    ↓
[Status: InPreparation]
    ↓
[Status: Ready]
    ↓
[Waiter Serves]
    ↓
[Status: Delivered]
    ↓
[Payment?] ← If not paid before
    ↓
[Status: Paid]
    ↓
[Order Closed]
```

### Takeaway Order Flow

```
[Customer Places Order]
    ↓
[Order Created - Status: Submitted]
    ↓
[Payment?] ← Configurable: Before or After
    ↓
[Kitchen Receives Order]
    ↓
[Status: InPreparation]
    ↓
[Status: Ready]
    ↓
[Customer Picks Up]
    ↓
[Payment?] ← If not paid before
    ↓
[Status: Paid]
    ↓
[Order Closed]
```

### Exception: Item Unavailable

```
[Order Creation]
    ↓
[Check Item Availability]
    ↓
[Item Available?]
    ├─ Yes → [Add to Order]
    └─ No → [Show Error: "Item Unavailable"]
            ↓
            [Suggest Alternatives?]
            ├─ Yes → [Show Alternative Items]
            └─ No → [Remove from Cart]
```

### Exception: Order Cancellation

```
[Cancel Order Request]
    ↓
[Check Order Status]
    ↓
[Status < InPreparation?]
    ├─ Yes → [Allow Cancellation]
    │        ↓
    │        [Log Reason]
    │        ↓
    │        [Status: Cancelled]
    └─ No → [Error: "Cannot cancel, order in preparation"]
```

### Exception: Void/Refund

```
[Void/Refund Request]
    ↓
[Check Payment Status]
    ↓
[Payment Captured?]
    ├─ No → [Void Payment]
    │       ↓
    │       [Status: Voided]
    └─ Yes → [Process Refund]
             ↓
             [Refund via Gateway]
             ↓
             [Status: Refunded]
             ↓
             [Log Reason]
```

### Exception: Receipt Reprint

```
[Reprint Request]
    ↓
[Check Order Status]
    ↓
[Order Paid?]
    ├─ No → [Error: "Order not paid"]
    └─ Yes → [Check Reprint Count]
             ↓
             [Count < Limit?]
             ├─ Yes → [Generate Receipt]
             │        ↓
             │        [Log Reprint]
             └─ No → [Require Manager Approval]
                      ↓
                      [Approved?]
                      ├─ Yes → [Generate Receipt]
                      └─ No → [Deny Reprint]
```

## Status Transition Rules

### Valid Status Transitions

- **Draft** → Submitted, Cancelled
- **Submitted** → InPreparation, Cancelled
- **InPreparation** → Ready, Cancelled (only if not started)
- **Ready** → Delivered, Paid
- **Delivered** → Paid
- **Paid** → (Final state)
- **Cancelled** → (Final state)

### Payment Status Transitions

- **Pending** → Authorized, Failed
- **Authorized** → Captured, Voided, Failed
- **Captured** → Refunded
- **Failed** → (Final state)
- **Refunded** → (Final state)
- **Voided** → (Final state)

## Implementation Notes

1. **Payment Timing Configuration:**
   - Per-order configuration: PayBeforeKitchen or PayAfterReady
   - Default: PayAfterReady for dine-in, PayBeforeKitchen for takeaway
   - Can be overridden per order

2. **Exception Logging:**
   - All exceptions logged with:
     - Timestamp
     - User/Staff ID
     - Order ID
     - Reason/Description
     - Resolution

3. **Audit Trail:**
   - All status changes logged
   - All payment transactions logged
   - All refunds/voids logged with reason
   - All receipt reprints logged with reason and user

4. **KPI Calculation:**
   - Metrics calculated in real-time
   - Stored in database for historical reporting
   - Available via API endpoints
   - Dashboard visualization (future phase)

