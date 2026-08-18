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

        private void SetGameMode(string mode)
        {
            Config.ApplyGameMode(mode);
            StateHasChanged();
        }

        private void SetGlobalDifficulty(string difficulty)
        {
            Config.DefaultBotDifficulty = difficulty;
            for (int i = 0; i < Config.PlayerList.Count; i++)
            {
                if (Config.PlayerList[i].IsBot)
                {
                    Config.PlayerList[i].BotDifficulty = difficulty;
                }
            }
            StateHasChanged();
        }

        private void TogglePlayerBot(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < Config.PlayerList.Count)
            {
                var player = Config.PlayerList[playerIndex];
                player.IsBot = !player.IsBot;
                if (player.IsBot)
                {
                    player.BotDifficulty = Config.DefaultBotDifficulty;
                    if (string.IsNullOrEmpty(player.Name) || player.Name == $"Player {playerIndex + 1}" || player.Name == "You (P1)")
                    {
                        var botName = playerIndex - 1 >= 0 && playerIndex - 1 < Config.PresetBotNames.Count
                            ? Config.PresetBotNames[playerIndex - 1]
                            : $"Bot {playerIndex + 1}";
                        player.Name = botName;
                    }
                }
                else
                {
                    if (Config.PresetBotNames.Contains(player.Name) || player.Name.StartsWith("Bot "))
                    {
                        player.Name = playerIndex == 0 ? "You (P1)" : $"Player {playerIndex + 1}";
                    }
                }
                StateHasChanged();
            }
        }

        private void SetPlayerBotDifficulty(int playerIndex, string difficulty)
        {
            if (playerIndex >= 0 && playerIndex < Config.PlayerList.Count)
            {
                Config.PlayerList[playerIndex].BotDifficulty = difficulty;
                StateHasChanged();
            }
        }

        private string GetStartButtonText()
        {
            var activePlayers = Config.PlayerList.Take(Config.NumberOfPlayer).ToList();
            int botCount = activePlayers.Count(p => p.IsBot);
            int humanCount = activePlayers.Count - botCount;

            if (humanCount == 1 && botCount == 1)
            {
                var bot = activePlayers.First(p => p.IsBot);
                return $"START MATCH · YOU vs {bot.Name.ToUpper()} ({bot.BotDifficulty.ToUpper()})";
            }
            else if (humanCount > 0 && botCount > 0)
            {
                return $"START MATCH · {humanCount} HUMAN{(humanCount > 1 ? "S" : "")} vs {botCount} BOT{(botCount > 1 ? "S" : "")}";
            }
            else if (botCount == activePlayers.Count)
            {
                return $"START BOT ARENA · {botCount} BOTS";
            }
            else
            {
                return $"START MATCH · {activePlayers.Count} PLAYERS";
            }
        }

        private void SetPlayerCount(int count)
        {
            Config.NumberOfPlayer = Math.Clamp(count, 2, 8);
            
            // Ensure player list has enough players
            while (Config.PlayerList.Count < Config.NumberOfPlayer)
            {
                int nextIdx = Config.PlayerList.Count;
                var preset = Config.PresetColors[nextIdx % Config.PresetColors.Count];
                bool isBot = Config.GameMode == "PvBot";
                var defaultName = isBot 
                    ? (nextIdx - 1 < Config.PresetBotNames.Count ? Config.PresetBotNames[nextIdx - 1] : $"Bot {nextIdx + 1}")
                    : $"Player {nextIdx + 1}";

                Config.PlayerList.Add(new Player
                {
                    Name = defaultName,
                    RColor = preset.R,
                    GColor = preset.G,
                    BColor = preset.B,
                    IsBot = isBot,
                    BotDifficulty = Config.DefaultBotDifficulty
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
                
                if (Config.GameMode == "PvBot")
                {
                    if (playerIndex == 0)
                    {
                        player.Name = "You (P1)";
                        player.IsBot = false;
                    }
                    else
                    {
                        player.Name = playerIndex - 1 < Config.PresetBotNames.Count ? Config.PresetBotNames[playerIndex - 1] : $"Bot {playerIndex + 1}";
                        player.IsBot = true;
                        player.BotDifficulty = Config.DefaultBotDifficulty;
                    }
                }
                else
                {
                    player.Name = $"Player {playerIndex + 1}";
                    player.IsBot = false;
                }

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
