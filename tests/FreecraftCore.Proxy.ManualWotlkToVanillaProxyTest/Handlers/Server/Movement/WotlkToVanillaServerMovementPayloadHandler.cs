using System;
using System.Threading.Tasks;
using Common.Logging;
using FreecraftCore.Serializer;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	[ServerPayloadHandler]
	public sealed class WotlkToVanillaServerMovementPayloadHandler : IPeerMessageHandler<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>
	{
		private static ISerializerService Serializer { get; }

		private ILog Logger { get; }

		private ITypeConverterProvider<MovementInfo_Vanilla, MovementInfo> MoveInfoConverter { get; }

		[WireDataContract]
		public class CustomMovePacketProxy
		{
			[WireMember(1)]
			public NetworkOperationCode OpCode { get; }

			[WireMember(2)]
			public PackedGuid MoveGuid { get; }


			[WireMember(3)]
			public MovementInfo MoveInfo { get; }

			/// <inheritdoc />
			public CustomMovePacketProxy(NetworkOperationCode opCode, [NotNull] PackedGuid moveGuid, [NotNull] MovementInfo moveInfo)
			{
				MoveGuid = moveGuid ?? throw new ArgumentNullException(nameof(moveGuid));
				OpCode = opCode;
				MoveInfo = moveInfo ?? throw new ArgumentNullException(nameof(moveInfo));
			}

			public CustomMovePacketProxy()
			{
				
			}
		}

		static WotlkToVanillaServerMovementPayloadHandler()
		{
			Serializer = new SerializerService();
			Serializer.RegisterType<CustomMovePacketProxy>();
			Serializer.Compile();
		}

		public WotlkToVanillaServerMovementPayloadHandler([NotNull] ILog logger, [NotNull] ITypeConverterProvider<MovementInfo_Vanilla, MovementInfo> moveInfoConverter)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			MoveInfoConverter = moveInfoConverter ?? throw new ArgumentNullException(nameof(moveInfoConverter));
		}

		/// <inheritdoc />
		public bool CanHandle(NetworkIncomingMessage<GamePacketPayload> message)
		{
			return message.Payload is IPlayerMovementPayload<MovementInfo_Vanilla, MovementFlags_Vanilla, PackedGuid>;
		}

		/// <inheritdoc />
		public async Task<bool> TryHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, NetworkIncomingMessage<GamePacketPayload> message)
		{
			NetworkOperationCode opCode = message.Payload.GetOperationCode();

			if(Logger.IsInfoEnabled)
				Logger.Info($"Server Sent: {opCode}:{opCode:X}");

			if(message.Payload is IPlayerMovementPayload<MovementInfo_Vanilla, MovementFlags_Vanilla, PackedGuid> payload)
			{
				//We should expect the payload to be a vanilla move info
				CustomMovePacketProxy proxy = new CustomMovePacketProxy(opCode, payload.MovementGuid, 
					MoveInfoConverter.Convert(payload.MoveInfo) ?? throw new InvalidOperationException($"Failed to convert the move info."));

				//We use a custom overload that allows low level byte writing to the proxy
				//This feature was LITERALLY implemented exactly for this payload
				//I added it to GladNet as a hack but it may see use elsewhere for performance reasons.
				await context.ProxyConnection.WriteAsync(Serializer.Serialize(proxy));
			}
			else
			{
				throw new InvalidOperationException($"Recieved non-movement payload in movement handler. OpCode: {opCode} Type: {message.Payload.GetType().Name}");
			}

			return true;
		}
	}
}