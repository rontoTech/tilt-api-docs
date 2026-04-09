import requests
import uuid

# Production fund-manager API
BASE_URL = "https://api.tiltprotocol.com"
HEADERS = {
    "TILT-API-KEY-ID": "ak_live_YOUR_KEY",
    "TILT-API-SECRET": "sk_live_YOUR_SECRET",
}

# Generate a unique client_order_id for idempotency
client_order_id = f"my-first-trade-{uuid.uuid4().hex[:8]}"

# Place a market buy
order = requests.post(f"{BASE_URL}/v1/trading/orders", headers=HEADERS, json={
    "symbol": "AAPL",
    "notional": "5000",
    "side": "buy",
    "type": "market",
    "time_in_force": "day",
    "client_order_id": client_order_id
}).json()

print(f"Order {order.get('id', 'error')}: {order.get('status', 'failed')} @ ${order.get('filled_avg_price', 'pending')}")
