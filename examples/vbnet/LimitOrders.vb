' =============================================================================
' Tilt API — Limit Orders (VB.NET)
' =============================================================================
' Place a GTC limit buy, poll until filled or cancel after timeout.
' =============================================================================

Imports System
Imports System.Threading.Tasks

Module LimitOrders
    Private Const KEY_ID  As String = "ak_live_YOUR_KEY"
    Private Const SECRET  As String = "sk_live_YOUR_SECRET"
    Private Const API_URL As String = "https://bowstring-backend-production.up.railway.app"

    Async Function Main() As Task
        Using client As New TiltApiClient(KEY_ID, SECRET, API_URL)

            ' --- Look up current NVDA price ---
            Dim asset = Await client.GetAssetAsync("NVDA")
            Dim currentPrice = Decimal.Parse(asset.Price)
            Console.WriteLine($"NVDA current price: ${currentPrice:F2}")

            ' --- Place a limit buy 2% below current price ---
            Dim limitPrice = Math.Round(currentPrice * 0.98D, 2)
            Console.WriteLine($"Placing limit buy for 5 shares @ ${limitPrice:F2} (GTC)...")

            Dim order = Await client.PlaceLimitBuyAsync("NVDA", qty:=5D,
                                                         limitPrice:=limitPrice,
                                                         timeInForce:="gtc")
            Console.WriteLine($"Order {order.Id}: {order.Status}")

            ' --- Poll for fill (max 60 seconds for demo) ---
            Console.WriteLine("Waiting for fill...")
            Try
                Dim filled = Await client.WaitForFillAsync(order.Id,
                                                            pollIntervalMs:=5000,
                                                            timeoutMs:=60000)
                Console.WriteLine($"Final status: {filled.Status}")
                If filled.Status = "filled" Then
                    Console.WriteLine($"  Filled @ ${filled.FilledAvgPrice}")
                End If
            Catch ex As TimeoutException
                ' Not filled within 60s — cancel it
                Console.WriteLine("Not filled within 60s, canceling...")
                Dim canceled = Await client.CancelOrderAsync(order.Id)
                Console.WriteLine($"Order {canceled.Id}: {canceled.Status}")
            End Try

            ' --- List all orders ---
            Console.WriteLine()
            Console.WriteLine("Recent orders:")
            Dim orders = Await client.ListOrdersAsync(status:="all", limit:=10)
            For Each o In orders
                Console.WriteLine($"  {o.Id}  {o.Symbol,-6} {o.Side,-4} {o.Type,-6} {o.Status}")
            Next
        End Using
    End Function
End Module
