' =============================================================================
' Tilt API — Search & Trade (VB.NET)
' =============================================================================
' Search for a stock (even if not yet listed on-chain), and buy it.
' The API auto-deploys unlisted tokens on the first trade.
' =============================================================================

Imports System
Imports System.Threading.Tasks

Module SearchAndTrade
    Private Const KEY_ID As String = "ak_live_YOUR_KEY"
    Private Const SECRET As String = "sk_live_YOUR_SECRET"

    Async Function Main() As Task
        Using client As New TiltApiClient(KEY_ID, SECRET)

            ' --- Search for stocks matching "Tesla" ---
            Console.WriteLine("Searching for 'Tesla'...")
            Dim assets = Await client.ListAssetsAsync(query:="TSLA")
            For Each a In assets
                Dim statusBadge = If(a.Status = "active", "[LISTED]", "[NEW]   ")
                Console.WriteLine($"  {statusBadge} {a.Symbol,-6} {a.Name,-40} ${a.Price}")
            Next
            Console.WriteLine()

            ' --- Buy an unlisted stock — it will be auto-deployed ---
            Console.WriteLine("Buying $500 of RIVN (auto-deploys if not listed)...")
            Try
                Dim order = Await client.PlaceMarketBuyAsync("RIVN", 500D)
                Console.WriteLine($"Order {order.Id}: {order.Status}")
                If order.Status = "filled" Then
                    Console.WriteLine($"  Filled @ ${order.FilledAvgPrice}")
                    Console.WriteLine($"  Tx: {order.TxHash}")
                End If
            Catch ex As TiltApiException
                Console.WriteLine($"Trade failed: {ex.Message}")
            End Try

            ' --- Verify position ---
            Console.WriteLine()
            Try
                Dim pos = Await client.GetPositionAsync("RIVN")
                Console.WriteLine($"RIVN position: {pos.Qty} shares (${pos.MarketValue})")
            Catch ex As TiltApiException When ex.StatusCode = 404
                Console.WriteLine("No RIVN position found (trade may still be processing)")
            End Try
        End Using
    End Function
End Module
