
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
        StateHasChanged();

        //Console.WriteLine($"{PlayerList.Count} : {count} : {PlayerList[count].Name}");

        if (string.IsNullOrEmpty(cell.Name) || cell.Name == PlayerList[count].Name)
        {
            busy = true;
            await Increase(cell, count);

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


    public async Task Increase(Cell cell, int index)
    {

        cell.CurrentCount++;
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
        //_cells[x][y].Name = PlayerList[index].Name;

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
        PlayerList = [
            new Player()
            {
                Name="Player One",
                RColor=255
            },
            new Player()
            {
                Name="Player Two",
                GColor=255
            }
        ];
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
