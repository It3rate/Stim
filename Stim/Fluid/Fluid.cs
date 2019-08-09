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
        private bool quit = false;
        FluidControl fluidControl;

        public double[,] vx;
        public double[,] vy;
        public double[,] dye;
        public double[,] pressure;
        public bool[,] boundary;

        public double dt = 0.1;
        public int n = 64;//32;
        public double visc = 0;//.001;
        public int maxiter = 4; // 120
        public double minerr = 1e-3;
        public double dw, idw;

        public double gravityX = 0.0;
        public double gravityY = -0.1;
        public double t;
        public Vector[] animals;
        public int animalCount = 30;

        private Random rnd = new Random(1234);
        private double[,] vxnew;
        private double[,] vynew;
        private double[,] pressurenew;
        private double[,] dyenew;
        private const int DOUBLE_SIZE = 8;

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
            vx = new double[n + 1, n]; 
            vy = new double[n, n + 1];
            dye = new double[n, n];
            pressure = new double[n, n];
            boundary = new bool[n, n];
            animals = new Vector[animalCount];

            vxnew = (double[,])vx.Clone();
            vynew = (double[,])vy.Clone();
            dyenew = (double[,])dye.Clone();
            pressurenew = (double[,])pressure.Clone();

            dw = 1.0 / n;
            idw = n - 2;// 1.0 / dw;


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

        public void SetBoundsX(double[,] vxnew)
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

        public void SetBoundsY(double[,] vynew)
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

        public void DiffuseVelocity()
        {
            double a = dt * visc;
            double c = 1 + 4.0f * a;
            double diff = 10.0f;
            double dv;
            int iter = maxiter;

            //gauss seidel
            //initial guess is current velocities
            Buffer.BlockCopy(vx, 0, vxnew, 0, vx.Length * DOUBLE_SIZE);
            Buffer.BlockCopy(vy, 0, vynew, 0, vy.Length * DOUBLE_SIZE);

            //x and y velocities are done in two steps
            //first x
            while ((diff > minerr) && (0 < iter--))
            {
                diff = 0.0f;
                for (int i = 1; i < n; i++)
                {
                    for (int j = 1; j < n - 1; j++)
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
                SetBoundsX(vxnew);
            }

            diff = 10.0f;
            iter = maxiter;
            //then y
            while ((diff > minerr) && (0 < iter--))
            {
                diff = 0.0f;
                for (int i = 1; i < n - 1; i++)
                {
                    for (int j = 1; j < n; j++)
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
                SetBoundsY(vynew);
            }
            Buffer.BlockCopy(vxnew, 0, vx, 0, vx.Length * DOUBLE_SIZE);
            Buffer.BlockCopy(vynew, 0, vy, 0, vy.Length * DOUBLE_SIZE);

            for (int i = 0; i < animals.Length; i++)
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

        public void DiffuseDensity()
        {
            double a = dt * visc;
            double c = 1 + 4.0f * a;
            double diff = 10.0f;
            double dd;
            int iter = maxiter;
            double temp;
            
            //initial guess is current values
            Buffer.BlockCopy(dye, 0, dyenew, 0, dye.Length * DOUBLE_SIZE);

            //gauss seidel
            while ((diff > minerr) && (0 < iter--))
            {
                diff = 0.0;
                for (int i = 1; i < n - 1; i++)
                {
                    for (int j = 1; j < n - 1; j++)
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
            Buffer.BlockCopy(dyenew, 0, dye, 0, dye.Length * DOUBLE_SIZE);
        }

        public void AdvectVelocity()
        {
            Vector from;
            int x1, y1;
            int x2, y2;
            double s1, s2, t1, t2, viy, vix;
            double dt0 = dt * idw;
            Vector v;
            //x and y velocities are done in two steps
            //first x
            for (int i = 0; i < n + 1; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i > 0) && (i < n))
                        viy = (vy[i, j] + vy[i, j + 1] + vy[i - 1, j] + vy[i - 1, j + 1]) / 4.0;
                    else if (i == n)
                        viy = (vy[i - 1, j] + vy[i - 1, j + 1]) / 2.0;
                    else
                        viy = (vy[i, j] + vy[i, j + 1]) / 2.0;
                    v = new Vector(vx[i, j], viy, 0);
                    from = new Vector(i, j, 0) - v * dt0;

                    x1 = Clamp((int)from.x, 0, n);
                    x2 = Clamp(x1 + 1, 0, n);
                    y1 = Clamp((int)from.y, 0, n - 1);
                    y2 = Clamp(y1 + 1, 0, n - 1);

                    s1 = (double)from.x - x1; s2 = 1.0f - s1;
                    t1 = (double)from.y - y1; t2 = 1.0f - t1;
                    vxnew[i, j] =
                        t1 * (s1 * vx[x2, y2] + s2 * vx[x1, y2]) +
                        t2 * (s1 * vx[x2, y1] + s2 * vx[x1, y1]);
                }
            }
            //then y
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                    if ((j > 0) && (j < n))
                        vix = (vx[i, j] + vx[i + 1, j] + vx[i, j - 1] + vx[i + 1, j - 1]) / 4.0;
                    else if (j == n)
                        vix = (vx[i, j - 1] + vx[i + 1, j - 1]) / 2.0;
                    else
                        vix = (vx[i, j] + vx[i + 1, j]) / 2.0;
                    v = new Vector(vix, vy[i, j], 0);
                    from = new Vector(i, j, 0) - v * dt0;

                    x1 = Clamp((int)from.x, 0, n - 1);
                    x2 = Clamp(x1 + 1, 0, n - 1);
                    y1 = Clamp((int)from.y, 0, n);
                    y2 = Clamp(y1 + 1, 0, n);
                    
                    s1 = (double)from.x - x1; s2 = 1.0f - s1;
                    t1 = (double)from.y - y1; t2 = 1.0f - t1;
                    vynew[i, j] =
                        t1 * (s1 * vy[x2, y2] + s2 * vy[x1, y2]) +
                        t2 * (s1 * vy[x2, y1] + s2 * vy[x1, y1]);
                }
            }
            Buffer.BlockCopy(vxnew, 0, vx, 0, vx.Length * DOUBLE_SIZE);
            Buffer.BlockCopy(vynew, 0, vy, 0, vy.Length * DOUBLE_SIZE);
            SetBoundsX(vx);
            SetBoundsY(vy);
        }
        
        public void AdvectDensity()
        {
            Vector from, v;
            int x1, y1;
            int x2, y2;
            double s1, s2, t1, t2, vix, viy;
            double dt0 = dt * idw;
            double temp;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    vix = (vx[i, j] + vx[i + 1, j]) / 2.0;
                    viy = (vy[i, j] + vy[i, j + 1]) / 2.0;
                    v = new Vector(vix, viy, 0);
                    from = new Vector(i, j, 0) - v * dt0;

                    x1 = Clamp((int)from.x, 0, n - 1);
                    x2 = Clamp(x1 + 1, 0, n - 1);
                    y1 = Clamp((int)from.y, 0, n - 1);
                    y2 = Clamp(y1 + 1, 0, n - 1);
                    
                    s1 = (double)from.x - x1; s2 = 1.0f - s1;
                    t1 = (double)from.y - y1; t2 = 1.0f - t1;
                    temp = t1 * (s1 * dye[x2, y2] + s2 * dye[x1, y2]) +
                        t2 * (s1 * dye[x2, y1] + s2 * dye[x1, y1]);
                    temp = Math.Min(99, Math.Max(0, temp));
                    dyenew[i, j] = temp;
                }
            }
            Buffer.BlockCopy(dyenew, 0, dye, 0, dye.Length * DOUBLE_SIZE);
        }

        public void Project()
        {
            double a = 1.0f;
            double c = 4.0f;
            double diff = 10.0;
            double dp;
            int iter = maxiter;

            //pressurenew = new double[n, n];
            Buffer.BlockCopy(pressure, 0, pressurenew, 0, pressure.Length * DOUBLE_SIZE);

            //compute b=grad.w
            //b will be stored in pressure
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    pressure[i, j] = -((vx[i + 1, j] - vx[i, j])
                                   + (vy[i, j + 1] - vy[i, j]));
                }
            }
            //gauss seidel
            while ((diff > minerr) && (0 < iter--))
            {
                diff = 0.0;
                for (int i = 1; i < n - 1; i++)
                {
                    for (int j = 1; j < n - 1; j++)
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
            Buffer.BlockCopy(pressurenew, 0, pressure, 0, pressure.Length * DOUBLE_SIZE);

            for (int i = 1; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    vx[i, j] -= (pressure[i, j] - pressure[i - 1, j]);
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 1; j < n; j++)
                {
                    vy[i, j] -= (pressure[i, j] - pressure[i, j - 1]);
                }
            }

            SetBoundsX(vx);
            SetBoundsY(vy);
        }

        public void ApplyExternalForces()
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
            
            //for (int i = 0; i < n; i++)
            //{
            //    for (int j = 0; j < n; j++)
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
            DiffuseVelocity();
            Project();
            AdvectVelocity();
            ApplyExternalForces();
            Project();

            DiffuseDensity();
            AdvectDensity();
        }

        // utils
        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
