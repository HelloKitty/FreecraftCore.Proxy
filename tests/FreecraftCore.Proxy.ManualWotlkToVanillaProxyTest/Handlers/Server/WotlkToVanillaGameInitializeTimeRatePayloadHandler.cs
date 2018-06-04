using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Fasterflect;
using FreecraftCore.Packet.CharSelect;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// Small structural difference between the vanilla SMSG_LOGIN_SETTIMESPEED_PAYLOAD_VANILLIA and the wotlk version.
	/// </summary>
	[ServerPayloadHandler]
	public sealed class WotlkToVanillaGameInitializeTimeRatePayloadHandler : BaseGameServerPayloadHandler<SMSG_LOGIN_SETTIMESPEED_PAYLOAD_VANILLIA>
	{
		/// <inheritdoc />
		public WotlkToVanillaGameInitializeTimeRatePayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{

		}

		/// <inheritdoc />
		public override Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SMSG_LOGIN_SETTIMESPEED_PAYLOAD_VANILLIA payload)
		{
			//We just forward the proper format to the wotlk client.
			return context.ProxyConnection.SendMessage(new SMSG_LOGIN_SETTIMESPEED_PAYLOAD(payload.GameTimeStamp, payload.GameTickRate));
		}
	}
}
