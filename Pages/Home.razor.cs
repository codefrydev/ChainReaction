using ChainReaction.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ChainReaction.Pages
{
    public partial class Home
    {
        [Inject] public NavigationManager Manager { get; set; } = null!;
        [Inject] public IJSRuntime JSRuntime { get; set; } = null!;

        private bool showReleasePopup = false;
        private string activeTab = "players"; // "players", "board", "settings"

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    bool hasSeen = await JSRuntime.InvokeAsync<bool>("blazorFunctions.HasSeenReleasePopup");
                    if (!hasSeen)
                    {
                        showReleasePopup = true;
                        StateHasChanged();
                    }
                }
                catch
                {
                    // Fallback
                }
            }
        }

        private async Task DismissReleasePopup()
        {
            showReleasePopup = false;
            try
            {
                await JSRuntime.InvokeVoidAsync("blazorFunctions.SetSeenReleasePopup");
            }
            catch {}
        }

        private void SetPlayerCount(int count)
        {
            Config.NumberOfPlayer = Math.Clamp(count, 2, 8);
            
            // Ensure player list has enough players
            while (Config.PlayerList.Count < Config.NumberOfPlayer)
            {
                int nextIdx = Config.PlayerList.Count;
                var preset = Config.PresetColors[nextIdx % Config.PresetColors.Count];
                Config.PlayerList.Add(new Player
                {
                    Name = $"Player {nextIdx + 1}",
                    RColor = preset.R,
                    GColor = preset.G,
                    BColor = preset.B
                });
            }
            StateHasChanged();
        }

        private void SetBoardPreset(string preset)
        {
            Config.BoardPreset = preset;
            switch (preset)
            {
                case "Quick":
                    Config.IsCusTomDimention = true;
                    Config.Rows = 6;
                    Config.Column = 4;
                    break;
                case "Classic":
                    Config.IsCusTomDimention = true;
                    Config.Rows = 9;
                    Config.Column = 6;
                    break;
                case "Large":
                    Config.IsCusTomDimention = true;
                    Config.Rows = 12;
                    Config.Column = 8;
                    break;
                case "Custom":
                    Config.IsCusTomDimention = true;
                    break;
                default: // Auto
                    Config.IsCusTomDimention = false;
                    break;
            }
            StateHasChanged();
        }

        private void SetPlayerColor(int playerIndex, (string Name, int R, int G, int B) preset)
        {
            if (playerIndex >= 0 && playerIndex < Config.PlayerList.Count)
            {
                var player = Config.PlayerList[playerIndex];
                player.RColor = preset.R;
                player.GColor = preset.G;
                player.BColor = preset.B;
                StateHasChanged();
            }
        }

        private void SetPlayerHexColor(int playerIndex, string hex)
        {
            if (playerIndex >= 0 && playerIndex < Config.PlayerList.Count)
            {
                Config.PlayerList[playerIndex].HexColor = hex;
                StateHasChanged();
            }
        }

        private void ResetPlayer(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < Config.PlayerList.Count)
            {
                var preset = Config.PresetColors[playerIndex % Config.PresetColors.Count];
                var player = Config.PlayerList[playerIndex];
                player.Name = $"Player {playerIndex + 1}";
                player.RColor = preset.R;
                player.GColor = preset.G;
                player.BColor = preset.B;
                StateHasChanged();
            }
        }

        private void StartGame()
        {
            try
            {
                _ = JSRuntime.InvokeVoidAsync("blazorFunctions.PlayPop", 1.2);
            }
            catch {}
            Manager.NavigateTo("Game");
        }
    }
}
