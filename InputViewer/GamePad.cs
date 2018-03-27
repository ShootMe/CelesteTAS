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
		Jump2 = 2048,
		Select = 4096
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
		private string inputs;
		private Actions actions;
		private float percent;
		public static Stream ReadResourceStream(string path) {
			Assembly current = Assembly.GetExecutingAssembly();
			return current.GetManifestResourceStream(typeof(GamePad).Namespace + "." + path);
		}
		public GamePad() {
			inputs = string.Empty;
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
					if (memory.HookProcess()) {
#if !Normal
					string newInputs = UpdateInputs();
					if (newInputs == null) {
#endif
						GamepadState state = memory.GamePadState();
						Actions newActions = Actions.None;
						newActions |= state.DPad.Up ? Actions.Up : Actions.None;
						newActions |= state.DPad.Down ? Actions.Down : Actions.None;
						newActions |= state.DPad.Left ? Actions.Left : Actions.None;
						newActions |= state.DPad.Right ? Actions.Right : Actions.None;
						newActions |= state.Buttons.A ? Actions.Jump : Actions.None;
						newActions |= state.Buttons.X ? Actions.Journal : Actions.None;
						newActions |= state.Buttons.Y ? Actions.Jump2 : Actions.None;
						newActions |= state.Buttons.B ? Actions.Dash : Actions.None;
						newActions |= state.Buttons.RightShoulder || state.Triggers.Right > 0.5 ? Actions.Grab : Actions.None;
						newActions |= state.Buttons.LeftShoulder || state.Triggers.Left > 0.5 ? Actions.Restart : Actions.None;
						newActions |= state.Buttons.Start ? Actions.Start : Actions.None;
						newActions |= state.Buttons.Back ? Actions.Select : Actions.None;
						int newDirection = 0;
						if (state.ThumbSticks.LeftX != 0 || state.ThumbSticks.LeftY != 0) {
							newActions |= Actions.Feather;
							newDirection = (int)(Math.Atan2(state.ThumbSticks.LeftX, state.ThumbSticks.LeftY) * 180 / Math.PI);
							if (newDirection < 0) { newDirection += 360; }
						}
						if (actions != newActions || newDirection != direction) {
							actions = newActions;
							direction = newDirection;
							Invoke((Action)Refresh);
						}
#if !Normal
					} else if (newInputs != inputs) {
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
#endif
					}
					Thread.Sleep(10);
				} catch { }
			}
		}
		private string UpdateInputs() {
			string input = memory.TASOutput();
			if (string.IsNullOrEmpty(input)) { return null; }

			int start = input.IndexOf(',');
			if (start < 0) { return string.Empty; }

			int end = input.IndexOf('(');
			if (end < 0) { return string.Empty; }

			return input.Substring(start + 1, end - start - 1);
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

			e.Graphics.DrawImage(pad, 0, 0, 468 * percent, 212 * percent);

			if (HasAction(Actions.Feather)) {
				using (Pen pen = new Pen(Color.Aqua, 8 * percent)) {
					e.Graphics.DrawLine(pen, 101 * percent, 105 * percent, 101 * percent + GetX() * 59 * percent, 105 * percent - GetY() * 59 * percent);
				}
			}
			if (HasAction(Actions.Start)) {
				e.Graphics.DrawImage(middle, 230 * percent, 105 * percent, 33 * percent, 31 * percent);
			}
			if (HasAction(Actions.Select)) {
				e.Graphics.DrawImage(middle, 178 * percent, 105 * percent, 33 * percent, 31 * percent);
			}
			if (HasAction(Actions.Journal)) {
				e.Graphics.DrawImage(button, 310 * percent, 89 * percent, 32 * percent, 32 * percent);
			}
			if (HasAction(Actions.Dash)) {
				e.Graphics.DrawImage(button, 394 * percent, 90 * percent, 32 * percent, 32 * percent);
			}
			if (HasAction(Actions.Jump2)) {
				e.Graphics.DrawImage(button, 353 * percent, 52 * percent, 32 * percent, 32 * percent);
			}
			if (HasAction(Actions.Jump)) {
				e.Graphics.DrawImage(button, 351 * percent, 127 * percent, 32 * percent, 32 * percent);
			}
			if (HasAction(Actions.Grab)) {
				using (Bitmap newImg = new Bitmap(bumper)) {
					newImg.RotateFlip(RotateFlipType.RotateNoneFlipX);
					e.Graphics.DrawImage(newImg, 314 * percent, 0, 101 * percent, 23 * percent);
				}
			}
			if (HasAction(Actions.Restart)) {
				e.Graphics.DrawImage(bumper, 53 * percent, 0, 101 * percent, 23 * percent);
			}
			if (HasAction(Actions.Up)) {
				using (Bitmap newImg = new Bitmap(dpad)) {
					newImg.RotateFlip(RotateFlipType.RotateNoneFlipY);
					e.Graphics.DrawImage(newImg, 85 * percent, 63 * percent, 32 * percent, 27 * percent);
				}
			}
			if (HasAction(Actions.Down)) {
				e.Graphics.DrawImage(dpad, 85 * percent, 122 * percent, 32 * percent, 27 * percent);
			}
			if (HasAction(Actions.Left)) {
				using (Bitmap newImg = new Bitmap(dpad)) {
					newImg.RotateFlip(RotateFlipType.Rotate90FlipY);
					e.Graphics.DrawImage(newImg, 58 * percent, 90 * percent, 27 * percent, 32 * percent);
				}
			}
			if (HasAction(Actions.Right)) {
				using (Bitmap newImg = new Bitmap(dpad)) {
					newImg.RotateFlip(RotateFlipType.Rotate270FlipNone);
					e.Graphics.DrawImage(newImg, 117 * percent, 90 * percent, 27 * percent, 32 * percent);
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