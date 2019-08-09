using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stim.Fluid
{
    public partial class FluidControl : UserControl
    {
        public FluidSolver Solver { get; set; }

        public int startX;
        public int startY;
        public int mouseX;
        public int mouseY;
        public bool isMoving = false;
        public bool isLeftDown = false;
        public bool isRightDown = false;

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
            startX = (int)(Solver.n * e.X / fluidRender.Width);
            startY = (int)(Solver.n * e.Y / fluidRender.Height);
            mouseX = startX;
            mouseY = startY;

            if (e.Button == MouseButtons.Middle)
            {
                Solver.boundary[mouseX, mouseY] = true;
                Solver.dye[mouseX, mouseY] = 0;
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
            mouseX = (int)(Solver.n * x / fluidRender.Width);
            mouseY = (int)(Solver.n * y / fluidRender.Height);
            isMoving = true;

            if (e.Button == MouseButtons.Middle)
            {
                Solver.boundary[mouseX, mouseY] = true;
                Solver.dye[mouseX, mouseY] = 0;
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
            startX = (int)(Solver.n * e.X / fluidRender.Width);
            startY = (int)(Solver.n * e.Y / fluidRender.Height);
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
            double d;
            Vector v;
            if ((mouseX != startX) || (mouseY != startY))
            {
                if (isLeftDown) Solver.vx[mouseX, mouseY] += (mouseX - startX) * 1300.0 * Solver.dw / fluidRender.Width;
                if (isLeftDown) Solver.vy[mouseX, mouseY] += (mouseY - startY) * 1300.0 * Solver.dw / fluidRender.Height;
            }
            if (isRightDown) Solver.dye[mouseX, mouseY] += 45.0f;

            for (i = 0; i < Solver.n; i++)
            {
                for (j = 0; j < Solver.n; j++)
                {
                    d = Solver.dye[i, j] / 10.0;
                    if (d < 0.0) d = 0.0;
                    if (d > 1.0) d = 1.0;
                    if (Solver.boundary[i, j])
                        DrawBoundryBlock(e.Graphics, i, j, 0.0, 0.2, 0.5);
                    else
                        DrawBoundryBlock(e.Graphics, i, j, d, d, d / 4.0);
                    v = new Vector(0.5 * (Solver.vx[i, j] + Solver.vx[i + 1, j]),
                                 0.5 * (Solver.vy[i, j] + Solver.vy[i, j + 1]), 0);
                    DrawVelocity(e.Graphics, i, j, v, 0.1, 0.0, 0.4);
                }
            }
            float pxw = fluidRender.Width / Solver.n;
            float pxh = fluidRender.Height / Solver.n;
            for (i = 0; i < Solver.animals.Length; i++)
            {
                e.Graphics.FillEllipse(Brushes.White, (float)Solver.animals[i].x * pxw, (float)Solver.animals[i].y * pxh, 10, 10);
            }
        }
        void DrawBoundryBlock(Graphics gfx, double x, double y, double r, double g, double b)
        {
            Pen pen = new Pen(Brushes.Black);
            pen.Color = Color.FromArgb(255, (int)(r * 255), (int)(g * 255), (int)(b * 255));
            Brush brush = new SolidBrush(pen.Color);
            float rx = (float)x / Solver.n * fluidRender.Width;
            float ry = (float)y / Solver.n * fluidRender.Height;
            float w = fluidRender.Width / Solver.n;
            float h = fluidRender.Height / Solver.n;
            gfx.FillRectangle(brush, rx, ry, w, h);
        }

        void DrawVelocity(Graphics gfx, double x, double y, Vector v, double r, double g, double b)
        {
            Pen pen = new Pen(Brushes.Black);
            float rx = (float)(x + 0.5f) / Solver.n * fluidRender.Width;
            float ry = (float)(y + 0.5f) / Solver.n * fluidRender.Height;
            float w = fluidRender.Width / Solver.n;
            float h = fluidRender.Height / Solver.n;
            pen.Color = Color.FromArgb(255,
                (int)(Math.Min(1.0, Math.Abs(v.x * 40)) * 255),
                0,
                (int)(Math.Min(1.0, Math.Abs(v.y * 40)) * 255));//, (int)(b * 255));

            gfx.DrawLine(pen, rx, ry, rx + (float)v.x * 500.0f, ry + (float)v.y * 500.0f);
        }
        #endregion
    }
}
