using System;

namespace CustomDesktopLogo.SingleInstance
{
	public class ArgumentsReceivedEventArgs : EventArgs
	{
		public string[] Args { get; set; }
	}
}
