# Error Handling

**Base URL:** `https://api.tiltprotocol.com`

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| `401` | Authentication required — no API key headers provided |
| `403` | Forbidden — API key is invalid or revoked |
| `404` | Resource not found |
| `422` | Validation error — request body or parameters are invalid |
| `429` | Rate limit exceeded |
| `500` | Internal server error |

---

## Error Response Format

All errors return a JSON body with a numeric `code` and a human-readable `message`:

```json
{
  "code": 42210001,
  "message": "qty or notional is required"
}
```

---

## Error Code Reference

### Authentication (401xx)

| Code | Description |
|------|-------------|
| `40110001` | Authentication required — missing `TILT-API-KEY-ID` or `TILT-API-SECRET` header |

### Authorization (403xx)

| Code | Description |
|------|-------------|
| `40310001` | Invalid API key — key does not exist or has been revoked |

### Validation (422xx)

| Code | Description |
|------|-------------|
| `42210001` | `qty` or `notional` is required |
| `42210002` | `symbol` is required |
| `42210003` | `side` must be `"buy"` or `"sell"` |
| `42210004` | `type` must be `"market"` or `"limit"` |
| `42210005` | `limit_price` is required for limit orders |
| `42210006` | `time_in_force` must be `"day"` or `"gtc"` |
| `42210007` | Insufficient buying power |
| `42210008` | Asset is not tradable (token not deployed) |

### Internal (500xx)

| Code | Description |
|------|-------------|
| `50010001` | Internal server error |
| `50010002` | Oracle price unavailable |
| `50010003` | On-chain transaction failed |
| `50010004` | Vault contract call reverted |
| `50010005` | Order matching engine unavailable |
| `50010006` | Token deployment failed |
| `50010007` | Delegate authorization check failed |
| `50010008` | Position indexer out of sync |
| `50010009` | Database write failed |
| `50010010` | Rate limiter unavailable |
| `50010011` | Upstream dependency timeout |

---

## Handling Errors

```python
import requests

response = requests.post(
    "https://api.tiltprotocol.com/v1/trading/orders",
    headers=HEADERS,
    json={"symbol": "AAPL", "side": "buy", "type": "market", "time_in_force": "day"},
)

if response.status_code != 200:
    error = response.json()
    print(f"Error {error['code']}: {error['message']}")
else:
    order = response.json()
    print(f"Order placed: {order['id']}")
```
