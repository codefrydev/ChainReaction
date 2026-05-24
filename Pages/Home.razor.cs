using Microsoft.AspNetCore.Components;

namespace ChainReaction.Pages
{
    public partial class Home
    {
        [Inject] public NavigationManager Manager { get; set; } = null!;

        bool showReleasePopup = true;

        void StartGame() => Manager.NavigateTo("Game");

        void OnReleasePopupChanged(bool isOpen) => showReleasePopup = isOpen;

        void DismissReleasePopup() => showReleasePopup = false;
    }
}
