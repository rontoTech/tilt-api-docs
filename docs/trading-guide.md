# Fund Manager Trading Guide

**Base URL:** `https://api.tiltprotocol.com`

This guide explains how to integrate with the Tilt Trading API end-to-end: authentication, market vs limit orders, time-in-force, how limits are filled, and operational requirements.

---

## 1. What you are trading

- Trades execute against **your vault** (a smart contract). The API key is bound to one vault when you create it.
- Each order ultimately becomes an on-chain **`executeTrade`** through the protocol’s router (USDC ↔ tokenized equity).
- **Market orders** are sent immediately after a fresh oracle price is pushed for that symbol.
- **Limit orders** are **resting orders**: they are stored server-side and monitored by an automation service (**limit order keeper**) that checks prices on a short interval and submits the trade when your price condition is met.

---

## 2. Prerequisites

### API keys

Create keys with `POST /v1/auth/keys` (wallet signature). See [Authentication](./authentication.md).

Every request needs:

| Header | Description |
|--------|-------------|
| `TILT-API-KEY-ID` | Key ID (`ak_live_…`) |
| `TILT-API-SECRET` | Secret (`sk_live_…`) |

The key implicitly scopes all trading to **the vault address** returned at key creation.

### Backend delegate (limit orders only)

Limit orders are executed by a **backend signer** (delegate) calling `executeTrade` on your vault. Your vault **curator** must have authorized that delegate on-chain (same flow as enabling API trading in the Tilt app).

If the delegate is not authorized, limit orders may be **accepted** in the order store but **fills will fail** when the keeper runs (order may move to `rejected` with an error). Ensure delegate setup is complete before relying on resting limits.

### Recognized symbols

`symbol` must be a supported ticker. Unknown tickers return `422`. Many symbols are **auto-deployed** on first trade if not yet on-chain.

---

## 3. Placing orders

**Endpoint:** `POST /v1/trading/orders`

| Field | Required | Description |
|-------|----------|-------------|
| `symbol` | yes | Ticker, e.g. `AAPL` |
| `side` | yes | `buy` or `sell` |
| `qty` **or** `notional` | yes | Share quantity **or** USD notional (buys often use `notional`) |
| `type` | yes | `market` or `limit` |
| `time_in_force` | yes* | `day`, `gtc`, `gtd`, `ioc`, or `fok` (see §5). Defaults to `day` if omitted in some clients—set explicitly for clarity. |
| `limit_price` | limit only | Max **average** price per share you accept on a buy, or min on a sell (see §6). |
| `expires_at` | GTD only | ISO-8601 datetime when the order must expire, e.g. `2026-06-30T16:00:00.000Z` |
| `client_order_id` | no | Your idempotency / correlation id |

\*Validation requires `time_in_force` to be one of the allowed values; see [Orders](./orders.md) for details.

---

## 4. Market orders

- `type`: `"market"`
- Executed **immediately** in the same request path: price refresh → on-chain trade → response usually `status: "filled"` with `tx_hash`.
- Typical latency is a few seconds, depending on RPC and confirmation.
- `time_in_force` is still validated; common choice is `"day"`.

---

## 5. Limit orders and time in force

- `type`: `"limit"`
- **`limit_price`** is required (string, e.g. `"250.00"`).

### Time in force values

| Value | Behavior |
|-------|----------|
| **`gtc`** | Good til canceled — stays open until filled, canceled via API, rejected, or (for day semantics elsewhere) expired by rules below. |
| **`day`** | **Day order** — if still open, expired when the service treats the US equity session as closed (see keeper / market-hours logic). |
| **`gtd`** | Good til date — must include **`expires_at`** (ISO-8601). Expires at that time if not filled. |
| **`ioc` / `fok`** | Accepted by the API for compatibility; behavior for resting flow may be limited — prefer `gtc` / `day` / `gtd` for standard limit workflows. |

### Response for a new limit order

You normally receive **`status`: `"accepted"`** immediately. The order is **not** filled until the keeper submits a successful on-chain trade.

Poll `GET /v1/trading/orders/:id` or list `GET /v1/trading/orders?status=open` until status becomes `filled`, `canceled`, `expired`, or `rejected`.

---

## 6. When does a limit order fill? (important)

The keeper compares a **live quote** (oracle feed used by the service) to your **`limit_price`**:

| Side | Fills when |
|------|------------|
| **Buy** | Market price **≤** `limit_price` (you are willing to pay **up to** the limit; fills when the market is at or below that). |
| **Sell** | Market price **≥** `limit_price` (you want **at least** the limit; if the market is below your limit, the order **does not** fill). |

Examples:

- **Buy** limit **above** current market: condition is usually **already true** (current price ≤ limit), so the keeper can attempt a fill soon (subject to market hours and execution success).
- **Sell** limit **above** current market: condition is **false** until the market **rises** to your limit — **not filling is expected** until then.

This matches standard limit semantics but surprises teams who expect “sell at market” behavior while entering a limit price above the last trade.

---

## 7. Market hours and evaluation cadence

- Limit **fills** are evaluated when the service considers US equity hours **open** (with a pre/post window as implemented by the backend). Outside that window, resting orders typically **stay open** without fill attempts; **`day`** orders may be **expired** after the close.
- **`gtd`** expiry is evaluated **independently** of market hours — an order can expire at `expires_at` even when the cash market is closed.
- The keeper runs on a **short polling interval** (order of **~5 seconds**, configurable server-side). Fills are not instantaneous.

---

## 8. Listing and canceling

**List orders:** `GET /v1/trading/orders?status=open|closed|all&limit=50`

- Default **`status`** on the server is **`all`** unless you pass `open` or `closed`.
- Use **`status=open`** to see resting limits and any other open states.

**Cancel:** `DELETE /v1/trading/orders/:id`

Only orders that are still open can be canceled.

---

## 9. Partial fills and liquidity

- Orders are **fully filled or not** in a single on-chain swap; **partial fills** are not supported in the current protocol shape.
- There is **no external order book** — execution is against the protocol router at oracle-based prices within slippage tolerances enforced on-chain.

---

## 10. Errors and debugging

Structured errors use `code` + `message`. See [Errors](./errors.md).

Common issues:

- **`422`** — missing `limit_price` on limits, invalid `time_in_force`, missing `expires_at` for `gtd`, unknown `symbol`, token deploy failure.
- **Limit never fills** — wrong side vs limit relative to market (§6), outside market hours, symbol missing quotes, delegate not set, or on-chain revert (check order `status` → `rejected` and logs).

---

## 11. Related docs

- [Orders](./orders.md) — field-level reference
- [Authentication](./authentication.md)
- [Positions](./positions.md) / [Account](./account.md)
- [Migration guide](./migration-guide.md) (Alpaca / IBKR)
- [OpenAPI spec](../openapi.yaml)

---

## 12. Example flows

Minimal **GTC limit buy** (curl):

```bash
curl -sS -X POST 'https://api.tiltprotocol.com/v1/trading/orders' \
  -H 'Content-Type: application/json' \
  -H 'TILT-API-KEY-ID: ak_live_...' \
  -H 'TILT-API-SECRET: sk_live_...' \
  -d '{
    "symbol": "AAPL",
    "qty": "10",
    "side": "buy",
    "type": "limit",
    "limit_price": "200.00",
    "time_in_force": "gtc"
  }'
```

**GTD sell** (requires `expires_at`):

```bash
curl -sS -X POST 'https://api.tiltprotocol.com/v1/trading/orders' \
  -H 'Content-Type: application/json' \
  -H 'TILT-API-KEY-ID: ak_live_...' \
  -H 'TILT-API-SECRET: sk_live_...' \
  -d '{
    "symbol": "MSFT",
    "qty": "5",
    "side": "sell",
    "type": "limit",
    "limit_price": "380.00",
    "time_in_force": "gtd",
    "expires_at": "2026-12-31T21:00:00.000Z"
  }'
```

Language examples: [Python](../examples/python/), [TypeScript](../examples/typescript/), [VB.NET](../examples/vbnet/), [curl](../examples/curl/examples.sh).
