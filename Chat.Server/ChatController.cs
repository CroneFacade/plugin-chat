using System.Linq;
using JetBrains.Annotations;
using NFive.Chat.Shared;
using NFive.SDK.Core.Diagnostics;
using NFive.SDK.Core.Events;
using NFive.SDK.Core.Extensions;
using NFive.SDK.Server.Communications;
using NFive.SDK.Server.Controllers;

namespace NFive.Chat.Server
{
	[PublicAPI]
	public class ChatController : ConfigurableController<Configuration>
	{
		private ICommunicationManager _comms;

		public ChatController(ILogger logger, Configuration configuration, ICommunicationManager comms) : base(logger, configuration)
		{
			this._comms = comms;

			// Send configuration when requested
			comms.Event(ChatEvents.Configuration).FromClients().OnRequest(e => e.Reply(this.Configuration));

			// Listen to new client messages
			comms.Event(ChatEvents.MessageEntered).FromClients().On<string>(MessageEntered);
		}

		private void MessageEntered(ICommunicationMessage e, string message)
		{
			// Check if message is a command
			if (string.IsNullOrWhiteSpace(this.Configuration.CommandPrefix) || message.Trim().StartsWith(this.Configuration.CommandPrefix))
			{
				// Split message by space, respecting double quotes
				var args = message.Trim().Substring(this.Configuration.CommandPrefix.Length).SplitArguments().ToList();

				// Dispatch command to sender
				_comms.Event(CoreEvents.CommandDispatch).ToClient(e.Client).Emit(args);
			}
			else
			{
				// Un-prefixed message, send to everyone
				_comms.Event(ChatEvents.ChatMessage)
					.ToClients()
					.Emit(new ChatMessage
					{
						Sender = e.User,
						Style = this.Configuration.DefaultStyle.ToString("G").ToLowerInvariant(),
						Template = "default",
						Values = new[]
						{
							e.User.Name,
							message
						}
					});
			}
		}
	}
}
