﻿using Celeste;
using Celeste.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Globalization;
using System.Text;
namespace TAS {
	[Flags]
	public enum State {
		None = 0,
		Enable = 1,
		Record = 2,
		FrameStep = 4,
		Disable = 8
	}
	public class Manager {
		public static bool Running, Recording;
		private static InputController controller = new InputController("Celeste.tas");
		public static State state, nextState;
		public static string CurrentStatus, PlayerStatus, Pico8LevelName;
		public static int FrameStepCooldown, FrameLoops = 1;
		private static bool frameStepWasDpadUp, frameStepWasDpadDown;
		private static Vector2 lastPos;
		private static long lastTimer;
		private static CultureInfo enUS = CultureInfo.CreateSpecificCulture("en-US");
		private static KeyboardState kbState;
		private static bool IsKeyDown(Keys key) {
			return kbState.IsKeyDown(key);
		}
		public static bool IsLoading() {
			if (Engine.Scene is SummitVignette summit) {
				return !summit.ready;
			}
			if (Engine.Scene is Overworld overworld) {
				return overworld.Current is OuiFileSelect slot && slot.SlotIndex >= 0 && slot.Slots[slot.SlotIndex].StartingGame;
			}
			if (Engine.Scene is Emulator emulator) {
				// PICO-8 runs at 30 FPS. It skipped the previous 60fps frame if skipFrame is true.
				return !emulator.skipFrame;
			}
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
			Player player = null;
			Classic.player classicPlayer = null;
			long chapterTime = 0;
			Pico8LevelName = "";
			if (Engine.Scene is Level level) {
				player = level.Tracker.GetEntity<Player>();
				if (player != null) {
					string statuses = ((int)(player.dashCooldownTimer * 60f) < 1 && player.Dashes > 0 ? "Dash " : string.Empty) + (player.LoseShards ? "Ground " : string.Empty) + (player.WallJumpCheck(1) ? "Wall-R " : string.Empty) + (player.WallJumpCheck(-1) ? "Wall-L " : string.Empty);
					chapterTime = ((Celeste.Celeste)Engine.Instance).AutoSplitterInfo.ChapterTime;
					StringBuilder sb = new StringBuilder();
					sb.Append("Pos: ").Append(player.ExactPosition.X.ToString("0.0", enUS)).Append(',').AppendLine(player.ExactPosition.Y.ToString("0.0", enUS));
					sb.Append("Speed: ").Append(player.Speed.X.ToString("0.00", enUS)).Append(',').Append(player.Speed.Y.ToString("0.00", enUS)).Append(',').AppendLine(player.Speed.Length().ToString("0.00", enUS));
					Vector2 diff = (player.ExactPosition - lastPos) * 60;
					sb.Append("Vel: ").Append(diff.X.ToString("0.00", enUS)).Append(',').Append(diff.Y.ToString("0.00", enUS)).Append(',').AppendLine(diff.Length().ToString("0.00", enUS));
					sb.Append("Stamina: ").Append(player.Stamina.ToString("0")).Append(" Timer: ").AppendLine(((double)chapterTime / (double)10000000).ToString("0.000", enUS));
					sb.Append(player.InControl && !level.Transitioning ? statuses : "NoControl ").Append(player.TimePaused ? "Paused " : string.Empty).Append(level.InCutscene ? "Cutscene " : string.Empty);
					PlayerStatus = sb.ToString();
				} else {
					PlayerStatus = level.InCutscene ? "Cutscene " : null;
				}
			} else if (Engine.Scene is Emulator emulator) {
				int levelIndex = (Classic.room.X % 8 + Classic.room.Y * 8);
				Pico8LevelName = emulator.booting ? "classic_booting" : levelIndex == 31 ? "classic_title" : levelIndex == 30 ? "classic_summit" : string.Concat("classic_", levelIndex + 1, "00m");
				classicPlayer = emulator.booting ? null : Classic.objects?.Find(o => o is Classic.player) as Classic.player;
				StringBuilder sb = new StringBuilder();
				if (classicPlayer != null) {
					chapterTime = ((Classic.minutes * 60) + Classic.seconds) * 30 + Classic.frames;
					sb.Append("Pos: ").Append(classicPlayer.x.ToString("0.00", enUS)).Append(',').AppendLine(classicPlayer.y.ToString("0.00", enUS));
					sb.Append("Speed: ").Append(classicPlayer.spd.X.ToString("0.00", enUS)).Append(',').Append(classicPlayer.spd.Y.ToString("0.00", enUS)).Append(',').AppendLine(classicPlayer.spd.Length().ToString("0.00", enUS));
					Vector2 diff = (new Vector2(classicPlayer.x, classicPlayer.y) - lastPos) * 60;
					sb.Append("Vel: ").Append(diff.X.ToString("0.00", enUS)).Append(',').Append(diff.Y.ToString("0.00", enUS)).Append(',').AppendLine(diff.Length().ToString("0.00", enUS));
					sb.Append("Ground:").Append(classicPlayer.grace).Append(' ')
						.Append("Dash:").Append(classicPlayer.dash_time).AppendLine();
				}
				if (Classic.freeze > 0) {
					sb.Append("Freeze: ").Append(Classic.freeze).Append(' ');
				} else if (Classic.pause_player) {
					sb.Append("NoControl ");
				} else if (classicPlayer != null) {
					sb.Append(classicPlayer.is_solid(0, 1) ? "OnGround " : "");
					sb.Append(classicPlayer.is_ice(0, 1) ? "Ice " : "");
					sb.Append(classicPlayer.djump > 0 ? "Dash " : "");
					sb.Append(classicPlayer.is_solid(-1, 0) ? "Wall-L " : "");
					sb.Append(classicPlayer.is_solid(1, 0) ? "Wall-R " : "");
				} else if (!emulator.booting && levelIndex < 31) {
					sb.Append("NoControl Cutscene ");
				}
				PlayerStatus = sb.ToString();
			} else if (Engine.Scene is SummitVignette summit) {
				PlayerStatus = string.Concat("SummitVignette ", summit.ready, ' ');
			} else if (Engine.Scene is Overworld overworld) {
				PlayerStatus = string.Concat("Overworld ", overworld.ShowInputUI, ' ');
			} else if (Engine.Scene != null) {
				PlayerStatus = Engine.Scene.GetType().Name;
			}

			kbState = Keyboard.GetState();
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

				if (!Engine.Instance.IsActive) {
					for (int i = 0; i < 4; i++) {
						if (MInput.GamePads[i].Attached) {
							MInput.GamePads[i].CurrentState = padState;
						}
					}
					MInput.UpdateVirtualInputs();
				}
			}

			if (player != null && chapterTime != lastTimer) {
				lastPos = player.ExactPosition;
				lastTimer = chapterTime;
			} else if (classicPlayer != null && chapterTime != lastTimer) {
				lastPos = new Vector2(classicPlayer.x, classicPlayer.y);
				lastTimer = chapterTime;
			}
		}
		private static void HandleFrameRates(GamePadState padState) {
			if (HasFlag(state, State.Enable) && !HasFlag(state, State.FrameStep) && !HasFlag(nextState, State.FrameStep) && !HasFlag(state, State.Record)) {
				if (controller.HasFastForward) {
					FrameLoops = controller.FastForwardSpeed;
					return;
				}

				float rightStickX = padState.ThumbSticks.Right.X;
				if (IsKeyDown(Keys.RightShift) && IsKeyDown(Keys.RightControl)) {
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
			bool dpadUp = padState.DPad.Up == ButtonState.Pressed || (IsKeyDown(Keys.OemOpenBrackets) && !IsKeyDown(Keys.RightControl));
			bool dpadDown = padState.DPad.Down == ButtonState.Pressed || (IsKeyDown(Keys.OemCloseBrackets) && !IsKeyDown(Keys.RightControl));

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
						controller.ReloadPlayback();
					}
					FrameStepCooldown = 60;
				} else if (!dpadDown && frameStepWasDpadDown) {
					state &= ~State.FrameStep;
					nextState &= ~State.FrameStep;
				} else if (HasFlag(state, State.FrameStep) && (padState.ThumbSticks.Right.X > 0.1 || (IsKeyDown(Keys.RightShift) && IsKeyDown(Keys.RightControl)))) {
					float rStick = padState.ThumbSticks.Right.X;
					if (rStick < 0.1f) {
						rStick = 0.5f;
					}
					FrameStepCooldown -= (int)((rStick - 0.1) * 80f);
					if (FrameStepCooldown <= 0) {
						FrameStepCooldown = 60;
						state &= ~State.FrameStep;
						nextState |= State.FrameStep;
						controller.ReloadPlayback();
					}
				}
			}

			frameStepWasDpadUp = dpadUp;
			frameStepWasDpadDown = dpadDown;
		}
		private static void CheckControls(GamePadState padState) {
			bool openBracket = IsKeyDown(Keys.RightControl) && IsKeyDown(Keys.OemOpenBrackets);
			bool rightStick = padState.Buttons.RightStick == ButtonState.Pressed || openBracket;

			if (rightStick) {
				if (!HasFlag(state, State.Enable)) {
					nextState |= State.Enable;
				} else {
					nextState |= State.Disable;
				}
			} else if (HasFlag(nextState, State.Enable)) {
				EnableRun();
			} else if (HasFlag(nextState, State.Disable)) {
				DisableRun();
			}
		}
		private static void DisableRun() {
			Running = false;
			if (Recording) {
				controller.WriteInputs();
			}
			Recording = false;
			state = State.None;
			nextState = State.None;
		}
		private static void EnableRun() {
			nextState &= ~State.Enable;
			UpdateVariables(false);
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