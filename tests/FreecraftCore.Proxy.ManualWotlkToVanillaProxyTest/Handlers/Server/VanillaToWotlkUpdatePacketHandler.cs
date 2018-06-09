using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using FreecraftCore;
using JetBrains.Annotations;
using Reinterpret.Net;
namespace FreecraftCore
{
	[ServerPayloadHandler]
	public sealed class VanillaToWotlkUpdatePacketHandler : BaseGameServerPayloadHandler<SMSG_UPDATE_OBJECT_Payload_Vanilla>
	{
		private ITypeConverterProvider<UpdateBlockCollection_Vanilla, UpdateBlockCollection> UpdateBlockConverter { get; }

		/// <inheritdoc />
		public VanillaToWotlkUpdatePacketHandler([NotNull] ILog logger, [NotNull] ITypeConverterProvider<UpdateBlockCollection_Vanilla, UpdateBlockCollection> updateBlockConverter) 
			: base(logger)
		{
			UpdateBlockConverter = updateBlockConverter ?? throw new ArgumentNullException(nameof(updateBlockConverter));
		}

		/// <inheritdoc />
		public override Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SMSG_UPDATE_OBJECT_Payload_Vanilla payload)
		{
			UpdateBlockCollection collection = UpdateBlockConverter.Convert(payload.UpdateBlocks);

			//Once the blocks are rebuilt we can send the packet off on its way
			return context.ProxyConnection.SendMessage(new SMSG_UPDATE_OBJECT_Payload(collection));
		}
	}

	[ServerPayloadHandler]
	public class VanillaToWotlkCompressedUpdatePacketHandler : BaseGameServerPayloadHandler<SMSG_COMPRESSED_UPDATE_OBJECT_Payload_Vanilla>
	{
		private ITypeConverterProvider<UpdateBlockCollection_Vanilla, UpdateBlockCollection> UpdateBlockConverter { get; }

		/// <inheritdoc />
		public VanillaToWotlkCompressedUpdatePacketHandler([NotNull] ILog logger, ITypeConverterProvider<UpdateBlockCollection_Vanilla, UpdateBlockCollection> updateBlockConverter)
			: base(logger)
		{
			UpdateBlockConverter = updateBlockConverter;
		}

		/// <inheritdoc />
		public override Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SMSG_COMPRESSED_UPDATE_OBJECT_Payload_Vanilla payload)
		{
			UpdateBlockCollection collection = UpdateBlockConverter.Convert(payload.UpdateBlocks);

			//Once the blocks are rebuilt we can send the packet off on its way
			return context.ProxyConnection.SendMessage(new SMSG_COMPRESSED_UPDATE_OBJECT_Payload(collection));
		}
	}
}
