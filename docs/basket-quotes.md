# Mainnet basket and cash quotes

This endpoint belongs to the immutable mainnet release candidate. It is available only after the backend is configured with the deployed basket factory and router. Testnet software. Not financial advice.

`POST /v1/vaults/{vault}/basket-quote` is a public, read-only endpoint. It returns a transaction for the investor to review and sign. It never signs, approves tokens, spends investor funds, or broadcasts a transaction. Requests are limited to ten per minute per client IP.

Use raw integer strings for vault shares and token amounts. For example, `"1000000"` is one unit only when that token uses six decimals. Read each token's decimals; do not assume vault shares have eighteen decimals.

| Field | Meaning |
|---|---|
| `mode` | `deposit` buys the proportional basket, then mints shares. `redeem` redeems shares, then converts the withdrawn basket to cash. |
| `caller` | Investor wallet that will sign. |
| `shares` | Exact net shares to mint, or exact shares to redeem, in raw units. |
| `maxBaseIn` | Required for deposits: maximum base tokens the router may pull, in raw units. Unspent cash is refunded. |
| `minBaseOut` | Required for cash redemption: minimum base tokens returned, in raw units. |
| `maxFeeBps` | Required fee ceiling: 0–100 for deposits, 0–200 for redemption. A lower current fee does not grant permission for a later increase above this ceiling. |
| `slippageBps` | Optional venue slippage setting, 0–50; defaults to 50. |

The response contains `transaction: {to,data,value:"0",chainId}`, an exact `approval: {token,spender,amount}`, the basket hash, fee information, and a Unix-seconds `expiresAt` matching the transaction deadline. Quotes expire within sixty seconds and may expire sooner when the venue quote does.

Verify the configured chain, factory, vault and router locally. Decode the transaction and check the shares, cash bound, fee ceiling and deadline before signing. Approve only the returned exact amount to the verified router. The backend's response does not grant an allowance itself.

`tokenAmounts` contains `{token,amount}` entries. `tokenAmountsKind` is `required` for deposits and `minimum` for cash redemptions. `previewTokenAmounts` contains the unbuffered preview. Cash redemptions reserve a ten-basis-point difference between preview and minimum amounts to accommodate fee accrual before inclusion; swaps sell the minimum amounts. **Any unsold tokens are returned to the investor.** `residualTokensPossible` signals this behavior.

`estimatedBaseAmount` is the total maximum quoted cost for a deposit or the conservative minimum cash proceeds for redemption. A quote that cannot meet the explicit cash bound fails before any transaction is built. Changed holdings, transfer restrictions, fee changes, price changes, or venue liquidity can still make a signed transaction revert.

If cash conversion is unavailable, use the vault's proportional `redeemBasket` path. Frozen token entitlements remain claimable through the vault. Cash conversion availability does not control the raw basket exit.

| HTTP / code | Meaning |
|---|---|
| `422` / `42210020` | Invalid request, unknown vault, fee ceiling exceeded, or cash bound cannot be met. |
| `429` / `42910001` | Quote rate limit reached. |
| `503` / `50310004` | Deployment, quote provider, or required read is unavailable. |

The response is a transaction proposal, not a completed deposit or redemption. Check the investor's transaction receipt for the final result.
