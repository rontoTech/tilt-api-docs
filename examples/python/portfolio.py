import requests

BASE_URL = "https://api.tiltprotocol.com"
HEADERS = {
    "TILT-API-KEY-ID": "ak_live_YOUR_KEY",
    "TILT-API-SECRET": "sk_live_YOUR_SECRET",
}

# Account overview
account = requests.get(f"{BASE_URL}/v1/trading/account", headers=HEADERS).json()
print(f"Portfolio: ${account['portfolio_value']}  Cash: ${account['cash']}")

# Positions
positions = requests.get(f"{BASE_URL}/v1/trading/positions", headers=HEADERS).json()
for pos in positions:
    print(f"  {pos['symbol']}: {pos['qty']} shares (${pos['market_value']})")
