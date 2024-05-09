
using ChainReaction.Components;
using ChainReaction.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
namespace ChainReaction.Pages;

public partial class Index
{
    [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] public IDialogService DialogService { get; set; } = null!;

    bool busy = false;
    int count = 0;
    int column = 0;
    int row = 0;
    WindowSize? windowSize;
    readonly int gridDimentions = 64;
    List<List<Cell>> gridOfCells = [];
    List<Player> livePlayerList = [];
    // Scoring handeling
    int lostPeriodForPlayerIndexingInLeaderboard = 0;
    bool allPlayerPlayed = false;
    readonly HashSet<string> allPlayerPlayedList = [];
    readonly Dictionary<string, (DateTime Date, string color, int Period)> lostPlayers = [];
    bool dialogShown = false;
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
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            DisableBackdropClick = true,
            FullWidth = true,
            NoHeader = true,
            FullScreen = true,
        };
        var parameters = new DialogParameters<GameOver>
        {
            {
                x=>x.ScoreLeaderWithTime,lostPlayers
            }
        };
        _ = DialogService.ShowAsync<GameOver>("Configuration", parameters, options);
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
    private async Task Feedback()
    {
        await JSRuntime.InvokeVoidAsync("blazorFunctions.BhukampLao",Config.Kampan,Config.Dhwani);
    }
    #region Setting Up Enviroment
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetInnerDimensions();
        }
    }

    private async Task GetInnerDimensions()
    {
        windowSize = await JSRuntime.InvokeAsync<WindowSize>("getInnerDimensions");
        Resize();
    }

    void Resize()
    {
        int extra = windowSize?.Width > 1400 ? 3 : 0;
        column = (windowSize?.Width / gridDimentions) - extra ?? 10;
        row = (windowSize?.Height / gridDimentions) - 2 ?? 10;
        Reset();
        StateHasChanged();
    }
    void Reset()
    {
        livePlayerList = Config.PlayerList.Take(Config.NumberOfPlayer).ToList();
        gridOfCells = [];

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

    // As of Now Ditching recursive call because of bug
    // In Future may be i will look into this 
    public async Task IncreaseNonRecursive(Cell cell)
    {
        var name = livePlayerList[count].Name;
        var color = livePlayerList[count].ColorFormed();
        cell.CurrentCount++;
        cell.Name = name;
        cell.Color = color;

        var queue = new Queue<Cell>();
        queue.Enqueue(cell);
        var ls = new List<(int X, int Y)>
        {
            ( 0,-1),
            ( 0, 1),
            (-1, 0),
            ( 1, 0)
        };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.CurrentCount > current.Capacity)
            {
                current.CurrentCount = 0;
                current.Name = string.Empty;
                current.Color = "white";
                foreach (var (X, Y) in ls)
                {
                    var x = current.X + X;
                    var y = current.Y + Y;

                    if (x >= 0 && y >= 0 && y < column && x < row)
                    {
                        var neighbourCell = gridOfCells[x][y];
                        neighbourCell.CurrentCount++;
                        neighbourCell.Name = name;
                        neighbourCell.Color = color;
                        // Later Neighbour may have more than 3 elements
                        Console.WriteLine($"{neighbourCell.CurrentCount}");
                        queue.Enqueue(neighbourCell);
                    }
                }
            }
            await Task.Delay(20);
            StateHasChanged();
        }
        StateHasChanged();
    }
}
