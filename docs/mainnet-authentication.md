# Mainnet beta authentication

**Mainnet HTTP base URL:** `https://tilt-mainnet-api-production.up.railway.app` — use this host for key management and mainnet trading calls. Set `MAINNET_API_URL` to this exact origin.

**Status: configured and closed; activation pending.** Mainnet uses real assets. The beta contracts have not been independently audited. Authentication or API availability does not authorize bypassing closed trading or deposit controls. Existing testnet clients retain the separate [testnet authentication flow](authentication.md) at `https://api.tiltprotocol.com`, chain 46630.

The signature's `Domain: https://api.tiltprotocol.com` is a fixed protocol identifier. Preserve those exact bytes even though HTTP requests go to the separate mainnet origin. The signed chain ID is **4663**.

The Tilt Trading API uses **API key pairs** for authentication. Every request must include two headers:

| Header | Description |
|--------|-------------|
| `TILT-API-KEY-ID` | Your API key ID (prefix `ak_live_`) |
| `TILT-API-SECRET` | Your secret key (prefix `sk_live_`) |

---

## Prerequisites

Before generating API keys you need:

1. **A wallet on Robinhood Chain mainnet, chain 4663** — any EVM-compatible wallet (MetaMask, Rabby, etc.)
2. **A vault created** — call the vault factory or use the Tilt dashboard to deploy your vault contract
3. **Backend authorized as delegate** — your vault must whitelist the Tilt backend address as a delegate so it can execute trades on your behalf

---

## Creating API Keys

```
POST https://tilt-mainnet-api-production.up.railway.app/v1/auth/keys
```

Sign a fresh EIP-191 `personal_sign` message with the vault's current curator or authorized delegate wallet. Use `chain_id: 4663`. Addresses in the signed message are lowercase.

### Signing message format

Use these exact UTF-8 bytes, including newlines:

```text
Tilt Protocol API authorization v2
Domain: https://api.tiltprotocol.com
Chain ID: 4663
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

API keys authorize manager API requests, including trading and portfolio reads. They cannot:

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
DELETE https://tilt-mainnet-api-production.up.railway.app/v1/auth/keys/{key_id}
```

The key's creator or the vault's current curator may revoke it. A delegate cannot revoke another wallet's key. Sign this exact EIP-191 message:

```text
Tilt Protocol API authorization v2
Domain: https://api.tiltprotocol.com
Chain ID: 4663
Action: revoke-key
Wallet: {signing_wallet_lowercase}
Key: {key_id}
Timestamp: {unix_seconds}
Nonce: {nonce}
```

Send `wallet_address`, `signature`, `timestamp`, a fresh `nonce`, and `chain_id: 4663` in the DELETE JSON body. The same timestamp and nonce rules apply. The wallet that originally created a key may revoke it even after losing its vault role. A successful response includes `success: true`; an invalid, expired or replayed proof returns HTTP403. Rotate by revoking the old key and creating a replacement with a fresh nonce.

> Listing keys (`GET /v1/auth/keys?wallet=…`) returns only key IDs and metadata
> (never secrets) and does not require a signature.

---

## Rate Limits

| Limit | Value |
|-------|-------|
| API requests per minute per source IP | **240** |
| Key-management requests per minute per source IP | **30**, also subject to the API-wide limit |

Exceeding these fixed-window limits returns `429 Too Many Requests` with `Retry-After`. These limits are per service instance. Wait before retrying; preserve existing order IDs and reconcile uncertain execution outcomes rather than creating replacement trades.

---

## Example Request

```bash
curl -s "$MAINNET_API_URL/v1/trading/account" \
  -H "TILT-API-KEY-ID: $TILT_API_KEY_ID" \
  -H "TILT-API-SECRET: $TILT_API_SECRET"
```

While portfolio valuation is unavailable, including during health recovery, the account endpoint returns HTTP503/code `50310005`. This is not a zero portfolio value. Raw basket exits remain available independently of account reporting. Use the [mainnet OpenAPI specification](../openapi.mainnet.yaml) for response schemas.
