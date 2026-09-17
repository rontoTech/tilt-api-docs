# tilt-api-docs — Agent Guide

Last verified: 2026-07-06. Public REST API reference for the Tilt Protocol Trading API (base `https://api.tiltprotocol.com`). Plain markdown + `openapi.yaml` — no build step; publishing = git push (rontoTech/tilt-api-docs). **Ground truth is the backend implementation**: `bowstring-backend/src/trading-api.ts`, `auth-api.ts`, `agent-api.ts`. Any backend route/field/error-code change must land here (openapi.yaml + the matching docs page) in the same change set, and usually also in `tilt-protocol-openclaw/SKILL.md` and `tilt-docs`.

## Shape of the API (keep these invariants when documenting)

- Deliberately **Alpaca-compatible**: `TILT-API-KEY-ID`/`TILT-API-SECRET` headers mirror Alpaca's, order lifecycle and field names match so migration is a header swap (`docs/migration-guide.md`).
- Auth: `POST /v1/auth/keys` from an EIP-191 `personal_sign` of an exact multi-line message (byte-for-byte; timestamp must be at or before server time and less than 5 min old — future-dated timestamps are rejected); server verifies signer is the vault's curator or delegate on-chain. Revocation needs a second signed message. Keys are trade-only, bound to one vault; secrets shown once.
- All money/quantity fields are decimal **strings**, never numbers.
- Two namespaces, two error shapes: `/v1/*` → `{code: <8-digit>, message}` (registry in `docs/errors.md` — must match backend); `/api/agents/*` → `{error: string}`.
- Market orders fill synchronously (atomic single swap at oracle prices — no partial fills, no order book); limit orders rest in Redis and fill via a backend keeper poll — **default 30s** (`LIMIT_ORDER_POLL_MS`), and require the vault to have authorized the backend delegate `0xe67B013939D4118333d94B58FAf82936ca7eE978` via `setDelegate`.
- The relayer/nonce guidance in `docs/orders.md` ("Relayer, nonces, and burst market orders") documents a real Apr 2026 production bug and its fix — keep it in sync with `bowstring-backend/src/tx-serializer.ts` behavior.

## Known stale claims to fix on contact

- `docs/trading-guide.md:~118`, `docs/orders.md:~14`, `docs/migration-guide.md:~47` say the limit keeper polls ~5s; the backend default has been 30s since 2026-05-04.
- `docs/assets.md` "7,000+ US equities" vs `tilt-docs/introduction.mdx` "2,000+" — reconcile when touched.

## Useful

- `examples/curl/examples.sh` exercises the main trading flows — account, market/limit orders, order list, positions, asset search (edit KEY/SECRET placeholders first; the cancel call is present but commented out, and the by-id/by-symbol GET endpoints aren't covered).
- `openapi.yaml` carries its own API version, independent of the OpenClaw skill version.
