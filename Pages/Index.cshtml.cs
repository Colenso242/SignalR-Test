using Microsoft.AspNetCore.Mvc.RazorPages;
using TestSignalR.Models.Enum;
using TestSignalR.ViewModels;

namespace TestSignalR.Pages;

public class IndexModel : PageModel
{
    public ConnectionPanelViewModel Connection { get; } = new("alice");
    public GroupPanelViewModel Group { get; } = new(3);

    // Built from the enum so the dropdown can never offer a value the server can't deserialize.
    public StatusUpdatePanelViewModel StatusUpdate { get; } = new(
        1, Enum.GetValues<OrderStatusEnum>().Where(s => s != OrderStatusEnum.None).ToList());
}
