# Moving an integration to mainnet beta

**Status:** contracts deployed and configured on September 22, 2026; activation, health recovery and funded verification are pending. This is an unaudited beta with real assets. Do not enable autonomous mainnet trading until activation is confirmed.

## Keep testnet separate

| Environment | Chain ID | App | API |
|---|---|---|---|
| Testnet | 46630 | https://testnet.tiltprotocol.com | https://api.tiltprotocol.com |
| Mainnet beta | 4663 | https://app.tiltprotocol.com | https://tilt-mainnet-api-production.up.railway.app |

The mainnet API is deployed; this does not announce trading or app activation. Require an explicitly selected mainnet API origin, check `GET /api/agents/contracts` reports chain 4663 and `vaultModel: "immutable-basket-v1"`, and compare the connected RPC's chain ID. Never fall back to the testnet API. Keep keys, queued orders and saved addresses isolated by chain.

The authentication message's `Domain: https://api.tiltprotocol.com` is a **fixed protocol signature domain**, even when HTTPS requests use another API origin. Keep those exact bytes; bind `chain_id: 4663`, the full action and a fresh one-time nonce. Recreate keys for the mainnet vault. Current curator/delegate authority is checked again for requests and settlement. See [mainnet authentication](mainnet-authentication.md) and the separate [mainnet OpenAPI specification](../openapi.mainnet.yaml). The default authentication page and `openapi.yaml` continue to describe testnet.

## Use the immutable basket interfaces

The mainnet factory is `0x463A6Db2731d4c5C5e9d937f7F9ccC82c2178D37`; the investor cash router is `0xB200A060eC342Ef65471595e0D506C830deAd8c1`; the read-only lens is `0x02eEFa923022E45F6f998e91a7Ad79bEbA9D5214`. Compare the current address book with the [published deployments](https://docs.tiltprotocol.com/contracts/deployments); an address alone does not mean creation/trading is open.

Do not call legacy `createUserVaultWithFees`, ERC-4626-style cash `deposit`/`redeem`, allocation or target-weight methods. Use `createVault(...,expectedConfig)`, `mintBasket` and `redeemBasket` with the current ABI. Read the factory base asset and configuration hash together. Every vault holds its own assets, and existing code or base asset cannot be changed in place; migration requires investor opt-in.

USDG is the initial base. Approved initial equities are AAPL, MSFT, TSLA, GOOGL and NVDA; query the live asset registry before submitting an order. Custody uses raw token units; displayed shares use the live ERC-8056 multiplier. Token decimals are not interchangeable. Mainnet does not mint test stock tokens or provide a faucet.

Management/performance rates are immutable per vault, including zero. Admin-controlled entry/exit fees are capped at 1%/2%, and the initial protocol share of manager fees is 10%. Verify current fees and signed fee ceilings. The temporary administrator is EOA `0x5387284206D648afE82240d2E303c20dc402D022`; it is not a Safe and independent audit is not complete.

## Beta execution policy

Manager orders route through approved 0x/Uniswap execution. Initial turnover caps are 1,000 USDG per vault and 10,000 USDG protocol-wide. The engine counts current plus previous UTC-day usage conservatively; this is not a deposit cap or a fresh allowance every midnight.

Beta sessions use 09:30–16:00 America/New_York on supported U.S. trading days, subject to holidays and early closes. Execution also requires the on-chain market flag, healthy operational status, valid fresh prices and issuer permissions. The schedule is a beta policy, not a claim that Robinhood Chain only supports these hours. `day` orders expire at UTC midnight of the creation date; `gtc` persists and `gtd` requires `expires_at`.

Preserve a stable `client_order_id`. HTTP202/`pending_new` can mean an already-executed trade awaiting reconciliation; do not create a replacement under a new ID. Cancellation cannot override a broadcast. Poll the existing record and reconcile the receipt. Missing fill amounts are not inferred from a quote.

Raw basket redemption and claims remain independent of the operational-health gate. Failed transfers can become reserved claims. Optional [cash conversion](basket-quotes.md) requires an investor-signed transaction with explicit bounds and can fail independently of raw redemption. Tilt's operational-health gate is not a Chainlink uptime oracle; expiry or incident recovery requires deliberate administrator action.

The testnet Alpaca/IBKR [migration guide](migration-guide.md) retains testnet-specific token deployment and execution assumptions. Do not carry those assumptions into this beta.
