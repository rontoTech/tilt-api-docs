# Assets

**Base URL:** `https://api.tiltprotocol.com`

The assets endpoints let you discover and search the 7,000+ US equities available for trading on Tilt.

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
| `status` | string | `"active"` — token deployed and tradable; `"not_deployed"` — token contract not yet deployed |
| `tradable` | boolean | `true` if orders can be placed for this asset |
| `price` | string | Latest oracle price in USD |
| `exchange` | string | Always `"TILT"` |

---

## Deploying Not-Yet-Deployed Tokens

Some assets may show as not yet deployed on-chain. The **Trading API** often auto-deploys valid tickers on the first order. To deploy manually (e.g. for agents), call:

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
