// SalesService/MessageBus/MessageBusClient.cs (CORRIGIDO)

using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using SalesService.Dtos; 
using SalesService.MessageBus; // Garante o acesso à IMessageBusClient

namespace SalesService.MessageBus
{
    // A DEFINIÇÃO DUPLICADA DA INTERFACE FOI REMOVIDA DAQUI!
    
    public class MessageBusClient : IMessageBusClient, IDisposable
    {
        // ... (Corpo da classe que já está OK: _connection, _channel, construtor, PublicarMensagem, Dispose)
        
        private readonly IConnection? _connection = null; 
        private readonly IModel? _channel = null; 
        private readonly IConfiguration _configuration;
        private const string ExchangeName = "vendas_direct_exchange";
        private const string RoutingKey = "venda-notificacao-estoque";
        private const string QueueName = "vendas-notificacao-estoque";

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            
            var factory = new ConnectionFactory() 
            { 
                HostName = _configuration["RabbitMQ:HostName"],
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672")
            };
            
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                
                _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);
                _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingKey);

                Console.WriteLine("--> Conexão com Message Bus estabelecida.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Não foi possível conectar ao Message Bus: {ex.Message}");
            }
        }

        public void PublicarMensagem(object mensagem)
        {
            if (_channel == null || _connection == null || !_connection.IsOpen)
            {
                Console.WriteLine("--> Tentativa de publicação falhou: Message Bus não está conectado.");
                return;
            }
            
            var json = JsonSerializer.Serialize(mensagem);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: ExchangeName,
                                 routingKey: RoutingKey,
                                 basicProperties: null,
                                 body: body);
            
            Console.WriteLine($"--> Mensagem publicada no RabbitMQ: {json}");
        }

        public void Dispose()
        {
            if (_channel != null && _channel.IsOpen)
            {
                _channel.Close();
            }
            if (_connection != null && _connection.IsOpen)
            {
                _connection.Close();
            }
        }
    }
}