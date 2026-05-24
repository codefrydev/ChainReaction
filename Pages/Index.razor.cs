
using ChainReaction.Components;
using ChainReaction.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace ChainReaction.Pages;

public partial class Index : IAsyncDisposable
{
    [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    bool busy = false;
    int count = 0;
    int column = 0;
    int row = 0;
    WindowSize? windowSize;
    DotNetObjectReference<Index>? dotNetRef;
    const int MinCellSize = 28;
    const int MaxCellSize = 64;
    const int HorizontalPadding = 48;
    const int VerticalPadding = 80;
    List<List<Cell>> gridOfCells = [];
    List<Player> livePlayerList = [];
    // Scoring handeling
    int lostPeriodForPlayerIndexingInLeaderboard = 0;
    bool allPlayerPlayed = false;
    readonly HashSet<string> allPlayerPlayedList = [];
    readonly Dictionary<string, (DateTime Date, string color, int Period)> lostPlayers = [];
    public Dictionary<string, (DateTime Date, string color, int Period)> ScoreLeaderWithTime => lostPlayers;
    bool dialogShown = false;
    bool closingForReplay = false;
    public async Task UserClicked(Cell cell)
    {

        if (busy) return;
        if (string.IsNullOrEmpty(cell.Name) || cell.Name == livePlayerList[count].Name)
        {
            busy = true;
            AllPlayerValidation();
            await Increase(cell);
            if (allPlayerPlayed)
            {
                CalculateScore();
                if (livePlayerList.Count == 1)
                {
                    await Task.Delay(1000);
                    ShowLeaderBoard();
                }
            }
            count++;
            count %= livePlayerList.Count;
        }
        Config.CurrentUserColor = livePlayerList[count].ColorFormed();
        Config.HoverColor = livePlayerList[count].HoverColorFormed();
        busy = false;
    }
    #region Score Logic
    void AllPlayerValidation()
    {
        if (!allPlayerPlayed)
        {
            allPlayerPlayedList.Add(livePlayerList[count].Name);
            allPlayerPlayed = (allPlayerPlayedList.Count == livePlayerList.Count);
        }
    }

    void CalculateScore()
    {
        lostPeriodForPlayerIndexingInLeaderboard++;
        var currentPlayer = livePlayerList[count];
        var alivePlayer = new HashSet<string>();
        for (var i = 0; i < gridOfCells.Count; i++)
        {
            for (int j = 0; j < gridOfCells[0].Count; j++)
            {
                var cell = gridOfCells[i][j];
                if (cell.CurrentCount > 0)
                {
                    alivePlayer.Add(cell.Name);
                }
            }
        }
        var finalPlayerLeft = new List<Player>();
        int pos = 0;
        int index = 0;
        foreach (var item in livePlayerList)
        {
            if (alivePlayer.Contains(item.Name))
            {
                finalPlayerLeft.Add(item);
                if (currentPlayer == item)
                {
                    pos = index;
                }
                index++;
            }
            else
            {
                lostPlayers.TryAdd(item.Name, (DateTime.Now, item.HoverColorFormed(), lostPeriodForPlayerIndexingInLeaderboard));
            }
        }
        livePlayerList = finalPlayerLeft;
        count = pos;
    }
    bool IsMoreThanOnePlayerAlive()
    {
        var alivePlayer = new HashSet<string>();
        for (var i = 0; i < gridOfCells.Count; i++)
        {
            for (int j = 0; j < gridOfCells[0].Count; j++)
            {
                var cell = gridOfCells[i][j];
                if (cell.CurrentCount > 0)
                {
                    alivePlayer.Add(cell.Name);
                }
            }
        }
        return alivePlayer.Count > 1;
    }
    void RecursiveLookUp()
    {
        if (!dialogShown)
        {
            if (Config.Kampan || Config.Dhwani)
            {
                _ = Feedback();
            }
            if (allPlayerPlayed && !IsMoreThanOnePlayerAlive())
            {
                CalculateScore();
                if (livePlayerList.Count == 1)
                {
                    Task.Delay(1000);
                    ShowLeaderBoard();
                }
            }
        }
    }
    void ShowLeaderBoard()
    {
        dialogShown = true;
        lostPeriodForPlayerIndexingInLeaderboard++;
        lostPlayers.TryAdd(livePlayerList[0].Name,
            (DateTime.Now, livePlayerList[0].HoverColorFormed(), lostPeriodForPlayerIndexingInLeaderboard));
        var list = lostPlayers.OrderBy(x => x.Value).Select(x => x.Key).ToList();
    }
    #endregion
    public async Task Increase(Cell cell)
    {
        cell.CurrentCount++;
        cell.Name = livePlayerList[count].Name;
        cell.Color = livePlayerList[count].ColorFormed();
        
        if (cell.CurrentCount > cell.Capacity)
        {
            RecursiveLookUp(); // take care of Infinite Case 
            // Bug Founed by Abhijeet Kumar

            cell.CurrentCount = 0;
            cell.Name = string.Empty;
            #region neighbour 

            var ls = new List<(int, int)>
            {
                (cell.X,     cell.Y - 1),
                (cell.X,     cell.Y + 1),
                (cell.X - 1, cell.Y ),
                (cell.X + 1, cell.Y )
            };

            foreach (var (x, y) in ls)
            {
                if (x >= 0 && y >= 0 && y < column && x < row)
                {
                    await Increase(gridOfCells[x][y]);
                    await Task.Delay(Config.DelayTimeInMilliSecond);
                    StateHasChanged();
                }
            }
            #endregion
        }
    } 
    public void GoHome()
    {
        dialogShown = false;
        NavigationManager.NavigateTo("");
    }

    public void PlayAgain()
    {
        closingForReplay = true;
        dialogShown = false;
        ResetForNewGame();
        closingForReplay = false;
        StateHasChanged();
    }

    async Task HandleGameOverModalChanged(bool isOpen)
    {
        dialogShown = isOpen;
        if (!isOpen && !closingForReplay)
        {
            NavigationManager.NavigateTo("");
        }
    }

    void ResetForNewGame()
    {
        busy = false;
        count = 0;
        lostPeriodForPlayerIndexingInLeaderboard = 0;
        allPlayerPlayed = false;
        allPlayerPlayedList.Clear();
        lostPlayers.Clear();
        Reset();
    }
    private async Task Feedback()
    {
        await JSRuntime.InvokeVoidAsync("blazorFunctions.BhukampLao",Config.Kampan,Config.Dhwani);
    }
    #region Setting Up Enviroment
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
        catch (JSDisconnectedException)
        {
            // Page is unloading.
        }
    }

    private async Task GetInnerDimensions()
    {
        windowSize = await JSRuntime.InvokeAsync<WindowSize>("getInnerDimensions");
        Resize();
    }

    void Resize()
    {
        if (windowSize is null)
        {
            return;
        }

        var availWidth = Math.Max(windowSize.Width - HorizontalPadding, MinCellSize * 4);
        var availHeight = Math.Max(windowSize.Height - VerticalPadding, MinCellSize * 4);

        int newColumn;
        int newRow;
        if (Config.IsCusTomDimention && Config.Rows >= 4 && Config.Rows <= 100 && Config.Column >= 4 && Config.Column <= 100)
        {
            newRow = Config.Rows;
            newColumn = Config.Column;
        }
        else
        {
            int extra = windowSize.Width > 1400 ? 3 : 0;
            newColumn = Math.Max(4, availWidth / MaxCellSize - extra);
            newRow = Math.Max(4, availHeight / MaxCellSize - 2);
        }

        var cellByWidth = availWidth / newColumn;
        var cellByHeight = availHeight / newRow;
        var fitCellSize = Math.Min(cellByWidth, cellByHeight);
        int newCellHeight;
        if (fitCellSize >= MinCellSize)
        {
            newCellHeight = Math.Min((int)fitCellSize, MaxCellSize);
        }
        else if (Config.IsCusTomDimention)
        {
            newCellHeight = Math.Max(20, (int)fitCellSize);
        }
        else
        {
            newCellHeight = MinCellSize;
        }

        var gridChanged = gridOfCells.Count == 0 || newColumn != column || newRow != row;
        column = newColumn;
        row = newRow;
        Config.CellHeight = newCellHeight;

        if (gridChanged)
        {
            Reset();
        }

        StateHasChanged();
    }
    void Reset()
    {

        livePlayerList = Config.SuffeledArray(Config.PlayerList.Take(Config.NumberOfPlayer).ToList());
        Config.CurrentUserColor = livePlayerList[0].ColorFormed();
        gridOfCells = [];
        if(Config.IsCusTomDimention &&Config.Rows>=4 && Config.Rows<=100 && Config.Column>=4 && Config.Column<=100)
        { 
            row = Config.Rows;
            column = Config.Column;
        }
        for (int i = 0; i < row; i++)
        {
            var ls = new List<Cell>();
            for (int j = 0; j < column; j++)
            {
                ls.Add(new Cell()
                {
                    X = i,
                    Y = j,
                    Capacity = Config.CellCapacity,
                    CurrentCount = 0
                });
            }
            gridOfCells.Add(ls);
        }

        // setting capacity for edge cases
        for (int i = 0; i < row; i++)
        {
            gridOfCells[i][0].Capacity = 2;
            gridOfCells[i][column - 1].Capacity = 2;
        }
        for (int i = 0; i < column; i++)
        {
            gridOfCells[0][i].Capacity = 2;
            gridOfCells[row - 1][i].Capacity = 2;
        }
        gridOfCells[0][0].Capacity = 1;
        gridOfCells[0][column - 1].Capacity = 1;
        gridOfCells[row - 1][0].Capacity = 1;
        gridOfCells[row - 1][column - 1].Capacity = 1;
    }

    #endregion
}
