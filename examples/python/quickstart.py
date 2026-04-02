import requests

# Production fund-manager API
BASE_URL = "https://api.tiltprotocol.com"
HEADERS = {
    "TILT-API-KEY-ID": "ak_live_YOUR_KEY",
    "TILT-API-SECRET": "sk_live_YOUR_SECRET",
}

# Place a market buy
order = requests.post(f"{BASE_URL}/v1/trading/orders", headers=HEADERS, json={
    "symbol": "AAPL",
    "notional": "5000",
    "side": "buy",
    "type": "market",
    "time_in_force": "day",
}).json()

print(f"Order {order['id']}: {order['status']} @ ${order.get('filled_avg_price', 'pending')}")
