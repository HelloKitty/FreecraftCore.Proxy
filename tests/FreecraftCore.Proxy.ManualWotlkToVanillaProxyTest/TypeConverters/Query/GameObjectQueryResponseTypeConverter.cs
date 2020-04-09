using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreecraftCore
{
	public sealed class GameObjectQueryResponseTypeConverter : ITypeConverterProvider<GameObjectQueryResponseInfo_Vanilla, GameObjectQueryResponseInfo>
	{
		/// <inheritdoc />
		public GameObjectQueryResponseInfo Convert(GameObjectQueryResponseInfo_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			return new GameObjectQueryResponseInfo(fromObject.GameObjectType, fromObject.DisplayId, fromObject.Names, "Default", "FreecraftCore Proxy", fromObject.UnkString, 
				fromObject.Data, 1.0f, new int[6]{ 0, 0, 0, 0, 0, 0});
		}
	}
}
