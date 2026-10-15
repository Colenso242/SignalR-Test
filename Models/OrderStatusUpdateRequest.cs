using TestSignalR.Models.Enum;

namespace TestSignalR.Models;

public record OrderStatusUpdateRequest(int OrderId, OrderStatusEnum Status);
