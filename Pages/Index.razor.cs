using ChainReaction.Components;
using ChainReaction.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ChainReaction.Pages;

public partial class Index : IAsyncDisposable
{
    [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    private bool busy = false;
    private int currentPlayerIndex = 0;
    private int column = 6;
    private int row = 9;
    private WindowSize? windowSize;
    private DotNetObjectReference<Index>? dotNetRef;

    private List<List<Cell>> gridOfCells = [];
    private List<Player> livePlayerList = [];
    private List<Player> initialPlayerList = [];

    // Scoring & Stats
    private int totalMoves = 0;
    private int currentRound = 1;
    private int lostPeriodForPlayerIndexingInLeaderboard = 0;
    private bool allPlayerPlayed = false;
    private readonly HashSet<string> allPlayerPlayedList = [];
    private readonly Dictionary<string, (DateTime Date, string color, int Period, int MaxCells)> lostPlayers = [];
    public Dictionary<string, (DateTime Date, string color, int Period, int MaxCells)> ScoreLeaderWithTime => lostPlayers;

    private bool dialogShown = false;
    private bool confirmLeaveOpen = false;
    private bool confirmRestartOpen = false;
    private bool helpOpen = false;

    public Player? CurrentPlayer => (livePlayerList.Count > 0 && currentPlayerIndex < livePlayerList.Count) 
        ? livePlayerList[currentPlayerIndex] 
        : null;

    public async Task UserClicked(Cell cell)
    {
        if (busy || CurrentPlayer is null) return;

        // Validation: player can only click on empty cells or cells they own
        if (cell.IsAvailableFor(CurrentPlayer.Name))
        {
            busy = true;
            totalMoves++;

            // Play orb placement pop sound
            if (Config.Dhwani)
            {
                try
                {
                    double pitch = 0.9 + (currentPlayerIndex * 0.1);
                    _ = JSRuntime.InvokeVoidAsync("blazorFunctions.PlayPop", pitch);
                }
                catch {}
            }

            AllPlayerValidation();

            // Execute placement and chain reactions
            int chainReactionCount = 0;
            await PlaceOrbAndResolve(cell, chainReactionCount);

            // Update stats
            UpdatePlayerStats();

            if (allPlayerPlayed)
            {
                CalculateScore();
                if (livePlayerList.Count == 1)
                {
                    await Task.Delay(400);
                    ShowLeaderBoard();
                    busy = false;
                    StateHasChanged();
                    return;
                }
            }

            // Next player turn
            if (livePlayerList.Count > 0)
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % livePlayerList.Count;
                if (currentPlayerIndex == 0)
                {
                    currentRound++;
                }

                var nextPlayer = livePlayerList[currentPlayerIndex];
                Config.CurrentUserColor = nextPlayer.ColorFormed();
                Config.HoverColor = nextPlayer.HoverColorFormed();
            }

            busy = false;
            StateHasChanged();
        }
    }

    private async Task PlaceOrbAndResolve(Cell initialCell, int chainLevel)
    {
        var player = CurrentPlayer;
        if (player is null) return;

        initialCell.CurrentCount++;
        initialCell.Name = player.Name;
        initialCell.Color = player.ColorFormed();

        // Check if any cell in grid exceeds its capacity
        Queue<(int X, int Y)> explosionQueue = new();
        if (initialCell.CurrentCount > initialCell.Capacity)
        {
            explosionQueue.Enqueue((initialCell.X, initialCell.Y));
        }

        int comboStep = 1;
        while (explosionQueue.Count > 0)
        {
            int waveSize = explosionQueue.Count;
            List<(int X, int Y)> nextOrbs = new();

            // Trigger feedback for explosion wave
            if (Config.Kampan || Config.Dhwani)
            {
                _ = Feedback(comboStep);
            }

            for (int w = 0; w < waveSize; w++)
            {
                var (cx, cy) = explosionQueue.Dequeue();
                var cell = gridOfCells[cx][cy];
                if (cell.CurrentCount <= cell.Capacity) continue;

                cell.CurrentCount = 0;
                cell.Name = string.Empty;
                cell.Color = string.Empty;

                var neighbors = new (int X, int Y)[]
                {
                    (cx - 1, cy),
                    (cx + 1, cy),
                    (cx, cy - 1),
                    (cx, cy + 1)
                };

                foreach (var (nx, ny) in neighbors)
                {
                    if (nx >= 0 && nx < row && ny >= 0 && ny < column)
                    {
                        nextOrbs.Add((nx, ny));
                    }
                }
            }

            // Distribute orbs to neighbors
            foreach (var (nx, ny) in nextOrbs)
            {
                var target = gridOfCells[nx][ny];
                target.CurrentCount++;
                target.Name = player.Name;
                target.Color = player.ColorFormed();

                if (target.CurrentCount > target.Capacity && !explosionQueue.Contains((nx, ny)))
                {
                    explosionQueue.Enqueue((nx, ny));
                }
            }

            comboStep++;
            StateHasChanged();
            await Task.Delay(Config.DelayTimeInMilliSecond);

            // Mid-chain victory check
            if (allPlayerPlayed && !IsMoreThanOnePlayerAlive())
            {
                break;
            }
        }
    }

    private void UpdatePlayerStats()
    {
        foreach (var p in initialPlayerList)
        {
            p.CellCount = 0;
            p.OrbCount = 0;
        }

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var cell = gridOfCells[i][j];
                if (!string.IsNullOrEmpty(cell.Name) && cell.CurrentCount > 0)
                {
                    var p = initialPlayerList.FirstOrDefault(x => x.Name == cell.Name);
                    if (p is not null)
                    {
                        p.CellCount++;
                        p.OrbCount += cell.CurrentCount;
                    }
                }
            }
        }
    }

    #region Score Logic
    private void AllPlayerValidation()
    {
        if (!allPlayerPlayed && CurrentPlayer is not null)
        {
            allPlayerPlayedList.Add(CurrentPlayer.Name);
            allPlayerPlayed = (allPlayerPlayedList.Count == initialPlayerList.Count);
        }
    }

    private void CalculateScore()
    {
        lostPeriodForPlayerIndexingInLeaderboard++;
        var alivePlayerNames = new HashSet<string>();
        for (var i = 0; i < gridOfCells.Count; i++)
        {
            for (int j = 0; j < gridOfCells[0].Count; j++)
            {
                var cell = gridOfCells[i][j];
                if (cell.CurrentCount > 0 && !string.IsNullOrEmpty(cell.Name))
                {
                    alivePlayerNames.Add(cell.Name);
                }
            }
        }

        var remainingPlayers = new List<Player>();
        int newCurrentIndex = 0;
        var currentPlayer = CurrentPlayer;

        foreach (var player in livePlayerList)
        {
            if (alivePlayerNames.Contains(player.Name))
            {
                remainingPlayers.Add(player);
                if (currentPlayer == player)
                {
                    newCurrentIndex = remainingPlayers.Count - 1;
                }
            }
            else
            {
                player.IsEliminated = true;
                lostPlayers.TryAdd(player.Name, (DateTime.Now, player.ColorFormed(), lostPeriodForPlayerIndexingInLeaderboard, player.CellCount));
                
                if (Config.Dhwani)
                {
                    try
                    {
                        _ = JSRuntime.InvokeVoidAsync("blazorFunctions.PlayEliminate");
                    }
                    catch {}
                }
            }
        }

        livePlayerList = remainingPlayers;
        if (livePlayerList.Count > 0)
        {
            currentPlayerIndex = Math.Clamp(newCurrentIndex, 0, livePlayerList.Count - 1);
        }
    }

    private bool IsMoreThanOnePlayerAlive()
    {
        var alivePlayers = new HashSet<string>();
        for (var i = 0; i < gridOfCells.Count; i++)
        {
            for (int j = 0; j < gridOfCells[0].Count; j++)
            {
                var cell = gridOfCells[i][j];
                if (cell.CurrentCount > 0 && !string.IsNullOrEmpty(cell.Name))
                {
                    alivePlayers.Add(cell.Name);
                }
            }
        }
        return alivePlayers.Count > 1;
    }

    private void ShowLeaderBoard()
    {
        dialogShown = true;
        lostPeriodForPlayerIndexingInLeaderboard++;
        if (livePlayerList.Count > 0)
        {
            var winner = livePlayerList[0];
            lostPlayers.TryAdd(winner.Name, (DateTime.Now, winner.ColorFormed(), lostPeriodForPlayerIndexingInLeaderboard + 10, winner.CellCount));
        }

        if (Config.Dhwani)
        {
            try
            {
                _ = JSRuntime.InvokeVoidAsync("blazorFunctions.PlayVictory");
            }
            catch {}
        }
    }
    #endregion

    private void ToggleSound()
    {
        Config.Dhwani = !Config.Dhwani;
        try
        {
            _ = JSRuntime.InvokeVoidAsync("blazorFunctions.SetMute", !Config.Dhwani);
        }
        catch {}
    }

    private void GoHome()
    {
        confirmLeaveOpen = false;
        dialogShown = false;
        NavigationManager.NavigateTo("");
    }

    private void RestartMatch()
    {
        confirmRestartOpen = false;
        dialogShown = false;
        ResetForNewGame();
        StateHasChanged();
    }

    private void ResetForNewGame()
    {
        busy = false;
        currentPlayerIndex = 0;
        totalMoves = 0;
        currentRound = 1;
        lostPeriodForPlayerIndexingInLeaderboard = 0;
        allPlayerPlayed = false;
        allPlayerPlayedList.Clear();
        lostPlayers.Clear();
        ResetGridAndPlayers();
    }

    private async Task Feedback(int comboLevel)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("blazorFunctions.BhukampLao", Config.Kampan, Config.Dhwani, comboLevel);
        }
        catch {}
    }

    #region Grid Initialization & Precision Sizing
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            dotNetRef = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("chainReactionLayout.register", dotNetRef);
            await GetInnerDimensions();
        }
    }

    [JSInvokable]
    public async Task OnWindowResize()
    {
        windowSize = await JSRuntime.InvokeAsync<WindowSize>("getInnerDimensions");
        Resize();
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        dotNetRef?.Dispose();
        try
        {
            await JSRuntime.InvokeVoidAsync("chainReactionLayout.unregister");
        }
        catch (JSDisconnectedException) {}
    }

    private async Task GetInnerDimensions()
    {
        windowSize = await JSRuntime.InvokeAsync<WindowSize>("getInnerDimensions");
        Resize();
    }

    private void Resize()
    {
        if (windowSize is null) return;

        bool isDesktop = windowSize.Width >= 1024;
        int boardOuterPadding = 16;

        int availWidth;
        int availHeight;

        if (isDesktop)
        {
            // On desktop: 320px Sidebar + 48px padding & gaps
            availWidth = Math.Max(windowSize.Width - 368, 200);
            availHeight = Math.Max(windowSize.Height - 48, 200);
        }
        else
        {
            // On mobile: ~105px top UI overhead + 20px bottom safe area
            int topUiOverhead = 105;
            availWidth = Math.Max(windowSize.Width - 24, 180);
            availHeight = Math.Max(windowSize.Height - topUiOverhead - 20, 180);
        }

        int newColumn;
        int newRow;
        if (Config.IsCusTomDimention && Config.Rows >= 4 && Config.Rows <= 30 && Config.Column >= 4 && Config.Column <= 30)
        {
            newRow = Config.Rows;
            newColumn = Config.Column;
        }
        else
        {
            // Smart auto-orientation matching screen ratio:
            if (availWidth < availHeight)
            {
                // Portrait (phone / vertical tablet)
                newColumn = 6;
                newRow = 9;
            }
            else
            {
                // Landscape (desktop / horizontal tablet)
                newColumn = 9;
                newRow = 6;
            }
        }

        // Exact maximum cell size guaranteeing ZERO overflow in both axes:
        int maxCellW = (availWidth - boardOuterPadding) / newColumn;
        int maxCellH = (availHeight - boardOuterPadding) / newRow;
        int fitCellSize = Math.Min(maxCellW, maxCellH);

        // Clamp between Min 22px and Max (80px on desktop, 64px on mobile)
        int maxAllowed = isDesktop ? 80 : 64;
        int newCellHeight = Math.Clamp(fitCellSize, 22, maxAllowed);

        var gridChanged = gridOfCells.Count == 0 || newColumn != column || newRow != row;
        column = newColumn;
        row = newRow;
        Config.CellHeight = newCellHeight;

        if (gridChanged)
        {
            ResetGridAndPlayers();
        }

        StateHasChanged();
    }

    private void ResetGridAndPlayers()
    {
        var rawPlayers = Config.PlayerList.Take(Config.NumberOfPlayer).ToList();
        foreach (var p in rawPlayers)
        {
            p.CellCount = 0;
            p.OrbCount = 0;
            p.IsEliminated = false;
        }

        initialPlayerList = rawPlayers.Select(p => p.Clone()).ToList();
        livePlayerList = Config.SuffeledArray(initialPlayerList.Select(p => p.Clone()).ToList());

        if (livePlayerList.Count > 0)
        {
            Config.CurrentUserColor = livePlayerList[0].ColorFormed();
            Config.HoverColor = livePlayerList[0].HoverColorFormed();
        }

        gridOfCells = [];
        for (int i = 0; i < row; i++)
        {
            var rowList = new List<Cell>();
            for (int j = 0; j < column; j++)
            {
                rowList.Add(new Cell
                {
                    X = i,
                    Y = j,
                    Capacity = 3,
                    CurrentCount = 0
                });
            }
            gridOfCells.Add(rowList);
        }

        // Set capacities for edges and corners
        for (int i = 0; i < row; i++)
        {
            gridOfCells[i][0].Capacity = 2;
            gridOfCells[i][column - 1].Capacity = 2;
        }
        for (int j = 0; j < column; j++)
        {
            gridOfCells[0][j].Capacity = 2;
            gridOfCells[row - 1][j].Capacity = 2;
        }

        gridOfCells[0][0].Capacity = 1;
        gridOfCells[0][column - 1].Capacity = 1;
        gridOfCells[row - 1][0].Capacity = 1;
        gridOfCells[row - 1][column - 1].Capacity = 1;
    }
    #endregion
}
