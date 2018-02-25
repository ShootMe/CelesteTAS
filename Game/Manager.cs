using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Runtime.InteropServices;
namespace TAS {
	[Flags]
	public enum State {
		None = 0,
		Enable = 1,
		Record = 2,
		FrameStep = 4
	}
	public class Manager {
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern short GetAsyncKeyState(Keys vkey);
		public static bool Running, Recording;
		private static InputController controller = new InputController("Celeste.tas");
		public static State state, nextState;
		public static string CurrentStatus, PlayerStatus;
		public static int FrameStepCooldown, FrameLoops = 1;
		private static bool frameStepWasDpadUp, frameStepWasDpadDown;
		private static bool IsKeyDown(Keys key) {
			return (GetAsyncKeyState(key) & 32768) == 32768;
		}
		public static bool IsLoading() {
			return (Engine.Scene is LevelExit) || (Engine.Scene is LevelLoader) || (Engine.Scene is OverworldLoader) || (Engine.Scene is GameLoader);
		}
		private static GamePadState GetGamePadState() {
			GamePadState padState = MInput.GamePads[0].CurrentState;
			for (int i = 0; i < 4; i++) {
				padState = GamePad.GetState((PlayerIndex)i);
				if (padState.IsConnected) {
					break;
				}
			}
			return padState;
		}
		public static void UpdateInputs() {
			Level level = Engine.Scene as Level;
			if (level != null) {
				Player player = level.Tracker.GetEntity<Player>();
				if (player != null) {
					string statuses = ((int)(player.dashCooldownTimer * 60f) < 1 && player.Dashes > 0 ? "Dash " : string.Empty) + (player.LoseShards ? "Ground " : string.Empty) + (player.WallJumpCheck(1) ? "Wall-R " : string.Empty) + (player.WallJumpCheck(-1) ? "Wall-L " : string.Empty);
					string info = "Pos: " + player.Position.X.ToString("0") + "," + player.Position.Y.ToString("0") + "\r\nSpeed: " + player.Speed.X.ToString("0.00") + "," + player.Speed.Y.ToString("0.00") + "," + player.Speed.Length().ToString("0.00") + "\r\nStamina: " + player.Stamina.ToString("0") + " Timer: " + ((double)((Celeste.Celeste)Engine.Instance).AutoSplitterInfo.ChapterTime / (double)10000000).ToString("0.000") + "\r\n" + (player.InControl && !level.Transitioning ? statuses : "NoControl ") + (player.TimePaused ? "Paused " : string.Empty);
					PlayerStatus = info;
				} else {
					PlayerStatus = null;
				}
			} else if (Engine.Scene != null) {
				PlayerStatus = Engine.Scene.GetType().Name;
			}

			GamePadState padState = GetGamePadState();
			HandleFrameRates(padState);
			CheckControls(padState);
			FrameStepping(padState);

			if (HasFlag(state, State.Enable)) {
				Running = true;

				if (HasFlag(state, State.FrameStep)) {
					return;
				}

				if (HasFlag(state, State.Record)) {
					controller.RecordPlayer();
				} else {
					bool fastForward = controller.HasFastForward;
					controller.PlaybackPlayer();
					if (fastForward && !controller.HasFastForward) {
						nextState |= State.FrameStep;
						FrameLoops = 1;
					}

					if (!controller.CanPlayback) {
						DisableRun();
					}
				}
				string status = controller.Current.Line + "[" + controller.ToString() + "]";
				CurrentStatus = status;
			} else {
				Running = false;
				CurrentStatus = null;
			}
		}
		private static void HandleFrameRates(GamePadState padState) {
			if (HasFlag(state, State.Enable) && !HasFlag(state, State.FrameStep) && !HasFlag(nextState, State.FrameStep) && !HasFlag(state, State.Record)) {
				if (controller.HasFastForward) {
					FrameLoops = 400;
					return;
				}

				float rightStickX = padState.ThumbSticks.Right.X;
				if (IsKeyDown(Keys.LShiftKey)) {
					rightStickX = -0.65f;
				} else if (IsKeyDown(Keys.RShiftKey)) {
					rightStickX = 1f;
				}

				if (rightStickX <= 0.2) {
					FrameLoops = 1;
				} else if (rightStickX <= 0.3) {
					FrameLoops = 2;
				} else if (rightStickX <= 0.4) {
					FrameLoops = 3;
				} else if (rightStickX <= 0.5) {
					FrameLoops = 4;
				} else if (rightStickX <= 0.6) {
					FrameLoops = 5;
				} else if (rightStickX <= 0.7) {
					FrameLoops = 6;
				} else if (rightStickX <= 0.8) {
					FrameLoops = 7;
				} else if (rightStickX <= 0.9) {
					FrameLoops = 8;
				} else {
					FrameLoops = 9;
				}
			} else {
				FrameLoops = 1;
			}
		}
		private static void FrameStepping(GamePadState padState) {
			bool rightTrigger = padState.Triggers.Right > 0.5f;
			bool dpadUp = padState.DPad.Up == ButtonState.Pressed || (IsKeyDown(Keys.OemOpenBrackets) && !IsKeyDown(Keys.ControlKey));
			bool dpadDown = padState.DPad.Down == ButtonState.Pressed || (IsKeyDown(Keys.OemCloseBrackets) && !IsKeyDown(Keys.ControlKey));

			if (HasFlag(state, State.Enable) && !HasFlag(state, State.Record) && !rightTrigger) {
				if (HasFlag(nextState, State.FrameStep)) {
					state |= State.FrameStep;
					nextState &= ~State.FrameStep;
				}

				if (!dpadUp && frameStepWasDpadUp) {
					if (!HasFlag(state, State.FrameStep)) {
						state |= State.FrameStep;
						nextState &= ~State.FrameStep;
					} else {
						state &= ~State.FrameStep;
						nextState |= State.FrameStep;
						ReloadRun();
					}
					FrameStepCooldown = 60;
				} else if (!dpadDown && frameStepWasDpadDown) {
					state &= ~State.FrameStep;
					nextState &= ~State.FrameStep;
				} else if (HasFlag(state, State.FrameStep) && padState.ThumbSticks.Right.X > 0.1) {
					FrameStepCooldown -= (int)((padState.ThumbSticks.Right.X - 0.1) * 66.6f);
					if (FrameStepCooldown <= 0) {
						FrameStepCooldown = 60;
						state &= ~State.FrameStep;
						nextState |= State.FrameStep;
						ReloadRun();
					}
				}
			}

			frameStepWasDpadUp = dpadUp;
			frameStepWasDpadDown = dpadDown;
		}
		private static void CheckControls(GamePadState padState) {
			bool openBracket = IsKeyDown(Keys.ControlKey) && IsKeyDown(Keys.OemOpenBrackets);
			bool closeBrackets = IsKeyDown(Keys.ControlKey) && IsKeyDown(Keys.OemCloseBrackets);
			bool backSpace = IsKeyDown(Keys.ControlKey) && IsKeyDown(Keys.Back);
			bool leftStick = padState.Buttons.LeftStick == ButtonState.Pressed || backSpace;
			bool rightStick = padState.Buttons.RightStick == ButtonState.Pressed || openBracket;
			bool dpadDown = padState.DPad.Down == ButtonState.Pressed || closeBrackets;
			bool rightTrigger = padState.Triggers.Right > 0.5f || openBracket || closeBrackets || backSpace;
			bool leftTrigger = padState.Triggers.Left > 0.5f || openBracket || closeBrackets || backSpace;

			if (rightTrigger && leftTrigger) {
				if (!HasFlag(state, State.Enable) && rightStick) {
					nextState |= State.Enable;
				} else if (HasFlag(state, State.Enable) && dpadDown) {
					DisableRun();
				} else if (!HasFlag(state, State.Enable) && !HasFlag(state, State.Record) && leftStick) {
					nextState |= State.Record;
				}
			}

			if (!rightTrigger && !leftTrigger) {
				if (HasFlag(nextState, State.Enable)) {
					EnableRun();
				} else if (HasFlag(nextState, State.Record)) {
					RecordRun();
				}
			}
		}
		private static void DisableRun() {
			Running = false;
			if (Recording) {
				controller.WriteInputs();
			}
			Recording = false;
			state &= ~State.Enable;
			state &= ~State.FrameStep;
			nextState &= ~State.FrameStep;
			state &= ~State.Record;
		}
		private static void EnableRun() {
			nextState &= ~State.Enable;
			UpdateVariables(false);
		}
		private static void RecordRun() {
			nextState &= ~State.Record;
			UpdateVariables(true);
		}
		private static void ReloadRun() {
			controller.ReloadPlayback();
		}
		private static void UpdateVariables(bool recording) {
			state |= State.Enable;
			state &= ~State.FrameStep;
			if (recording) {
				Recording = recording;
				state |= State.Record;
				controller.InitializeRecording();
			} else {
				state &= ~State.Record;
				controller.InitializePlayback();
			}
			Running = true;
		}
		private static bool HasFlag(State state, State flag) {
			return (state & flag) == flag;
		}
	}
}