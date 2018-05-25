using GladNet;

namespace FreecraftCore
{
	public interface IGenericMessageContextFactory<out TPayloadWriteType, out TMessageContextType> 
		where TPayloadWriteType : class
	{
		TMessageContextType CreateMessageContext(IConnectionService connectionService, IPeerPayloadSendService<TPayloadWriteType> sendService, SessionDetails details);
	}
}