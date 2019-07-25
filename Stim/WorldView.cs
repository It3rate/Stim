using Stim.World;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stim
{
	public partial class WorldView : Form
	{
		private WorldGrid grid;
		private Color[] colorList = { Color.White, Color.Yellow, Color.Orange, Color.Red, Color.DarkRed, Color.DarkBlue, Color.Black };
		public WorldView()
		{
			InitializeComponent();
			Initialize();
        }

		private void Initialize()
		{
            grid = new WorldGrid();
            worldGridView.grid = grid;
		}
		
	}
}
