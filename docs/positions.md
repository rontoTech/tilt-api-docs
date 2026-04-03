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
    "asset_id": "0x...token_address",
    "qty": "26.315789",
    "market_value": "5000.00",
    "current_price": "190.00",
    "avg_entry_price": "181.42",
    "side": "long",
    "exchange": "TILT"
  },
  {
    "symbol": "TSLA",
    "asset_id": "0x...token_address",
    "qty": "10.000000",
    "market_value": "1800.00",
    "current_price": "180.00",
    "avg_entry_price": "165.00",
    "side": "long",
    "exchange": "TILT"
  }
]
```

---

## Get Single Position

```
GET /v1/trading/positions/:symbol
```

Returns the position for a specific symbol, or `404` if the vault holds no tokens of that symbol.

### Response

```json
{
  "symbol": "AAPL",
  "asset_id": "0x...token_address",
  "qty": "26.315789",
  "market_value": "5000.00",
  "current_price": "190.00",
  "avg_entry_price": "181.42",
  "side": "long",
  "exchange": "TILT"
}
```

---

## Fields

| Field | Type | Description |
|-------|------|-------------|
| `symbol` | string | Ticker symbol |
| `asset_id` | string | On-chain token contract address |
| `qty` | string | Number of tokenized shares held |
| `market_value` | string | `qty × current_price` denominated in the vault's base asset (tiltUSDC) |
| `current_price` | string | Latest oracle price for the asset in USD |
| `avg_entry_price` | string \| null | Weighted-average cost basis per share across all filled buy orders. `null` if no buy history exists in the order log. |
| `side` | string | Always `"long"` (short positions are not supported) |
| `exchange` | string | Always `"TILT"` |

---

## avg_entry_price — how it is calculated

`avg_entry_price` uses the **Weighted-Average Cost (AVCO)** method, updated incrementally on every fill:

- **Buy fill** — blends the fill price into the running average:
  ```
  new_avg = (held_qty × old_avg + filled_qty × fill_price) / (held_qty + filled_qty)
  ```
- **Sell fill** — reduces `qty`; the per-share average is unchanged for remaining shares.

The value is stored in Redis on every order fill and is available immediately after the fill response is returned.

### Backfill for existing positions

For positions opened before `avg_entry_price` tracking was introduced, the server automatically replays the full order history for that symbol on the first `/positions` call and caches the result. Subsequent calls return the cached value.

### Using avg_entry_price for stop-loss rules

```python
# Example: 15% stop-loss from entry
for position in positions:
    if position["avg_entry_price"] is None:
        continue  # no cost basis — skip
    entry = float(position["avg_entry_price"])
    current = float(position["current_price"])
    pnl_pct = (current - entry) / entry * 100
    if pnl_pct <= -15:
        place_sell_order(position["symbol"], position["qty"])
```

> Positions are derived from the vault's on-chain token balances. If tokens are transferred into the vault outside of the API (e.g. direct on-chain swaps), they will appear as positions but `avg_entry_price` will be `null` since there is no corresponding order history.
