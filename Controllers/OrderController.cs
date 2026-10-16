using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestSignalR.Models;
using TestSignalR.Models.Enum;
using TestSignalR.Services;

namespace TestSignalR.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public IReadOnlyList<Order> GetAll() => orderService.GetAll();

    [HttpGet("mine")]
    public IReadOnlyList<Order> GetMine() =>
        orderService.GetForUser(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{orderId:int}")]
    public ActionResult<Order> GetOrder(int orderId)
    {
        var order = orderService.GetOrder(orderId);
        return order.IsNone ? NotFound() : order;
    }

    [HttpPut("{orderId:int}/status")]
    public async Task<ActionResult<Order>> UpdateOrderStatus(int orderId, StatusChange change)
    {
        if (change.Status is OrderStatusEnum.None)
            return BadRequest("Status is required.");

        var order = await orderService.UpdateOrderStatus(orderId, change.Status);
        return order.IsNone ? NotFound() : order;
    }
}
