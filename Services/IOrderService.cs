using TestSignalR.Models;
using TestSignalR.Models.Enum;

namespace TestSignalR.Services;

public interface IOrderService
{
    IReadOnlyList<Order> GetAll();
    IReadOnlyList<Order> GetForUser(string userId);
    Order GetOrder(int id);
    Task<Order> UpdateOrderStatus(int orderId, OrderStatusEnum status);
}
