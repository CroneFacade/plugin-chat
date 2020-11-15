using System;
using System.Collections.Generic;
using System.Linq;
using NFive.Chat.Shared;
using NFive.SDK.Client.Interface;

namespace NFive.Chat.Client.Overlays
{
	public class ChatOverlay : Overlay
	{
		private readonly int historyLimit;

		public Dictionary<string, string> Templates = new Dictionary<string, string>();

		public event EventHandler<MessageEventArgs> MessageEntered;
		public event EventHandler<MessageEventArgs> ExportHandler;

		public ChatOverlay(IOverlayManager manager, int historyLimit) : base(manager)
		{
			this.historyLimit = historyLimit;

			On("message", new Action<string>(message => this.MessageEntered?.Invoke(this, new MessageEventArgs(this, message))));
			On("export", new Action<string>(message => this.ExportHandler?.Invoke(this, new MessageEventArgs(this, message))));

			On("blur", Blur);
		}

		protected override dynamic Ready() => new
		{
			history = this.historyLimit // Max chat history items
		};

		public void AddMessage(ChatMessage message)
		{
			Emit("add-message", new
			{
				style = message.Style,
				message = string.Format(this.Templates[message.Template], message.Values
					.Select(v => v
						.Replace("&", "&amp;")
						.Replace("<", "&lt;")
						.Replace(">", "&gt;")
						.Replace("\"", "&quot;")
					)
					.Cast<object>()
					.ToArray())
			});
		}

		public void AddTemplate(string name, string template)
		{
			this.Templates[name] = template;
		}

		public void Open()
		{
			Emit("open");
		}
	}
}
