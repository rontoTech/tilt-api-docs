# Assets

**Base URL:** `https://api.tiltprotocol.com`

The assets endpoints let you discover and search enabled assets. Mainnet uses its administrator-managed Robinhood token registry; the legacy testnet stock catalog is separate.

On mainnet, `price` is a USD decimal string per display share, or `null` if no positive validated price is available. `price_available` is true exactly when `price` is non-null. A closed market, stopped/recovering health gate, invalid feed or unavailable display multiplier must never appear as a fabricated zero price. `status: "active"` describes registry/feed and engine-allowlist eligibility; `tradable` additionally requires an unpaused engine and positive validated stock/base prices. This is advisory: wallet authority, vault limits, liquidity and issuer restrictions are checked when executing. Raw basket exits and claims do not depend on `tradable`.

---

## List Assets

```
GET /v1/trading/assets
```

### Query Parameters

| Param | Type | Description |
|-------|------|-------------|
| `q` | string | Search by symbol or name (e.g. `?q=AAPL`) |
| `limit` | integer | Max results to return (default 200, max 1000) |
| `offset` | integer | Number of results to skip (default 0) |
| `page` | integer | Page number (1-indexed, alternative to offset) |
| `after` | string | Ticker symbol to start after (exclusive, e.g. `?after=AAPL`) |

### Response

```json
[
  {
    "symbol": "AAPL",
    "name": "Apple Inc.",
    "status": "active",
    "tradable": true,
    "price": "190.00",
    "exchange": "TILT"
  },
  {
    "symbol": "MSFT",
    "name": "Microsoft Corporation",
    "status": "active",
    "tradable": true,
    "price": "420.00",
    "exchange": "TILT"
  }
]
```

---

## Get Single Asset

```
GET /v1/trading/assets/:symbol
```

### Response

```json
{
  "symbol": "AAPL",
  "name": "Apple Inc.",
  "status": "active",
  "tradable": true,
  "price": "190.00",
  "exchange": "TILT"
}
```

---

## Fields

| Field | Type | Description |
|-------|------|-------------|
| `symbol` | string | Ticker symbol |
| `name` | string | Full company name |
| `status` | string | Mainnet: `"active"` or `"inactive"` eligibility; testnet can also report `"deployable"` |
| `tradable` | boolean | Advisory availability as described above; not a guarantee of settlement |
| `price` | string or null | Mainnet validated USD price per display share; unavailable references are null |
| `price_available` | boolean | Mainnet only: true exactly when `price` is non-null |
| `exchange` | string | Always `"TILT"` |

---

## Deploying Not-Yet-Deployed Tokens

This section applies only to **testnet**. Mainnet rejects `/deploy-token` with HTTP410; only administrators select existing issuer tokens. On testnet, some assets may show as not yet deployed on-chain. The **Trading API** often auto-deploys valid tickers on the first order. To deploy manually (e.g. for agents), call:

```
POST https://api.tiltprotocol.com/api/agents/deploy-token
```

### Request

```json
{
  "symbol": "XYZ"
}
```

### Response

```json
{
  "symbol": "XYZ",
  "status": "active",
  "tx_hash": "0xdef456..."
}
```

After deployment the asset becomes `active` and `tradable`.
