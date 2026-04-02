# Authentication

**Base URL:** `https://api.tiltprotocol.com` — use this host for `POST /v1/auth/keys` and all authenticated trading calls.

The Tilt Trading API uses **API key pairs** for authentication. Every request must include two headers:

| Header | Description |
|--------|-------------|
| `TILT-API-KEY-ID` | Your public key (prefix `ak_live_` or `ak_test_`) |
| `TILT-API-SECRET` | Your secret key (prefix `sk_live_` or `sk_test_`) |

---

## Prerequisites

Before generating API keys you need:

1. **A wallet on Robinhood Chain testnet** — any EVM-compatible wallet (MetaMask, Rabby, etc.)
2. **A vault created** — call the vault factory or use the Tilt dashboard to deploy your vault contract
3. **Backend authorized as delegate** — your vault must whitelist the Tilt backend address as a delegate so it can execute trades on your behalf

---

## Creating API Keys

```
POST /v1/auth/keys
```

You authenticate this single request by signing a message with your wallet. After that, all subsequent requests use the returned key pair.

### Signing Message Format

```
Sign this message to generate Tilt API keys.\n\nVault: {vault_address}\nTimestamp: {unix_timestamp}\nNonce: {random_nonce}
```

### Request

```json
{
  "vault_address": "0xYourVaultAddress",
  "signature": "0xYourWalletSignature",
  "timestamp": 1719000000,
  "nonce": "a1b2c3d4"
}
```

### Response

```json
{
  "key_id": "ak_live_abc123",
  "secret": "sk_live_xyz789",
  "vault_address": "0xYourVaultAddress",
  "created_at": "2025-06-01T12:00:00Z",
  "permissions": ["trade"]
}
```

> **Store your secret immediately.** It is only returned once and cannot be retrieved later.

---

## Key Permissions

API keys can **only** execute trades. They cannot:

- Withdraw funds from the vault
- Pause or unpause the vault
- Change vault configuration or delegates
- Rotate other keys

This design keeps on-chain admin operations wallet-only, limiting the blast radius if a key is compromised.

---

## Rate Limits

| Limit | Value |
|-------|-------|
| Requests per second per key | **10** |
| Orders per minute per vault | **100** |

Exceeding these limits returns `429 Too Many Requests`. Back off and retry with exponential backoff.

---

## Example Request

```bash
curl -s -X GET "https://api.tiltprotocol.com/v1/trading/account" \
  -H "TILT-API-KEY-ID: ak_live_abc123" \
  -H "TILT-API-SECRET: sk_live_xyz789"
```

```json
{
  "id": "vault_001",
  "status": "active",
  "cash": "50000.00",
  "portfolio_value": "125000.00"
}
```
