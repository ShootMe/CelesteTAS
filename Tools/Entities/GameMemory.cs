using System;
using System.Diagnostics;
namespace CelesteStudio.Entities {
	public class GameMemory {
		private static ProgramPointer Celeste = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "83C604F30F7E06660FD6078BCBFF15????????8D15", 21));
		private static ProgramPointer TAS = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "8B0D????????3909FF15????????EB158325", 2));
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
		public double LevelTime() {
			//Celeste.Instance.AutosplitterInfo.ChapterTime
			return (double)Celeste.Read<long>(Program, 0x0, 0xac, 0x4) / (double)10000000;
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
