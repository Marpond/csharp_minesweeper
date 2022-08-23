using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TextBlock = Emoji.Wpf.TextBlock;

namespace minesweeper;

public class Cell : Button
{
    private const int FONT_SIZE = 350 / MainWindow.GRID_SIZE;

    public Cell()
    {
        State = CellState.Empty;
        Content = "";
        PreviewMouseDown += OnClick;
    }

    public CellState State { get; set; }

    private TextBlock GetColoredTextBlock(int bombCount)
    {
        var brush = new SolidColorBrush();
        switch (bombCount)
        {
            case 1:
                brush.Color = Colors.Blue;
                break;
            case 2:
                brush.Color = Colors.Green;
                break;
            case 3:
                brush.Color = Colors.Red;
                break;
            case 4:
                brush.Color = Colors.DarkOrange;
                break;
            case 5:
                brush.Color = Colors.DarkOrchid;
                break;
            case 6:
                brush.Color = Colors.Indigo;
                break;
            case 7:
                brush.Color = Colors.DarkRed;
                break;
            case 8:
                brush.Color = Colors.DeepPink;
                break;
        }

        var coloredTextBlock = new TextBlock
        {
            Text = bombCount.ToString(),
            FontSize = FONT_SIZE,
            Foreground = brush
        };
        return coloredTextBlock;
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        // Risk it all
        if (e.LeftButton is MouseButtonState.Pressed)
        {
            Debug.Print("Left click");
            var cell = sender as Cell;
            ClickCell(cell!);
        }
        // Flag the cell
        else if (e.RightButton is MouseButtonState.Pressed)
        {
            Debug.Print("Right click");
            var cell = sender as Cell;
            FlagCell(cell!);
        }
    }

    public void ClickCell(Cell cell)
    {
        cell.IsEnabled = false;
        switch (cell.State)
        {
            // Empty cell
            case CellState.Empty:
                Debug.Print("Empty");
                var bombCount = GetNeighbours(cell).Count(x => x.State == CellState.IsBomb);
                // If there are no bombs, reveal all neighbours and recurse
                if (bombCount is 0)
                    foreach (var neighbour in GetNeighbours(cell)
                                 .Where(neighbour => neighbour.State == CellState.Empty && neighbour.IsEnabled))
                        OnClick(neighbour, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
                // Set the cell content to the number of bomb neighbours
                else
                    cell.Content = GetColoredTextBlock(bombCount);
                break;
            // Bomb cell
            case CellState.IsBomb:
                // Reveal all bombs
                foreach (var c in MainWindow.CellList.Where(c => c.State is CellState.IsBomb))
                {
                    c.Content = new TextBlock { Text = Emojis.BOMB, FontSize = FONT_SIZE };
                    c.IsEnabled = false;
                }

                // Disable these 2 if debugging
                MessageBox.Show("You have lost.\nTry again!");
                MainWindow.AppWindow.InitializeGame();
                break;
        }
    }

    private void FlagCell(Cell cell)
    {
        cell.IsEnabled = false;
        cell.State = CellState.Flagged;
        MainWindow.FlagCount--;
        MainWindow.AppWindow.flagTextBlock.Text = $"{MainWindow.FlagCount} {Emojis.FLAG}";
        cell.Content = new TextBlock { Text = Emojis.FLAG, FontSize = FONT_SIZE };
        // If there are no flags remaining
        if (MainWindow.FlagCount != 0) return;
        // There are bombs remaining
        if (MainWindow.CellList.Any(c => c.State is CellState.IsBomb))
        {
            // Reveal all bombs
            foreach (var c in MainWindow.CellList.Where(c => c.State is CellState.IsBomb))
            {
                c.Content = new TextBlock { Text = Emojis.BOMB, FontSize = FONT_SIZE };
                c.IsEnabled = false;
            }

            MessageBox.Show("You have ran out of flags.\nTry again!");
            MainWindow.AppWindow.InitializeGame();
        }
        // Victory
        else
        {
            MessageBox.Show("You have won.\nCongratulations!");
            MainWindow.AppWindow.InitializeGame();
        }
    }

    /**
     * Return a list of all (max 8) neighbours of the cell
     */
    private static List<Cell> GetNeighbours(Cell cell)
    {
        List<Cell> list = new();

        var cellRowPos = Grid.GetRow(cell);
        var cellColPos = Grid.GetColumn(cell);

        // For the 8 neighbouring cells
        for (var row = cellRowPos - 1; row <= cellRowPos + 1; row++)
        for (var col = cellColPos - 1; col <= cellColPos + 1; col++)
        {
            // If the cell is not the current cell and the cell is in the grid
            if (row < 0 || row >= MainWindow.AppWindow.grid.RowDefinitions.Count) continue;
            if (col < 0 || col >= MainWindow.AppWindow.grid.ColumnDefinitions.Count) continue;
            list.Add(MainWindow.CellList[row * MainWindow.AppWindow.grid.ColumnDefinitions.Count + col]);
        }

        return list;
    }
}