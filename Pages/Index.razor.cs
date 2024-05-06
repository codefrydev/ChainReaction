
using Microsoft.AspNetCore.Components; 
using MudBlazor;
namespace ChainReaction.Pages;

public partial class Index
{
    List<List<Cell>> _cells;
    bool busy = false;
    int count = 0;
    List<Player> PlayerList = [];
    protected override async Task OnInitializedAsync()
    {
        
        await Reset(); 
        await  base.OnInitializedAsync();
    }
    public async Task UserClicked(Cell cell)
    {
        if (busy) return;

        Console.WriteLine($"{PlayerList.Count} : {count} : {PlayerList[count].Name}");

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
        busy=false;
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
            int x = cell.X;
            int y = cell.Y;
            // up
            int upX = x;
            int upY = y - 1;
            if (upY >= 0)
            {
                await CallIncrease(upX, upY, index);
            }
            // down
            int downX = x;
            int downY = y + 1;
            if (downY < Config.Width)
            {
                await CallIncrease(downX, downY, index);
            }
            // left
            int leftX = x - 1;
            int leftY = y;
            if (leftX >= 0)
            {
                await CallIncrease(leftX, leftY, index);
            }
            // right
            int rightX = x + 1;
            int rightY = y;
            if (rightX < Config.Height)
            {
                await CallIncrease(rightX, rightY, index);
            }
            #endregion

        }
    }
    public async Task CallIncrease(int x, int y, int index)
    {
        //_cells[x][y].Name = PlayerList[index].Name;

        await Increase(_cells[x][y], index);
        await Task.Delay(100);
        StateHasChanged();
    }
    string color = "white";
    void OnEnter()
    {
        color = PlayerList[count].ColorFormed();
    }
    void OnExit()
    {
        color = "white";
    }
    
    async Task Reset()
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

        for (int i = 0; i < Config.Height; i++)
        {
            var ls = new List<Cell>();
            for (int j = 0; j < Config.Width; j++)
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
        for (int i = 0; i < Config.Height; i++)
        {
            _cells[i][0].Capacity = 2;
            _cells[i][Config.Width - 1].Capacity = 2;
        }
        for (int i = 0; i < Config.Width; i++)
        {
            _cells[0][i].Capacity = 2;
            _cells[Config.Height - 1][i].Capacity = 2;
        }
        _cells[0][0].Capacity = 1;
        _cells[0][Config.Width - 1].Capacity = 1;
        _cells[Config.Height - 1][0].Capacity = 1;
        _cells[Config.Height - 1][Config.Width - 1].Capacity = 1;

    }

}
