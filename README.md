# Tilt Protocol Trading API

Trade tokenized equities on-chain through a familiar REST API. If you've used Alpaca or IBKR, you already know how this works.

## Quick Start

### 1. Get your API keys

After creating a vault on [tiltprotocol.com](https://tiltprotocol.com), generate API keys:

```bash
curl -X POST https://api.tiltprotocol.com/v1/auth/keys \
  -H "Content-Type: application/json" \
  -d '{
    "wallet_address": "0xYourWallet",
    "vault_address": "0xYourVault",
    "signature": "0x...",
    "timestamp": 1711584000
  }'
```

Response:
```json
{
  "key_id": "ak_live_abc123...",
  "secret": "sk_live_xyz789...",
  "vault_address": "0xYourVault",
  "note": "Store the secret securely — it will not be shown again."
}
```

### 2. Place your first trade

```bash
curl -X POST https://api.tiltprotocol.com/v1/trading/orders \
  -H "TILT-API-KEY-ID: ak_live_abc123..." \
  -H "TILT-API-SECRET: sk_live_xyz789..." \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "AAPL",
    "notional": "5000",
    "side": "buy",
    "type": "market",
    "time_in_force": "day"
  }'
```

### 3. Check your positions

```bash
curl https://api.tiltprotocol.com/v1/trading/positions \
  -H "TILT-API-KEY-ID: ak_live_abc123..." \
  -H "TILT-API-SECRET: sk_live_xyz789..."
```

## Base URL

Fund managers and integrations should use this host for **all** HTTPS API traffic (trading, auth keys, and agent helpers such as deploy-token):

```
https://api.tiltprotocol.com
```

- Append paths directly, e.g. `https://api.tiltprotocol.com/v1/trading/orders`. Do not use legacy staging hostnames.
- A trailing slash on the host alone is optional; clients should normalize the base URL without a trailing slash.

## Authentication

Every request requires two headers:

| Header | Description |
|--------|-------------|
| `TILT-API-KEY-ID` | Your API key ID (starts with `ak_live_`) |
| `TILT-API-SECRET` | Your API secret (starts with `sk_live_`) |

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/v1/trading/orders` | Place a market or limit order |
| GET | `/v1/trading/orders` | List orders |
| GET | `/v1/trading/orders/:id` | Get order by ID |
| DELETE | `/v1/trading/orders/:id` | Cancel an order |
| GET | `/v1/trading/positions` | List all positions |
| GET | `/v1/trading/positions/:symbol` | Get position by symbol |
| GET | `/v1/trading/account` | Account / vault info |
| GET | `/v1/trading/assets` | List available stocks |
| GET | `/v1/trading/assets/:symbol` | Get stock detail |

## Documentation

- [Authentication](docs/authentication.md)
- [Orders](docs/orders.md)
- [Positions](docs/positions.md)
- [Account](docs/account.md)
- [Assets](docs/assets.md)
- [Errors](docs/errors.md)
- [WebSockets (future)](docs/websockets.md)
- [Migration Guide (Alpaca/IBKR)](docs/migration-guide.md)

## Examples

- [Python](examples/python/)
- [TypeScript](examples/typescript/)
- [VB.NET](examples/vbnet/) — drop-in client for IB TWS API users
- [curl](examples/curl/examples.sh)

## OpenAPI Spec

Import `openapi.yaml` into Postman or Swagger UI for interactive docs.
