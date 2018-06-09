using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Common.Logging;
using FreecraftCore.Serializer;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	[ClientPayloadHandler]
	public sealed class WotlkToVanillaClientMovementPayloadHandler : IPeerMessageHandler<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>
	{
		private static ISerializerService Serializer { get; }

		private ILog Logger { get; }

		private ITypeConverterProvider<MovementInfo, MovementInfo_Vanilla> MoveInfoConverter { get; }

		[WireDataContract]
		public class CustomMovePacketProxy_Vanilla
		{
			[WireMember(1)]
			public NetworkOperationCode OpCode { get; }

			[WireMember(2)]
			public MovementInfo_Vanilla MoveInfo { get; }

			/// <inheritdoc />
			public CustomMovePacketProxy_Vanilla(NetworkOperationCode opCode, MovementInfo_Vanilla moveInfo)
			{
				OpCode = opCode;
				MoveInfo = moveInfo;
			}

			public CustomMovePacketProxy_Vanilla()
			{

			}
		}

		static WotlkToVanillaClientMovementPayloadHandler()
		{
			Serializer = new SerializerService();
			Serializer.RegisterType<CustomMovePacketProxy_Vanilla>();
			Serializer.Compile();
		}

		public WotlkToVanillaClientMovementPayloadHandler([NotNull] ILog logger, [NotNull] ITypeConverterProvider<MovementInfo, MovementInfo_Vanilla> moveInfoConverter)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			MoveInfoConverter = moveInfoConverter ?? throw new ArgumentNullException(nameof(moveInfoConverter));
		}

		/// <inheritdoc />
		public bool CanHandle(NetworkIncomingMessage<GamePacketPayload> message)
		{
			return message.Payload is IPlayerMovementPayload<MovementInfo, MovementFlag, PackedGuid>;
		}

		/// <inheritdoc />
		public async Task<bool> TryHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, NetworkIncomingMessage<GamePacketPayload> message)
		{
			NetworkOperationCode opCode = message.Payload.GetOperationCode();

			if(Logger.IsInfoEnabled)
				Logger.Info($"Client Sent: {opCode}:{opCode:X}");

			if(message.Payload is IPlayerMovementPayload<MovementInfo, MovementFlag, PackedGuid> payload)
			{
				//We should expect the payload to be a vanilla move info
				//Do NOT send guid. only server sends guid in 1.12.1
				CustomMovePacketProxy_Vanilla proxy = new CustomMovePacketProxy_Vanilla(opCode, 
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
