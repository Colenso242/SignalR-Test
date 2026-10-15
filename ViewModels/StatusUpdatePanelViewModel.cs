using TestSignalR.Models.Enum;

namespace TestSignalR.ViewModels;

public record StatusUpdatePanelViewModel(int DefaultOrderId, IReadOnlyList<OrderStatusEnum> Statuses);
