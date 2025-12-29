# 🚗 Plataforma de Revenda de Veículos – Arquitetura em Microserviços - SAGA orquestrada
Este projeto foi desenvolvido como trabalho acadêmico da Pós-Tech em Software Architecture, com o objetivo de implementar uma plataforma de revenda de veículos automotores, utilizando arquitetura de microserviços, padrão SAGA Orquestrada, .NET 8 e serviços cloud-ready.

A solução contempla todo o fluxo de compra de um veículo, desde o cadastro do cliente até a confirmação da venda, com foco em escalabilidade, segurança e consistência eventual.

🧩 Visão Geral da Solução

A plataforma é composta por 5 microserviços independentes, cada um responsável por um domínio específico do negócio:

CustomerService – Cadastro de compradores e tratamento de dados sensíveis (LGPD)

InventoryService – Cadastro de veículos, reserva, cancelamento e confirmação de venda

PaymentService – Criação e controle de pagamentos

SalesService – Registro e histórico de vendas

OrchestratorService – Coordenação do fluxo de compra (SAGA Orquestrada)

Cada microserviço possui seu próprio banco de dados DynamoDB, garantindo isolamento e baixo acoplamento.

🏗️ Arquitetura
🔹 Estilo Arquitetural

Arquitetura baseada em microserviços

Arquitetura Hexagonal (Ports & Adapters) em cada serviço

SAGA Orquestrada para transações distribuídas

Consistência eventual

🔹 Tecnologias Principais

.NET 8

FastEndpoints

Amazon DynamoDB (via LocalStack para desenvolvimento)

Docker & Docker Compose

AWS Cloud-ready (ECS / Lambda / DynamoDB / KMS)

🔄 Fluxo de Compra (SAGA Orquestrada)

Cliente seleciona um veículo no front-end

Orchestrator valida se o cliente existe

Inventory reserva o veículo

Payment cria o pagamento e retorna um código

Cliente realiza o pagamento

Orchestrator valida o status do pagamento

Inventory confirma a venda

Sales registra a venda

Processo é finalizado com sucesso

👉 Em caso de falha em qualquer etapa, o Orchestrator executa ações compensatórias, como o cancelamento da reserva.

🔐 Segurança e LGPD

A solução foi projetada seguindo os princípios da Lei Geral de Proteção de Dados (LGPD):

Dados sensíveis (CPF, e-mail, telefone, endereço) ficam restritos ao CustomerService

Dados sensíveis são criptografados antes da persistência

Chaves criptográficas são mantidas fora do código-fonte

Cada microserviço acessa apenas seu próprio banco de dados

Princípio do menor privilégio aplicado

Em ambiente AWS real, a solução pode utilizar:

AWS KMS para gerenciamento de chaves

AWS Secrets Manager para segredos sensíveis

📦 Estrutura do Projeto

<img width="414" height="632" alt="image" src="https://github.com/user-attachments/assets/ef74d6e3-5fe9-4474-9ea5-797cbd729691" />



Cada serviço segue o padrão:

Core: Domínio, UseCases e Ports

Adapters: Persistência, Integrações externas

Api: Endpoints HTTP

▶️ Como Executar o Projeto (Local)
Pré-requisitos

Docker

Docker Compose

.NET 8 SDK (opcional para debug local)

Subir a aplicação
docker compose up -d --build


Serviços disponíveis:

Orchestrator: http://localhost:7000

Customer: http://localhost:7001

Inventory: http://localhost:7002

Sales: http://localhost:7003

Payment: http://localhost:7004

🧪 Testes End-to-End (Smoke Test)

Fluxo básico:

Criar cliente (CustomerService)

Criar veículo (InventoryService)

Iniciar compra (Orchestrator)

Pagar (PaymentService)

Finalizar compra (Orchestrator)

Validar venda (SalesService)

☁️ Cloud Ready

Embora o desenvolvimento utilize LocalStack, a arquitetura está preparada para execução em nuvem real, com:

AWS DynamoDB

AWS ECS / Fargate

AWS Lambda (opcional)

AWS KMS / Secrets Manager

Amazon SQS / EventBridge (evolução futura)

🎓 Contexto Acadêmico

Este projeto foi desenvolvido como trabalho substitutivo da Pós-Tech em Software Architecture, demonstrando:

Aplicação prática de microserviços

Uso do padrão SAGA

Arquitetura Hexagonal

Boas práticas de segurança e LGPD

Uso de serviços gerenciáveis de nuvem

👨‍💻 Autor

Douglas Ferreira Miyamoto
Pós-Tech em Software Architecture – FIAP
