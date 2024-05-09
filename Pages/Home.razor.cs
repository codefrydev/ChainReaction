using Microsoft.AspNetCore.Components;

namespace ChainReaction.Pages
{
    public partial class Home
    {
        [Inject] public NavigationManager Manager { get; set; } = null!;
        void StartGame() => Manager.NavigateTo("Game");
    }
}
