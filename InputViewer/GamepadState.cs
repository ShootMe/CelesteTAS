namespace InputViewer {
	public struct GamepadState {
		public GamePadButtons Buttons;
		public GamePadDPad DPad;
		public bool IsConnected;
		public int PacketNumber;
		public GamePadThumbSticks ThumbSticks;
		public GamePadTriggers Triggers;
	}
	public struct GamePadButtons {
		public bool RightShoulder;
		public bool LeftStick;
		public bool LeftShoulder;
		public bool Start;
		public bool Y;
		public bool X;
		public bool RightStick;
		public bool Back;
		public bool A;
		public bool B;
		public bool BigButton;
	}
	public struct GamePadDPad {
		public bool Left;
		public bool Right;
		public bool Up;
		public bool Down;
	}
	public struct GamePadThumbSticks {
		public float LeftX, LeftY;
		public float RightX, RightY;
	}
	public struct GamePadTriggers {
		public float Left;
		public float Right;
	}
}
