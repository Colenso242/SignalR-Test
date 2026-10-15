using WebApplication1.Models.Enum;

namespace WebApplication1.Models;

public record Order(int Id, string UserId, string Product, OrderStatusEnum Status, DateTimeOffset UpdatedAt)
{
    public static Order None => NoOrder.Instance;
    public virtual bool IsNone => false;
    public Order OrThrow(Func<Exception> e) => IsNone ? throw e() : this;
};
    
public record NoOrder() : Order(-1, string.Empty, string.Empty, OrderStatusEnum.None, DateTimeOffset.MinValue)
{
    public static NoOrder Instance { get; } = new NoOrder();
}