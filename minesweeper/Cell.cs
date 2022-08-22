using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Emoji.Wpf;

namespace minesweeper
{
	public class Cell : Button
	{
		public enum CellState
		{
			Empty,
			Flagged,
			IsBomb
		}
		public CellState State { get; set; }

		public Cell()
		{
			State = CellState.Empty;
			Content = "";
			PreviewMouseDown += OnClick;
		}

		private Emoji.Wpf.TextBlock GetColoredTextBlock(int bombCount)
		{
			SolidColorBrush brush = new SolidColorBrush();
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

			var coloredTextBlock = new Emoji.Wpf.TextBlock
			{
				Text = bombCount.ToString(),
				FontSize = 350 / MainWindow.GRID_SIZE,
				Foreground = brush,
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
				cell!.IsEnabled = false;
				switch (cell.State)
				{
					// Empty cell
					case CellState.Empty:
						Debug.Print("Empty");
						int bombCount = GetNeighbours(cell).Count(x => x.State == CellState.IsBomb);
						// If there are no bombs, reveal all neighbours and recurse
						if (bombCount is 0)
						{
							foreach (Cell neighbour in GetNeighbours(cell)
								         .Where(neighbour => neighbour.State == CellState.Empty && neighbour.IsEnabled))
							{
								OnClick(neighbour, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
							}
						}
						// Set the cell content to the number of bomb neighbours
						else
						{
							cell.Content = GetColoredTextBlock(bombCount);
						}
						break;
					// Bomb cell
					case CellState.IsBomb:
						// Reveal all bombs
						foreach (var c in MainWindow.CellList.Where(c => c.State is CellState.IsBomb))
						{
							c.Content = new Emoji.Wpf.TextBlock { Text = "💣", FontSize = 350 / MainWindow.GRID_SIZE };
						}
						MessageBox.Show("You have lost.\nTry again!");
						MainWindow.AppWindow.InitializeGame();
						break;
				}
			}
			// Flag the cell
			else if (e.RightButton is MouseButtonState.Pressed)
			{
				var cell = sender as Cell;
				cell!.IsEnabled = false;
				Debug.Print("Right click");
				cell.State = CellState.Flagged;
				MainWindow.FlagCount--;
				MainWindow.AppWindow.flagTextBlock.Text = $"{MainWindow.FlagCount} 🚩";
				cell.Content = new Emoji.Wpf.TextBlock { Text = Emojis.Flag , FontSize = 350 / MainWindow.GRID_SIZE };
				// If there are no flags remaining
				if (MainWindow.FlagCount == 0)
				{
					// There are bombs remaining
					if (IsBombExists())
					{
						// Reveal all bombs
						foreach (var c in MainWindow.CellList.Where(c => c.State is CellState.IsBomb))
						{
							c.Content = new Emoji.Wpf.TextBlock { Text = "💣", FontSize = 350 / MainWindow.GRID_SIZE };
						}
						MessageBox.Show("You have lost.\nTry again!");
						MainWindow.AppWindow.InitializeGame();
					}
					// Victory
					else
					{
						MessageBox.Show("You have won.\nCongratulations!");
						MainWindow.AppWindow.InitializeGame();
					}
					return;
				}
			}
		}

		/**
		 * Return a list of all neighbours of the cell
		 */
		private static List<Cell> GetNeighbours(Cell cell)
		{
			List<Cell> list = new();
			
			var cellRowPos = Grid.GetRow(cell);
			var cellColPos = Grid.GetColumn(cell);
			
			// For the 8 neighbouring cells
			for (var row = cellRowPos - 1; row <= cellRowPos + 1; row++)
			{
				for (var col = cellColPos - 1; col <= cellColPos + 1; col++)
				{
					// If the cell is not the current cell and the cell is in the grid
					if (row < 0 || row >= MainWindow.AppWindow.grid.RowDefinitions.Count) continue;
					if (col < 0 || col >= MainWindow.AppWindow.grid.ColumnDefinitions.Count) continue; 
					list.Add(MainWindow.CellList[row * MainWindow.AppWindow.grid.ColumnDefinitions.Count + col]);
				}
			}
			return list;
		}
		
		private static bool IsBombExists()
		{
			return MainWindow.CellList.Any(cell => cell.State is CellState.IsBomb);
		}
	}
}