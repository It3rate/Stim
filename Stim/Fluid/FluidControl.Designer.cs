namespace Stim.Fluid
{
    partial class FluidControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fluidRender = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.fluidRender)).BeginInit();
            this.SuspendLayout();
            // 
            // fluidRender
            // 
            this.fluidRender.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fluidRender.Location = new System.Drawing.Point(0, 0);
            this.fluidRender.Name = "fluidRender";
            this.fluidRender.Size = new System.Drawing.Size(300, 300);
            this.fluidRender.TabIndex = 0;
            this.fluidRender.TabStop = false;
            this.fluidRender.Paint += new System.Windows.Forms.PaintEventHandler(this.fluidRender_Paint);
            this.fluidRender.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.fluidRender.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.fluidRender.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
            // 
            // FluidControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fluidRender);
            this.Name = "FluidControl";
            this.Size = new System.Drawing.Size(300, 300);
            ((System.ComponentModel.ISupportInitialize)(this.fluidRender)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox fluidRender;
    }
}
