/*
 * Created by SharpDevelop.
 * User: Alexandre
 * Date: 2/15/2007
 * Time: 8:44 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace fluid_mac
{
	partial class MainForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.PicFluid = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.PicFluid)).BeginInit();
			this.SuspendLayout();
			// 
			// PicFluid
			// 
			this.PicFluid.Location = new System.Drawing.Point(0, 0);
			this.PicFluid.Name = "PicFluid";
            this.PicFluid.Size = new System.Drawing.Size(512, 512);// 256, 256);
			this.PicFluid.TabIndex = 0;
			this.PicFluid.TabStop = false;
			this.PicFluid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PicFluidMouseDown);
			this.PicFluid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PicFluidMouseMove);
			this.PicFluid.Paint += new System.Windows.Forms.PaintEventHandler(this.PicFluidPaint);
			this.PicFluid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PicFluidMouseUp);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(515, 515); //259, 259);
			this.Controls.Add(this.PicFluid);
			this.Name = "MainForm";
			this.Text = "fluid_mac";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainFormFormClosed);
			((System.ComponentModel.ISupportInitialize)(this.PicFluid)).EndInit();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.PictureBox PicFluid;
	}
}
