using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Fasterflect;
using FreecraftCore.Serializer;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/*[ServerPayloadHandler]
	public sealed class UpdatePacketCaptureResponsePayloadHandler : GameServerPacketLoggerServerPayloadHandler<SMSG_COMPRESSED_UPDATE_OBJECT_DTO_PROXY>
	{
		/// <inheritdoc />
		public UpdatePacketCaptureResponsePayloadHandler([NotNull] ILog logger) 
			: base(logger, true)
		{

		}
	}

	[ServerPayloadHandler]
	public sealed class UpdatePacketUncompressedCaptureResponsePayloadHandler : GameServerPacketLoggerServerPayloadHandler<SMSG_UPDATE_OBJECT_DTO_PROXY>
	{
		/// <inheritdoc />
		public UpdatePacketUncompressedCaptureResponsePayloadHandler([NotNull] ILog logger)
			: base(logger, true)
		{

		}
	}*/

	[ServerPayloadHandler]
	public sealed class UpdatePacketCaptureResponsePayloadHandler : BaseGameServerPayloadHandler<SMSG_COMPRESSED_UPDATE_OBJECT_Payload>
	{
		/// <inheritdoc />
		public UpdatePacketCaptureResponsePayloadHandler([NotNull] ILog logger)
			: base(logger)
		{

		}

		/// <inheritdoc />
		public override Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SMSG_COMPRESSED_UPDATE_OBJECT_Payload payload)
		{
			//Remove all blocks except for players
			return context.ProxyConnection.SendMessage(new SMSG_COMPRESSED_UPDATE_OBJECT_Payload(new UpdateBlockCollection(payload.UpdateBlocks.Items.Where(u => IsPlayerUpdateBlock(u)).ToArray())));
		}

		private static bool IsPlayerUpdateBlock(ObjectUpdateBlock u)
		{
			if(u.UpdateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
				return ((ObjectUpdateCreateObject1Block)u).CreationData.CreationObjectType == ObjectType.Player;
			else if(u.UpdateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT2)
				return ((ObjectUpdateCreateObject2Block)u).CreationData.CreationObjectType == ObjectType.Player;

			return false;
		}
	}
}
