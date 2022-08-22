using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Image = Emoji.Wpf.Image;
using TextBlock = Emoji.Wpf.TextBlock;


namespace minesweeper

{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public const int GRID_SIZE = 5;
		public static int FlagCount;

		public static List<Cell> CellList = new();

		public static MainWindow AppWindow;

		public MainWindow()
		{
			AppWindow = this;
			InitializeComponent();
			// Create the grid
			for (int i = 0; i < GRID_SIZE; i++)
			{
				grid.RowDefinitions.Add(new RowDefinition());
				grid.ColumnDefinitions.Add(new ColumnDefinition());
			}
			InitializeGame();
		}

		public void InitializeGame()
		{
			// Remove all children from the grid
			CellList.Clear();
			
			// Set the flag count
			FlagCount = GRID_SIZE * GRID_SIZE / 6;
			flagTextBlock.Text = $"{FlagCount} 🚩";
			
			// Insert cells into grid
			CellList = new List<Cell>();
			for (int row = 0; row < GRID_SIZE; row++)
			{
				for (int column = 0; column < GRID_SIZE; column++)
				{
					Cell cell = new();
					Grid.SetRow(cell, row);
					Grid.SetColumn(cell, column); 
					grid.Children.Add(cell);
					CellList.Add(cell);
				}
			}

			// Create bombs
			Random random = new();
			while (CellList.Count(cell => cell.State == Cell.CellState.IsBomb) != FlagCount)
			{
				CellList[random.Next(0, CellList.Count - 1)].State = Cell.CellState.IsBomb;
			}
		}
	}
}