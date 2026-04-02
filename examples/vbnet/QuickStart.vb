' =============================================================================
' Tilt API — Quick Start (VB.NET)
' =============================================================================
' Place a market buy, then check the fill status.
'
' Prerequisites:
'   - .NET 6+ (or .NET Framework 4.7.2+ with System.Text.Json NuGet)
'   - Add TiltApiClient.vb to your project
'
' Run:
'   dotnet run
' =============================================================================

Imports System
Imports System.Threading.Tasks

Module QuickStart
    Private Const KEY_ID As String = "ak_live_YOUR_KEY"
    Private Const SECRET As String = "sk_live_YOUR_SECRET"

    Async Function Main() As Task
        ' Production API: https://api.tiltprotocol.com (default when base URL is omitted)
        Using client As New TiltApiClient(KEY_ID, SECRET)

            ' --- Place a $5,000 market buy on AAPL ---
            Console.WriteLine("Placing $5,000 market buy on AAPL...")
            Dim order = Await client.PlaceMarketBuyAsync("AAPL", 5000D)
            Console.WriteLine($"Order {order.Id}: {order.Status}")

            If order.Status = "filled" Then
                Console.WriteLine($"  Filled @ ${order.FilledAvgPrice}")
                Console.WriteLine($"  Shares: {order.FilledQty}")
                Console.WriteLine($"  Tx: {order.TxHash}")
            End If

            ' --- Check account ---
            Dim acct = Await client.GetAccountAsync()
            Console.WriteLine()
            Console.WriteLine($"Portfolio: ${acct.PortfolioValue}")
            Console.WriteLine($"Cash:      ${acct.Cash}")
        End Using
    End Function
End Module
