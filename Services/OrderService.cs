using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using TestSignalR.Hubs;
using TestSignalR.Models;
using TestSignalR.Models.Enum;

namespace TestSignalR.Services;

public class OrderService(IHubContext<OrderNotificationHub, IOrderNotificationClient> hubContext) : IOrderService
{ 
    private readonly ConcurrentDictionary<int, Order> _orders = new(
        new[]
        {
            new Order(1,"User A", "Product 1",OrderStatusEnum.Pending,DateTimeOffset.UtcNow),
            new Order(2,"User B", "Order 2",OrderStatusEnum.Shipped,DateTimeOffset.UtcNow),
            new Order(3,"User C", "Order 3",OrderStatusEnum.Delivered,DateTimeOffset.UtcNow)
        }.ToDictionary(o => o.Id));
    public IReadOnlyList<Order> GetAll() => _orders.OrderBy(o => o.Key).Select(o => o.Value).ToList();

    public Order GetOrder(int orderId) => _orders.GetValueOrDefault(orderId, Order.None);

    public Task UpdateOrderStatus(Order order)
    {
        throw new NotImplementedException();
    }
}