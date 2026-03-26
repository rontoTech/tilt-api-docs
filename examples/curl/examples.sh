#!/bin/bash
# Tilt Protocol Trading API — curl examples
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

echo "=== Place Limit Buy ==="
curl -s -X POST "$BASE/v1/trading/orders" \
  -H "TILT-API-KEY-ID: $KEY" -H "TILT-API-SECRET: $SECRET" \
  -H "Content-Type: application/json" \
  -d '{"symbol":"TSLA","qty":"5","side":"buy","type":"limit","limit_price":"180.00","time_in_force":"gtc"}' | jq .

echo "=== Cancel Order (replace ORDER_ID) ==="
# curl -s -X DELETE "$BASE/v1/trading/orders/ORDER_ID" -H "TILT-API-KEY-ID: $KEY" -H "TILT-API-SECRET: $SECRET" | jq .
