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

O projeto já tem uma solução C# com camadas de domínio, acesso a dados e Windows Forms, além do script inicial do SQL Server. Foram iniciados login, cadastro de clientes/espaços/sistemas/acessórios e criação de reservas com validação de agenda, capacidade e estoque. A solução foi compilada no Windows com MSBuild 14.0 e o compilador C# de 2015, em Debug e Release. Conexão SQL Server, autenticação e fluxo inicial de reservas foram verificados; veja as evidências e limites em [docs/PROGRESSO.md](docs/PROGRESSO.md).

## Abrir e configurar no Windows

1. Use o Visual Studio 2015 Update 2 ou posterior, com o **.NET Framework 4.6.1 Developer Pack** instalado. O projeto foi convertido para projetos clássicos C# (`.csproj`) e sintaxe C# 6.
2. Abra `aiRbPG.sln` no Windows e selecione a configuração Debug/Any CPU.
3. Instale/configure uma instância SQL Server no computador Windows que será o servidor do banco. A instância pode ser SQL Server Express para desenvolvimento local.
4. Para SQL Server Express local com autenticação Windows, execute `powershell -NoProfile -File scripts\configurar-banco.ps1` na raiz do projeto. O script executa o SQL inicial e configura a conexão para o usuário atual. Para outra instância, use `-Servidor "SERVIDOR\INSTANCIA"`. Alternativamente, execute [`database/scripts/001_create_database.sql`](database/scripts/001_create_database.sql) no SQL Server Management Studio ou `sqlcmd`.
5. Se usou o script, a variável `AIRBPG_SQL_CONNECTION` já estará configurada. Caso contrário, configure-a conforme [`database/README.md`](database/README.md). Reabra o Visual Studio para carregar a variável.
6. Defina `src/aiRbPG.WinForms` como projeto de inicialização e execute (F5). No primeiro acesso, o aplicativo solicita a criação do administrador inicial.

> O alvo .NET Framework 4.6.1 atende ao Visual Studio 2015 Update 2. Essa versão do framework está fora do suporte da Microsoft; use-a por compatibilidade com o ambiente exigido e planeje migrar para uma versão suportada quando a restrição deixar de existir.

Ainda falta confirmar a fórmula comercial de cobrança, completar alteração/cancelamento, pagamentos, retirada/devolução e relatórios, e validar os demais fluxos e a abertura na IDE Visual Studio 2015.
