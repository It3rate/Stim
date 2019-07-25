namespace Stim
{
	partial class WorldView
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.worldGridView = new Stim.Controls.WorldGridView();
            this.SuspendLayout();
            // 
            // worldGridView
            // 
            this.worldGridView.Location = new System.Drawing.Point(22, 45);
            this.worldGridView.Name = "worldGridView";
            this.worldGridView.Size = new System.Drawing.Size(1367, 970);
            this.worldGridView.TabIndex = 0;
            this.worldGridView.Text = "worldGridView1";
            // 
            // WorldView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1401, 1027);
            this.Controls.Add(this.worldGridView);
            this.Name = "WorldView";
            this.Text = "Form1";
            this.ResumeLayout(false);

		}

        #endregion

        private Controls.WorldGridView worldGridView;
    }
}

