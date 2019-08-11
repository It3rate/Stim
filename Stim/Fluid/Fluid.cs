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
        public double[,,] dye;
        public double[,] pressure;
        public bool[,] boundary;

        public int dyeCount = 2;
        public double dt = 0.1;
        public int n = 150;//32;
        public double visc = 0;//.001;
        public int maxiter = 4; // 120
        public double minerr = 1e-3;
        public double dw, idw;

        public double gravityX = 0.0;
        public double gravityY = 0.0; // -0.1;
        public double t;
        public Vector[] animals;
        public int animalCount = 30;

        private Random rnd = new Random(1234);
        private double[,] vxTemp;
        private double[,] vyTemp;
        private double[,] pressureTemp;
        private double[,,] dyeTemp;
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
                //Thread.Sleep(1);
            }
        }

        public void OnClose() 
        {
            quit = true;
            thread.Abort();
            thread.Join();
        }


        public void Initialize()
        {
            vx = new double[n + 1, n]; 
            vy = new double[n, n + 1];
            dye = new double[n, n, dyeCount];
            pressure = new double[n, n];
            boundary = new bool[n, n];
            animals = new Vector[animalCount];

            vxTemp = (double[,])vx.Clone();
            vyTemp = (double[,])vy.Clone();
            dyeTemp = (double[,,])dye.Clone();
            pressureTemp = (double[,])pressure.Clone();

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
                animals[i] = new Vector(rnd.Next(n - 4) + 2, rnd.Next(n - 4) + 2, 0, 0);
            }
        }

        public void SetBoundsX(double[,] vxRef)
        {
            int i, j;
            for (i = 1; i < n - 1; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if (boundary[i - 1, j]) vxRef[i, j] = 0.0;
                    if (boundary[i + 1, j]) vxRef[i, j] = 0.0;
                }
            }
        }

        public void SetBoundsY(double[,] vyRef)
        {
            int i, j;
            for (i = 0; i < n; i++)
            {
                for (j = 1; j < n - 1; j++)
                {
                    if (boundary[i, j - 1]) vyRef[i, j] = 0.0;
                    if (boundary[i, j + 1]) vyRef[i, j] = 0.0;
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
            Buffer.BlockCopy(vx, 0, vxTemp, 0, vx.Length * DOUBLE_SIZE);
            Buffer.BlockCopy(vy, 0, vyTemp, 0, vy.Length * DOUBLE_SIZE);

            //x and y velocities are done in two steps
            //first x
            while ((diff > minerr) && (0 < iter--))
            {
                diff = 0.0f;
                for (int i = 1; i < n; i++)
                {
                    for (int j = 1; j < n - 1; j++)
                    {
                        dv = vxTemp[i, j];
                        vxTemp[i, j] = (vx[i, j] +
                            a * (
                            vxTemp[i - 1, j] +
                            vxTemp[i + 1, j] +
                            vxTemp[i, j - 1] +
                            vxTemp[i, j + 1] )) / c;
                        dv -= vxTemp[i, j];
                        diff += dv * dv;
                    }
                }
                SetBoundsX(vxTemp);
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
                        dv = vyTemp[i, j];
                        vyTemp[i, j] = (vy[i, j] +
                            a * (
                            vyTemp[i - 1, j] +
                            vyTemp[i + 1, j] +
                            vyTemp[i, j - 1] +
                            vyTemp[i, j + 1] )) / c;
                        dv -= vxTemp[i, j];
                        diff += dv * dv;
                    }
                }
                SetBoundsY(vyTemp);
            }
            Buffer.BlockCopy(vxTemp, 0, vx, 0, vx.Length * DOUBLE_SIZE);
            Buffer.BlockCopy(vyTemp, 0, vy, 0, vy.Length * DOUBLE_SIZE);
        }

        public void MoveAnimals()
        {
            for (int i = 0; i < animals.Length; i++)
            {
                Vector animal = animals[i];
                int ax = Math.Max(0, Math.Min(n - 1, (int)animal.x));
                int ay = Math.Max(0, Math.Min(n - 1, (int)animal.y));
                double xd = (vx[ax, ay] + vx[ax + 1, ay] + vx[ax - 1, ay] + vx[ax, ay + 1] + vx[ax, ay - 1]) / 5.0;
                double yd = (vy[ax, ay] + vy[ax + 1, ay] + vy[ax - 1, ay] + vy[ax, ay + 1] + vy[ax, ay - 1]) / 5.0;
                //animal.x += Math.Min(10, xd * 30) + (n / 1000.0) * (rnd.NextDouble() - 0.5);
                //animal.y += Math.Min(10, yd * 30) + (n / 1000.0) * (rnd.NextDouble() - 0.5);
                animal.x += (float)(Math.Min(10, xd * 30) + animal.z);
                animal.y += (float)(Math.Min(10, yd * 30) + animal.w) + (n/1000.0); // slight gravity for animals
                animal.z += (float)((n / 1000.0) * (rnd.NextDouble() - 0.5));
                animal.w += (float)((n / 1000.0) * (rnd.NextDouble() - 0.5));

                animal.x = Math.Max(2, Math.Min(n - 3, animal.x));
                animal.y = Math.Max(2, Math.Min(n - 3, animal.y));
                animal.z = Math.Max(-.2, Math.Min(.2, animal.z));
                animal.w = Math.Max(-.2, Math.Min(.2, animal.w));

                dye[ax, ay, i % 2] += n/150.0;
            }
        }

        public void DiffuseDensity()
        {
            double diff = 10.0f;
            double dd;
            int iter = maxiter;
            double temp;
            
            //initial guess is current values
            Buffer.BlockCopy(dye, 0, dyeTemp, 0, dye.Length * DOUBLE_SIZE);

            //gauss seidel
            double a = dt * visc;
            double c = 1 + 4.0f * a;
            while ((diff > minerr) && (0 < iter--))
            {
                diff = 0.0;
                for (int i = 1; i < n - 1; i++)
                {
                    for (int j = 1; j < n - 1; j++)
                    {
                        for (int dyeType = 0; dyeType < dyeCount; dyeType++)
                        {
                            dd = dyeTemp[i, j, dyeType];
                            temp = (dye[i, j, dyeType] +
                                a * (dyeTemp[i - 1, j, dyeType] +
                                dyeTemp[i + 1, j, dyeType] +
                                dyeTemp[i, j - 1, dyeType] +
                                dyeTemp[i, j + 1, dyeType])) / c;
                            temp = Math.Min(999, Math.Max(0, temp));
                            temp -= n*0.00001; // slowly fade dye based on size (larger is slower)
                            dyeTemp[i, j, dyeType] = temp;
                            dd -= temp;
                            diff += dd * dd;
                        }

                        // attract to self
                        double dyeDiff = dyeTemp[i, j, 0] - dyeTemp[i, j, 1];
                        double inc = Math.Abs(dyeDiff * 0.1);
                        if ((dyeDiff > 0) && dyeTemp[i, j, 1] > inc)
                        {
                            dyeTemp[i, j, 0] += inc;
                            dyeTemp[i, j, 1] -= inc;
                        }
                        else if ((dyeDiff < 0) && dyeTemp[i, j, 0] > inc)
                        {
                            dyeTemp[i, j, 0] -= inc;
                            dyeTemp[i, j, 1] += inc;
                        }
                    }
                }
            }
            Buffer.BlockCopy(dyeTemp, 0, dye, 0, dye.Length * DOUBLE_SIZE);
        }

        public void AdvectVelocity()
        {
            Vector from;
            int x1, y1;
            int x2, y2;
            double s1, s2, t1, t2, viy, vix;
            double dt0 = dt * idw;
            Vector v;

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
                    vxTemp[i, j] =
                        t1 * (s1 * vx[x2, y2] + s2 * vx[x1, y2]) +
                        t2 * (s1 * vx[x2, y1] + s2 * vx[x1, y1]);
                }
            }

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
                    vyTemp[i, j] =
                        t1 * (s1 * vy[x2, y2] + s2 * vy[x1, y2]) +
                        t2 * (s1 * vy[x2, y1] + s2 * vy[x1, y1]);
                }
            }
            Buffer.BlockCopy(vxTemp, 0, vx, 0, vx.Length * DOUBLE_SIZE);
            Buffer.BlockCopy(vyTemp, 0, vy, 0, vy.Length * DOUBLE_SIZE);
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

            for (int dyeType = 0; dyeType < dyeCount; dyeType++)
            {
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

                        s1 = (double)from.x - x1;
                        s2 = 1.0f - s1;
                        t1 = (double)from.y - y1;
                        t2 = 1.0f - t1;
                        temp = t1 * (s1 * dye[x2, y2, dyeType] + s2 * dye[x1, y2, dyeType]) +
                            t2 * (s1 * dye[x2, y1, dyeType] + s2 * dye[x1, y1, dyeType]);
                        temp = Math.Min(99, Math.Max(0, temp));
                        dyeTemp[i, j, dyeType] = temp;
                    }
                }
            }
            Buffer.BlockCopy(dyeTemp, 0, dye, 0, dye.Length * DOUBLE_SIZE);
        }

        public void Project()
        {
            double a = 1.0f;
            double c = 4.0f;
            double diff = 10.0;
            double dp;
            int iter = maxiter;
     
            Buffer.BlockCopy(pressure, 0, pressureTemp, 0, pressure.Length * DOUBLE_SIZE);

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
                        dp = pressureTemp[i, j];
                        pressureTemp[i, j] = (pressure[i, j] +
                            a * (pressureTemp[i - 1, j] +
                            pressureTemp[i + 1, j] +
                            pressureTemp[i, j - 1] +
                            pressureTemp[i, j + 1])) / c;
                        dp -= pressureTemp[i, j];
                        diff += dp * dp;
                    }
                }
            }
            Buffer.BlockCopy(pressureTemp, 0, pressure, 0, pressure.Length * DOUBLE_SIZE);

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

        int volcanoX = 20;
        public void ApplyExternalForces()
        {
            t += 0.03;
            int loc = (int)(n * 0.4);
            vx[loc, loc] = (Math.Sin(t)) * (n / 200.0) + 0.15;//rnd.NextDouble() * 0.05 + 0.2;
            vy[loc, loc] = (Math.Cos(t)) * (n / 200.0) + 0.18;// rnd.NextDouble() * 0.05 + 0.175;
            if (rnd.NextDouble() < 0.1)
            {
                volcanoX = rnd.Next((int)(n * 0.8) + (int)(n * 0.1)) + 1;
            }
            vx[volcanoX, n - 3] += rnd.NextDouble() * (n / 200.0) - (n/400.0);
            vy[volcanoX, n - 3] += rnd.NextDouble() * (n / -30.0);

            // gravity
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (!boundary[i, j])
                    {
                        vx[i, j] += gravityX * dt;
                        vy[i, j] += gravityY * dt;
                    }
                }
            }
        }

        public void Step()
        {
            DiffuseVelocity();
            MoveAnimals();
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
