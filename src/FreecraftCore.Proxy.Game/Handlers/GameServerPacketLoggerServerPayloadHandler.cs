using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;
using Reinterpret.Net;

namespace FreecraftCore
{
	[ServerPayloadHandler]
	public class GameServerPacketLoggerServerPayloadHandler<TUnimplementedPacketPayloadType> : BaseGameServerPayloadHandler<TUnimplementedPacketPayloadType>
		where TUnimplementedPacketPayloadType : GamePacketPayload, IUnimplementedGamePacketPayload
	{
		/// <summary>
		/// Indicates if the packet should be forwared.
		/// </summary>
		public bool ShouldForward { get; }

		/// <summary>
		/// The operation code.
		/// </summary>
		public static NetworkOperationCode OpCode { get; } = typeof(TUnimplementedPacketPayloadType).GetCustomAttribute<GamePayloadOperationCodeAttribute>().OperationCode;

		/// <summary>
		/// Cached byte version of the opcode.
		/// </summary>
		private static byte[] OpCodeBytes = ((short)OpCode).Reinterpret();

		private static string BaseFileName { get; } = OpCode.ToString();

		static GameServerPacketLoggerServerPayloadHandler()
		{
			if(!Directory.Exists("Packet"))
				Directory.CreateDirectory("Packet");

			if(!Directory.Exists(PacketLogPath()))
				Directory.CreateDirectory(PacketLogPath());
		}

		private static string PacketLogPath()
		{
			return $"Packet/{typeof(TUnimplementedPacketPayloadType).GetCustomAttribute<GamePayloadOperationCodeAttribute>().OperationCode}";
		}

		/// <inheritdoc />
		public GameServerPacketLoggerServerPayloadHandler([NotNull] ILog logger, bool shouldForward) 
			: base(logger)
		{
			ShouldForward = shouldForward;
		}

		/// <inheritdoc />
		public override async Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, TUnimplementedPacketPayloadType payload)
		{
			//When we recieve a packet we want to log the data BUT we always need to make sure we log the opcode appended to it so it can be deserialized
			Guid newGuid = Guid.NewGuid();

			if(Logger.IsDebugEnabled)
				Logger.Debug($"Logging: {OpCode} to file with GUID: {newGuid}");

			using(FileStream fs = File.Open(Path.Combine(PacketLogPath(), $"{BaseFileName}_{newGuid}"), FileMode.CreateNew))
			{
				await fs.WriteAsync(OpCodeBytes, 0, OpCodeBytes.Length);
				await fs.WriteAsync(payload.Data, 0, payload.Data.Length);
			}

			await OnForwardingPayload(context, payload);
		}

		/// <summary>
		/// Inheritors can override this method to handle
		/// forwarding differently.
		/// This method will only be called if <see cref="ShouldForward"/> is true.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="payload"></param>
		protected virtual Task OnForwardingPayload(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, TUnimplementedPacketPayloadType payload)
		{
			return context.ProxyConnection.SendMessage(payload);
		}
	}
}
