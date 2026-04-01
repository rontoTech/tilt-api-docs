' =============================================================================
' Tilt API — Portfolio Overview (VB.NET)
' =============================================================================
' Fetch account info and display all positions with P&L estimates.
' =============================================================================

Imports System
Imports System.Threading.Tasks

Module Portfolio
    Private Const KEY_ID  As String = "ak_live_YOUR_KEY"
    Private Const SECRET  As String = "sk_live_YOUR_SECRET"
    Private Const API_URL As String = "https://bowstring-backend-production.up.railway.app"

    Async Function Main() As Task
        Using client As New TiltApiClient(KEY_ID, SECRET, API_URL)

            ' --- Account summary ---
            Dim acct = Await client.GetAccountAsync()
            Console.WriteLine("═══════════════════════════════════════")
            Console.WriteLine($" {acct.VaultName} ({acct.VaultSymbol})")
            Console.WriteLine("═══════════════════════════════════════")
            Console.WriteLine($"  Status:     {acct.Status}")
            Console.WriteLine($"  NAV:        ${acct.PortfolioValue}")
            Console.WriteLine($"  Cash:       ${acct.Cash}")
            Console.WriteLine($"  Share Price: ${acct.SharePrice}")
            Console.WriteLine()

            ' --- Positions ---
            Dim positions = Await client.ListPositionsAsync()
            If positions.Count = 0 Then
                Console.WriteLine("  No open positions.")
                Return
            End If

            Console.WriteLine($"  {"Symbol",-8} {"Qty",>12} {"Price",>10} {"Value",>14}")
            Console.WriteLine($"  {New String("─"c, 48)}")

            For Each pos In positions
                Console.WriteLine(
                    $"  {pos.Symbol,-8} {pos.Qty,>12} {("$" & pos.CurrentPrice),>10} {("$" & pos.MarketValue),>14}")
            Next

            Console.WriteLine()

            ' --- Lookup a single position ---
            If positions.Count > 0 Then
                Dim sym = positions(0).Symbol
                Dim single = Await client.GetPositionAsync(sym)
                Console.WriteLine($"Detail for {single.Symbol}: {single.Qty} shares @ ${single.CurrentPrice}")
            End If
        End Using
    End Function
End Module
