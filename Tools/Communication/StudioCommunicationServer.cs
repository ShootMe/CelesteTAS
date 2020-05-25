using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;


namespace CelesteStudio.Communication {
	public sealed class StudioCommunicationServer : StudioCommunicationBase {

		public static StudioCommunicationServer instance;

		private StudioCommunicationServer() {
		}

		public static void Run() {
			instance = new StudioCommunicationServer();

			ThreadStart mainLoop = new ThreadStart(instance.UpdateLoop);
			Thread updateThread = new Thread(mainLoop);
			updateThread.Name = "StudioCom Server";
			updateThread.Start();
		}


		#region Read
		protected override void ReadData(Message message) {
			switch (message.ID) {
				case MessageIDs.SendState:
					ProcessSendState(message.Data);
					break;
				case MessageIDs.SendPlayerData:
					ProcessSendPlayerData(message.Data);
					break;
				case MessageIDs.SendCurrentBindings:
					ProcessSendCurrentBindings(message.Data);
					break;
				default:
					throw new InvalidOperationException($"{message.ID}");
			}
		}

		private void ProcessSendPath(byte[] data) {
			string path = Encoding.Default.GetString(data);
			Log(path);
			Wrapper.gamePath = path;
		}

		private void ProcessSendState(byte[] data) {
			string state = Encoding.Default.GetString(data);
			Log(state);
			Wrapper.state = state;
		}

		private void ProcessSendPlayerData(byte[] data) {
			string playerData = Encoding.Default.GetString(data);
			//Log(playerData);
			Wrapper.playerData = playerData;
		}

		private void ProcessSendCurrentBindings(byte[] data) {
			List<Keys>[] keys = FromByteArray<List<Keys>[]>(data);
			foreach (List<Keys> key in keys)
				Log(key.ToString());
			Wrapper.bindings = keys;
		}

		#endregion

		#region Write


		protected override void EstablishConnection() {
			var studio = this;
			var celeste = this;
			celeste = null;

			Message? lastMessage;

			studio?.WriteMessageGuaranteed(new Message(MessageIDs.EstablishConnection, new byte[0]));
			celeste?.ReadMessageGuaranteed();

			celeste?.SendPath(null, true);
			lastMessage = studio?.ReadMessageGuaranteed();
			studio?.ProcessSendPath(lastMessage?.Data);

			studio?.SendPath(Studio.instance.tasText.LastFileName, false);
			lastMessage = celeste?.ReadMessageGuaranteed();
			celeste?.ProcessSendPath(lastMessage?.Data);

			//celeste?.SendCurrentBindings(Hotkeys.listHotkeyKeys);
			lastMessage = studio?.ReadMessageGuaranteed();
			studio?.ProcessSendCurrentBindings(lastMessage?.Data);

			Initialized = true;
		}

		public void SendPath(string path, bool canFail) {
			if (Initialized || !canFail) {
				byte[] pathBytes;
				if (path != null)
					pathBytes = Encoding.Default.GetBytes(path);
				else
					pathBytes = new byte[0];
				WriteMessageGuaranteed(new Message(MessageIDs.SendPath, pathBytes));
			}
		}

		public void SendHotkeyPressed(HotkeyIDs hotkey) {
			if (!Initialized)
				return;
			byte[] hotkeyByte = new byte[] { (byte)hotkey };
			WriteMessageGuaranteed(new Message(MessageIDs.SendHotkeyPressed, hotkeyByte));
		}

		private void SendNewBindings(List<Keys> keys) {
			byte[] data = ToByteArray(keys);
			WriteMessageGuaranteed(new Message(MessageIDs.SendNewBindings, data));
		}

		private void SendReloadBindings(byte[] data) {
			WriteMessageGuaranteed(new Message(MessageIDs.ReloadBindings, new byte[0]));
		}

		#endregion
	}
}
