using Celeste;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace TAS {
	public class InputController {
		private List<InputRecord> inputs = new List<InputRecord>();
		private int currentFrame, inputIndex, frameToNext;
		private string filePath;
		private List<InputRecord> fastForwards = new List<InputRecord>();
		public InputController(string filePath) {
			this.filePath = filePath;
		}

		public bool CanPlayback { get { return inputIndex < inputs.Count; } }
		public bool HasFastForward { get { return fastForwards.Count > 0; } }
		public int FastForwardSpeed { get { return fastForwards.Count == 0 ? 1 : fastForwards[0].Frames == 0 ? 400 : fastForwards[0].Frames; } }
		public int CurrentFrame { get { return currentFrame; } }
		public int CurrentInputFrame { get { return currentFrame - frameToNext + Current.Frames; } }
		public InputRecord Current { get; set; }
		public InputRecord Previous {
			get {
				if (frameToNext != 0 && inputIndex - 1 >= 0 && inputs.Count > 0) {
					return inputs[inputIndex - 1];
				}
				return null;
			}
		}
		public InputRecord Next {
			get {
				if (frameToNext != 0 && inputIndex + 1 < inputs.Count) {
					return inputs[inputIndex + 1];
				}
				return null;
			}
		}
		public bool HasInput(Actions action) {
			InputRecord input = Current;
			return input.HasActions(action);
		}
		public bool HasInputPressed(Actions action) {
			InputRecord input = Current;

			return input.HasActions(action) && CurrentInputFrame == 1;
		}
		public bool HasInputReleased(Actions action) {
			InputRecord current = Current;
			InputRecord previous = Previous;

			return !current.HasActions(action) && previous != null && previous.HasActions(action) && CurrentInputFrame == 1;
		}
		public override string ToString() {
			if (frameToNext == 0 && Current != null) {
				return Current.ToString() + "(" + currentFrame.ToString() + ")";
			} else if (inputIndex < inputs.Count && Current != null) {
				int inputFrames = Current.Frames;
				int startFrame = frameToNext - inputFrames;
				return Current.ToString() + "(" + (currentFrame - startFrame).ToString() + " / " + inputFrames + " : " + currentFrame + ")";
			}
			return string.Empty;
		}
		public string NextInput() {
			if (frameToNext != 0 && inputIndex + 1 < inputs.Count) {
				return inputs[inputIndex + 1].ToString();
			}
			return string.Empty;
		}
		public void InitializePlayback() {
			int trycount = 5;
			while (!ReadFile() && trycount >= 0) {
				System.Threading.Thread.Sleep(50);
				trycount--;
			}

			currentFrame = 0;
			inputIndex = 0;
			if (inputs.Count > 0) {
				Current = inputs[0];
				frameToNext = Current.Frames;
			} else {
				Current = new InputRecord();
				frameToNext = 1;
			}
		}
		public void ReloadPlayback() {
			int playedBackFrames = currentFrame;
			InitializePlayback();
			currentFrame = playedBackFrames;

			while (currentFrame >= frameToNext) {
				if (inputIndex + 1 >= inputs.Count) {
					inputIndex++;
					return;
				}
				if (Current.FastForward) {
					fastForwards.RemoveAt(0);
				}
				Current = inputs[++inputIndex];
				frameToNext += Current.Frames;
			}
		}
		public void InitializeRecording() {
			currentFrame = 0;
			inputIndex = 0;
			Current = new InputRecord();
			frameToNext = 0;
			inputs.Clear();
			fastForwards.Clear();
		}
		public void PlaybackPlayer() {
			if (inputIndex < inputs.Count && !Manager.IsLoading()) {
				if (currentFrame >= frameToNext) {
					if (inputIndex + 1 >= inputs.Count) {
						inputIndex++;
						return;
					}
					if (Current.FastForward) {
						fastForwards.RemoveAt(0);
					}
					Current = inputs[++inputIndex];
					frameToNext += Current.Frames;
				}

				currentFrame++;
			}
			Manager.SetInputs(Current);
		}
		public void RecordPlayer() {
			InputRecord input = new InputRecord() { Frames = currentFrame };
			GetCurrentInputs(input);

			if (currentFrame == 0 && input == Current) {
				return;
			} else if (input != Current && !Manager.IsLoading()) {
				Current.Frames = currentFrame - Current.Frames;
				inputIndex++;
				if (Current.Frames != 0) {
					inputs.Add(Current);
				}
				Current = input;
			}
			currentFrame++;
		}
		private static void GetCurrentInputs(InputRecord record) {
			if (Input.Jump.Check || Input.MenuConfirm.Check) { record.Actions |= Actions.Jump; }
			if (Input.Dash.Check || Input.MenuCancel.Check || Input.Talk.Check) { record.Actions |= Actions.Dash; }
			if (Input.Grab.Check) { record.Actions |= Actions.Grab; }
			if (Input.MenuJournal.Check) { record.Actions |= Actions.Journal; }
			if (Input.Pause.Check) { record.Actions |= Actions.Start; }
			if (Input.QuickRestart.Check) { record.Actions |= Actions.Restart; }
			if (Input.MenuLeft.Check || Input.MoveX.Value < 0) { record.Actions |= Actions.Left; }
			if (Input.MenuRight.Check || Input.MoveX.Value > 0) { record.Actions |= Actions.Right; }
			if (Input.MenuUp.Check || Input.MoveY.Value < 0) { record.Actions |= Actions.Up; }
			if (Input.MenuDown.Check || Input.MoveY.Value > 0) { record.Actions |= Actions.Down; }
		}
		public void WriteInputs() {
			using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
				for (int i = 0; i < inputs.Count; i++) {
					InputRecord record = inputs[i];
					byte[] data = Encoding.ASCII.GetBytes(record.ToString() + "\r\n");
					fs.Write(data, 0, data.Length);
				}
				fs.Close();
			}
		}
		private bool ReadFile() {
			try {
				inputs.Clear();
				fastForwards.Clear();
				ReadFile(filePath, 0, int.MaxValue);
				return true;
			} catch {
				return false;
			}
		}
		private void ReadFile(string file, int skipLines, int numLines) {
			int lineNum = 0;
			using (StreamReader sr = new StreamReader(filePath)) {
				while (!sr.EndOfStream) {
					string line = sr.ReadLine();

					lineNum++;
					if (lineNum <= skipLines) { continue; }
					if (lineNum > numLines) { break; }

					if (line.IndexOf("Read", System.StringComparison.OrdinalIgnoreCase) == 0 && line.Length > 5) {
						string readCommand = line.Substring(5);
						int index = readCommand.IndexOf(',');
						string extraFile = index > 0 ? readCommand.Substring(0, index) : readCommand;
						int extraSkipLines = 0;
						int extraNumLines = int.MaxValue;
						if (index > 0) {
							int indexLen = readCommand.IndexOf(',', index + 1);
							if (indexLen > 0) {
								int.TryParse(readCommand.Substring(index + 1, indexLen - index - 1), out extraSkipLines);
								int.TryParse(readCommand.Substring(indexLen + 1), out extraNumLines);
							} else {
								int.TryParse(readCommand.Substring(index + 1), out extraSkipLines);
							}
						}
						// interpret the path as relative to the current file, instead of relative to Celeste.exe
						extraFile = Path.Combine(Path.GetDirectoryName(file), extraFile);
						ReadFile(extraFile, extraSkipLines, extraNumLines);
					}

					InputRecord input = new InputRecord(Path.GetFileName(file) + ":" + lineNum, line);
					if (input.FastForward) {
						fastForwards.Add(input);

						if (inputs.Count > 0) {
							inputs[inputs.Count - 1].ForceBreak = input.ForceBreak;
							inputs[inputs.Count - 1].FastForward = true;
						}
					} else if (input.Frames != 0) {
						inputs.Add(input);
					}
				}
			}
		}
	}
}
