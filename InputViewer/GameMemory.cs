using System;
using System.Diagnostics;
namespace InputViewer {
	//.load C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll
	public class GameMemory {
		private static ProgramPointer MInput = new ProgramPointer(AutoDeref.Single,
			new ProgramSignature(PointerVersion.XNA, "80783100745EA1????????3A40048D78048D7024B9", 7),
			new ProgramSignature(PointerVersion.OpenGL, "558BEC50894DFC833D????????007405E8????????8B45FC8D15????????E8????????908BE55DC3DC", 26));
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;

		public GameMemory() {
			lastHooked = DateTime.MinValue;
		}

		public GamepadState GamePadState() {
			GamepadState state = new GamepadState();
			for (int i = 0; i < 4; i++) {
				IntPtr pad = (IntPtr)MInput.Read<uint>(Program, MInput.Version == PointerVersion.XNA ? 0x8 : 0x0, 0x8 + (i * 4));
				bool attached = Program.Read<bool>(pad, 0x10);
				if (attached) {
					if (MInput.Version == PointerVersion.XNA) {
						byte[] data = Program.Read(pad, 0x60, 0x80);
						state.IsConnected = true;
						state.PacketNumber = BitConverter.ToInt32(data, 0x4);

						state.ThumbSticks.LeftX = BitConverter.ToSingle(data, 0x8);
						state.ThumbSticks.LeftY = BitConverter.ToSingle(data, 0xc);
						state.ThumbSticks.RightX = BitConverter.ToSingle(data, 0x10);
						state.ThumbSticks.RightY = BitConverter.ToSingle(data, 0x14);

						state.Triggers.Left = BitConverter.ToSingle(data, 0x18);
						state.Triggers.Right = BitConverter.ToSingle(data, 0x1c);

						state.Buttons.A = BitConverter.ToInt32(data, 0x20) != 0;
						state.Buttons.B = BitConverter.ToInt32(data, 0x24) != 0;
						state.Buttons.X = BitConverter.ToInt32(data, 0x28) != 0;
						state.Buttons.Y = BitConverter.ToInt32(data, 0x2c) != 0;
						state.Buttons.LeftStick = BitConverter.ToInt32(data, 0x30) != 0;
						state.Buttons.RightStick = BitConverter.ToInt32(data, 0x34) != 0;
						state.Buttons.LeftShoulder = BitConverter.ToInt32(data, 0x38) != 0;
						state.Buttons.RightShoulder = BitConverter.ToInt32(data, 0x3c) != 0;
						state.Buttons.Back = BitConverter.ToInt32(data, 0x40) != 0;
						state.Buttons.Start = BitConverter.ToInt32(data, 0x44) != 0;
						state.Buttons.BigButton = BitConverter.ToInt32(data, 0x48) != 0;

						state.DPad.Up = BitConverter.ToInt32(data, 0x4c) != 0;
						state.DPad.Right = BitConverter.ToInt32(data, 0x50) != 0;
						state.DPad.Down = BitConverter.ToInt32(data, 0x54) != 0;
						state.DPad.Left = BitConverter.ToInt32(data, 0x58) != 0;
					} else {
						byte[] data = Program.Read(pad, 0x34, 0x48);
						state.IsConnected = true;
						state.PacketNumber = BitConverter.ToInt32(data, 0x4);

						int buttons = BitConverter.ToInt32(data, 0x8);
						state.Buttons.A = (buttons & 4096) != 0;
						state.Buttons.B = (buttons & 8192) != 0;
						state.Buttons.X = (buttons & 16384) != 0;
						state.Buttons.Y = (buttons & 32768) != 0;
						state.Buttons.LeftStick = (buttons & 64) != 0;
						state.Buttons.RightStick = (buttons & 128) != 0;
						state.Buttons.LeftShoulder = (buttons & 256) != 0;
						state.Buttons.RightShoulder = (buttons & 512) != 0;
						state.Buttons.Back = (buttons & 32) != 0;
						state.Buttons.Start = (buttons & 16) != 0;
						state.Buttons.BigButton = (buttons & 2048) != 0;

						state.DPad.Up = (buttons & 1) != 0;
						state.DPad.Down = (buttons & 2) != 0;
						state.DPad.Left = (buttons & 4) != 0;
						state.DPad.Right = (buttons & 8) != 0;
						
						state.ThumbSticks.LeftX = BitConverter.ToSingle(data, 0x1c);
						state.ThumbSticks.LeftY = BitConverter.ToSingle(data, 0x20);
						state.ThumbSticks.RightX = BitConverter.ToSingle(data, 0x24);
						state.ThumbSticks.RightY = BitConverter.ToSingle(data, 0x28);

						state.Triggers.Left = BitConverter.ToSingle(data, 0x2c);
						state.Triggers.Right = BitConverter.ToSingle(data, 0x30);
					}
					break;
				}
			}
			return state;
		}
		public bool HookProcess() {
			IsHooked = Program != null && !Program.HasExited;
			if (!IsHooked && DateTime.Now > lastHooked.AddSeconds(1)) {
				lastHooked = DateTime.Now;
				Process[] processes = Process.GetProcessesByName("Celeste");
				Program = processes != null && processes.Length > 0 ? processes[0] : null;

				if (Program != null && !Program.HasExited) {
					MemoryReader.Update64Bit(Program);
					IsHooked = true;
				}
			}

			return IsHooked;
		}
		public void Dispose() {
			if (Program != null) {
				Program.Dispose();
			}
		}
	}
}
