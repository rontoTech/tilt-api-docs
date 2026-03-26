# WebSockets (planned)

Real-time order and fill updates over WebSockets are not available yet. Use polling:

- `GET /v1/trading/orders?status=open` for resting orders
- `GET /v1/trading/orders/:id` for a single order

This document will be updated when a streaming API ships.
