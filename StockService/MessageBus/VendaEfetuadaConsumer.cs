// StockService/MessageBus/VendaEfetuadaConsumer.cs (VERSÃO FINAL LIMPA)

using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using StockService.Data;
using Microsoft.EntityFrameworkCore;
using StockService.MessageBus;
using Microsoft.Extensions.Configuration; 
using StockService.Models; 

public class VendaEfetuadaConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    
    private IConnection? _connection; 
    private IModel? _channel;

    private const string QUEUE_NAME = "estoque_vendas_fila"; // ⬅️ Filas presas

    public VendaEfetuadaConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    private void TryConnect()
    {
        try
        {
            var factory = new ConnectionFactory() 
            { 
                HostName = _configuration["RabbitMQ:HostName"] ?? "127.0.0.1" 
            }; 
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.QueueDeclare(
                queue: QUEUE_NAME,
                durable: true, 
                exclusive: false,
                autoDelete: false,
                arguments: null);
                
            Console.WriteLine($"\n\n✅ Consumer conectado à fila: {QUEUE_NAME}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n❌ FALHA DE CONEXÃO: Não foi possível conectar ao Message Bus: {ex.Message}");
            _connection = null;
            _channel = null;
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TryConnect(); 

        if (_channel == null) return Task.CompletedTask; 

        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (ch, ea) =>
        {
            var content = Encoding.UTF8.GetString(ea.Body.ToArray());
            Console.WriteLine($"\n\n[RABBITMQ] Mensagem Recebida: {content}");
            
            VendaEfetuadaDto? vendaDto = JsonSerializer.Deserialize<VendaEfetuadaDto>(content);

            if (vendaDto != null)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<StockContext>();
                    
                    // ⬅️ BUSCA PADRÃO: Deixe o EF CORE rastrear o objeto automaticamente
                    var produto = await context.Produtos
                        .FirstOrDefaultAsync(p => p.Id == vendaDto.ProdutoId, stoppingToken);

                    if (produto != null && produto.QuantidadeEmEstoque >= vendaDto.Quantidade)
                    {
                        try
                        {
                            produto.QuantidadeEmEstoque -= vendaDto.Quantidade;
                            
                            // ⬅️ REMOVEMOS A FORÇA: Confiamos no rastreamento padrão do EF Core
                            int rowsAffected = await context.SaveChangesAsync(stoppingToken); 
                            
                            if (rowsAffected > 0)
                            {
                                Console.WriteLine($"\n\n✅ ESTOQUE ATUALIZADO! Produto {vendaDto.ProdutoId}. Novo estoque: {produto.QuantidadeEmEstoque}\n");
                            }
                            else
                            {
                                // Se falhar agora, é uma falha de SaveChangesAsync e não de rastreamento.
                                Console.WriteLine($"\n\n❌ ERRO: 0 linhas afetadas. A persistência falhou.\n");
                            }
                        }
                        catch (Exception dbEx)
                        {
                            // Se o SQL Server rejeitar a transação
                            Console.WriteLine($"\n\n❌ ERRO FATAL DE PERSISTÊNCIA: {dbEx.Message}\n"); 
                        }
                    }
                }
            }

            _channel.BasicAck(ea.DeliveryTag, false); 
        };

        _channel.BasicConsume(queue: QUEUE_NAME, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        if (_channel != null && _channel.IsOpen) _channel.Close();
        if (_connection != null && _connection.IsOpen) _connection.Close();
        base.Dispose();
    }
}