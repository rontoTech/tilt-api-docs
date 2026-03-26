# Orders

## Order Types

| Type | Behavior |
|------|----------|
| **market** | Executes immediately at the current oracle price. Typical fill time ~2–5 seconds. |
| **limit** | Resting order. The matching engine checks every 5 seconds whether the oracle price meets your limit price. |

## Time in Force

| Value | Meaning |
|-------|---------|
| `day` | Expires at the next market close (4:00 PM ET) |
| `gtc` | Good til canceled — stays open until filled or explicitly canceled |

---

## Order Lifecycle

```
new ──▶ accepted ──▶ filled
                 ├──▶ canceled
                 ├──▶ expired
                 └──▶ rejected
```

- **new** — order received, pending validation
- **accepted** — validated and queued for execution
- **filled** — fully executed on-chain
- **canceled** — canceled by the user (via `DELETE`)
- **expired** — `day` order that reached market close without filling
- **rejected** — failed validation or insufficient buying power

---

## Place an Order

```
POST /v1/trading/orders
```

### Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `symbol` | string | yes | Ticker symbol (e.g. `"AAPL"`) |
| `qty` | string | one of `qty` or `notional` | Number of shares |
| `notional` | string | one of `qty` or `notional` | Dollar amount to invest |
| `side` | string | yes | `"buy"` or `"sell"` |
| `type` | string | yes | `"market"` or `"limit"` |
| `time_in_force` | string | yes | `"day"` or `"gtc"` |
| `limit_price` | string | limit only | Required when `type` is `"limit"` |
| `client_order_id` | string | no | Idempotency key (your own unique ID) |

### Example Request

```json
{
  "symbol": "AAPL",
  "notional": "5000",
  "side": "buy",
  "type": "market",
  "time_in_force": "day"
}
```

### Response — Order Object

```json
{
  "id": "ord_abc123",
  "client_order_id": null,
  "symbol": "AAPL",
  "asset_id": "asset_aapl",
  "qty": null,
  "notional": "5000",
  "filled_qty": "26.315789",
  "filled_avg_price": "190.00",
  "side": "buy",
  "type": "market",
  "time_in_force": "day",
  "limit_price": null,
  "status": "filled",
  "created_at": "2025-06-01T14:30:00Z",
  "updated_at": "2025-06-01T14:30:03Z",
  "filled_at": "2025-06-01T14:30:03Z",
  "tx_hash": "0xabc123def456..."
}
```

---

## List Orders

```
GET /v1/trading/orders
```

### Query Parameters

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `status` | string | `"open"` | `"open"`, `"closed"`, or `"all"` |
| `limit` | integer | 50 | Max results (1–500) |
| `after` | string | — | Cursor for pagination |
| `symbols` | string | — | Comma-separated symbol filter |

### Response

Returns an array of Order objects.

---

## Get Order by ID

```
GET /v1/trading/orders/:order_id
```

Returns a single Order object.

---

## Cancel an Order

```
DELETE /v1/trading/orders/:order_id
```

Attempts to cancel an open order. Returns the updated Order object with `status: "canceled"` on success. Orders already filled or expired cannot be canceled.
