using System;
using System.Diagnostics;
namespace InputViewer {
	public class GameMemory {
		private static ProgramPointer TAS = new ProgramPointer(AutoDeref.Single,
			new ProgramSignature(PointerVersion.XNA, "8B0D????????3909FF15????????EB158325", 2),
			new ProgramSignature(PointerVersion.OpenGL, "89458C837D8C007417908B0D", 12));
		private static ProgramPointer Celeste = new ProgramPointer(AutoDeref.Single,
			new ProgramSignature(PointerVersion.XNA, "83C604F30F7E06660FD6078BCBFF15????????8D15", 21),
			new ProgramSignature(PointerVersion.OpenGL, "8B55F08B45E88D5274E8????????8B45F08D15", 19),
			new ProgramSignature(PointerVersion.Itch, "8D5674E8????????8D15????????E8????????C605", 10));
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;

		public GameMemory() {
			lastHooked = DateTime.MinValue;
		}

		public string TASOutput() {
			return TAS.Read(Program, 0x4, 0x0);
		}
		public string TASPlayerOutput() {
			return TAS.Read(Program, 0x8, 0x0);
		}
		public string LevelName() {
			//Celeste.Instance.AutosplitterInfo.Level
			if (Celeste.Version == PointerVersion.XNA) {
				return Celeste.Read(Program, 0x0, 0xac, 0x14, 0x0);
			}
			return Celeste.Read(Program, 0x0, 0x8c, 0x14, 0x0);
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
