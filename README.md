# aiRbPG — Airbnb de RPG

Projeto acadêmico da disciplina Linguagem de Programação VIII, da Fatec Itapetininga. A proposta é criar um aplicativo desktop para alugar espaços para sessões de RPG e reservar acessórios temáticos — como livros, miniaturas, dados e fantasias — junto com o espaço.

Nome definido pelo grupo: **aiRbPG**.

Integrantes: Alessandro Silva Vieira, Felipe Gabriel Leite Moreira e Mayzon Rezende do Nascimento.

## Direção técnica

- C# e Windows Forms no Visual Studio para Windows.
- SQL Server para persistência.
- Aplicativo desktop para uso por Administrador e Atendente; clientes não acessam o sistema.
- Entrega e demonstração previstas para **24/11/2026**.
- Documentação final prevista nos formatos DOCX e PDF.
- Nome sugerido do pacote de entrega: `aiRbPG - ALESSANDRO SILVA VIEIRA - FELIPE GABRIEL LEITE MOREIRA - MAYZON REZENDE DO NASCIMENTO`.
- Modelo para a lista exigida pelo professor: [`integrantes.txt`](integrantes.txt).
- Escopo e decisões iniciais: [`docs/visao-geral.md`](docs/visao-geral.md), [`docs/requisitos.md`](docs/requisitos.md), [`docs/modelo-dados.md`](docs/modelo-dados.md) e [`docs/plano-de-implementacao.md`](docs/plano-de-implementacao.md).

## Situação

O projeto já tem uma solução C# com camadas de domínio, acesso a dados e Windows Forms, além do script inicial do SQL Server. Foram iniciados login, cadastro de clientes/espaços/sistemas/acessórios e criação de reservas com validação de agenda, capacidade e estoque. A aplicação WinForms precisa ser compilada, executada e validada em um computador Windows; este notebook Linux serve para preparar o código e a documentação.

## Abrir e configurar no Windows

1. Instale o SDK .NET 10 e o Visual Studio compatível com desenvolvimento desktop .NET, ou use o SDK pela linha de comando.
2. Abra `aiRbPG.sln` no Windows.
3. Instale/configure uma instância SQL Server no computador Windows que será o servidor do banco. A instância pode ser SQL Server Express para desenvolvimento local.
4. Execute [`database/scripts/001_create_database.sql`](database/scripts/001_create_database.sql) no SQL Server Management Studio ou `sqlcmd`.
5. Configure `AIRBPG_SQL_CONNECTION` conforme [`database/README.md`](database/README.md).
6. Inicie `src/aiRbPG.WinForms`. No primeiro acesso, o aplicativo solicita a criação do administrador inicial.

Ainda falta confirmar a fórmula comercial de cobrança, completar alteração/cancelamento, pagamentos, retirada/devolução e relatórios, e validar o sistema no Windows com o SQL Server escolhido.
