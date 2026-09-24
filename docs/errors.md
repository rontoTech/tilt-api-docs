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
| `503` | Authorization or another required service is temporarily unavailable |

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
| `40310001` | Invalid/revoked key, wrong chain, wrong secret, or lost vault authority |
| `40310005` | Signature nonce already used or invalid |
| `50310003` | Cannot verify current authorization; retry when Redis/RPC recovers |
| `50310004` | Basket cash quote or required deployment/read is unavailable |

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
| `42210011` | Market is closed |
| `42210012` | Pricing health is unavailable or recovering; includes an expired/stopped Tilt operational-health gate |
| `42210013` | Required price is stale or unavailable |
| `42210014` | Token oracle is paused or its pause status cannot be verified |
| `42210015` | Firm quote exceeds the allowed oracle deviation |
| `42210017` | Vault base asset, execution engine or price router differs from this manager API deployment |
| `42210016` | Firm quote cannot satisfy the exact mainnet limit price; the limit order remains resting |
| `42210020` | Invalid basket quote request or cash/fee bound cannot be met |
| `42210021` | Vault already has 1,000 live orders; close resting orders before creating more |
| `42910001` | Public basket quote rate limit exceeded |

HTTP **202** with `status: "pending_new"` is an unresolved execution outcome, not a rejection. Poll the existing order and never retry it under a new `client_order_id`; it may already have settled even when the transaction hash is unavailable. See [Orders](./orders.md#unknown-execution-outcomes).

Other `422` responses may return an **order-shaped body** with `status: "rejected"` and `error_message` when a **market** order fails execution.

#### `rejected` market orders and relayer nonces

If `error_message` mentions **`nonce`** (e.g. *nonce has already been used*), treat the outcome as **uncertain** until you verify on-chain state:

- Poll **`GET /v1/trading/orders/:id`** — the order may later show **`filled`** with a `tx_hash` if a mempool transaction succeeded.
- Check **`GET /v1/trading/positions`** and **`GET /v1/trading/account`** before submitting a **duplicate** trade.

**Prevention:** Send a unique **`client_order_id`** on every intended order; the API deduplicates by `(vault, client_order_id)` and returns the existing order on retry (see [Orders](./orders.md#relayer-nonces-and-burst-market-orders)). The backend also **serializes** relayer transactions to avoid nonce collisions; agents should still avoid blind retries after `rejected`.

### Internal (500xx)

Mainnet account reporting returns HTTP **503** with code **`50310005`**, `Vault valuation temporarily unavailable`, when the basket lens cannot compute NAV. Treat this as unavailable valuation, never zero holdings or zero value. Raw basket exits and reserved-token claims remain separate and available. Unexpected RPC/ABI/configuration failures retain the generic `50010007` account error.

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
