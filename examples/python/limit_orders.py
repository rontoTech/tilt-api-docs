import requests
import time

BASE_URL = "https://api.tiltprotocol.com"
HEADERS = {
    "TILT-API-KEY-ID": "ak_live_YOUR_KEY",
    "TILT-API-SECRET": "sk_live_YOUR_SECRET",
}

# Place a limit buy
order = requests.post(f"{BASE_URL}/v1/trading/orders", headers=HEADERS, json={
    "symbol": "AAPL",
    "qty": "10",
    "side": "buy",
    "type": "limit",
    "limit_price": "180.00",
    "time_in_force": "gtc",
}).json()

print(f"Limit order placed: {order['id']} (status: {order['status']})")

# Poll for fill
while True:
    status = requests.get(f"{BASE_URL}/v1/trading/orders/{order['id']}", headers=HEADERS).json()
    print(f"  Status: {status['status']}")
    if status["status"] in ("filled", "canceled", "expired", "rejected"):
        break
    time.sleep(5)

print(f"Final: {status['status']} @ ${status.get('filled_avg_price', 'N/A')}")
