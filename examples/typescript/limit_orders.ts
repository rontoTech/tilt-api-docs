/**
 * Tilt Trading API — limit orders (resting GTC / GTD / day).
 *
 * Limits return status "accepted" until the keeper fills or terminal state.
 * Buy fill: quote <= limit_price. Sell fill: quote >= limit_price.
 */
const BASE_URL = "https://api.tiltprotocol.com";
const HEADERS: Record<string, string> = {
  "TILT-API-KEY-ID": "ak_live_YOUR_KEY",
  "TILT-API-SECRET": "sk_live_YOUR_SECRET",
  "Content-Type": "application/json",
};

interface Order {
  id: string;
  status: string;
  filled_avg_price?: string | null;
  tx_hash?: string | null;
  expires_at?: string | null;
  time_in_force?: string;
}

async function pollUntilTerminal(orderId: string, pollMs = 5000): Promise<Order> {
  for (;;) {
    const statusRes = await fetch(`${BASE_URL}/v1/trading/orders/${orderId}`, {
      headers: HEADERS,
    });
    const status: Order = await statusRes.json();
    console.log(`  ${status.id}: status=${status.status}`);
    if (["filled", "canceled", "expired", "rejected"].includes(status.status)) {
      return status;
    }
    await new Promise((r) => setTimeout(r, pollMs));
  }
}

async function main() {
  // GTC limit buy
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
  console.log(`GTC limit buy: ${order.id} (${order.status})`);

  // GTD: expires_at required (ISO-8601)
  const exp = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString();
  const gtdRes = await fetch(`${BASE_URL}/v1/trading/orders`, {
    method: "POST",
    headers: HEADERS,
    body: JSON.stringify({
      symbol: "TSLA",
      qty: "1",
      side: "sell",
      type: "limit",
      limit_price: "500.00",
      time_in_force: "gtd",
      expires_at: exp,
    }),
  });
  const gtd: Order = await gtdRes.json();
  console.log(`GTD limit sell: ${gtd.id} expires_at=${gtd.expires_at}`);

  // Open orders
  const listRes = await fetch(
    `${BASE_URL}/v1/trading/orders?status=open&limit=20`,
    { headers: HEADERS }
  );
  const open = (await listRes.json()) as Order[];
  console.log(`Open orders: ${open.length}`);

  // Optional: await pollUntilTerminal(order.id);
}

main().catch(console.error);
