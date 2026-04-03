# Error Handling

**Base URL:** `https://api.tiltprotocol.com`

## HTTP status codes

| Code | Meaning |
|------|---------|
| `401` | Authentication required — no API key headers provided |
| `403` | Forbidden — API key is invalid or revoked |
| `404` | Resource not found |
| `422` | Validation error — request body or parameters are invalid |
| `429` | Rate limit exceeded |
| `500` | Internal server error |

---

## Error response format

Errors return JSON with a numeric `code` and a human-readable `message`:

```json
{
  "code": 42210010,
  "message": "expires_at is required when time_in_force is gtd"
}
```

---

## Error code reference

### Authentication (401xx)

| Code | Description |
|------|-------------|
| `40110001` | Authentication required — missing `TILT-API-KEY-ID` or `TILT-API-SECRET` header |

### Authorization (403xx)

| Code | Description |
|------|-------------|
| `40310001` | Invalid API key — key does not exist or secret does not match |

### Validation (422xx) — trading `POST /v1/trading/orders`

| Code | Description |
|------|-------------|
| `42210001` | `symbol` is required |
| `42210002` | `side` must be `"buy"` or `"sell"` |
| `42210003` | `qty` or `notional` is required |
| `42210004` | `type` must be `"market"` or `"limit"` |
| `42210005` | `limit_price` is required for limit orders |
| `42210006` | `time_in_force` must be `day`, `gtc`, `gtd`, `ioc`, or `fok` |
| `42210007` | Symbol not recognized, or token could not be resolved for that ticker |
| `42210008` | Order cannot be canceled (already filled or canceled) |
| `42210009` | Failed to deploy token for symbol (message truncated in response) |
| `42210010` | `expires_at` is required when `time_in_force` is `gtd` |

Other `422` responses may return an **order-shaped body** with `status: "rejected"` and `error_message` when a **market** order fails execution.

#### `rejected` market orders and relayer nonces

If `error_message` mentions **`nonce`** (e.g. *nonce has already been used*), treat the outcome as **uncertain** until you verify on-chain state:

- Poll **`GET /v1/trading/orders/:id`** — the order may later show **`filled`** with a `tx_hash` if a mempool transaction succeeded.
- Check **`GET /v1/trading/positions`** and **`GET /v1/trading/account`** before submitting a **duplicate** trade.

**Prevention:** Send a unique **`client_order_id`** on every intended order; the API deduplicates by `(vault, client_order_id)` and returns the existing order on retry (see [Orders](./orders.md#relayer-nonces-and-burst-market-orders)). The backend also **serializes** relayer transactions to avoid nonce collisions; agents should still avoid blind retries after `rejected`.

### Internal (500xx)

| Code | Description |
|------|-------------|
| `50010001` | Internal error placing order |
| `50010002` | Failed to list orders |

(See server logs / OpenAPI for additional `500100xx` codes on other routes.)

---

## Best practices

- Log `code` and `message` for support tickets.
- For limit orders, poll `GET /v1/trading/orders/:id` until terminal state; `rejected` often indicates an on-chain or configuration issue (e.g. delegate).
- For **market** orders from automation: always set **`client_order_id`**; after **`rejected`**, verify positions/account before re-submitting with a **new** id.
- See the [Trading guide](./trading-guide.md) (§12) and [Orders](./orders.md#relayer-nonces-and-burst-market-orders) for relayer nonce behavior and pacing.
