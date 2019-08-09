using System;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Stim.Fluid
{
    public class FluidSolver
    {
        Thread thread;
        public bool quit = false;
        FluidControl fluidControl;

        public double[,] vx;
        public double[,] vy;
        public double[,] dye;
        //pressure is stored to provide a good initial 
        // guess for iteration methods
        public double[,] pressure;
        public bool[,] boundary;
        public double dt = 0.1;
        public int n = 64;//32;
        public double visc = 0.0000001;
        public int maxiter = 4; // 120
        public double minerr = 1e-3;
        public double dw, idw;

        public Random rnd = new Random(1234);
        public double gravityX = 0.0;
        public double gravityY = -0.1;
        public double t;
        public Vector[] animals;
        public int animalCount = 30;

        public FluidSolver(FluidControl fluidControl)
        {
            this.fluidControl = fluidControl;
            fluidControl.Solver = this;
            Initialize();

            thread = new Thread(this.ThreadStart);
            thread.Start();
        }

        void ThreadStart()
        {
            while (!quit)
            {
                Step();
                if (fluidControl != null)
                {
                    fluidControl.Invoke((MethodInvoker)delegate {
                        fluidControl.fluidRender.Invalidate();
                    });
                }
                Thread.Sleep(5);
            }
        }

        public void OnClose() 
        {
            quit = true;
            thread.Join();
        }


        public void Initialize()
        {
            vx = new double[n + 1, n]; // 0.0s
            vy = new double[n, n + 1]; // 0.0s
            dye = new double[n, n]; // 0.0s
            pressure = new double[n, n]; // 0.0s
            boundary = new bool[n, n];
            animals = new Vector[animalCount];

            double speedUp = 1.0; // 3.0
            dw = 1.0 / (n * speedUp);
            idw = 1.0 / dw;


            int i, j;
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    boundary[i, j] = ((i == 0) || (i == n - 1) || (j == 0) || (j == n - 1));
                }
            }

            for (i = 0; i < animals.Length; i++)
            {
                animals[i] = new Vector(rnd.Next(n - 4) + 2, rnd.Next(n - 4) + 2, 0);
            }
        }

        public void set_bounds_x(double[,] vxnew)
        {
            int i, j;
            for (i = 1; i < n - 1; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if (boundary[i - 1, j]) vxnew[i, j] = 0.0;
                    if (boundary[i + 1, j]) vxnew[i, j] = 0.0;
                }
            }
        }

        public void set_bounds_y(double[,] vynew)
        {
            int i, j;
            for (i = 0; i < n; i++)
            {
                for (j = 1; j < n - 1; j++)
                {
                    if (boundary[i, j - 1]) vynew[i, j] = 0.0;
                    if (boundary[i, j + 1]) vynew[i, j] = 0.0;
                }
            }
        }

        public void vel_diffuse()
        {
            double a = dt * visc * idw * idw;
            double c = 1 + 4.0f * a;
            double diff = 10.0f;
            double dv;
            int iter = maxiter;
            double[,] vxnew;
            double[,] vynew;
            vxnew = new double[n + 1, n];
            vynew = new double[n, n + 1];

            //gauss seidel
            //initial guess is current velocities
            int i, j;
            for (i = 0; i < n + 1; i++)
            {
                for (j = 0; j < n; j++)
                {
                    vxnew[i, j] = vx[i, j];
                }
            }
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n + 1; j++)
                {
                    vynew[i, j] = vy[i, j];
                }
            }

            //x and y velocities are done in two steps
            //first x
            while ((diff > minerr) && (0 < iter--))
            {
                diff = 0.0f;
                for (i = 1; i < n; i++)
                {
                    for (j = 1; j < n - 1; j++)
                    {
                        dv = vxnew[i, j];
                        vxnew[i, j] = (vx[i, j] +
                            a * (vxnew[i - 1, j] +
                            vxnew[i + 1, j] +
                            vxnew[i, j - 1] +
                            vxnew[i, j + 1])) / c;
                        dv -= vxnew[i, j];
                        diff += dv * dv;
                    }
                }
                set_bounds_x(vxnew);
            }

            diff = 10.0f;
            iter = maxiter;
            //then y
            while ((diff > minerr) && (0 < iter--))
            {
                diff = 0.0f;
                for (i = 1; i < n - 1; i++)
                {
                    for (j = 1; j < n; j++)
                    {
                        dv = vynew[i, j];
                        vynew[i, j] = (vy[i, j] +
                            a * (vynew[i - 1, j] +
                            vynew[i + 1, j] +
                            vynew[i, j - 1] +
                            vynew[i, j + 1])) / c;
                        dv -= vxnew[i, j];
                        diff += dv * dv;
                    }
                }
                set_bounds_y(vynew);
            }
            vx = vxnew;
            vy = vynew;

            for (i = 0; i < animals.Length; i++)
            {
                Vector animal = animals[i];
                int ax = Math.Max(0, Math.Min(n - 1, (int)animal.x));
                int ay = Math.Max(0, Math.Min(n - 1, (int)animal.y));
                double xd = (vx[ax, ay] + vx[ax + 1, ay] + vx[ax - 1, ay] + vx[ax, ay + 1] + vx[ax, ay - 1]) / 5.0;
                double yd = (vy[ax, ay] + vy[ax + 1, ay] + vy[ax - 1, ay] + vy[ax, ay + 1] + vy[ax, ay - 1]) / 5.0;
                animal.x += Math.Min(10, xd * 30) + 0.02 * (rnd.NextDouble() - 0.5);
                animal.y += Math.Min(10, yd * 30) + 0.02 * (rnd.NextDouble() - 0.5);
                animal.x = Math.Max(2, Math.Min(n - 3, animal.x));
                animal.y = Math.Max(2, Math.Min(n - 3, animal.y));
            }
        }

        public void dense_diffuse()
        {
            double a = dt * visc * idw * idw;
            double c = 1 + 4.0f * a;
            double diff = 10.0f;
            double dd;
            int iter = maxiter;
            double[,] dyenew;
            dyenew = new double[n, n];
            double temp;

            //initial guess is current values
            int i, j;
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    dyenew[i, j] = dye[i, j];
                }
            }
            //gauss seidel
            while ((diff > minerr) && (0 < iter--))
            {
                diff = 0.0;
                for (i = 1; i < n - 1; i++)
                {
                    for (j = 1; j < n - 1; j++)
                    {
                        dd = dyenew[i, j];
                        temp = (dye[i, j] +
                            a * (dyenew[i - 1, j] +
                            dyenew[i + 1, j] +
                            dyenew[i, j - 1] +
                            dyenew[i, j + 1])) / c;
                        temp = Math.Min(999, Math.Max(0, temp)) * 0.999;
                        dyenew[i, j] = temp;
                        dd -= temp;
                        diff += dd * dd;
                    }
                }
            }
            dye = dyenew;
        }

        public void vel_advect()
        {
            Vector from;
            int x1, y1;
            int x2, y2;
            double s1, s2, t1, t2, viy, vix;
            double dt0 = dt * idw;
            Vector v;
            double[,] vxnew;
            double[,] vynew;
            vxnew = new double[n + 1, n];
            vynew = new double[n, n + 1];

            //x and y velocities are done in two steps
            //first x
            int i, j;
            for (i = 0; i < n + 1; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if ((i > 0) && (i < n))
                        viy = (vy[i, j] + vy[i, j + 1] + vy[i - 1, j] + vy[i - 1, j + 1]) / 4.0;
                    else if (i == n)
                        viy = (vy[i - 1, j] + vy[i - 1, j + 1]) / 2.0;
                    else
                        viy = (vy[i, j] + vy[i, j + 1]) / 2.0;
                    v = new Vector(vx[i, j], viy, 0);
                    from = new Vector(i, j, 0) - v * dt0;
                    x1 = (int)from.x; x2 = x1 + 1;
                    y1 = (int)from.y; y2 = y1 + 1;
                    if (x1 < 0) x1 = 0; if (x1 >= n + 1) x1 = n;
                    if (x2 < 0) x2 = 0; if (x2 >= n + 1) x2 = n;
                    if (y1 < 0) y1 = 0; if (y1 >= n) y1 = n - 1;
                    if (y2 < 0) y2 = 0; if (y2 >= n) y2 = n - 1;
                    s1 = (double)from.x - x1; s2 = 1.0f - s1;
                    t1 = (double)from.y - y1; t2 = 1.0f - t1;
                    vxnew[i, j] =
                        t1 * (s1 * vx[x2, y2] + s2 * vx[x1, y2]) +
                        t2 * (s1 * vx[x2, y1] + s2 * vx[x1, y1]);
                }
            }
            //then y
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n + 1; j++)
                {
                    if ((j > 0) && (j < n))
                        vix = (vx[i, j] + vx[i + 1, j] + vx[i, j - 1] + vx[i + 1, j - 1]) / 4.0;
                    else if (j == n)
                        vix = (vx[i, j - 1] + vx[i + 1, j - 1]) / 2.0;
                    else
                        vix = (vx[i, j] + vx[i + 1, j]) / 2.0;
                    v = new Vector(vix, vy[i, j], 0);
                    from = new Vector(i, j, 0) - v * dt0;
                    x1 = (int)from.x; x2 = x1 + 1;
                    y1 = (int)from.y; y2 = y1 + 1;
                    if (x1 < 0) x1 = 0; if (x1 >= n) x1 = n - 1;
                    if (x2 < 0) x2 = 0; if (x2 >= n) x2 = n - 1;
                    if (y1 < 0) y1 = 0; if (y1 >= n + 1) y1 = n;
                    if (y2 < 0) y2 = 0; if (y2 >= n + 1) y2 = n;
                    s1 = (double)from.x - x1; s2 = 1.0f - s1;
                    t1 = (double)from.y - y1; t2 = 1.0f - t1;
                    vynew[i, j] =
                        t1 * (s1 * vy[x2, y2] + s2 * vy[x1, y2]) +
                        t2 * (s1 * vy[x2, y1] + s2 * vy[x1, y1]);
                }
            }
            vx = vxnew;
            vy = vynew;
            set_bounds_x(vx);
            set_bounds_y(vy);
        }

        public void dense_advect()
        {
            Vector from, v;
            int x1, y1;
            int x2, y2;
            double s1, s2, t1, t2, vix, viy;
            double dt0 = dt * idw;
            double temp;
            double[,] dyenew;
            dyenew = new double[n, n];
            int i, j;
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    vix = (vx[i, j] + vx[i + 1, j]) / 2.0;
                    viy = (vy[i, j] + vy[i, j + 1]) / 2.0;
                    v = new Vector(vix, viy, 0);
                    from = new Vector(i, j, 0) - v * dt0;
                    x1 = (int)from.x; x2 = x1 + 1;
                    y1 = (int)from.y; y2 = y1 + 1;
                    if (x1 < 0) x1 = 0; if (x1 >= n) x1 = n - 1;
                    if (x2 < 0) x2 = 0; if (x2 >= n) x2 = n - 1;
                    if (y1 < 0) y1 = 0; if (y1 >= n) y1 = n - 1;
                    if (y2 < 0) y2 = 0; if (y2 >= n) y2 = n - 1;
                    s1 = (double)from.x - x1; s2 = 1.0f - s1;
                    t1 = (double)from.y - y1; t2 = 1.0f - t1;
                    temp = t1 * (s1 * dye[x2, y2] + s2 * dye[x1, y2]) +
                        t2 * (s1 * dye[x2, y1] + s2 * dye[x1, y1]);
                    temp = Math.Min(99, Math.Max(0, temp));
                    dyenew[i, j] = temp;
                }
            }
            dye = dyenew;
        }

        public void project()
        {
            double a = 1.0f;
            double c = 4.0f;
            double diff = 10.0;
            double dp;
            int iter = maxiter;
            double[,] pressurenew;
            pressurenew = new double[n, n];

            int i, j;
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    pressurenew[i, j] = pressure[i, j];
                }
            }
            //compute b=grad.w
            //b will be stored in pressure
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    pressure[i, j] = -((vx[i + 1, j] - vx[i, j])
                                   + (vy[i, j + 1] - vy[i, j]));
                }
            }
            //gauss seidel
            while ((diff > minerr) && (0 < iter--))
            {
                diff = 0.0;
                for (i = 1; i < n - 1; i++)
                {
                    for (j = 1; j < n - 1; j++)
                    {
                        dp = pressurenew[i, j];
                        pressurenew[i, j] = (pressure[i, j] +
                            a * (pressurenew[i - 1, j] +
                            pressurenew[i + 1, j] +
                            pressurenew[i, j - 1] +
                            pressurenew[i, j + 1])) / c;
                        dp -= pressurenew[i, j];
                        diff += dp * dp;
                    }
                }
                //set_bounds();
            }

            pressure = pressurenew;
            for (i = 1; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    vx[i, j] -= (pressure[i, j] - pressure[i - 1, j]);
                }
            }

            for (i = 0; i < n; i++)
            {
                for (j = 1; j < n; j++)
                {
                    vy[i, j] -= (pressure[i, j] - pressure[i, j - 1]);
                }
            }

            set_bounds_x(vx);
            set_bounds_y(vy);
        }

        public void externalForces()
        {
            t += 0.04;
            vx[13, 13] = (Math.Sin(t)) * 0.2 + 0.15;//rnd.NextDouble() * 0.05 + 0.2;
            vy[13, 13] = (Math.Cos(t)) * 0.2 + 0.18;// rnd.NextDouble() * 0.05 + 0.175;
            if (rnd.NextDouble() < 0.1)
            {
                int rx = rnd.Next((int)(n * 0.8) + (int)(n * 0.1)) + 1;
                vx[rx, n - 3] = rnd.NextDouble() * 0.5 - 0.15;
                vy[rx, n - 3] = rnd.NextDouble() * 4.0 - 4.0;
            }

            //int i, j;
            //for (i = 0; i < n; i++)
            //{
            //    for (j = 0; j < n; j++)
            //    {
            //        if (!boundary[i, j])
            //        {
            //            vx[i, j] += gravityX * dt;
            //            vy[i, j] += gravityY * dt;
            //        }
            //    }
            //}
        }
        
        public void Step()
        {
            vel_diffuse();
            project();
            vel_advect();
            externalForces();
            project();

            dense_diffuse();
            dense_advect();
        }
    }






    /*
    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class FluidForm
    {
        public FluidSolver fl;
        Thread thread;
        public bool quit = false;

        public int startX;
        public int startY;
        public int mouseX;
        public int mouseY;
        public bool isMoving = false;
        public bool isLeftDown = false;
        public bool isRightDown = false;

        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FluidForm());
        }

        void ThreadStart()
        {
            while (!quit)
            {
                fl.Step();
                PicFluid.Invalidate();
                Thread.Sleep(5);
            }
        }


        public FluidForm()
        {
            InitializeComponent();
            fl = new FluidSolver();
            fl.Initialize();

            thread = new Thread(this.ThreadStart);
            thread.Start();
        }

        void FluidFormFormClosed(object sender, FormClosedEventArgs e)
        {
            quit = true;
            thread.Join();
        }

        #region Mouse
        void PicFluidMouseDown(object sender, MouseEventArgs e)
        {
            startX = (int)(fl.n * e.X / PicFluid.Width);
            startY = (int)(fl.n * e.Y / PicFluid.Height);
            mouseX = startX;
            mouseY = startY;

            if (e.Button == MouseButtons.Middle)
            {
                fl.boundary[mouseX, mouseY] = true;
                fl.dye[mouseX, mouseY] = 0;
                isRightDown = false;
                isLeftDown = false;
            }

            if (e.Button == MouseButtons.Left)
            {
                isLeftDown = true;
            }
            else
            {
                isRightDown = false;
            }
            if (e.Button == MouseButtons.Right)
            {
                isRightDown = true;
            }
            else
            {
                isRightDown = false;
            }
            PicFluid.Invalidate();
        }

        void PicFluidMouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X < 0 ? 0 : e.X >= PicFluid.Width ? PicFluid.Width - 1 : e.X;
            int y = e.Y < 0 ? 0 : e.Y >= PicFluid.Height ? PicFluid.Height - 1 : e.Y;
            mouseX = (int)(fl.n * x / PicFluid.Width);
            mouseY = (int)(fl.n * y / PicFluid.Height);
            isMoving = true;

            if (e.Button == MouseButtons.Middle)
            {
                fl.boundary[mouseX, mouseY] = true;
                fl.dye[mouseX, mouseY] = 0;
                isRightDown = false;
                isLeftDown = false;
            }

            if (e.Button == MouseButtons.Left)
            {
                isLeftDown = true;
            }
            else
            {
                isRightDown = false;
            }
            if (e.Button == MouseButtons.Right)
            {
                isRightDown = true;
            }
            else
            {
                isRightDown = false;
            }
            PicFluid.Invalidate();

        }

        void PicFluidMouseUp(object sender, MouseEventArgs e)
        {
            startX = (int)(fl.n * e.X / PicFluid.Width);
            startY = (int)(fl.n * e.Y / PicFluid.Height);
            mouseX = startX;
            mouseY = startY;
            isLeftDown = false;
            isRightDown = false;
            isMoving = false;
            PicFluid.Invalidate();
        }
        #endregion

        #region Paint
        void PicFluidPaint(object sender, PaintEventArgs e)
        {
            int i, j;
            double d;
            Vector v;
            if ((mouseX != startX) || (mouseY != startY))
            {
                if (isLeftDown) fl.vx[mouseX, mouseY] += (mouseX - startX) * 1300.0 * fl.dw / PicFluid.Width;
                if (isLeftDown) fl.vy[mouseX, mouseY] += (mouseY - startY) * 1300.0 * fl.dw / PicFluid.Height;
            }
            if (isRightDown) fl.dye[mouseX, mouseY] += 45.0f;

            for (i = 0; i < fl.n; i++)
            {
                for (j = 0; j < fl.n; j++)
                {
                    d = fl.dye[i, j] / 10.0;
                    if (d < 0.0) d = 0.0;
                    if (d > 1.0) d = 1.0;
                    if (fl.boundary[i, j])
                        DrawBoundryBlock(e.Graphics, i, j, 0.0, 0.2, 0.5);
                    else
                        DrawBoundryBlock(e.Graphics, i, j, d, d, d / 4.0);
                    v = new Vector(0.5 * (fl.vx[i, j] + fl.vx[i + 1, j]),
                                 0.5 * (fl.vy[i, j] + fl.vy[i, j + 1]), 0);
                    DrawVelocity(e.Graphics, i, j, v, 0.1, 0.0, 0.4);
                }
            }
            float pxw = PicFluid.Width / fl.n;
            float pxh = PicFluid.Height / fl.n;
            for (i = 0; i < fl.animals.Length; i++)
            {
                e.Graphics.FillEllipse(Brushes.White, (float)fl.animals[i].x * pxw, (float)fl.animals[i].y * pxh, 10, 10);
            }
        }

        void DrawBoundryBlock(Graphics gfx, double x, double y, double r, double g, double b)
        {
            Pen pen = new Pen(Brushes.Black);
            pen.Color = Color.FromArgb(255, (int)(r * 255), (int)(g * 255), (int)(b * 255));
            Brush brush = new SolidBrush(pen.Color);
            float rx = (float)x / fl.n * PicFluid.Width;
            float ry = (float)y / fl.n * PicFluid.Height;
            float w = PicFluid.Width / fl.n;
            float h = PicFluid.Height / fl.n;
            gfx.FillRectangle(brush, rx, ry, w, h);
        }

        void DrawVelocity(Graphics gfx, double x, double y, Vector v, double r, double g, double b)
        {
            Pen pen = new Pen(Brushes.Black);
            float rx = (float)(x + 0.5f) / fl.n * PicFluid.Width;
            float ry = (float)(y + 0.5f) / fl.n * PicFluid.Height;
            float w = PicFluid.Width / fl.n;
            float h = PicFluid.Height / fl.n;
            pen.Color = Color.FromArgb(255,
                (int)(Math.Min(1.0, Math.Abs(v.x * 40)) * 255),
                0,
                (int)(Math.Min(1.0, Math.Abs(v.y * 40)) * 255));//, (int)(b * 255));

            gfx.DrawLine(pen, rx, ry, rx + (float)v.x * 500.0f, ry + (float)v.y * 500.0f);
        }
        #endregion
    }
    */
}
