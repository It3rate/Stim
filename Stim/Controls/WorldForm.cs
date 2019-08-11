using Stim.Fluid;
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

namespace Stim.Controls
{
    public partial class WorldForm : Form
    {
        //private WorldBitmapView worldBitmapView;
        //WorldGrid grid;
        FluidControl fluidControl;

        public WorldForm()
        {
            InitializeComponent();

            //grid = new WorldGrid(512, 512);
            //worldBitmapView = new WorldBitmapView(grid);
            //worldBitmapView.Location = new Point(0, 0);
            //worldBitmapView.Size = new Size(512, 512);
            //Controls.Add(worldBitmapView);

            fluidControl = new FluidControl();
            fluidControl.Location = new Point(0, 0);
            fluidControl.Size = this.ClientSize;// new Size(1024, 1024);

            FluidSolver solver = new FluidSolver(fluidControl);
            Controls.Add(fluidControl);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            Controls.Remove(fluidControl);
            fluidControl = null;
        }
    }
}
