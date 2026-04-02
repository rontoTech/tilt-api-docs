# Migration Guide — Alpaca / IBKR to Tilt

**Base URL:** `https://api.tiltprotocol.com`

If you're coming from Alpaca or Interactive Brokers, this guide maps the concepts and endpoints you already know to their Tilt equivalents.

---

## Authentication Mapping

| Alpaca | Tilt |
|--------|------|
| `APCA-API-KEY-ID` header | `TILT-API-KEY-ID` header |
| `APCA-API-SECRET-KEY` header | `TILT-API-SECRET` header |
| Dashboard-generated keys | Wallet-signature-generated keys (`POST /v1/auth/keys`) |

---

## Endpoint Mapping

| Action | Alpaca | Tilt |
|--------|--------|------|
| Get account | `GET /v2/account` | `GET /v1/trading/account` |
| List orders | `GET /v2/orders` | `GET /v1/trading/orders` |
| Place order | `POST /v2/orders` | `POST /v1/trading/orders` |
| Get order | `GET /v2/orders/:id` | `GET /v1/trading/orders/:id` |
| Cancel order | `DELETE /v2/orders/:id` | `DELETE /v1/trading/orders/:id` |
| List positions | `GET /v2/positions` | `GET /v1/trading/positions` |
| Get position | `GET /v2/positions/:symbol` | `GET /v1/trading/positions/:symbol` |
| List assets | `GET /v2/assets` | `GET /v1/trading/assets` |
| Get asset | `GET /v2/assets/:symbol` | `GET /v1/trading/assets/:symbol` |

---

## Key Differences

### On-chain settlement
Every trade settles as an on-chain transaction on Robinhood Chain. Filled orders include a `tx_hash` field you can verify on the block explorer.

### Vault-based, not account-based
Instead of a brokerage account, you have a **vault** — a smart contract that holds your USDC and tokenized shares. The API key acts as a delegate with trade-only permissions.

### No fractional shares (yet)
Quantities map to on-chain token balances. Fractional share support is on the roadmap but not yet available. Use `notional` orders to invest a dollar amount; the protocol handles the math.

### Limit orders: resting behavior and hours
Limit orders are **resting**: `POST` returns `accepted` and a **keeper** attempts fills on a short interval (~5s) when US equity hours are **open** (with a pre/post window on the server). Outside that window, orders usually stay open without fill attempts; **`day`** orders may **expire** after the close. **`gtd`** orders also expire at **`expires_at`** regardless of market hours.

Fill rules: **buy** when quote **≤** `limit_price`; **sell** when quote **≥** `limit_price`. See [Trading guide](./trading-guide.md).

### Delegate required for limit fills
The backend **delegate** wallet must be authorized on your vault to execute limit fills. Without it, orders may sit as `accepted` or end as `rejected`.

### Token deployment
Valid tickers are often **auto-deployed** on first trade via `POST /v1/trading/orders`. For a manual deploy (e.g. agents), use `POST https://api.tiltprotocol.com/api/agents/deploy-token` with `{"symbol":"TICK"}`. Alpaca/IBKR don't have this concept.

---

## What Stays the Same

- **REST API** — standard JSON over HTTPS, same request/response patterns
- **JSON responses** — field names and structures are intentionally close to Alpaca's
- **Order lifecycle** — `new → accepted → filled/canceled/expired/rejected` mirrors Alpaca exactly
- **Position tracking** — same `symbol`, `qty`, `market_value` fields
- **Order types** — `market` and `limit` with `day`, `gtc`, and **`gtd`** (`expires_at` ISO-8601 required for GTD)

---

## Quick Migration Checklist

1. Replace `APCA-API-KEY-ID` / `APCA-API-SECRET-KEY` headers with `TILT-API-KEY-ID` / `TILT-API-SECRET`
2. Update base URL to `https://api.tiltprotocol.com`
3. Change API version prefix from `/v2/` to `/v1/trading/`
4. Handle `tx_hash` in order responses (new field)
5. Rely on auto-deploy for trading where supported, or call `https://api.tiltprotocol.com/api/agents/deploy-token` when you need an explicit deploy step
6. Update error handling to use Tilt error codes (see [Errors](./errors.md))
