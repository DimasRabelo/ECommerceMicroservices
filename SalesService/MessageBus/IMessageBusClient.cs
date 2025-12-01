// SalesService/MessageBus/IMessageBusClient.cs
namespace SalesService.MessageBus
{
    public interface IMessageBusClient
    {
        void PublicarMensagem(object mensagem);
    }
}