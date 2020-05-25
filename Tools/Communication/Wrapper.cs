using Xna = Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinForms = System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CelesteStudio.Communication {

	static class Wrapper {
		public static string gamePath;
		public static string state;
		public static string playerData = "";
		public static List<WinForms.Keys>[] bindings;

		[DllImport("User32.dll")]
		public static extern short GetAsyncKeyState(WinForms.Keys key);

		public static string LevelName() {
			int nameStart = playerData.IndexOf('[') + 1;
			int nameEnd = playerData.IndexOf(']');
			return playerData.Substring(nameStart, nameEnd);
		}

		public static void SetBindings(List<Xna.Keys>[] newBindings) {
			bindings = new List<WinForms.Keys>[newBindings.Length];
			int i = 0;
			foreach (List<Xna.Keys> keys in newBindings) {
				bindings[i] = new List<WinForms.Keys>();
				foreach (Xna.Keys key in keys) {
					bindings[i].Add((WinForms.Keys)key);
				}
				i++;
			}
		}

		public static bool CheckControls() {
			if (Environment.OSVersion.Platform == PlatformID.Unix || bindings == null)
				return false;
			
			bool anyPressed = false;
			for (int i = 0; i < bindings.Length; i++) {

				if (i == (int)HotkeyIDs.FastForward)
					continue;
				
				List<WinForms.Keys> keys = bindings[i];
				bool pressed = true;
				if (keys == null || keys.Count == 0)
					pressed = false;
				foreach (WinForms.Keys key in keys) {
					if ((GetAsyncKeyState(key) & 0x8000) != 0x8000) {
						pressed = false;
						break;
					}
				}
				if (pressed) {
					StudioCommunicationServer.instance.SendHotkeyPressed((HotkeyIDs)i);
					anyPressed = true;
				}
			}
			return anyPressed;
		}

		public static void CheckFastForward() {
			if (Environment.OSVersion.Platform == PlatformID.Unix || bindings == null)
				return;

			List<WinForms.Keys> keys = bindings[(int)HotkeyIDs.FastForward];
			bool pressed = true;
			if (keys == null || keys.Count == 0)
				pressed = false;
			foreach (WinForms.Keys key in keys) {
				if ((GetAsyncKeyState(key) & 0x8000) != 0x8000) {
					pressed = false;
					break;
				}
			}
			if (!pressed) {
				StudioCommunicationServer.instance.SendHotkeyPressed(HotkeyIDs.FastForward);
			}
		}
	}
}
