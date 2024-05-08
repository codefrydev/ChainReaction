
using ChainReaction.Components;
using ChainReaction.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
namespace ChainReaction.Pages;

public partial class Index
{
    List<List<Cell>> _cells = [];
    List<Player> PlayerList = [];
    [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] public IDialogService DialogService { get; set; } = null!;
    bool busy = false;

    int count = 0;
    int column = 0;
    int row = 0;
    WindowSize? windowSize;
    int cellSize = 64;
    bool allPlayerPlayed = false;
    bool scoreShown = false;
    readonly HashSet<string> playerName = [];
    readonly Dictionary<string, int> playerScore = [];

    readonly bool idiotScoreLogic = false; // make this true for working on score system
    public int CellSize
    {
        get { return cellSize; }
        set
        {
            cellSize = value;
            Resize();
        }
    }
    public void CogClicked()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        DialogService.ShowAsync<DialogComponent>("Configuration", options);
        Resize();
        StateHasChanged();
    }

    public async Task UserClicked(Cell cell)
    {

        if (busy) return;
        if (string.IsNullOrEmpty(cell.Name) || cell.Name == PlayerList[count].Name)
        {
            busy = true;

            if (idiotScoreLogic)
            {
                ScoreLogicOne();
            }
            cell.Name = PlayerList[count].Name;
            await Increase(cell, count);

            if (idiotScoreLogic)
            {
                count -= ScoreLogicTwo();
            }

            count++;
            if (count >= PlayerList.Count)
            {
                count = 0;
            }
        }
        Config.CurrentUserColor = PlayerList[count].ColorFormed();
        Config.HoverColor = PlayerList[count].HoverColorFormed();
        busy = false;
    }

    #region Score Logic not working Properly Screwing Everything
    void ScoreLogicOne()
    {
        if (!allPlayerPlayed)
        {
            playerScore.Add(PlayerList[count].Name, 0);
            allPlayerPlayed = (playerScore.Count == PlayerList.Count);
        }
    }
    int ScoreLogicTwo()
    {
        int playerRemoved = 0;
        if (allPlayerPlayed)
        {
            foreach (var i in playerScore)
            {
                if (i.Value < 0)
                {
                    for (var j = 0; j < PlayerList.Count; j++)
                    {
                        if (i.Key == PlayerList[j].Name && j <= count)
                        {
                            playerRemoved++;
                        }
                    }
                    playerScore.Remove(i.Key);

                }
            }
            if (playerScore.Count == 1)
            {
                ShowLeaderBoard();
            }
        }
        return playerRemoved;
    }
    void ScoreLogicThree(Cell cell)
    {
        if (allPlayerPlayed && PlayerList.Count == 1)
        {
            if (!scoreShown)
            {
                ShowLeaderBoard();
                scoreShown = true;
            }
            return;
        }

        playerScore[PlayerList[count].Name]++;
        Console.WriteLine($"{cell.Name}: {playerScore[PlayerList[count].Name]}");
        if (cell.CurrentCount > cell.Capacity && playerScore.ContainsKey(cell.Name))
        {
            playerScore[cell.Name]--;
            Console.WriteLine($"{cell.Name}: {playerScore[cell.Name]}");
            if (playerScore[cell.Name] == 0)
            {
                playerName.Add(cell.Name);
            }
        }
    }
    void ShowLeaderBoard()
    {
        playerName.Add(PlayerList[0].Name);
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            DisableBackdropClick = true,
            FullWidth = true,
            NoHeader = true
        };
        var parameters = new DialogParameters<GameOver>
        {
            { x => x.ScoreLeaderBard, playerName.ToList() }
        };
        _ = DialogService.ShowAsync<GameOver>("Configuration", parameters, options);
    }
    #endregion
    public async Task Increase(Cell cell, int index)
    {
        cell.CurrentCount++;
        if (idiotScoreLogic)
        {
            ScoreLogicThree(cell);
        }

        cell.Name = PlayerList[index].Name;
        cell.Color = PlayerList[index].ColorFormed();

        if (cell.CurrentCount > cell.Capacity)
        {
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
                if(x>=0 && y>=0 && y<column && x<row)
                {
                    await CallIncrease(x, y, index);
                }
            }
            #endregion

        }
    }
    public async Task CallIncrease(int x, int y, int index)
    {
        await Increase(_cells[x][y], index);
        //if(!Config.icon) for future implement
        await Task.Delay(20);
        StateHasChanged();
    }
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
        int extra = windowSize?.Width > 1600 ? 3 : 0;
        column = (windowSize?.Width / cellSize) -extra ?? 10;
        row = (windowSize?.Height / cellSize) - 2 ?? 10;
        Reset();
        StateHasChanged();
    }
    void Reset()
    {
        PlayerList = Config.PlayerList.Take(Config.NumberOfPlayer).ToList();
        _cells = [];

        for (int i = 0; i < row; i++)
        {
            var ls = new List<Cell>();
            for (int j = 0; j < column; j++)
            {
                ls.Add(new Cell()
                {
                    X = i,
                    Y = j,
                    Capacity = 3,
                    CurrentCount = 0
                });
            }
            _cells.Add(ls);
        }

        // setting capacity for edge cases
        for (int i = 0; i < row; i++)
        {
            _cells[i][0].Capacity = 2;
            _cells[i][column - 1].Capacity = 2;
        }
        for (int i = 0; i < column; i++)
        {
            _cells[0][i].Capacity = 2;
            _cells[row - 1][i].Capacity = 2;
        }
        _cells[0][0].Capacity = 1;
        _cells[0][column - 1].Capacity = 1;
        _cells[row - 1][0].Capacity = 1;
        _cells[row - 1][column - 1].Capacity = 1;
    }
}
