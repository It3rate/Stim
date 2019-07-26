using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Stim.World;

namespace Stim.Controls
{
    public partial class WorldBitmapView : UserControl
    {
        public DirectBitmap bmp;

        const int colorCount = 100;
        const int xWidth = 1;
        const int yWidth = xWidth;
        
        private uint[] brushes;
        private uint[] negBrushes;

        public WorldGrid Grid { get; private set; }

        public WorldBitmapView(WorldGrid grid)
        {
            InitializeComponent();
            Grid = grid;
            Initialize();
            Width = grid.Width;
            Height = grid.Height;
            bmp = new DirectBitmap(this.Width, this.Height);
            UpdateBitmap();
        }

        private void Initialize()
        {
            brushes = new uint[colorCount];
            negBrushes = new uint[colorCount];
            for (int i = 0; i < colorCount; i++)
            {
                double v = (i + 1) / (double)colorCount;
                brushes[i] = 0xFF000000 + 
                    ((uint)(v * 0xFF0000) & 0xFF0000) + 
                    ((uint)(v * 0xFF00) & 0xFF00) + 
                    (uint)(v * 0xFF);
                negBrushes[i] = 0xFF000000 + 
                    ((uint)(v * 0xFF0000) & 0xFF0000) +
                    ((uint)(245 * 0xFF00) & 0xFF00) + 
                    (uint)(245 * 0xFF);
            }
        }

        public void UpdateBitmap()
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                for (int x = 0; x < Grid.Width; x++)
                {
                    int index = Grid[x, y];
                    bool isNeg = (index < 0);
                    index = isNeg ? -index - 1 : index;
                    index = (index < colorCount) ? index : colorCount - 1;
                    bmp.SetPixel(x, y, (index < 0) ? negBrushes[index] : brushes[index]);
                }
            }
            Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int x = e.X / xWidth;
            int y = e.Y / yWidth;
            Grid.fastCircleAt(x, y, 30);
            UpdateBitmap();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawImage(bmp.Bitmap, new Point(0, 0));
        }
    }
}
