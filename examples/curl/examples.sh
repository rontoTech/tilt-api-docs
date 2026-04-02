#!/bin/bash
# Tilt Protocol Trading API — curl examples
# Production API host (fund managers): https://api.tiltprotocol.com
# Replace YOUR_KEY and YOUR_SECRET with real values

KEY="ak_live_YOUR_KEY"
SECRET="sk_live_YOUR_SECRET"
BASE="https://api.tiltprotocol.com"

echo "=== Account ==="
curl -s "$BASE/v1/trading/account" -H "TILT-API-KEY-ID: $KEY" -H "TILT-API-SECRET: $SECRET" | jq .

echo "=== Place Market Buy ==="
curl -s -X POST "$BASE/v1/trading/orders" \
  -H "TILT-API-KEY-ID: $KEY" -H "TILT-API-SECRET: $SECRET" \
  -H "Content-Type: application/json" \
  -d '{"symbol":"AAPL","notional":"5000","side":"buy","type":"market","time_in_force":"day"}' | jq .

echo "=== List Orders ==="
curl -s "$BASE/v1/trading/orders?status=all&limit=10" -H "TILT-API-KEY-ID: $KEY" -H "TILT-API-SECRET: $SECRET" | jq .

echo "=== Positions ==="
curl -s "$BASE/v1/trading/positions" -H "TILT-API-KEY-ID: $KEY" -H "TILT-API-SECRET: $SECRET" | jq .

echo "=== Assets (search AAPL) ==="
curl -s "$BASE/v1/trading/assets?q=AAPL" -H "TILT-API-KEY-ID: $KEY" -H "TILT-API-SECRET: $SECRET" | jq .

echo "=== Place Limit Buy (GTC, resting until filled) ==="
curl -s -X POST "$BASE/v1/trading/orders" \
  -H "TILT-API-KEY-ID: $KEY" -H "TILT-API-SECRET: $SECRET" \
  -H "Content-Type: application/json" \
  -d '{"symbol":"TSLA","qty":"5","side":"buy","type":"limit","limit_price":"180.00","time_in_force":"gtc"}' | jq .

echo "=== Place Limit Sell (GTD — requires expires_at ISO-8601) ==="
EXPIRES="$(python3 -c 'from datetime import datetime,timedelta,timezone; print((datetime.now(timezone.utc)+timedelta(days=30)).strftime("%Y-%m-%dT%H:%M:%S.000Z"))')"
curl -s -X POST "$BASE/v1/trading/orders" \
  -H "TILT-API-KEY-ID: $KEY" -H "TILT-API-SECRET: $SECRET" \
  -H "Content-Type: application/json" \
  -d "{\"symbol\":\"MSFT\",\"qty\":\"2\",\"side\":\"sell\",\"type\":\"limit\",\"limit_price\":\"400.00\",\"time_in_force\":\"gtd\",\"expires_at\":\"$EXPIRES\"}" | jq .

echo "=== List Open Orders (resting limits) ==="
curl -s "$BASE/v1/trading/orders?status=open&limit=20" -H "TILT-API-KEY-ID: $KEY" -H "TILT-API-SECRET: $SECRET" | jq .

echo "=== Cancel Order (replace ORDER_ID) ==="
# curl -s -X DELETE "$BASE/v1/trading/orders/ORDER_ID" -H "TILT-API-KEY-ID: $KEY" -H "TILT-API-SECRET: $SECRET" | jq .
