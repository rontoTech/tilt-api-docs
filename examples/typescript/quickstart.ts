const BASE_URL = "https://api.tiltprotocol.com";
const HEADERS = {
  "TILT-API-KEY-ID": "ak_live_YOUR_KEY",
  "TILT-API-SECRET": "sk_live_YOUR_SECRET",
  "Content-Type": "application/json",
};

async function main() {
  const res = await fetch(`${BASE_URL}/v1/trading/orders`, {
    method: "POST",
    headers: HEADERS,
    body: JSON.stringify({
      symbol: "AAPL",
      notional: "5000",
      side: "buy",
      type: "market",
      time_in_force: "day",
    }),
  });
  const order = await res.json();
  console.log(`Order ${order.id}: ${order.status} @ $${order.filled_avg_price ?? "pending"}`);
}

main();
