namespace InputViewer {
	partial class Viewer {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Viewer));
			this.gamePad = new InputViewer.GamePad();
			((System.ComponentModel.ISupportInitialize)(this.gamePad)).BeginInit();
			this.SuspendLayout();
			// 
			// gamePad
			// 
			this.gamePad.BackColor = System.Drawing.Color.Transparent;
			this.gamePad.BackgroundColor = System.Drawing.Color.Empty;
			this.gamePad.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gamePad.ErrorImage = null;
			this.gamePad.InitialImage = null;
			this.gamePad.Location = new System.Drawing.Point(0, 0);
			this.gamePad.Name = "gamePad";
			this.gamePad.Size = new System.Drawing.Size(469, 213);
			this.gamePad.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.gamePad.TabIndex = 0;
			this.gamePad.TabStop = false;
			// 
			// Viewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.ClientSize = new System.Drawing.Size(469, 213);
			this.Controls.Add(this.gamePad);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Viewer";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Input Viewer";
			((System.ComponentModel.ISupportInitialize)(this.gamePad)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private GamePad gamePad;
	}
}

