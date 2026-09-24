# Account

**Base URL:** `https://api.tiltprotocol.com`

The account endpoint returns details about the vault associated with your API key.

On mainnet, the immutable basket lens supplies available balances, NAV and share price. If it reports unavailable valuation (including stopped or recovering operational health), this endpoint returns **HTTP 503**, `{ "code": 50310005, "message": "Vault valuation temporarily unavailable" }`. This does not mean the holdings are worth zero. Raw basket redemption and reserved-token claims remain callable; do not make them depend on this endpoint succeeding. Unexpected RPC, ABI or configuration errors remain generic HTTP 500 responses.

---

## Get Account

```
GET /v1/trading/account
```

### Response

```json
{
  "id": "vault_001",
  "status": "ACTIVE",
  "currency": "USD",
  "cash": "50000.00",
  "portfolio_value": "125000.00",
  "share_price": "1.25",
  "buying_power": "50000.00",
  "vault_name": "My Trading Vault",
  "vault_symbol": "MTV"
}
```

---

## Fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | Unique vault identifier |
| `status` | string | Vault status (`"ACTIVE"`, `"INACTIVE"`) |
| `currency` | string | Base currency — always `"USD"` |
| `cash` | string | Available base-token balance (USDG on mainnet), excluding reserved claims |
| `portfolio_value` | string | Total net asset value (cash + positions) |
| `share_price` | string | Current price per vault share |
| `buying_power` | string | Available funds for new orders (currently equal to `cash`) |
| `vault_name` | string | Human-readable vault name |
| `vault_symbol` | string | Short vault token symbol |

### Notes

- **cash** is the amount of USDC sitting in the vault that is not allocated to any position.
- **portfolio_value** is the total NAV: the sum of `cash` and the market value of all open positions.
- **buying_power** is currently identical to `cash`. Future margin features may cause these to diverge.
