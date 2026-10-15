using TestSignalR.Models;

namespace TestSignalR.Hubs;

public interface IOrderNotificationClient
{
    //nomi task devono matchare con js connection.on
    Task OrderStatusUpdated(Order o);
    Task SystemMessage(string msg);
}