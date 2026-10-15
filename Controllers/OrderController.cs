using Microsoft.AspNetCore.Mvc;
using TestSignalR.Models;
using TestSignalR.Services;

namespace TestSignalR.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public IReadOnlyList<Order> GetAll() => orderService.GetAll();
    [HttpGet("{orderId:int}")]
    public Order GetOrder(int orderId) => orderService.GetOrder(orderId);
}
