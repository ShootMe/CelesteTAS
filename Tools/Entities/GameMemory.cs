using System;
using System.Diagnostics;
namespace CelesteStudio.Entities {
	public class GameMemory {
		private static ProgramPointer TAS = new ProgramPointer(AutoDeref.Single,
			new ProgramSignature(PointerVersion.XNA, "8B0D????????3909FF15????????EB158325", 2),
			new ProgramSignature(PointerVersion.OpenGL, "89458C837D8C007417908B0D", 12));
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
