using Microsoft.AspNetCore.SignalR;

namespace VT3CyberZone.Server.Hubs;

/// <summary>
/// SignalR Hub — kênh giao tiếp realtime giữa Server và Client C#
/// Client kết nối: ws://192.168.1.10:5000/hubs/cyber
/// </summary>
public class CyberHub : Hub
{
    // Lưu ánh xạ: machineCode -> connectionId
    private static readonly Dictionary<string, string> MachineConnections = [];

    /// <summary>Client gọi khi khởi động app — đăng ký định danh máy</summary>
    public async Task RegisterMachine(string machineCode, string ipAddress)
    {
        MachineConnections[machineCode] = Context.ConnectionId;
        await Groups.AddToGroupAsync(Context.ConnectionId, machineCode);
        Console.WriteLine($"[HUB] Máy {machineCode} ({ipAddress}) đã kết nối: {Context.ConnectionId}");
        await Clients.Caller.SendAsync("ReceiveCommand", new
        {
            cmd = "REGISTER_OK",
            message = $"Máy {machineCode} đã đăng ký thành công"
        });
    }

    /// <summary>Client gửi heartbeat mỗi 30 giây</summary>
    public async Task SendHeartbeat(string machineCode)
    {
        MachineConnections[machineCode] = Context.ConnectionId;
        await Clients.Caller.SendAsync("ReceiveCommand", new { cmd = "PONG" });
    }

    /// <summary>Client gửi tin chat tới quầy thu ngân</summary>
    public async Task SendChatMessage(string machineCode, string message)
    {
        // Gửi tới group "STAFF" (thu ngân)
        await Clients.Group("STAFF").SendAsync("ReceiveCommand", new
        {
            cmd = "CHAT_MESSAGE",
            from = machineCode,
            message
        });
    }

    /// <summary>Client yêu cầu đổi mật khẩu</summary>
    public async Task RequestPasswordChange(string machineCode, int userId)
    {
        await Clients.Group("STAFF").SendAsync("ReceiveCommand", new
        {
            cmd = "PASSWORD_CHANGE_REQUEST",
            machineCode,
            userId
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var machine = MachineConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (machine.Key != null)
        {
            MachineConnections.Remove(machine.Key);
            Console.WriteLine($"[HUB] Máy {machine.Key} đã ngắt kết nối");
        }
        await base.OnDisconnectedAsync(exception);
    }

    // ─── Static methods gọi từ Services ───────────────────────────────────────

    /// <summary>Lấy connectionId của 1 máy trạm theo mã máy</summary>
    public static string? GetConnectionId(string machineCode)
        => MachineConnections.TryGetValue(machineCode, out var id) ? id : null;

    /// <summary>Lấy danh sách máy đang kết nối</summary>
    public static IReadOnlyCollection<string> GetOnlineMachines()
        => MachineConnections.Keys;
}

