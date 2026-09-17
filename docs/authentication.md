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

1. **A wallet on the target Robinhood Chain deployment** — any EVM-compatible wallet (MetaMask, Rabby, etc.)
2. **A vault created** — call the vault factory or use the Tilt dashboard to deploy your vault contract
3. **Backend authorized as delegate** — your vault must whitelist the Tilt backend address as a delegate so it can execute trades on your behalf

---

## Creating API Keys

```
POST /v1/auth/keys
```

Sign a fresh EIP-191 `personal_sign` message with the vault's current curator or authorized delegate wallet. The canonical domain is fixed; `chain_id` must equal the backend deployment (4663 for mainnet, 46630 for testnet). Addresses in the signed message are lowercase.

### Signing message format

Use these exact UTF-8 bytes, including newlines:

```text
Tilt Protocol API authorization v2
Domain: https://api.tiltprotocol.com
Chain ID: {chain_id}
Action: create-key
Wallet: {wallet_address_lowercase}
Vault: {vault_address_lowercase}
Permissions: trading
Timestamp: {unix_seconds}
Nonce: {nonce}

This key authorizes trading in this vault. It cannot withdraw funds.
```

Generate `nonce` using 32 cryptographically random bytes encoded as `0x` followed by 64 lowercase hexadecimal characters. A nonce can authorize one mutation. The timestamp must be at or before server time and less than 300 seconds old. Future timestamps and legacy signature formats are rejected.

### Request

```json
{
  "wallet_address": "0xYourCuratorWallet",
  "vault_address": "0xYourVaultAddress",
  "signature": "0xYourWalletSignature",
  "timestamp": 1789600000,
  "nonce": "0x32RandomBytesAs64LowercaseHexCharacters",
  "chain_id": 4663
}
```

The server checks the signer and current on-chain authority before consuming the nonce atomically. A replay returns `403` (`40310005`) and creates no second key. If a response is lost, list the wallet's keys, revoke any unwanted key, then use a fresh nonce to create another. Secrets cannot be recovered.

### Response

```json
{
  "key_id": "ak_live_abc123",
  "secret": "sk_live_xyz789",
  "vault_address": "0xYourVaultAddress",
  "note": "Store the secret securely — it will not be shown again."
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

The backend rechecks the key wallet's current curator/delegate role for every authenticated request. Removed delegates lose access immediately on the next check. Redis/RPC failures return `503` and authorize no action. Resting orders retain their authorizing wallet, key ID and chain; the keeper checks that the key still exists and the wallet remains authorized immediately before settlement.

**Rollout:** keys without chain binding must be recreated. Resting orders without recorded wallet/chain authorization must also be recreated; the keeper will not infer authority for historical records. Transactions already submitted on-chain cannot be revoked by deleting an API key.


---

## Revoking Keys

```
DELETE /v1/auth/keys/{key_id}
```

The key's creator or the vault's current curator may revoke it. A delegate cannot revoke another wallet's key. Sign this exact EIP-191 message:

```text
Tilt Protocol API authorization v2
Domain: https://api.tiltprotocol.com
Chain ID: {chain_id}
Action: revoke-key
Wallet: {signing_wallet_lowercase}
Key: {key_id}
Timestamp: {unix_seconds}
Nonce: {nonce}
```

Send `wallet_address`, `signature`, `timestamp`, a fresh `nonce`, and `chain_id` in the DELETE JSON body. The same timestamp and nonce rules apply. The wallet that originally created a key may revoke it even after losing its vault role.

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
