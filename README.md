<h3>1. 🥇 Título e Resumo do Desafio</h3>



# Desafio Técnico - Arquitetura de Microsserviços e E-commerce (.NET Core)

Este projeto implementa uma arquitetura de microsserviços para gerenciamento de Vendas e Estoque, focando na separação de responsabilidades e na comunicação assíncrona robusta.

**Status:** Desafio Concluído e Validado (Incluindo fluxo de persistência assíncrona).

<h3>2. ⚙️ Tecnologias e Arquitetura</h3>



## Tecnologias e Padrões

* **Linguagem & Framework:** .NET 9 (C#) e ASP.NET Core Web API.
* **Persistência:** Entity Framework Core (EF Core).
* **Banco de Dados:** **SQL Server (Docker)** (Garantindo o isolamento de dados e a concorrência).
* **Comunicação Assíncrona:** **RabbitMQ** (Message Broker).
* **Segurança:** **JWT** (JSON Web Tokens) para Autenticação.
* **Roteamento:** **API Gateway (Ocelot)**.

<h3>3. 🗺️ Visão Geral da Arquitetura (Fluxo de Venda)</h3>



## Arquitetura e Fluxo de Dados

A solução é dividida em quatro microsserviços essenciais:

1.  **ECommerce.Auth.Api (5128):** Serviço responsável por emitir o Token JWT após o login (simulado).
2.  **ECommerce.Gateway (5117):** Ponto de entrada único. Valida o Token JWT e roteia as requisições para os serviços internos.
3.  **StockService (5254):** Gerencia produtos e o estoque. Atua como **Consumer** (Ouvinte) do RabbitMQ.
4.  **SalesService (5091):** Gerencia pedidos. Atua como **Producer** (Produtor) de eventos RabbitMQ.

**O fluxo de venda é Síncrono-Assíncrono:**
* **1. Síncrono (Validação):** O SalesService consulta o StockService via HTTP para verificar a disponibilidade de estoque.
* **2. Assíncrono (Atualização):** Após a confirmação do pedido, o SalesService envia uma mensagem para o RabbitMQ. O StockService consome essa mensagem e persiste a redução do estoque no banco de dados.

<h3>4. 🚀 Como Executar o Projeto</h3>



## Instruções para Execução

1.  **Infraestrutura:** Inicie o **RabbitMQ** e o **SQL Server** via Docker.
    * **Contêiner SQL:** Nome: `sqlserver_estoque`. Porta: `1434`.
2.  **Microsserviços:** Inicie todos os projetos via `dotnet run` (Auth, Gateway, SalesService, StockService).
3.  **Acesso:** Use o Gateway (porta **5117**) para todos os testes.

<h3>5. 🧪 Prova de Validação (Endpoints Chave)</h3>



## 🧪 Prova de Sucesso (Teste de Persistência Assíncrona)

A prova final é o abatimento do estoque.

**Sequência de Teste:**
1.  **Login:** POST `http://localhost:5117/api/auth/login` (Obtém Token JWT).
2.  **Venda Assíncrona:** POST `http://localhost:5117/api/vendas/Pedidos` (Vende N unidades).
3.  **Resultado Esperado (Console do StockService):** O log confirma que o Consumer processou o evento e executou o UPDATE no SQL Server:
