using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TestSignalR.Models;
using TestSignalR.Services;

namespace TestSignalR.Hubs;

public class OrderNotificationHub(IOrderService orderService, ILogger<OrderNotificationHub> logger) : Hub<IOrderNotificationClient>
{
    [Authorize]
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
}