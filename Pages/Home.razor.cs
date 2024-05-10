using ChainReaction.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ChainReaction.Pages
{
    public partial class Home
    {
        [Inject] public NavigationManager Manager { get; set; } = null!;
        void StartGame() => Manager.NavigateTo("Game");
        [Inject] public IDialogService DialogService { get; set; } = null!;
        private void OpenDialog()
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                CloseButton = true, 
            };
            DialogService.Show<Guide>("How To Play/ Rules", options);
        }
    }
}
