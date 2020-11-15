using NFive.SDK.Core.Controllers;
using NFive.SDK.Core.Input;

namespace NFive.Chat.Shared
{
	public class Configuration : ControllerConfiguration
	{
		public InputControl Hotkey { get; set; } = InputControl.MpTextChatAll;

		public string CommandPrefix { get; set; } = "/";

		public int HistoryLimit { get; set; } = 50;

		public string DefaultTemplate { get; set; } = "<h1>{0}</h1><i class=\"alert-separator\">|</i> {1}";

		public ChatStyles DefaultStyle { get; set; } = ChatStyles.Secondary;
	}
}
