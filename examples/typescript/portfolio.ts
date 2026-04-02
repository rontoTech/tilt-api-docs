/** Production fund-manager API */
const BASE_URL = "https://api.tiltprotocol.com";
const HEADERS = {
  "TILT-API-KEY-ID": "ak_live_YOUR_KEY",
  "TILT-API-SECRET": "sk_live_YOUR_SECRET",
};

interface Account {
  portfolio_value: string;
  cash: string;
}

interface Position {
  symbol: string;
  qty: string;
  market_value: string;
}

async function main() {
  // Account overview
  const accountRes = await fetch(`${BASE_URL}/v1/trading/account`, {
    headers: HEADERS,
  });
  const account: Account = await accountRes.json();
  console.log(`Portfolio: $${account.portfolio_value}  Cash: $${account.cash}`);

  // Positions
  const positionsRes = await fetch(`${BASE_URL}/v1/trading/positions`, {
    headers: HEADERS,
  });
  const positions: Position[] = await positionsRes.json();
  for (const pos of positions) {
    console.log(`  ${pos.symbol}: ${pos.qty} shares ($${pos.market_value})`);
  }
}

main();
