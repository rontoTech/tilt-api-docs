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

The server verifies **EIP-191 `personal_sign`** with this **exact** UTF-8 message (newlines matter):

```
Sign this message to create a Tilt Protocol API key.

This does not cost gas and does not grant access to your funds.

Vault: {vault_address}
Timestamp: {unix_timestamp}
```

`timestamp` is Unix **seconds** and must be within **5 minutes** of server time when `POST /v1/auth/keys` is received.

### Request

```json
{
  "wallet_address": "0xYourCuratorWallet",
  "vault_address": "0xYourVaultAddress",
  "signature": "0xYourWalletSignature",
  "timestamp": 1719000000
}
```

`wallet_address` must match the address that signed the message. The server
**verifies on-chain** that this wallet is the vault's **curator** (or an
authorized **delegate** of the vault) before issuing a key — a key cannot be
minted for a vault you do not control. A wallet that is neither returns `403`.

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

## Revoking Keys

```
DELETE /v1/auth/keys/{key_id}
```

Revocation requires a **wallet signature from the key's owner** (the wallet that
created it) — so learning a `key_id` is not enough to revoke it. Sign this
**exact** EIP-191 message (newlines matter) and send `signature` + `timestamp`
in the request body:

```
Sign this message to revoke a Tilt Protocol API key.

This does not cost gas and does not grant access to your funds.

Key: {key_id}
Timestamp: {unix_timestamp}
```

`timestamp` is Unix **seconds** and must be within **5 minutes** of server time.

```json
{
  "signature": "0xYourWalletSignature",
  "timestamp": 1719000000
}
```

A revoke with a missing or invalid signature returns `400`/`403`.

> Listing keys (`GET /v1/auth/keys?wallet=…`) returns only key IDs and metadata
> (never secrets) and does not require a signature.

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
