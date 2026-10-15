using TestSignalR.Models;

namespace TestSignalR.Services;

public interface IOrderService
{
    IReadOnlyList<Order> GetAll();
    Order GetOrder(int id);
    Task UpdateOrderStatus(Order order);
}