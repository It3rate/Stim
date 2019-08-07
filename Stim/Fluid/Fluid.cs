using System;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace fluid_mac
{
    public class FluidSolver
    {
        public double[,] vx;
        public double[,] vy;
        public double[,] dye;
        //pressure is stored to provide a good initial 
        // guess for iteration methods
        public double[,] pressure;
        public bool[,] surface;
        public bool[,] fluid;
        public bool[,] boundary;
        private int i, j;
        public double dt = 0.1;
        public double eps1 = 0.01f;
        public double eps2 = 0.02f;
        public int n = 64;//32;
        public double visc = 0;//.0001;
        public int maxiter = 12;
        public double minerr = 1e-5;
        public double dw, idw; //idw=1/dw

        public void Initialize()
        {
            vx = new double[n + 1, n];
            vy = new double[n, n + 1];
            dye = new double[n, n];
            pressure = new double[n, n];
            surface = new bool[n, n];
            boundary = new bool[n, n];
            fluid = new bool[n, n];

            dw = 1.0 / n;
            idw = n*4.0;// 1.0 / dw;
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if ((i == 0) || (i == n - 1) || (j == 0) || (j == n - 1))
                    {
                        boundary[i, j] = true;
                    }
                    else
                    {
                        boundary[i, j] = false;
                    }
                    surface[i, j] = false;
                    fluid[i, j] = true;
                    dye[i, j] = 0.0;
                    pressure[i, j] = 0.0;
                }
            }
            for (i = 0; i < n + 1; i++)
            {
                for (j = 0; j < n; j++)
                {
                    vx[i, j] = 0.0;
                }
            }
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n + 1; j++)
                {
                    vy[i, j] = 0.0;
                }
            }
        }

        public void set_bounds_x(double[,] vxnew)
        {
            int i, j;
            for (i = 0; i < n + 1; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if (i - 1 >= 0)
                        if (boundary[i - 1, j])
                            vxnew[i, j] = 0.0;
                    if (i + 1 < n)
                        if (boundary[i + 1, j])
                            vxnew[i, j] = 0.0;
                }
            }
        }

        public void set_bounds_y(double[,] vynew)
        {
            int i, j;
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n + 1; j++)
                {
                    if (j - 1 >= 0)
                        if (boundary[i, j - 1])
                            vynew[i, j] = 0.0;
                    if (j + 1 < n)
                        if (boundary[i, j + 1])
                            vynew[i, j] = 0.0;
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

            //initial guess is current values
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
                        dyenew[i, j] = (dye[i, j] +
                            a * (dyenew[i - 1, j] +
                            dyenew[i + 1, j] +
                            dyenew[i, j - 1] +
                            dyenew[i, j + 1])) / c;
                        dd -= dyenew[i, j];
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
            double[,] dyenew;
            dyenew = new double[n, n];
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
                    dyenew[i, j] =
                        t1 * (s1 * dye[x2, y2] + s2 * dye[x1, y2]) +
                        t2 * (s1 * dye[x2, y1] + s2 * dye[x1, y1]);
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

        public void force()
        {
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if (fluid[i, j])
                    {
                        vy[i, j + 1] += 0.03f * dt;
                    }
                }
            }
        }

        public void dense_step()
        {
            dense_diffuse();
            dense_advect();
        }

        public void vel_step()
        {
            vel_diffuse();
            project();
            vel_advect();
            //force();
            project();
            //set_bounds();
        }

        public void RunFluid()
        {
            //set_bounds();
            vel_step();
            dense_step();
        }
    }
    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm
    {
        public FluidSolver fl;
        Thread thread;
        public bool quit = false;
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        void ThreadStart()
        {
            while (!quit)
            {
                PicFluid.Invalidate();
                Thread.Sleep(10);
            }
        }


        public MainForm()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            //
            // TODO: Add constructor code after the InitializeComponent() call.
            //

            fl = new FluidSolver();
            fl.Initialize();
            //rnd=new Random();		
            thread = new Thread(this.ThreadStart);
            thread.Start();
        }

        void Draw(Graphics gfx, double x, double y, double r, double g, double b)
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

        void DrawVector(Graphics gfx, double x, double y, Vector v, double r, double g, double b)
        {
            Pen pen = new Pen(Brushes.Black);
            pen.Color = Color.FromArgb(255, (int)(r * 255), (int)(g * 255), (int)(b * 255));
            float rx = (float)(x + 0.5f) / fl.n * PicFluid.Width;
            float ry = (float)(y + 0.5f) / fl.n * PicFluid.Height;
            float w = PicFluid.Width / fl.n;
            float h = PicFluid.Height / fl.n;
            gfx.DrawLine(pen, rx, ry, rx + (float)v.x * 15.0f, ry + (float)v.y * 15.0f);
        }

        public int ox;
        public int oy;
        public int mx;
        public int my;
        public bool left = false;
        public bool right = false;

        void MainFormFormClosed(object sender, FormClosedEventArgs e)
        {
            quit = true;
            thread.Join();
        }

        void PicFluidMouseDown(object sender, MouseEventArgs e)
        {
            ox = (int)(fl.n * e.X / PicFluid.Width);
            oy = (int)(fl.n * e.Y / PicFluid.Height);
            mx = ox;
            my = oy;
            if (e.Button == MouseButtons.Left)
            {
                left = true;
            }
            else
            {
                right = false;
            }
            if (e.Button == MouseButtons.Right)
            {
                right = true;
            }
            else
            {
                right = false;
            }
            PicFluid.Invalidate();
        }

        void PicFluidMouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X < 0 ? 0 : e.X >= PicFluid.Width ? PicFluid.Width - 1 : e.X;
            int y = e.Y < 0 ? 0 : e.Y >= PicFluid.Height ? PicFluid.Height - 1 : e.Y;
            mx = (int)(fl.n * x / PicFluid.Width);
            my = (int)(fl.n * y / PicFluid.Height);

            if (e.Button == MouseButtons.Left)
            {
                left = true;
            }
            else
            {
                right = false;
            }
            if (e.Button == MouseButtons.Right)
            {
                right = true;
            }
            else
            {
                right = false;
            }
            PicFluid.Invalidate();

        }

        void PicFluidMouseUp(object sender, MouseEventArgs e)
        {
            ox = (int)(fl.n * e.X / PicFluid.Width);
            oy = (int)(fl.n * e.Y / PicFluid.Height);
            mx = ox;
            my = oy;
            left = false;
            right = false;
            PicFluid.Invalidate();
        }

        void PicFluidPaint(object sender, PaintEventArgs e)
        {
            int i, j;
            double d;
            Vector v;
            if ((mx != ox) || (my != oy))
            {
                if (left) fl.vx[mx, my] += (mx - ox) * 100.0 * fl.dw / PicFluid.Width;
                if (left) fl.vy[mx, my] += (my - oy) * 100.0 * fl.dw / PicFluid.Height;
            }
            if (right) fl.dye[mx, my] += 45.0f;
            fl.RunFluid();
            for (i = 0; i < fl.n; i++)
            {
                for (j = 0; j < fl.n; j++)
                {
                    d = fl.dye[i, j] / 10.0;
                    if (d < 0.0) d = 0.0;
                    if (d > 1.0) d = 1.0;
                    if (fl.boundary[i, j])
                        Draw(e.Graphics, i, j, 0.0, 0.4, 0.1);
                    else
                        Draw(e.Graphics, i, j, 0.25 + d/2.0, d/4.0, d);
                    v = new Vector(0.5 * (fl.vx[i, j] + fl.vx[i + 1, j]),
                                 0.5 * (fl.vy[i, j] + fl.vy[i, j + 1]), 0);
                    DrawVector(e.Graphics, i, j, v, 1.0, 0.0, 0.0);
                }
            }
        }
    }
}
