
using ChainReaction.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
namespace ChainReaction.Pages;

public partial class Index
{
    List<List<Cell>> _cells=new();
    bool busy = false;
    int count = 0;
    List<Player> PlayerList = [];
    [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;

    protected int DeviceWidth { get; set; }
    protected int DeviceHeight { get; set; }
    [Inject] public IDialogService DialogService { get; set; } = null!;
    public async Task CogClicked()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        await DialogService.ShowAsync<DialogComponent>("Configuration", options);
        Reset();
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
        busy =false;
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
        await Task.Delay(20);
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
    void Possible()
    {
        Config.Width = (DeviceWidth) / 64;
        Config.Height = (DeviceHeight) / 64;
        Reset();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Register the JavaScript function and pass the callback
            await JSRuntime.InvokeVoidAsync("window.getDeviceWidth", DotNetObjectReference.Create(this));
            await JSRuntime.InvokeVoidAsync("window.getDeviceHeight", DotNetObjectReference.Create(this));
        }
    }

    

    [JSInvokable]
    public void UpdateDeviceWidth(int width)
    {
        Console.WriteLine("Width received from JavaScript: " + width);
        DeviceWidth = width;
        StateHasChanged();
    }
    [JSInvokable]
    public void UpdateDeviceHeight(int width)
    {
        DeviceHeight = width;
        StateHasChanged();
    }
    string GetWidth()
    {
        return $"{(DeviceWidth) / (Config.Width + 2)}px";
    }
    protected override async Task OnInitializedAsync()
    {

        Reset();
        await base.OnInitializedAsync();
    }
}
