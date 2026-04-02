/** Production fund-manager API */
const BASE_URL = "https://api.tiltprotocol.com";
const HEADERS = {
  "TILT-API-KEY-ID": "ak_live_YOUR_KEY",
  "TILT-API-SECRET": "sk_live_YOUR_SECRET",
  "Content-Type": "application/json",
};

interface Order {
  id: string;
  status: string;
  filled_avg_price?: string;
}

async function main() {
  // Place a limit buy
  const res = await fetch(`${BASE_URL}/v1/trading/orders`, {
    method: "POST",
    headers: HEADERS,
    body: JSON.stringify({
      symbol: "AAPL",
      qty: "10",
      side: "buy",
      type: "limit",
      limit_price: "180.00",
      time_in_force: "gtc",
    }),
  });
  const order: Order = await res.json();
  console.log(`Limit order placed: ${order.id} (status: ${order.status})`);

  // Poll for fill
  while (true) {
    const statusRes = await fetch(`${BASE_URL}/v1/trading/orders/${order.id}`, {
      headers: HEADERS,
    });
    const status: Order = await statusRes.json();
    console.log(`  Status: ${status.status}`);

    if (["filled", "canceled", "expired", "rejected"].includes(status.status)) {
      console.log(`Final: ${status.status} @ $${status.filled_avg_price ?? "N/A"}`);
      break;
    }

    await new Promise((resolve) => setTimeout(resolve, 5000));
  }
}

main();
