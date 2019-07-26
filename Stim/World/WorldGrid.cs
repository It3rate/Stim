using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hybridizer.Runtime.CUDAImports;

namespace Stim.World
{
	public class WorldGrid
	{
        public int Width { get; private set; }
        public int Height { get; private set; }

        private int[,] grid;
        private Random rnd = new Random(1234);

        public WorldGrid(int width, int height)
        {
            Width = width;
            Height = height;
            grid = new int[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    this[x, y] = rnd.Next(0,5);
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
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    double dist = Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    this[x, y] = (dist <= r) ? (int)dist : 1000;// rnd.Next(0,5);
                }
            }
        }

        public void fastCircleAt(int cx, int cy, int r)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    double adx = Math.Abs(x - cx);
                    double ady = Math.Abs(y - cy);
                    double amax = Math.Max(adx, ady);
                    double amin = Math.Min(adx, ady);
                    double dx = (x - cx); //Math.Abs(x - cx);
                    double dy = (y - cy); //Math.Abs(y - cy);
                    double max = Math.Max(dx, dy);
                    double min = Math.Min(dx, dy);
                    //double dist = (dx * dx * dx * dx + dy * dy * dy * dy) / (double)((dx - dy) * (dx - dy));
                    //double dist = (dx * dx + dy * dy) / (Math.Abs(dx - dy) / 2.0);
                    //double dist = (dx * ((dx - dy) / max + 1) + dy * ((dy - dx) / max + 1));
                    //double dist = (dx * (dx / max) + dy * (dy / max));
                    double dist = (dx * (dx / max) + dy * (dy / max));

                    //this[x, y] = (int)rnd.Next(0,5)*r;
                    //this[x, y] = (int)dist;
                    this[x, y] = (int)(amax + 0.5 * amin);
                    //this[x, y] = (int)(max + 0.5 * min);
                }
            }
        }
    }
}
