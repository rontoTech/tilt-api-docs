"""
Tilt Trading API — limit orders (resting GTC / GTD / day).

Limit orders return status "accepted" immediately. Poll GET /v1/trading/orders/{id}
until filled, canceled, expired, or rejected.

Fill logic (keeper):
  - Buy:  fills when live quote <= limit_price
  - Sell: fills when live quote >= limit_price

Requires: TILT-API-KEY-ID, TILT-API-SECRET, vault delegate authorized for fills.
"""
import time
import uuid
from datetime import datetime, timezone, timedelta

import requests

BASE_URL = "https://api.tiltprotocol.com"
HEADERS = {
    "TILT-API-KEY-ID": "ak_live_YOUR_KEY",
    "TILT-API-SECRET": "sk_live_YOUR_SECRET",
}


def poll_until_terminal(order_id: str, poll_s: float = 5.0):
    while True:
        r = requests.get(f"{BASE_URL}/v1/trading/orders/{order_id}", headers=HEADERS, timeout=30)
        r.raise_for_status()
        status = r.json()
        print(f"  {status['id']}: status={status['status']}")
        if status["status"] in ("filled", "canceled", "expired", "rejected"):
            return status
        time.sleep(poll_s)


def example_gtc_limit_buy():
    """GTC limit buy by share qty."""
    client_order_id = f"gtc-buy-{uuid.uuid4().hex[:8]}"
    r = requests.post(
        f"{BASE_URL}/v1/trading/orders",
        headers=HEADERS,
        json={
            "symbol": "AAPL",
            "qty": "10",
            "side": "buy",
            "type": "limit",
            "limit_price": "180.00",
            "time_in_force": "gtc",
            "client_order_id": client_order_id
        },
        timeout=60,
    )
    r.raise_for_status()
    order = r.json()
    print(f"GTC limit buy placed: {order['id']} (status: {order['status']})")
    return order


def example_notional_limit_buy():
    """Limit buy sized by USD notional."""
    client_order_id = f"notional-buy-{uuid.uuid4().hex[:8]}"
    r = requests.post(
        f"{BASE_URL}/v1/trading/orders",
        headers=HEADERS,
        json={
            "symbol": "MSFT",
            "notional": "2500.00",
            "side": "buy",
            "type": "limit",
            "limit_price": "350.00",
            "time_in_force": "gtc",
            "client_order_id": client_order_id
        },
        timeout=60,
    )
    r.raise_for_status()
    return r.json()


def example_gtd_limit_sell():
    """GTD requires expires_at (ISO-8601)."""
    exp = (datetime.now(timezone.utc) + timedelta(days=30)).strftime("%Y-%m-%dT%H:%M:%S.000Z")
    client_order_id = f"gtd-sell-{uuid.uuid4().hex[:8]}"
    r = requests.post(
        f"{BASE_URL}/v1/trading/orders",
        headers=HEADERS,
        json={
            "symbol": "NVDA",
            "qty": "2",
            "side": "sell",
            "type": "limit",
            "limit_price": "999.00",
            "time_in_force": "gtd",
            "expires_at": exp,
            "client_order_id": client_order_id
        },
        timeout=60,
    )
    r.raise_for_status()
    return r.json()


def list_open_orders():
    r = requests.get(
        f"{BASE_URL}/v1/trading/orders",
        headers=HEADERS,
        params={"status": "open", "limit": 20},
        timeout=30,
    )
    r.raise_for_status()
    return r.json()


if __name__ == "__main__":
    order = example_gtc_limit_buy()
    # Optional: wait for terminal state
    # final = poll_until_terminal(order["id"])
    # print(f"Final: {final['status']} avg={final.get('filled_avg_price')} tx={final.get('tx_hash')}")

    open_orders = list_open_orders()
    print(f"Open orders: {len(open_orders)}")

    # Uncomment to try other shapes:
    # print("notional:", example_notional_limit_buy()["id"])
    # print("gtd:", example_gtd_limit_sell()["id"])
