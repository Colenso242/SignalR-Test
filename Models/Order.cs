using TestSignalR.Models.Enum;

namespace TestSignalR.Models;

public record Order(int Id, string UserId, string Product, OrderStatusEnum Status, DateTimeOffset UpdatedAt)
{
    public static Order None => NoOrder.Instance;
    public virtual bool IsNone => false;
    public Order OrThrow(Func<Exception> e) => IsNone ? throw e() : this;
};

public sealed record NoOrder: Order
{
    private NoOrder() : base(-1, string.Empty, string.Empty, OrderStatusEnum.None, DateTimeOffset.MinValue) { }
    public override bool IsNone => true;
    public static NoOrder Instance { get; } = new();
    
}