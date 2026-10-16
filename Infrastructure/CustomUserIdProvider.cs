using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace TestSignalR.Infrastructure;

// decide lo UserIdentifier di ogni connessione, così Clients.User(userId) raggiunge tutte le connessioni dell'utente
public class CustomUserIdProvider : IUserIdProvider
{
    //sub viene rimappato su NameIdentifier dal JwtBearer se mapping attivo
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? connection.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
}
