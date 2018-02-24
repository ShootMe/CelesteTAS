using System;
using System.Text;
namespace TAS {
	[Flags]
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
		Restart = 256
	}
	public class InputRecord {
		public int Line { get; set; }
		public int Frames { get; set; }
		public Actions Actions { get; set; }
		public bool FastForward { get; set; }
		public InputRecord() { }
		public InputRecord(int number, string line) {
			Line = number;

			int index = 0;
			Frames = ReadFrames(line, ref index);
			if (Frames == 0) {
				if (line == "***") {
					FastForward = true;
				}
				return;
			}

			while (index < line.Length) {
				char c = line[index];

				switch (char.ToUpper(c)) {
					case 'L': Actions ^= Actions.Left; break;
					case 'R': Actions ^= Actions.Right; break;
					case 'U': Actions ^= Actions.Up; break;
					case 'D': Actions ^= Actions.Down; break;
					case 'J': Actions ^= Actions.Jump; break;
					case 'X': Actions ^= Actions.Dash; break;
					case 'G': Actions ^= Actions.Grab; break;
					case 'S': Actions ^= Actions.Start; break;
					case 'Q': Actions ^= Actions.Restart; break;
				}

				index++;
			}
		}
		private int ReadFrames(string line, ref int start) {
			bool foundFrames = false;
			int frames = 0;

			while (start < line.Length) {
				char c = line[start];

				if (!foundFrames) {
					if (char.IsDigit(c)) {
						foundFrames = true;
						frames = c ^ 0x30;
					} else if (c != ' ') {
						return frames;
					}
				} else if (char.IsDigit(c)) {
					if (frames < 9999) {
						frames = frames * 10 + (c ^ 0x30);
					} else {
						frames = 9999;
					}
				} else if (c != ' ') {
					return frames;
				}

				start++;
			}

			return frames;
		}
		public bool HasActions(Actions actions) {
			return (Actions & actions) != 0;
		}
		public override string ToString() {
			return Frames == 0 ? string.Empty : Frames.ToString().PadLeft(4, ' ') + ActionsToString();
		}
		public string ActionsToString() {
			StringBuilder sb = new StringBuilder();
			if (HasActions(Actions.Left)) { sb.Append(",L"); }
			if (HasActions(Actions.Right)) { sb.Append(",R"); }
			if (HasActions(Actions.Up)) { sb.Append(",U"); }
			if (HasActions(Actions.Down)) { sb.Append(",D"); }
			if (HasActions(Actions.Jump)) { sb.Append(",J"); }
			if (HasActions(Actions.Dash)) { sb.Append(",X"); }
			if (HasActions(Actions.Grab)) { sb.Append(",G"); }
			if (HasActions(Actions.Start)) { sb.Append(",S"); }
			if (HasActions(Actions.Restart)) { sb.Append(",Q"); }
			return sb.ToString();
		}
		public override bool Equals(object obj) {
			return obj is InputRecord && ((InputRecord)obj) == this;
		}
		public override int GetHashCode() {
			return Frames ^ (int)Actions;
		}
		public static bool operator ==(InputRecord one, InputRecord two) {
			bool oneNull = (object)one == null;
			bool twoNull = (object)two == null;
			if (oneNull != twoNull) {
				return false;
			} else if (oneNull && twoNull) {
				return true;
			}
			return one.Actions == two.Actions;
		}
		public static bool operator !=(InputRecord one, InputRecord two) {
			bool oneNull = (object)one == null;
			bool twoNull = (object)two == null;
			if (oneNull != twoNull) {
				return true;
			} else if (oneNull && twoNull) {
				return false;
			}
			return one.Actions != two.Actions;
		}
	}
}
