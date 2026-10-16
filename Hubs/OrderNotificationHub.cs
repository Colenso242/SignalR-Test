using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TestSignalR.Models;
using TestSignalR.Models.Enum;
using TestSignalR.Services;

namespace TestSignalR.Hubs;

[Authorize]
public class OrderNotificationHub(IOrderService orderService, ILogger<OrderNotificationHub> logger) : Hub<IOrderNotificationClient>
{
    public static string OrderGroup(int orderId) => $"Order-{orderId}";
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected: {ConnectionId} (user: {UserId})", Context.ConnectionId, Context.UserIdentifier);
        await Clients.All.SystemMessage($"Client {Context.UserIdentifier} has connected.");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        //implementazione base fa tutto, loggo e basta
        if(exception is null)
        {
            logger.LogInformation("Client disconnected: {ConnectionId} (user: {UserId})", Context.ConnectionId, Context.UserIdentifier);
        }
        else
        {
            logger.LogError(exception, "Connection dropped with {ConnectionId} (user: {UserId})", Context.ConnectionId, Context.UserIdentifier);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinOrderGroup(int orderId)
    {
        if (orderService.GetOrder(orderId).IsNone)
            throw new HubException($"Order {orderId} not found.");

        await Groups.AddToGroupAsync(Context.ConnectionId, OrderGroup(orderId));
    }

    public Task LeaveOrderGroup(int orderId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, OrderGroup(orderId));

    public async Task<Order> SendOrderStatusUpdate(OrderStatusUpdateRequest request)
    {
        if (request.Status is OrderStatusEnum.None)
            throw new HubException("Status is required.");

        return (await orderService.UpdateOrderStatus(request.OrderId, request.Status))
            .OrThrow(() => new HubException($"Order {request.OrderId} not found."));
    }
}