# Tilt API — VB.NET Examples

Drop-in VB.NET client for the Tilt Protocol Trading API, designed for fund managers familiar with Interactive Brokers TWS API.

## Setup

### .NET 6+ (recommended)

```bash
dotnet new console -lang VB -o TiltTrading
cd TiltTrading
```

Copy `TiltApiClient.vb` into the project, then copy whichever example you want to run into `Program.vb`.

### .NET Framework 4.7.2+

Add the `System.Text.Json` NuGet package:

```
Install-Package System.Text.Json
```

Then add `TiltApiClient.vb` to your project.

## Files

| File | Description |
|------|-------------|
| `TiltApiClient.vb` | Full API client — add this to any VB.NET project |
| `QuickStart.vb` | Place a market buy and check account |
| `Portfolio.vb` | Display account info and all positions |
| `LimitOrders.vb` | GTC limit buy — poll for fill or cancel on timeout ([limit orders guide](../../docs/trading-guide.md)) |
| `SearchAndTrade.vb` | Search stocks and buy unlisted tokens (auto-deploy) |

## Quick Example

Default production host is `https://api.tiltprotocol.com` (omit the third constructor argument). Override only for staging.

```vb
Dim client As New TiltApiClient("ak_live_...", "sk_live_...")

' Place a $5,000 market buy
Dim order = Await client.PlaceMarketBuyAsync("AAPL", 5000D)
Console.WriteLine($"Filled @ ${order.FilledAvgPrice}")

' Check positions
Dim positions = Await client.ListPositionsAsync()
For Each pos In positions
    Console.WriteLine($"{pos.Symbol}: {pos.Qty} shares")
Next
```

## API Reference

The `TiltApiClient` exposes these methods:

### Orders
- `PlaceMarketBuyAsync(symbol, notional)` — Buy by dollar amount
- `PlaceMarketBuyQtyAsync(symbol, qty)` — Buy by share count
- `PlaceMarketSellAsync(symbol, qty)` — Sell shares
- `PlaceLimitBuyAsync(symbol, qty, limitPrice, …, expiresAtIso8601)` — Resting limit buy (`gtc`/`day`/`gtd`; pass `expiresAtIso8601` when `time_in_force` is `gtd`)
- `PlaceLimitSellAsync(symbol, qty, limitPrice, …, expiresAtIso8601)` — Resting limit sell
- `PlaceOrderAsync(..., expiresAtIso8601)` — Generic placement; use `notional` or `qty`, and `expires_at` for GTD
- `ListOrdersAsync(status, limit)` — List orders
- `GetOrderAsync(orderId)` — Get single order
- `CancelOrderAsync(orderId)` — Cancel open order
- `WaitForFillAsync(orderId, pollIntervalMs, timeoutMs)` — Poll until terminal state

### Positions
- `ListPositionsAsync()` — All open positions
- `GetPositionAsync(symbol)` — Single position

### Account
- `GetAccountAsync()` — NAV, cash, share price

### Assets
- `ListAssetsAsync(query, limit)` — Search / list stocks
- `GetAssetAsync(symbol)` — Single stock detail

## Migrating from Interactive Brokers

| IB TWS API | Tilt API |
|------------|----------|
| `EClientSocket.placeOrder()` | `client.PlaceMarketBuyAsync()` |
| `EWrapper.orderStatus()` | `client.GetOrderAsync()` or `WaitForFillAsync()` |
| `EClientSocket.reqPositions()` | `client.ListPositionsAsync()` |
| `EWrapper.accountSummary()` | `client.GetAccountAsync()` |
| `EClientSocket.reqContractDetails()` | `client.GetAssetAsync()` |

Key differences:
- **No socket connection** — pure REST/HTTP, no TWS gateway needed
- **No contract objects** — just use ticker symbols like `"AAPL"`
- **Async/Await** — all methods are async (no callback events)
- **Auto-deploy** — stocks not yet on-chain are deployed automatically on first trade
- **Limit orders** — `POST` returns `accepted` until filled; see [Trading guide](../../docs/trading-guide.md) for GTC/GTD/day, fill rules, and delegate setup
