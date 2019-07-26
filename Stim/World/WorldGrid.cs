using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stim.World
{
	public class WorldGrid
	{
		public const int gridWidth = 200;
		public const int gridHeight = 200;
		private int[,] grid = new int[gridWidth, gridHeight];
        private Random rnd = new Random(1234);

        public WorldGrid()
        {
            for (int y = 0; y < WorldGrid.gridHeight; y++)
            {
                for (int x = 0; x < WorldGrid.gridWidth; x++)
                {
                    this[x, y] = 0;// rnd.Next(0,5);
                }
            }
            //circleAt(100, 100, 70);
            fastCircleAt(100, 100, 30);
        }
        public int this[int x, int y]
        {
            get { return grid[x, y]; }
            set { grid[x, y] = value; }
        }

        public void circleAt(int cx, int cy, int r)
        {
            for (int y = 0; y < WorldGrid.gridHeight; y++)
            {
                for (int x = 0; x < WorldGrid.gridWidth; x++)
                {
                    double dist = Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    this[x, y] = (dist <= r) ? (int)dist : 1000;// rnd.Next(0,5);
                }
            }
        }

        public void fastCircleAt(int cx, int cy, int r)
        {
            for (int y = 0; y < WorldGrid.gridHeight; y++)
            {
                for (int x = 0; x < WorldGrid.gridWidth; x++)
                {
                    double adx = Math.Abs(x - cx);
                    double ady = Math.Abs(y - cy);
                    double amax = Math.Max(adx, ady);
                    double amin = Math.Min(adx, ady);
                    double dx = (x - cx); //Math.Abs(x - cx);
                    double dy = (y - cy); //Math.Abs(y - cy);
                    double max = Math.Max(dx, dy);
                    //double dist = (dx * dx * dx * dx + dy * dy * dy * dy) / (double)((dx - dy) * (dx - dy));
                    //double dist = (dx * dx + dy * dy) / (Math.Abs(dx - dy) / 2.0);
                    //double dist = (dx * ((dx - dy) / max + 1) + dy * ((dy - dx) / max + 1));
                    //double dist = (dx * (dx / max) + dy * (dy / max));// / 4.0;
                    double dist = (dx * (dx / max) + dy * (dy / max));// / 4.0;

                    //this[x, y] = (int)rnd.Next(0,5)*r;
                    this[x, y] = (int)dist;
                    //this[x, y] = (int)((amax + 0.5*amin));
                }
            }
        }
    }
}
