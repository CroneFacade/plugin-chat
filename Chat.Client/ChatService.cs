using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using JetBrains.Annotations;
using NFive.Chat.Client.Overlays;
using NFive.Chat.Shared;
using NFive.SDK.Client.Commands;
using NFive.SDK.Client.Communications;
using NFive.SDK.Client.Events;
using NFive.SDK.Client.Extensions;
using NFive.SDK.Client.Input;
using NFive.SDK.Client.Interface;
using NFive.SDK.Client.Services;
using NFive.SDK.Core.Diagnostics;
using NFive.SDK.Core.Events;
using NFive.SDK.Core.Models.Player;

namespace NFive.Chat.Client
{
	[PublicAPI]
	public class ChatService : Service
	{
		private Configuration config;
		private Hotkey hotkey;
		private ChatOverlay overlay;
		private ILogger _logger;

		public ChatService(ILogger logger, ITickManager ticks, ICommunicationManager comms, ICommandManager commands, IOverlayManager overlay, User user) : base(logger, ticks, comms, commands, overlay, user)
		{
			this._logger = logger;

		}

		public override Task Loaded()
		{
			// Disable stock GTA chat UI
			API.SetTextChatEnabled(false);

			return base.Loaded();
		}

		public override async Task Started()
		{
			// Request server configuration
			this.config = await this.Comms.Event(ChatEvents.Configuration).ToServer().Request<Configuration>();
			hotkey = new Hotkey(this.config.Hotkey);

			// Create overlay
			this.overlay = new ChatOverlay(this.OverlayManager, this.config.HistoryLimit);

			// Add default message template
			this.overlay.AddTemplate("default", this.config.DefaultTemplate);

			// Listen to overlay
			this.overlay.MessageEntered += OnOverlayOnMessageEntered;

			this.overlay.ExportHandler += OnOverlayOnExport;

			// Listen for messages
			this.Comms.Event(ChatEvents.ChatMessage).FromServer().On<ChatMessage>(ChatMessage);

			// Attach a tick handler
			this.Ticks.On((Action)ChatTick);
		}

		private void ChatTick()
		{
			if (!hotkey.IsJustPressed()) return;

			// Show chat overlay
			this.overlay.Open();
			API.SetNuiFocus(true, false);
		}

		private void OnOverlayOnExport(object s, MessageEventArgs a)
		{
			this._logger.Debug(a.Message);
		}

		private void OnOverlayOnMessageEntered(object s, MessageEventArgs a)
		{
			// Transmit message
			this.Comms.Event(ChatEvents.MessageEntered).ToServer().Emit(a.Message);
		}

		private void ChatMessage(ICommunicationMessage e, ChatMessage message)
		{
			if (message.Location != null && message.Radius.HasValue && message.Radius > 0f && World.GetDistance(message.Location.ToCitVector3(), Game.PlayerPed.Position) > message.Radius)
			{
				// Player is not in rage
				return;
			}

			this.overlay.AddMessage(message);
		}
	}
}
