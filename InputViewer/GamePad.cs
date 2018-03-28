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
		Jump1 = 16,
		Dash1 = 32,
		Jump2 = 64,
		Dash2 = 128,
		RightBumper = 256,
		LeftBumper = 512,
		Start = 1024,
		Select = 2048,
		Analog = 4096
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
		private Bitmap button, dpad, middle, pad, bumper;
		private GameMemory memory;
		private Actions actions;
		private float percent;
		public static Stream ReadResourceStream(string path) {
			Assembly current = Assembly.GetExecutingAssembly();
			return current.GetManifestResourceStream(typeof(GamePad).Namespace + "." + path);
		}
		public GamePad() {
			memory = new GameMemory();
			using (Stream stream = ReadResourceStream("Images.button.png")) {
				button = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.dpad.png")) {
				dpad = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.middle.png")) {
				middle = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.bumper.png")) {
				bumper = new Bitmap(stream);
			}
			using (Stream stream = ReadResourceStream("Images.pad.png")) {
				pad = new Bitmap(stream);
			}
			Thread game = new Thread(UpdateViewer);
			game.IsBackground = true;
			game.Start();
		}

		private void UpdateViewer() {
			while (true) {
				try {
					bool skipSleep = false;
					if (memory.HookProcess()) {
						GamepadState state = memory.GamePadState();
						Actions newActions = Actions.None;
						newActions |= state.DPad.Up ? Actions.Up : Actions.None;
						newActions |= state.DPad.Down ? Actions.Down : Actions.None;
						newActions |= state.DPad.Left ? Actions.Left : Actions.None;
						newActions |= state.DPad.Right ? Actions.Right : Actions.None;
						newActions |= state.Buttons.A ? Actions.Jump1 : Actions.None;
						newActions |= state.Buttons.B ? Actions.Dash1 : Actions.None;
						newActions |= state.Buttons.Y ? Actions.Jump2 : Actions.None;
						newActions |= state.Buttons.X ? Actions.Dash2 : Actions.None;
						newActions |= state.Buttons.RightShoulder || state.Triggers.Right > 0.5 ? Actions.RightBumper : Actions.None;
						newActions |= state.Buttons.LeftShoulder || state.Triggers.Left > 0.5 ? Actions.LeftBumper : Actions.None;
						newActions |= state.Buttons.Start ? Actions.Start : Actions.None;
						newActions |= state.Buttons.Back ? Actions.Select : Actions.None;
						int newDirection = 0;
						if (state.ThumbSticks.LeftX != 0 || state.ThumbSticks.LeftY != 0) {
							newActions |= Actions.Analog;
							newDirection = (int)(Math.Atan2(state.ThumbSticks.LeftX, state.ThumbSticks.LeftY) * 180 / Math.PI);
							if (newDirection < 0) { newDirection += 360; }
						}
						if (actions != newActions || newDirection != direction) {
							actions = newActions;
							direction = newDirection;
							skipSleep = true;
							Invoke((Action)Refresh);
						}
					}
					if (!skipSleep) {
						Thread.Sleep(10);
					}
				} catch { }
			}
		}
		public bool HasAction(Actions action) {
			return (actions & action) != 0;
		}
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			percent = (float)Width / 469f;
			float hp = (float)Height / 213f;
			percent = Math.Min(percent, hp);
			Refresh();
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

			e.Graphics.DrawImage(pad, 0, 0, (int)(468 * percent), (int)(212 * percent));

			if (HasAction(Actions.Analog)) {
				using (Pen pen = new Pen(Color.Aqua, 8 * percent)) {
					e.Graphics.DrawLine(pen, (int)(101 * percent), (int)(105 * percent), (int)(101 * percent) + (int)(GetX() * 59 * percent), (int)(105 * percent) - (int)(GetY() * 59 * percent));
				}
			}
			if (HasAction(Actions.Start)) {
				e.Graphics.DrawImage(middle, (int)(230 * percent), (int)(105 * percent), (int)(33 * percent), (int)(31 * percent));
			}
			if (HasAction(Actions.Select)) {
				e.Graphics.DrawImage(middle, (int)(178 * percent), (int)(105 * percent), (int)(33 * percent), (int)(31 * percent));
			}
			if (HasAction(Actions.Jump1)) {
				e.Graphics.DrawImage(button, (int)(351 * percent), (int)(127 * percent), (int)(32 * percent), (int)(32 * percent));
			}
			if (HasAction(Actions.Jump2)) {
				e.Graphics.DrawImage(button, (int)(353 * percent), (int)(52 * percent), (int)(32 * percent), (int)(32 * percent));
			}
			if (HasAction(Actions.Dash1)) {
				e.Graphics.DrawImage(button, (int)(394 * percent), (int)(90 * percent), (int)(32 * percent), (int)(32 * percent));
			}
			if (HasAction(Actions.Dash2)) {
				e.Graphics.DrawImage(button, (int)(310 * percent), (int)(89 * percent), (int)(32 * percent), (int)(32 * percent));
			}
			if (HasAction(Actions.LeftBumper)) {
				e.Graphics.DrawImage(bumper, (int)(53 * percent), 0, (int)(101 * percent), (int)(23 * percent));
			}
			if (HasAction(Actions.RightBumper)) {
				using (Bitmap newImg = new Bitmap(bumper)) {
					newImg.RotateFlip(RotateFlipType.RotateNoneFlipX);
					e.Graphics.DrawImage(newImg, (int)(314 * percent), 0, (int)(101 * percent), (int)(23 * percent));
				}
			}
			if (HasAction(Actions.Up)) {
				using (Bitmap newImg = new Bitmap(dpad)) {
					newImg.RotateFlip(RotateFlipType.RotateNoneFlipY);
					e.Graphics.DrawImage(newImg, (int)(85 * percent), (int)(63 * percent), (int)(32 * percent), (int)(27 * percent));
				}
			}
			if (HasAction(Actions.Down)) {
				e.Graphics.DrawImage(dpad, (int)(85 * percent), (int)(122 * percent), (int)(32 * percent), (int)(27 * percent));
			}
			if (HasAction(Actions.Left)) {
				using (Bitmap newImg = new Bitmap(dpad)) {
					newImg.RotateFlip(RotateFlipType.Rotate90FlipY);
					e.Graphics.DrawImage(newImg, (int)(58 * percent), (int)(90 * percent), (int)(27 * percent), (int)(32 * percent));
				}
			}
			if (HasAction(Actions.Right)) {
				using (Bitmap newImg = new Bitmap(dpad)) {
					newImg.RotateFlip(RotateFlipType.Rotate270FlipNone);
					e.Graphics.DrawImage(newImg, (int)(117 * percent), (int)(90 * percent), (int)(27 * percent), (int)(32 * percent));
				}
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