using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;
using Reinterpret.Net;

namespace FreecraftCore
{
	public class GamePacketLoggerPayloadHandler<TUnimplementedPacketPayloadType> : BaseGamePayloadHandler<TUnimplementedPacketPayloadType>
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

		static GamePacketLoggerPayloadHandler()
		{
			
		}

		/// <summary>
		/// Lazily generated RootPath.
		/// It is generated as lazy so that the folder and directory is only created when the
		/// packet is first seen.
		/// </summary>
		public static Lazy<string> RootPath { get; } = new Lazy<string>(() =>
		{
			string rootPath = "Packet";
			string subfolderPath = $"{typeof(TUnimplementedPacketPayloadType).GetCustomAttribute<GamePayloadOperationCodeAttribute>().OperationCode}";
			string fullRootPath = Path.Combine(rootPath, subfolderPath);

			if(!Directory.Exists(rootPath))
				Directory.CreateDirectory(rootPath);

			if(!Directory.Exists(fullRootPath))
				Directory.CreateDirectory(fullRootPath);

			return fullRootPath;
		}, true);

		private static string PacketLogPath()
		{
			return RootPath.Value;
		}

		/// <inheritdoc />
		public GamePacketLoggerPayloadHandler([NotNull] ILog logger, bool shouldForward) 
			: base(logger)
		{
			ShouldForward = shouldForward;
		}

		/// <inheritdoc />
		public GamePacketLoggerPayloadHandler([NotNull] ILog logger)
			: this(logger, true)
		{
		}

		/// <inheritdoc />
		public override async Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, TUnimplementedPacketPayloadType payload)
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

			if(ShouldForward)
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
