# Orders

**Base URL:** `https://api.tiltprotocol.com` — paths below are appended to this host (e.g. `POST https://api.tiltprotocol.com/v1/trading/orders`).

For a narrative walkthrough (fill logic, delegate, market hours), see the **[Fund Manager Trading Guide](./trading-guide.md)**.

---

## Order types

| Type | Behavior |
|------|----------|
| **market** | Executes immediately: fresh oracle price is pushed, then the vault trades on-chain. Response is usually `filled` within seconds, with `tx_hash`. |
| **limit** | **Resting order.** Stored as `accepted` and monitored by the limit-order keeper (~5s poll). When the live quote meets your limit rule (see guide), the service submits `executeTrade` on your vault. |

---

## Time in force

| Value | Meaning |
|-------|---------|
| **`day`** | Day order — expires after the service’s US equity session rules (typically after close / extended window end). |
| **`gtc`** | Good til canceled — remains open until filled, `DELETE` canceled, expired by other rules, or **rejected** on failed execution. |
| **`gtd`** | Good til date — requires **`expires_at`** (ISO-8601). Order expires at that instant if not yet filled (evaluated even when the cash market is closed). |
| **`ioc`**, **`fok`** | Accepted for API compatibility; prefer **`day` / `gtc` / `gtd`** for standard resting limits. |

### GTD and `expires_at`

When `time_in_force` is **`gtd`**, you **must** send **`expires_at`** as an ISO-8601 string, for example:

`"2026-06-15T20:00:00.000Z"`

Omitting it returns **`422`** with code `42210010`.

---

## Order lifecycle

```
new ──▶ accepted ──▶ filled
        (limit)  ├──▶ canceled   (DELETE)
                 ├──▶ expired    (day / gtd)
                 └──▶ rejected   (validation or on-chain failure)
```

| Status | Meaning |
|--------|---------|
| **new** | Market order: received, about to execute. |
| **accepted** | Limit order: validated and resting until fill or terminal state. |
| **pending_new** | Transitional (rare in responses). |
| **partially_filled** | Reserved; full fills are atomic today. |
| **filled** | Trade completed on-chain; `tx_hash` set when available. |
| **canceled** | Canceled via `DELETE`. |
| **expired** | `day` or `gtd` cutoff reached without fill. |
| **rejected** | Failed validation or keeper execution error; see `error_message` if present. |

---

## Place an order

```
POST /v1/trading/orders
```

### Request body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `symbol` | string | yes | Ticker (e.g. `"AAPL"`). |
| `qty` | string | one of `qty` or `notional` | Share quantity (decimal string). |
| `notional` | string | one of `qty` or `notional` | USD amount for the order (common on **buy**). |
| `side` | string | yes | `"buy"` or `"sell"`. |
| `type` | string | yes | `"market"` or `"limit"`. |
| `time_in_force` | string | yes | `"day"`, `"gtc"`, `"gtd"`, `"ioc"`, or `"fok"`. |
| `limit_price` | string | limit only | Required when `type` is `"limit"` — USD per share. |
| `expires_at` | string | GTD only | ISO-8601; required when `time_in_force` is `"gtd"`. |
| `client_order_id` | string | no | Your correlation / idempotency key. |

### Example — market buy (notional)

```json
{
  "symbol": "AAPL",
  "notional": "5000",
  "side": "buy",
  "type": "market",
  "time_in_force": "day"
}
```

### Example — limit buy GTC (shares)

```json
{
  "symbol": "AAPL",
  "qty": "10",
  "side": "buy",
  "type": "limit",
  "limit_price": "200.00",
  "time_in_force": "gtc"
}
```

### Example — limit sell GTD

```json
{
  "symbol": "MSFT",
  "qty": "5",
  "side": "sell",
  "type": "limit",
  "limit_price": "400.00",
  "time_in_force": "gtd",
  "expires_at": "2026-12-31T21:00:00.000Z"
}
```

### Response — order object

Limit orders return immediately with `status: "accepted"` (and no `tx_hash` until filled). Market orders typically return `status: "filled"`.

```json
{
  "id": "ord_abc123",
  "client_order_id": null,
  "vault": "0x…",
  "symbol": "AAPL",
  "qty": "10",
  "notional": null,
  "filled_qty": "0",
  "side": "buy",
  "type": "limit",
  "time_in_force": "gtc",
  "limit_price": "200.00",
  "filled_avg_price": null,
  "status": "accepted",
  "created_at": "2026-04-01T14:30:00.000Z",
  "updated_at": "2026-04-01T14:30:00.000Z",
  "filled_at": null,
  "expired_at": null,
  "canceled_at": null,
  "expires_at": null,
  "tx_hash": null,
  "error_message": null
}
```

For **`gtd`**, `expires_at` echoes your requested expiry. Other time-in-force values store `expires_at` as `null`.

---

## List orders

```
GET /v1/trading/orders
```

### Query parameters

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `status` | string | `all` | `open`, `closed`, or `all`. Use **`open`** for resting limits. |
| `limit` | integer | 50 | Max results (capped server-side, e.g. 200). |

### Response

JSON array of order objects.

---

## Get order by ID

```
GET /v1/trading/orders/:order_id
```

Returns a single order. Use this to poll a limit order until it reaches a terminal state.

---

## Cancel an order

```
DELETE /v1/trading/orders/:order_id
```

Cancels an open order. Returns the order with `status: "canceled"`. Already **filled**, **canceled**, or **expired** orders cannot be canceled (`422`).

---

## Fill logic (quick reference)

- **Buy limit** fills when **market quote ≤ `limit_price`**.
- **Sell limit** fills when **market quote ≥ `limit_price`**.

See the [Trading guide](./trading-guide.md#6-when-does-a-limit-order-fill-important) for examples and common misconceptions.
