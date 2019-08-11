using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace Stim.Fluid
{

    public partial class FluidControl : UserControl
    {
        [DllImport("shlwapi.dll")]
        public static extern int ColorHLSToRGB(int H, int L, int S);

        public int startX;
        public int startY;
        public int mouseX;
        public int mouseY;
        public bool isMoving = false;
        public bool isLeftDown = false;
        public bool isRightDown = false;

        public bool showVelocity = false;

        float pixelWidth;
        float pixelHeight;

        private FluidSolver solver;
        public FluidSolver Solver
        {
            get
            {
                return solver;
            }
            set
            {
                solver = value;
                pixelWidth = fluidRender.Width / (float)solver.n;
                pixelHeight = fluidRender.Height / (float)solver.n;
            }
        }

        public FluidControl()
        {
            InitializeComponent();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            Solver.OnClose();
        }

        #region Mouse
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            startX = (int)(e.X / pixelWidth);
            startY = (int)(e.Y / pixelHeight);
            mouseX = startX;
            mouseY = startY;

            if (e.Button == MouseButtons.Middle)
            {
                Solver.boundary[mouseX, mouseY] = true;
                Solver.dye[mouseX, mouseY, 0] = 0;
                Solver.dye[mouseX, mouseY, 1] = 0;
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

        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X < 0 ? 0 : e.X >= fluidRender.Width ? fluidRender.Width - 1 : e.X;
            int y = e.Y < 0 ? 0 : e.Y >= fluidRender.Height ? fluidRender.Height - 1 : e.Y;
            mouseX = (int)(e.X / pixelWidth);
            mouseY = (int)(e.Y / pixelHeight);
            isMoving = true;

            if (e.Button == MouseButtons.Middle)
            {
                Solver.boundary[mouseX, mouseY] = true;
                Solver.dye[mouseX, mouseY, 0] = 0;
                Solver.dye[mouseX, mouseY, 1] = 0;
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
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            startX = (int)(e.X / pixelWidth);
            startY = (int)(e.Y / pixelHeight);
            mouseX = startX;
            mouseY = startY;
            isLeftDown = false;
            isRightDown = false;
            isMoving = false;
        }

        #endregion

        #region Paint
        private void fluidRender_Paint(object sender, PaintEventArgs e)
        { 
            int i, j;
            double d0, d1;
            Vector v;
            if ((mouseX != startX) || (mouseY != startY))
            {
                if (isLeftDown) Solver.vx[mouseX, mouseY] += (mouseX - startX) * 1300.0 * Solver.dw / fluidRender.Width;
                if (isLeftDown) Solver.vy[mouseX, mouseY] += (mouseY - startY) * 1300.0 * Solver.dw / fluidRender.Height;
            }
            if (isRightDown) Solver.dye[mouseX, mouseY, 0] += Solver.n;
            if (isLeftDown) Solver.dye[mouseX, mouseY, 1] += Solver.n;

            for (i = 0; i < Solver.n; i++)
            {
                for (j = 0; j < Solver.n; j++)
                {
                    if (Solver.boundary[i, j])
                    {
                        DrawBoundryBlock(e.Graphics, i, j, 0, 0, 10);
                    }
                    else
                    {
                        d0 = Solver.dye[i, j, 0] / 10.0;
                        if (d0 < 0.0) d0 = 0.0;
                        if (d0 > 1.0) d0 = 1.0;
                        d1 = Solver.dye[i, j, 1] / 10.0;
                        if (d1 < 0.0) d1 = 0.0;
                        if (d1 > 1.0) d1 = 1.0;
                        DrawBoundryBlock(e.Graphics, i, j, d0, d1, 0);
                    }

                    if (showVelocity)
                    {
                        v = new Vector(0.5 * (Solver.vx[i, j] + Solver.vx[i + 1, j]),
                                     0.5 * (Solver.vy[i, j] + Solver.vy[i, j + 1]), 0);
                        DrawVelocity(e.Graphics, i, j, v);
                    }
                }
            }

            for (i = 0; i < Solver.animals.Length; i++)
            {
                e.Graphics.FillEllipse(Brushes.White, (float)Solver.animals[i].x * pixelWidth, (float)Solver.animals[i].y * pixelHeight, 10, 10);
            }
        }
        Color blockColor = Color.FromArgb(50, 40, 15);
        void DrawBoundryBlock(Graphics gfx, double x, double y, double val0, double val1, int type)
        {
            Color c;
            if (type == 10)
            {
                c = blockColor;
            }
            else
            {
                c = Color.FromArgb(
                    (int)(Math.Sin(val0 * (Math.PI / 2.0)) * 255),
                    (int)(Math.Sin(val1 * (Math.PI / 2.0)) * 255),
                    0);
            }

            Brush brush = new SolidBrush(c);
            float rx = (float)x * pixelWidth;
            float ry = (float)y * pixelHeight;
            gfx.FillRectangle(brush, rx, ry, (float)Math.Ceiling(pixelWidth + 0.5), (float)Math.Ceiling(pixelHeight + 0.5));
        }

        void DrawVelocity(Graphics gfx, double x, double y, Vector v)
        {
            if (Math.Abs(v.x) + Math.Abs(v.y) > 0.01)
            {
                Pen pen = new Pen(Brushes.Black);
                float rx = (float)(x + 0.5f) * pixelWidth;
                float ry = (float)(y + 0.5f) * pixelHeight;
                pen.Color = Color.FromArgb(255,
                    0,
                    (int)(Math.Min(1.0, Math.Abs(v.x * 40)) * 32),
                    (int)(Math.Min(1.0, Math.Abs(v.y * 40)) * 64 + 10));//, (int)(b * 255));

                gfx.DrawLine(pen, rx, ry, rx + (float)v.x * 500.0f, ry + (float)v.y * 500.0f);
            }
        }
        #endregion
    }
}
