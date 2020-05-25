using Xna = Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;

namespace CelesteStudio.Communication {

	static class Wrapper {
		public static string gamePath;
		public static string state;
		public static string playerData;
		public static List<Xna.Keys>[] bindings;
		

		public static string LevelName() {
			int nameStart = playerData.IndexOf('[') + 1;
			int nameEnd = playerData.IndexOf(']');
			return playerData.Substring(nameStart, nameEnd);
		}

		public static bool CheckControls() {
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				return false;
			
			KeyboardState kb = Keyboard.GetState();
			bool anyPressed = false;
			for (int i = 0; i < bindings.Length; i++) {
				List<Xna.Keys> keys = bindings[i];
				bool pressed = true;
				if (keys == null || keys.Count == 0)
					pressed = false;
				foreach (Xna.Keys key in keys) {
					if (!kb.IsKeyDown(key)) {
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
	}
}
