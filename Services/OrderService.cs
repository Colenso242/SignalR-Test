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
            new Order(1,"User A", "Order 1",OrderStatusEnum.Pending,DateTimeOffset.UtcNow),
            new Order(2,"User B", "Order 2",OrderStatusEnum.Shipped,DateTimeOffset.UtcNow),
            new Order(3,"User C", "Order 3",OrderStatusEnum.Delivered,DateTimeOffset.UtcNow)
        }.ToDictionary(o => o.Id));
    public IReadOnlyList<Order> GetAll() => _orders.OrderBy(o => o.Key).Select(o => o.Value).ToList();

    public IReadOnlyList<Order> GetForUser(string userId) =>
        _orders.Values.Where(o => o.UserId == userId).OrderBy(o => o.Id).ToList();

    public Order GetOrder(int orderId) => _orders.GetValueOrDefault(orderId, Order.None);

    public async Task<Order> UpdateOrderStatus(int orderId, OrderStatusEnum status)
    {
        if (!_orders.TryGetValue(orderId, out var existing))
            return Order.None;

        var updated = existing with { Status = status, UpdatedAt = DateTimeOffset.UtcNow };
        _orders[orderId] = updated;

        //owner notificato su tutte le sue connessioni, chi osserva l'ordine tramite il gruppo
        await hubContext.Clients.User(updated.UserId).OrderStatusUpdated(updated);
        await hubContext.Clients.Group(OrderNotificationHub.OrderGroup(orderId)).OrderStatusUpdated(updated);

        return updated;
    }
}