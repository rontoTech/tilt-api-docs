# Positions

**Base URL:** `https://api.tiltprotocol.com`

Positions reflect the on-chain token balances held in your vault, priced in real time via the Tilt oracle.

---

## List All Positions

```
GET /v1/trading/positions
```

Returns an array of position objects for every asset the vault currently holds.

### Response

```json
[
  {
    "symbol": "AAPL",
    "asset_id": "asset_aapl",
    "qty": "26.315789",
    "market_value": "5000.00",
    "current_price": "190.00",
    "side": "long"
  },
  {
    "symbol": "TSLA",
    "asset_id": "asset_tsla",
    "qty": "10.000000",
    "market_value": "1800.00",
    "current_price": "180.00",
    "side": "long"
  }
]
```

---

## Get Single Position

```
GET /v1/trading/positions/:symbol
```

Returns the position for a specific symbol.

### Response

```json
{
  "symbol": "AAPL",
  "asset_id": "asset_aapl",
  "qty": "26.315789",
  "market_value": "5000.00",
  "current_price": "190.00",
  "side": "long"
}
```

---

## Fields

| Field | Type | Description |
|-------|------|-------------|
| `symbol` | string | Ticker symbol |
| `asset_id` | string | Internal asset identifier |
| `qty` | string | Number of tokenized shares held |
| `market_value` | string | `qty × current_price` in USD |
| `current_price` | string | Latest oracle price for the asset |
| `side` | string | Always `"long"` (short positions are not supported) |

> Positions are derived from the vault's on-chain token balances. If tokens are transferred into the vault outside of the API, they will appear as positions once the indexer picks them up.
