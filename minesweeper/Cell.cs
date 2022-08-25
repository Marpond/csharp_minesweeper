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
        Type = CellType.Empty;
        BombNeighbourCount = 0;
        
        Content = "";
        PreviewMouseDown += OnClick;
    }

    public CellType Type { get; set; }
    public int BombNeighbourCount { get; set; }

    private static TextBlock GetColoredTextBlock(int bombCount)
    {
        var brush = new SolidColorBrush();
        brush.Color = bombCount switch
        {
            1 => Colors.Blue,
            2 => Colors.Green,
            3 => Colors.Red,
            4 => Colors.DarkOrange,
            5 => Colors.DarkOrchid,
            6 => Colors.Indigo,
            7 => Colors.DarkRed,
            8 => Colors.DeepPink,
            _ => brush.Color
        };

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
        switch (cell.Type)
        {
            // Empty cell
            case CellType.Empty:
                Debug.Print("Empty");
                // If there are no bombs, reveal all neighbours and recurse
                if (cell.BombNeighbourCount is 0)
                    foreach (var neighbour in GetNeighbours(cell)
                                 .Where(neighbour => neighbour.Type == CellType.Empty && neighbour.IsEnabled))
                        OnClick(neighbour, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
                // Set the cell content to the number of bomb neighbours
                else
                    cell.Content = GetColoredTextBlock(cell.BombNeighbourCount);
                break;
            // Bomb cell
            case CellType.Mine:
                // Reveal all bombs
                foreach (var c in MainWindow.CellList.Where(c => c.Type is CellType.Mine))
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
        cell.Type = CellType.Flag;
        MainWindow.FlagCount--;
        MainWindow.AppWindow.flagTextBlock.Text = $"{MainWindow.FlagCount} {Emojis.FLAG}";
        cell.Content = new TextBlock { Text = Emojis.FLAG, FontSize = FONT_SIZE };
        // If there are flags remaining
        if (MainWindow.FlagCount != 0) return;
        // There are mines remaining
        if (MainWindow.CellList.Any(c => c.Type is CellType.Mine))
        {
            // Reveal all mines
            foreach (var c in MainWindow.CellList.Where(c => c.Type is CellType.Mine))
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
    public static List<Cell> GetNeighbours(Cell cell)
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