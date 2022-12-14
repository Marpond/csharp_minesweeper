using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace minesweeper;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public const int GRID_SIZE = 20;
    public static int FlagCount;

    public static List<Cell> CellList = new();
    
    public static MainWindow AppWindow = null!;
    public MainWindow()
    {
        AppWindow = this;
        InitializeComponent();
        // Create the grid
        for (var i = 0; i < GRID_SIZE; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        InitializeGame();
        // Debugging only
        //foreach (var cell in CellList) cell.ClickCell(cell);
    }

    public void InitializeGame()
    {
        CellList.Clear();

        // Set the flag count
        FlagCount = GRID_SIZE * GRID_SIZE / 6;
        flagTextBlock.Text = $"{FlagCount} {Emojis.FLAG}";

        // Insert cells into grid
        CellList = new List<Cell>();
        for (var row = 0; row < GRID_SIZE; row++)
        for (var column = 0; column < GRID_SIZE; column++)
        {
            Cell cell = new();
            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, column);
            grid.Children.Add(cell);
            CellList.Add(cell);
        }

        // Create bombs
        Random random = new();
        while (CellList.Count(cell => cell.Type == CellType.Mine) != FlagCount)
            CellList[random.Next(0, CellList.Count - 1)].Type = CellType.Mine;
        
        // Calculate bomb neighbor counts
        foreach (var cell in CellList)
            cell.BombNeighbourCount = Cell.GetNeighbours(cell).Count(neighbour => neighbour.Type == CellType.Mine);
    }
}