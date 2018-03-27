using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
namespace InputViewer {
	public enum Actions {
		None,
		Left = 1,
		Right = 2,
		Up = 4,
		Down = 8,
		Jump = 16,
		Dash = 32,
		Grab = 64,
		Start = 128,
		Restart = 256,
		Feather = 512,
		Journal = 1024,
		Jump2 = 2048
	}
	public class GamePad : PictureBox {
		[DefaultValue(0)]
		public int Opacity { get { return opacity; } set { opacity = value; Invalidate(); } }
		[DefaultValue(typeof(Color), "Transparent")]
		public Color BackgroundColor { get { return backColor; } set { backColor = value; Invalidate(); } }
		[Browsable(false)]
		public new Image InitialImage { get; set; }
		[Browsable(false)]
		public new Image ErrorImage { get; set; }
		[Browsable(false)]
		public new Color BackColor { get { return base.BackColor; } set { base.BackColor = value; } }
		private int opacity, direction;
		private Color backColor;
		private Bitmap red, green, blue, yellow, middle, pad, leftBumper, rightBumper, leftDpad, rightDpad, upDpad, downDpad;
		private GameMemory memory;
		private string inputs;
		private Actions actions;
		public static Stream ReadResourceStream(string path) {
			Assembly current = Assembly.GetExecutingAssembly();
			return current.GetManifestResourceStream(typeof(GamePad).Namespace + "." + path);
		}
		public GamePad() {
			inputs = string.Empty;
			memory = new GameMemory();
			using (Stream stream = ReadResourceStream("Images.blue.png")) {
				blue = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.green.png")) {
				green = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.yellow.png")) {
				yellow = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.red.png")) {
				red = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.middle.png")) {
				middle = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.leftbumper.png")) {
				leftBumper = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.rightbumper.png")) {
				rightBumper = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.pad.png")) {
				pad = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.up.png")) {
				upDpad = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.down.png")) {
				downDpad = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.left.png")) {
				leftDpad = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.right.png")) {
				rightDpad = new Bitmap(stream);
			}
			Thread game = new Thread(UpdateViewer);
			game.IsBackground = true;
			game.Start();
		}

		private void UpdateViewer() {
			while (true) {
				try {
					string newInputs = UpdateInputs();
					if (newInputs != inputs) {
						inputs = newInputs;
						actions = Actions.None;
						for (int i = 0; i < inputs.Length; i++) {
							char c = inputs[i];
							switch (c) {
								case 'R': actions |= Actions.Right; break;
								case 'L': actions |= Actions.Left; break;
								case 'U': actions |= Actions.Up; break;
								case 'D': actions |= Actions.Down; break;
								case 'J': actions |= Actions.Jump; break;
								case 'K': actions |= Actions.Jump2; break;
								case 'X': actions |= Actions.Dash; break;
								case 'G': actions |= Actions.Grab; break;
								case 'S': actions |= Actions.Start; break;
								case 'Q': actions |= Actions.Restart; break;
								case 'F':
									direction = 0;
									int.TryParse(inputs.Substring(i + 2), out direction);
									actions |= Actions.Feather;
									break;
							}
						}
						Invoke((Action)Refresh);
					}
					Thread.Sleep(10);
				} catch { }
			}
		}
		private string UpdateInputs() {
			if (!memory.HookProcess()) { return string.Empty; }

			string input = memory.TASOutput();
			if (string.IsNullOrEmpty(input)) { return string.Empty; }

			int start = input.IndexOf(',');
			if (start < 0) { return string.Empty; }

			int end = input.IndexOf('(');
			if (end < 0) { return string.Empty; }

			return input.Substring(start + 1, end - start - 1);
		}
		public bool HasAction(Actions action) {
			return (actions & action) != 0;
		}
		protected override void OnPaintBackground(PaintEventArgs e) {
			base.OnPaintBackground(e);
			Graphics g = e.Graphics;

			if (Parent != null) {
				base.BackColor = Color.Transparent;
				int index = Parent.Controls.GetChildIndex(this);

				for (int i = Parent.Controls.Count - 1; i > index; i--) {
					Control c = Parent.Controls[i];
					if (c.Bounds.IntersectsWith(Bounds) && c.Visible) {
						Bitmap bmp = new Bitmap(c.Width, c.Height, g);
						c.DrawToBitmap(bmp, c.ClientRectangle);

						g.TranslateTransform(c.Left - Left, c.Top - Top);
						g.DrawImageUnscaled(bmp, Point.Empty);
						g.TranslateTransform(Left - c.Left, Top - c.Top);
						bmp.Dispose();
					}
				}
				g.FillRectangle(new SolidBrush(Color.FromArgb(Opacity * 255 / 100, BackgroundColor)), this.ClientRectangle);
			} else {
				g.Clear(Color.Transparent);
				g.FillRectangle(new SolidBrush(Color.FromArgb(Opacity * 255 / 100, BackgroundColor)), this.ClientRectangle);
			}
		}
		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);

			e.Graphics.DrawImage(pad, 0, 0, 468, 212);

			if (HasAction(Actions.Feather)) {
				using (Pen pen = new Pen(Color.Aqua, 8)) {
					e.Graphics.DrawLine(pen, 101, 105, 101 + GetX() * 59, 105 - GetY() * 59);
				}
			}
			if (HasAction(Actions.Start)) {
				e.Graphics.DrawImage(middle, 230, 105, 33, 31);
			}
			//if (HasAction(Actions.Restart)) {
			//	e.Graphics.DrawImage(middle, 178, 105, 33, 31);
			//}
			if (HasAction(Actions.Journal)) {
				e.Graphics.DrawImage(green, 310, 89, 32, 32);
			}
			if (HasAction(Actions.Dash)) {
				e.Graphics.DrawImage(red, 394, 90, 32, 32);
			}
			if (HasAction(Actions.Jump2)) {
				e.Graphics.DrawImage(blue, 353, 52, 32, 32);
			}
			if (HasAction(Actions.Jump)) {
				e.Graphics.DrawImage(yellow, 351, 127, 32, 32);
			}
			if (HasAction(Actions.Grab)) {
				e.Graphics.DrawImage(rightBumper, 314, 0, 101, 23);
			}
			if (HasAction(Actions.Restart)) {
				e.Graphics.DrawImage(leftBumper, 53, 0, 101, 23);
			}
			if (HasAction(Actions.Up)) {
				e.Graphics.DrawImage(upDpad, 85, 63, 32, 27);
			}
			if (HasAction(Actions.Down)) {
				e.Graphics.DrawImage(downDpad, 85, 122, 32, 27);
			}
			if (HasAction(Actions.Left)) {
				e.Graphics.DrawImage(leftDpad, 58, 90, 27, 32);
			}
			if (HasAction(Actions.Right)) {
				e.Graphics.DrawImage(rightDpad, 117, 90, 27, 32);
			}
		}
		public float GetX() {
			return (float)Math.Sin(direction * Math.PI / 180.0);
		}
		public float GetY() {
			return (float)Math.Cos(direction * Math.PI / 180.0);
		}
	}
}