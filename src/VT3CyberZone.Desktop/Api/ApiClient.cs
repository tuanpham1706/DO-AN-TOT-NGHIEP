using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace VT3CyberZone.Desktop.Api;

internal sealed class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private string? _token;

    private ApiClient()
    {
        var addressFile = Path.Combine(AppContext.BaseDirectory, "ServerAddress.txt");
        if (!File.Exists(addressFile))
        {
            throw new InvalidOperationException(
                $"Không tìm thấy file địa chỉ server: {addressFile}");
        }

        var address = File.ReadAllText(addressFile).Trim();
        if (!Uri.TryCreate(address, UriKind.Absolute, out var baseAddress)
            || (baseAddress.Scheme != Uri.UriSchemeHttp && baseAddress.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "Địa chỉ server trong ServerAddress.txt không hợp lệ. Hãy dùng dạng http://192.168.1.40:5121.");
        }

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress.ToString().TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public static ApiClient Instance { get; } = new();
    public UserDto? CurrentUser { get; private set; }

    public async Task LoginAsync(string username, string password)
    {
        var result = await SendAsync<LoginResponse>(
            HttpMethod.Post,
            "api/auth/login",
            new LoginRequest(username, password),
            authenticated: false);

        _token = result.Token;
        CurrentUser = result.User;
    }

    public Task<ShopRegistrationResult> RegisterShopAsync(
        string shopName,
        string ownerName,
        string username,
        string email,
        string? phone,
        string password)
        => SendAsync<ShopRegistrationResult>(
            HttpMethod.Post,
            "api/shops/register",
            new RegisterCafeShopRequest(shopName, ownerName, username, email, phone, password),
            authenticated: false);

    public Task<ShopActivationResponse> ActivateShopAsync()
        => SendAsync<ShopActivationResponse>(
            HttpMethod.Post,
            "api/shops/activate",
            null);

    public async Task<MemberPage> GetMembersAsync(string? search)
    {
        // Luôn truyền search lên server để lọc server-side, tránh bị cắt bởi pageSize
        var query = "api/members?page=1&pageSize=100";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search.Trim())}";
        }

        return await SendAsync<MemberPage>(HttpMethod.Get, query);
    }

    public Task<TransactionPage> GetTransactionsAsync(DateTime from, DateTime to, int page = 1, int pageSize = 100)
    {
        var query = $"api/transactions?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&page={page}&pageSize={pageSize}";
        return SendAsync<TransactionPage>(HttpMethod.Get, query);
    }

    public Task<RevenueResponse> GetRevenueAsync(DateTime from, DateTime to)
    {
        var query = $"api/reports/revenue?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        return SendAsync<RevenueResponse>(HttpMethod.Get, query);
    }

    public Task<CreatedMember> CreateMemberAsync(string username, string password, string? fullName, string? phone)
        => SendAsync<CreatedMember>(
            HttpMethod.Post,
            "api/members",
            new CreateMemberRequest(username, password, fullName, phone));

    public Task<List<WorkstationDto>> GetWorkstationsAsync(string? search = null, string? status = null)
    {
        var query = "api/workstations";
        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add($"status={Uri.EscapeDataString(status.Trim())}");
        }

        if (filters.Count > 0)
        {
            query += "?" + string.Join("&", filters);
        }

        return SendAsync<List<WorkstationDto>>(HttpMethod.Get, query);
    }

    public Task<OpenSessionResponse> OpenSessionAsync(int workstationId, int? memberId)
        => SendAsync<OpenSessionResponse>(
            HttpMethod.Post,
            "api/sessions/open",
            new OpenSessionRequest(workstationId, memberId));

    public Task<CloseSessionResponse> CloseSessionAsync(int sessionId)
        => SendAsync<CloseSessionResponse>(
            HttpMethod.Post,
            $"api/sessions/{sessionId}/close",
            null);

    public Task<TopUpResponse> TopUpAsync(int memberId, decimal amount)
        => SendAsync<TopUpResponse>(
            HttpMethod.Post,
            $"api/members/{memberId}/topup",
            new TopUpRequest(amount, null));

    public Task<WorkstationStatusResponse> UpdateWorkstationStatusAsync(int workstationId, string status)
        => SendAsync<WorkstationStatusResponse>(
            HttpMethod.Patch,
            $"api/workstations/{workstationId}/status",
            new UpdateWorkstationStatusRequest(status));

    public Task<WorkstationStatusResponse> RestartWorkstationAsync(int workstationId)
        => SendAsync<WorkstationStatusResponse>(
            HttpMethod.Post,
            $"api/workstations/{workstationId}/restart",
            null);

    public void Logout()
    {
        _token = null;
        CurrentUser = null;
    }

    private async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body = null,
        bool authenticated = true)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        if (authenticated)
        {
            if (string.IsNullOrWhiteSpace(_token))
            {
                throw new ApiException(HttpStatusCode.Unauthorized, "Vui lòng đăng nhập lại.");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        using var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var message = await ReadErrorMessageAsync(response);
            if (string.IsNullOrWhiteSpace(message))
            {
                message = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Phiên đăng nhập hết hạn hoặc tài khoản không hợp lệ. Hãy đăng nhập lại.",
                    HttpStatusCode.Forbidden => "Tài khoản không có quyền thực hiện thao tác này.",
                    HttpStatusCode.NotFound => "Không tìm thấy dữ liệu yêu cầu trên server.",
                    HttpStatusCode.Conflict => "Tên đăng nhập đã tồn tại trên server.",
                    _ => $"Server trả về lỗi HTTP {(int)response.StatusCode}."
                };
            }
            throw new ApiException(response.StatusCode, message);
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Server trả về dữ liệu rỗng hoặc sai định dạng.");
    }

    private static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String)
                return message.GetString();
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}

internal sealed class ApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

internal sealed record LoginRequest(string Username, string Password);
internal sealed record LoginResponse(string Token, UserDto User);
internal sealed record UserDto(
    int Id,
    string Username,
    string? FullName,
    string? Phone,
    string Role,
    decimal Balance,
    string Status,
    int CafeShopId);
internal sealed record WorkstationDto(
    int Id,
    string MachineCode,
    string GroupName,
    decimal HourlyRate,
    string IpAddress,
    string Status,
    ActiveSessionDto? CurrentSession);
internal sealed record ActiveSessionDto(
    int SessionId,
    string? MemberName,
    DateTime StartTime,
    int MinutesElapsed,
    decimal CostSoFar,
    decimal MemberBalance);
internal sealed record MemberPage(int Total, List<UserDto> Data);
internal sealed record RegisterCafeShopRequest(
    string ShopName,
    string OwnerName,
    string Username,
    string Email,
    string? Phone,
    string Password);
internal sealed record ShopRegistrationResult(int Id, string Name, string Status, string Message);
internal sealed record ShopActivationResponse(bool Activated, int ShopId, string Name, DateTime? ActivatedAt);
internal sealed record TransactionPage(int Total, List<TransactionDto> Data);
internal sealed record TransactionDto(
    string TransCode,
    DateTime CreatedAt,
    string? MemberName,
    string? StaffName,
    string Type,
    decimal Amount,
    string? OrderCode,
    string? PaymentMethod);
internal sealed record RevenueResponse(
    decimal TotalRevenue,
    decimal GameFeeRevenue,
    decimal FoodRevenue,
    List<DailyRevenueDto> ByDay);
internal sealed record DailyRevenueDto(string Date, decimal GameFee, decimal Food, decimal Total);
internal sealed record CreateMemberRequest(string Username, string Password, string? FullName, string? Phone);
internal sealed record CreatedMember(int Id, string Message);
internal sealed record OpenSessionRequest(int WorkstationId, int? MemberId);
internal sealed record OpenSessionResponse(
    int SessionId,
    string MachineCode,
    string? MemberName,
    decimal Balance,
    int EstimatedMinutes,
    string Message);
internal sealed record CloseSessionResponse(
    int SessionId,
    int TotalMinutes,
    decimal TotalCost,
    decimal RemainingBalance,
    string TransCode);
internal sealed record TopUpRequest(decimal Amount, string? Note);
internal sealed record TopUpResponse(decimal NewBalance, string TransCode, string Message);
internal sealed record UpdateWorkstationStatusRequest(string Status);
internal sealed record WorkstationStatusResponse(string Message, string Status);
