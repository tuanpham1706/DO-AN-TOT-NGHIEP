// DTOs dùng chung cho request/response — tránh expose trực tiếp Entity

namespace VT3CyberZone.Server.DTOs;

// ─── Auth ─────────────────────────────────────────────────────────────────────
public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, UserDto User);

public record UserDto(int Id, string Username, string? FullName, string? Phone, string Role, decimal Balance, string Status, int CafeShopId);

public record RegisterCafeShopRequest(
    string ShopName,
    string OwnerName,
    string Username,
    string Email,
    string? Phone,
    string Password
);

public record PlatformLoginRequest(string Username, string Password);

public record PlatformLoginResponse(string Token, string Username);

public record CafeShopDto(
    int Id,
    string Name,
    string OwnerName,
    string Email,
    string? Phone,
    string Status,
    string? LicenseKey,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    int Downloads,
    int Activations
);

public record RecordSubscriptionPaymentRequest(
    decimal Amount,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateTime? PaidAt,
    string? Note
);

public record SubscriptionPaymentDto(
    int Id,
    int CafeShopId,
    string CafeShopName,
    decimal Amount,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateTime PaidAt,
    string? Note
);

public record SubscriptionRevenueResponse(decimal Total, List<SubscriptionPaymentDto> Payments);

// ─── Member ───────────────────────────────────────────────────────────────────
public record CreateMemberRequest(string Username, string Password, string? FullName, string? Phone);

public record UpdateMemberStatusRequest(string Status);

public record ChangePasswordRequest(string? OldPassword, string NewPassword);

public record TopUpRequest(decimal Amount, string? Note);

public record TopUpResponse(decimal NewBalance, string TransCode, string Message);

// ─── Workstation ──────────────────────────────────────────────────────────────
public record WorkstationDto(
    int Id,
    string MachineCode,
    string GroupName,
    decimal HourlyRate,
    string? IpAddress,
    string Status,
    ActiveSessionDto? CurrentSession
);

public record ActiveSessionDto(
    int SessionId,
    string? MemberName,
    DateTime StartTime,
    int MinutesElapsed,
    decimal CostSoFar,
    decimal MemberBalance
);

// ─── Session ──────────────────────────────────────────────────────────────────
public record CreateWorkstationRequest(string MachineCode, int GroupId, string? IpAddress, string? Status);

public record UpdateWorkstationStatusRequest(string Status);

public record OpenSessionRequest(int WorkstationId, int? MemberId);

public record OpenSessionResponse(
    int SessionId,
    string MachineCode,
    string? MemberName,
    decimal Balance,
    int EstimatedMinutes,
    string Message
);

public record CloseSessionResponse(
    int SessionId,
    int TotalMinutes,
    decimal TotalCost,
    decimal RemainingBalance,
    string TransCode
);

// ─── Food ─────────────────────────────────────────────────────────────────────
public record PlaceOrderRequest(
    int WorkstationId,
    int? UserId,
    string PaymentMethod,
    string? Note,
    List<OrderItemRequest> Items
);

public record OrderItemRequest(int ProductId, int Quantity);

public record PlaceOrderResponse(
    string OrderCode,
    decimal TotalAmount,
    string PaymentMethod,
    decimal? RemainingBalance,
    string Status,
    string Message
);

public record ProductDto(int Id, string Name, decimal Price, string? ImageUrl, bool IsAvailable);

public record CategoryMenuDto(int CategoryId, string CategoryName, List<ProductDto> Products);

// ─── Inventory ────────────────────────────────────────────────────────────────
public record IngredientDto(int Id, string Name, string Unit, decimal StockQuantity, decimal MinThreshold, bool IsLow);

public record RestockRequest(decimal Quantity, string? Note);

// ─── Reports ──────────────────────────────────────────────────────────────────
public record RevenueResponse(
    decimal TotalRevenue,
    decimal GameFeeRevenue,
    decimal FoodRevenue,
    List<DailyRevenueDto> ByDay
);

public record DailyRevenueDto(string Date, decimal GameFee, decimal Food, decimal Total);

public record TransactionDto(
    string TransCode,
    DateTime CreatedAt,
    string? MemberName,
    string? StaffName,
    string Type,
    decimal Amount,
    string? OrderCode,
    string? PaymentMethod
);

public record TransactionPage(int Total, List<TransactionDto> Data);
