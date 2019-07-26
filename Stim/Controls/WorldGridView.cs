using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Stim.World;


namespace Stim.Controls
{
    public partial class WorldGridView : Control
    {
        public WorldGrid grid;
        const int colorCount = 100;
        const int xWidth = 5;
        const int yWidth = xWidth;

        private Color[] colors;
        private Pen[] pens;
        private Brush[] brushes;
        private Brush[] negBrushes;

        public WorldGridView()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            colors = new Color[colorCount];
            for (int i = 0; i < colorCount; i++)
            {
                double v = (i + 1) / (double)colorCount;
                colors[i] = Color.FromArgb(
                    (int)(v * 255),
                    (int)(v * 255),
                    (int)(v * 255)
                    //(int)(v * 128 + 127),
                    //(int)((1.0 - Math.Sin(v * Math.PI)) * 255)
                    );
            }
            pens = new Pen[colors.Length];
            brushes = new Brush[colors.Length];
            negBrushes = new Brush[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                brushes[i] = new SolidBrush(colors[i]);
                negBrushes[i] = new SolidBrush(Color.FromArgb(colors[i].R, 245, 245));
                pens[i] = new Pen(colors[i], 2);
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int x = e.X / xWidth;
            int y = e.Y / yWidth;
            grid.fastCircleAt(x, y, 30);
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            Rectangle r = new Rectangle(0, 0, xWidth, yWidth);
            Brush[] brushList;
            for (int y = 0; y < WorldGrid.gridHeight; y++)
            {
                for (int x = 0; x < WorldGrid.gridWidth; x++)
                {
                    int index = grid[x, y];
                    brushList = index < 0 ? negBrushes : brushes;
                    index = (index < 0) ? -index - 1 : index;
                    index = (index < colorCount) ? index : colorCount - 1;
                    r.Offset(xWidth, 0);
                    pe.Graphics.FillRectangle(brushList[index], r);
                }
                r.Offset(-xWidth * WorldGrid.gridWidth, yWidth);
            }
        }
    }
}
