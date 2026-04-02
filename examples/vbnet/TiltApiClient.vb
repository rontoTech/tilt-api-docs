' =============================================================================
' Tilt Protocol Trading API — VB.NET Client
' =============================================================================
' Drop-in REST client for fund managers familiar with Interactive Brokers TWS
' API conventions.  Uses System.Net.Http (built into .NET 4.5+ / .NET Core).
'
' Usage:
'   Dim client As New TiltApiClient("ak_live_...", "sk_live_...")
'   Dim order  As OrderResult = Await client.PlaceMarketBuyAsync("AAPL", notional:=5000D)
' =============================================================================

Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Threading.Tasks

' ---------------------------------------------------------------------------
' Data models
' ---------------------------------------------------------------------------

Public Class TiltOrder
    <JsonPropertyName("id")>
    Public Property Id As String

    <JsonPropertyName("client_order_id")>
    Public Property ClientOrderId As String

    <JsonPropertyName("symbol")>
    Public Property Symbol As String

    <JsonPropertyName("asset_id")>
    Public Property AssetId As String

    <JsonPropertyName("qty")>
    Public Property Qty As String

    <JsonPropertyName("notional")>
    Public Property Notional As String

    <JsonPropertyName("filled_qty")>
    Public Property FilledQty As String

    <JsonPropertyName("filled_avg_price")>
    Public Property FilledAvgPrice As String

    <JsonPropertyName("side")>
    Public Property Side As String

    <JsonPropertyName("type")>
    Public Property Type As String

    <JsonPropertyName("time_in_force")>
    Public Property TimeInForce As String

    <JsonPropertyName("limit_price")>
    Public Property LimitPrice As String

    <JsonPropertyName("status")>
    Public Property Status As String

    <JsonPropertyName("created_at")>
    Public Property CreatedAt As String

    <JsonPropertyName("updated_at")>
    Public Property UpdatedAt As String

    <JsonPropertyName("filled_at")>
    Public Property FilledAt As String

    <JsonPropertyName("tx_hash")>
    Public Property TxHash As String
End Class

Public Class TiltPosition
    <JsonPropertyName("symbol")>
    Public Property Symbol As String

    <JsonPropertyName("asset_id")>
    Public Property AssetId As String

    <JsonPropertyName("qty")>
    Public Property Qty As String

    <JsonPropertyName("market_value")>
    Public Property MarketValue As String

    <JsonPropertyName("current_price")>
    Public Property CurrentPrice As String

    <JsonPropertyName("avg_entry_price")>
    Public Property AvgEntryPrice As String

    <JsonPropertyName("side")>
    Public Property Side As String

    <JsonPropertyName("exchange")>
    Public Property Exchange As String
End Class

Public Class TiltAccount
    <JsonPropertyName("id")>
    Public Property Id As String

    <JsonPropertyName("account_number")>
    Public Property AccountNumber As String

    <JsonPropertyName("status")>
    Public Property Status As String

    <JsonPropertyName("currency")>
    Public Property Currency As String

    <JsonPropertyName("cash")>
    Public Property Cash As String

    <JsonPropertyName("portfolio_value")>
    Public Property PortfolioValue As String

    <JsonPropertyName("share_price")>
    Public Property SharePrice As String

    <JsonPropertyName("shares_outstanding")>
    Public Property SharesOutstanding As String

    <JsonPropertyName("vault_name")>
    Public Property VaultName As String

    <JsonPropertyName("vault_symbol")>
    Public Property VaultSymbol As String

    <JsonPropertyName("buying_power")>
    Public Property BuyingPower As String
End Class

Public Class TiltAsset
    <JsonPropertyName("id")>
    Public Property Id As String

    <JsonPropertyName("symbol")>
    Public Property Symbol As String

    <JsonPropertyName("name")>
    Public Property Name As String

    <JsonPropertyName("status")>
    Public Property Status As String

    <JsonPropertyName("tradable")>
    Public Property Tradable As Boolean

    <JsonPropertyName("price")>
    Public Property Price As String

    <JsonPropertyName("exchange")>
    Public Property Exchange As String

    <JsonPropertyName("class")>
    Public Property AssetClass As String
End Class

Public Class TiltApiError
    <JsonPropertyName("code")>
    Public Property Code As Integer

    <JsonPropertyName("message")>
    Public Property Message As String
End Class

Public Class TiltApiException
    Inherits Exception

    Public Property StatusCode As Integer
    Public Property ErrorCode As Integer

    Public Sub New(statusCode As Integer, errorCode As Integer, message As String)
        MyBase.New(message)
        Me.StatusCode = statusCode
        Me.ErrorCode = errorCode
    End Sub
End Class

' ---------------------------------------------------------------------------
' API client
' ---------------------------------------------------------------------------

Public Class TiltApiClient
    Implements IDisposable

    Private ReadOnly _http As HttpClient
    Private ReadOnly _baseUrl As String
    Private ReadOnly _jsonOpts As JsonSerializerOptions
    Private _disposed As Boolean = False

    ''' <summary>
    ''' Create a new Tilt API client.
    ''' </summary>
    ''' <param name="keyId">API Key ID (starts with ak_live_)</param>
    ''' <param name="secret">API Secret (starts with sk_live_)</param>
    ''' <param name="baseUrl">Override the API base URL (default: https://api.tiltprotocol.com — no trailing slash)</param>
    Public Sub New(keyId As String, secret As String,
                   Optional baseUrl As String = "https://api.tiltprotocol.com")
        _baseUrl = baseUrl.TrimEnd("/"c)
        _jsonOpts = New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True,
            .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        }

        _http = New HttpClient()
        _http.DefaultRequestHeaders.Add("TILT-API-KEY-ID", keyId)
        _http.DefaultRequestHeaders.Add("TILT-API-SECRET", secret)
        _http.DefaultRequestHeaders.Accept.Add(
            New MediaTypeWithQualityHeaderValue("application/json"))
    End Sub

    ' ===================================================================
    ' Orders
    ' ===================================================================

    ''' <summary>
    ''' Place a market buy order using a dollar amount.
    ''' </summary>
    Public Async Function PlaceMarketBuyAsync(symbol As String, notional As Decimal,
            Optional timeInForce As String = "day",
            Optional clientOrderId As String = Nothing) As Task(Of TiltOrder)
        Return Await PlaceOrderAsync(symbol, "buy", "market", timeInForce,
                                     notional:=notional.ToString("F2"),
                                     clientOrderId:=clientOrderId)
    End Function

    ''' <summary>
    ''' Place a market buy order using a share quantity.
    ''' </summary>
    Public Async Function PlaceMarketBuyQtyAsync(symbol As String, qty As Decimal,
            Optional timeInForce As String = "day",
            Optional clientOrderId As String = Nothing) As Task(Of TiltOrder)
        Return Await PlaceOrderAsync(symbol, "buy", "market", timeInForce,
                                     qty:=qty.ToString(),
                                     clientOrderId:=clientOrderId)
    End Function

    ''' <summary>
    ''' Place a market sell order using a share quantity.
    ''' </summary>
    Public Async Function PlaceMarketSellAsync(symbol As String, qty As Decimal,
            Optional timeInForce As String = "day",
            Optional clientOrderId As String = Nothing) As Task(Of TiltOrder)
        Return Await PlaceOrderAsync(symbol, "sell", "market", timeInForce,
                                     qty:=qty.ToString(),
                                     clientOrderId:=clientOrderId)
    End Function

    ''' <summary>
    ''' Place a limit buy order.
    ''' </summary>
    Public Async Function PlaceLimitBuyAsync(symbol As String, qty As Decimal,
            limitPrice As Decimal,
            Optional timeInForce As String = "gtc",
            Optional clientOrderId As String = Nothing) As Task(Of TiltOrder)
        Return Await PlaceOrderAsync(symbol, "buy", "limit", timeInForce,
                                     qty:=qty.ToString(),
                                     limitPrice:=limitPrice.ToString("F2"),
                                     clientOrderId:=clientOrderId)
    End Function

    ''' <summary>
    ''' Place a limit sell order.
    ''' </summary>
    Public Async Function PlaceLimitSellAsync(symbol As String, qty As Decimal,
            limitPrice As Decimal,
            Optional timeInForce As String = "gtc",
            Optional clientOrderId As String = Nothing) As Task(Of TiltOrder)
        Return Await PlaceOrderAsync(symbol, "sell", "limit", timeInForce,
                                     qty:=qty.ToString(),
                                     limitPrice:=limitPrice.ToString("F2"),
                                     clientOrderId:=clientOrderId)
    End Function

    ''' <summary>
    ''' Generic order placement. Prefer the convenience methods above.
    ''' </summary>
    Public Async Function PlaceOrderAsync(symbol As String, side As String,
            orderType As String, timeInForce As String,
            Optional qty As String = Nothing,
            Optional notional As String = Nothing,
            Optional limitPrice As String = Nothing,
            Optional clientOrderId As String = Nothing) As Task(Of TiltOrder)

        Dim body As New Dictionary(Of String, String) From {
            {"symbol", symbol.ToUpper()},
            {"side", side},
            {"type", orderType},
            {"time_in_force", timeInForce}
        }
        If qty IsNot Nothing Then body("qty") = qty
        If notional IsNot Nothing Then body("notional") = notional
        If limitPrice IsNot Nothing Then body("limit_price") = limitPrice
        If clientOrderId IsNot Nothing Then body("client_order_id") = clientOrderId

        Return Await PostAsync(Of TiltOrder)("/v1/trading/orders", body)
    End Function

    ''' <summary>
    ''' List orders with optional status filter.
    ''' </summary>
    Public Async Function ListOrdersAsync(
            Optional status As String = "all",
            Optional limit As Integer = 50) As Task(Of List(Of TiltOrder))
        Return Await GetAsync(Of List(Of TiltOrder))(
            $"/v1/trading/orders?status={status}&limit={limit}")
    End Function

    ''' <summary>
    ''' Get a single order by ID.
    ''' </summary>
    Public Async Function GetOrderAsync(orderId As String) As Task(Of TiltOrder)
        Return Await GetAsync(Of TiltOrder)($"/v1/trading/orders/{orderId}")
    End Function

    ''' <summary>
    ''' Cancel an open order.
    ''' </summary>
    Public Async Function CancelOrderAsync(orderId As String) As Task(Of TiltOrder)
        Return Await DeleteAsync(Of TiltOrder)($"/v1/trading/orders/{orderId}")
    End Function

    ''' <summary>
    ''' Wait for an order to reach a terminal state (filled, canceled, expired, rejected).
    ''' </summary>
    Public Async Function WaitForFillAsync(orderId As String,
            Optional pollIntervalMs As Integer = 3000,
            Optional timeoutMs As Integer = 120000) As Task(Of TiltOrder)
        Dim sw = Diagnostics.Stopwatch.StartNew()
        Dim terminalStates = {"filled", "canceled", "expired", "rejected"}

        While sw.ElapsedMilliseconds < timeoutMs
            Dim order = Await GetOrderAsync(orderId)
            If terminalStates.Contains(order.Status) Then Return order
            Await Task.Delay(pollIntervalMs)
        End While

        Throw New TimeoutException($"Order {orderId} did not fill within {timeoutMs}ms")
    End Function

    ' ===================================================================
    ' Positions
    ' ===================================================================

    ''' <summary>
    ''' List all open positions in the vault.
    ''' </summary>
    Public Async Function ListPositionsAsync() As Task(Of List(Of TiltPosition))
        Return Await GetAsync(Of List(Of TiltPosition))("/v1/trading/positions")
    End Function

    ''' <summary>
    ''' Get a single position by ticker symbol.
    ''' </summary>
    Public Async Function GetPositionAsync(symbol As String) As Task(Of TiltPosition)
        Return Await GetAsync(Of TiltPosition)($"/v1/trading/positions/{symbol.ToUpper()}")
    End Function

    ' ===================================================================
    ' Account
    ' ===================================================================

    ''' <summary>
    ''' Get vault account info (NAV, cash, share price).
    ''' </summary>
    Public Async Function GetAccountAsync() As Task(Of TiltAccount)
        Return Await GetAsync(Of TiltAccount)("/v1/trading/account")
    End Function

    ' ===================================================================
    ' Assets
    ' ===================================================================

    ''' <summary>
    ''' Search / list tradable stocks.
    ''' </summary>
    Public Async Function ListAssetsAsync(
            Optional query As String = "",
            Optional limit As Integer = 50) As Task(Of List(Of TiltAsset))
        Dim url = $"/v1/trading/assets?limit={limit}"
        If Not String.IsNullOrEmpty(query) Then url &= $"&q={Uri.EscapeDataString(query)}"
        Return Await GetAsync(Of List(Of TiltAsset))(url)
    End Function

    ''' <summary>
    ''' Get details for a single stock.
    ''' </summary>
    Public Async Function GetAssetAsync(symbol As String) As Task(Of TiltAsset)
        Return Await GetAsync(Of TiltAsset)($"/v1/trading/assets/{symbol.ToUpper()}")
    End Function

    ' ===================================================================
    ' HTTP helpers
    ' ===================================================================

    Private Async Function GetAsync(Of T)(path As String) As Task(Of T)
        Dim resp = Await _http.GetAsync(_baseUrl & path)
        Return Await HandleResponseAsync(Of T)(resp)
    End Function

    Private Async Function PostAsync(Of T)(path As String, body As Object) As Task(Of T)
        Dim json = JsonSerializer.Serialize(body, _jsonOpts)
        Dim content As New StringContent(json, Encoding.UTF8, "application/json")
        Dim resp = Await _http.PostAsync(_baseUrl & path, content)
        Return Await HandleResponseAsync(Of T)(resp)
    End Function

    Private Async Function DeleteAsync(Of T)(path As String) As Task(Of T)
        Dim resp = Await _http.DeleteAsync(_baseUrl & path)
        Return Await HandleResponseAsync(Of T)(resp)
    End Function

    Private Async Function HandleResponseAsync(Of T)(resp As HttpResponseMessage) As Task(Of T)
        Dim body = Await resp.Content.ReadAsStringAsync()

        If Not resp.IsSuccessStatusCode Then
            Dim errCode = 0
            Dim errMsg = $"HTTP {CInt(resp.StatusCode)}: {body}"
            Try
                Dim apiErr = JsonSerializer.Deserialize(Of TiltApiError)(body, _jsonOpts)
                If apiErr IsNot Nothing Then
                    errCode = apiErr.Code
                    errMsg = apiErr.Message
                End If
            Catch
                ' body wasn't JSON — use the raw text
            End Try
            Throw New TiltApiException(CInt(resp.StatusCode), errCode, errMsg)
        End If

        Return JsonSerializer.Deserialize(Of T)(body, _jsonOpts)
    End Function

    ' ===================================================================
    ' IDisposable
    ' ===================================================================

    Public Sub Dispose() Implements IDisposable.Dispose
        If Not _disposed Then
            _http.Dispose()
            _disposed = True
        End If
    End Sub
End Class
