# Progresso do aiRbPG

## Ambiente Windows em 22/11/2026

- Cópia local: C:\Meus Projetos\aiRbPG, origin AleFeroiz/aiRbPG, base 003c206. Sem publicação remota nesta preparação.
- Visual Studio Community 2022 17.6.5 já existente. Instalado .NET Framework 4.6.1 Developer Pack oficial; assinatura Microsoft válida, saída 0.
- Instalado Microsoft Build Tools 2015 14.0.23107.10, assinatura Microsoft válida, saída 0. Compilador C# 1.0.0.50618.
- Rebuild de Debug e Release/Any CPU com C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe concluído com zero erros. Um aviso CS0105 em CatalogKind.cs (using System duplicado), já presente na base.
- Preservados TargetFrameworkVersion v4.6.1, LangVersion 6, ToolsVersion 14.0 e solução Visual Studio 14. Não é necessário converter o projeto para usar VS2015.
- A IDE completa Visual Studio 2015 não foi instalada nem aberta nesta máquina. A evidência é a compilação com suas ferramentas reais; teste de edição/Designer/F5 dentro dessa IDE permanece pendente.

## SQL Server e execução

- Instalado SQL Server 2022 Express 16.0.1000.6, instância localhost\SQLEXPRESS, serviço automático e autenticação integrada do Windows. TCP e Named Pipes desativados nesta instalação local; sem configuração de acesso remoto.
- Primeira tentativa pelo winget falhou por mídia pt-BR com opção ENU. Instalação direta da mídia em português concluiu com saída 0. Logs em C:\Program Files\Microsoft SQL Server\160\Setup Bootstrap\Log\20261122_154255.
- scripts/configurar-banco.ps1 executado duas vezes no Windows PowerShell 5.1, sem erro; SQL original preservado. Banco aiRbPG com 11 tabelas e dados demonstrativos do script, sem duplicação na segunda execução.
- AIRBPG_SQL_CONNECTION salva para o usuário Windows atual, sem senha. Configuração aponta para banco aiRbPG e autenticação Windows. Reabrir o Visual Studio após configurar.
- Verificação isolada em aiRbPG_Validacao com código compilado pelo C# 2015: administrador inicial; login correto; rejeição de senha incorreta; leitura dos quatro catálogos; reserva de duas horas com total 160; segunda reserva no mesmo período rejeitada. Banco temporário removido ao final. Não foram criados usuários de teste no banco de uso.
- Banco de uso conferido com zero usuários e zero reservas. Aplicativo Debug aberto e tela "Primeiro acesso" observada no Windows, sem mensagem de falha de banco.

## Próximo uso e limites

- Na tela aberta, o usuário escolhe nome, login e senha de no mínimo 10 caracteres; depois entra com a conta criada. Nenhuma senha padrão foi definida.
- Para abrir o código, usar aiRbPG.sln e definir aiRbPG.WinForms como projeto de inicialização. VS2015 requer Update 2 ou posterior e Developer Pack 4.6.1.
- SQL Server é um programa separado da IDE: esta instalação local usa a versão 2022. Não implica exigir Visual Studio 2022 para compilar a aplicação. Em outra máquina, configurar a instância e executar o script de banco.
- Não foram validados todos os formulários, Designer do VS2015, permissões por perfil ou todos os fluxos de negócio. Pendências funcionais anteriores continuam no plano.

## Implementação operacional em 22/11/2026
- Cópia canônica agora em Zé/aiRbPG. Referências do professor preservadas em Zé/referencias. A pasta antiga está bloqueada por um processo e será limpa somente após conferência da cópia.
- Implementados edição transacional, confirmação, cancelamento, pagamentos parciais, desconto administrativo, retirada integral, devolução parcial com dano/perda, quitação/finalização, histórico, usuários/atendentes e relatórios por período.
- Validada autorização de alterações de tarifas/catálogos e consultas administrativas na camada de dados. Inativação lógica e credenciais com hash preservadas.
- Script de configuração agora aplica migrações SQL ordenadas; 002_historico.sql adiciona histórico sem alterar o SQL original.
- Correções: entradas de quantidades inválidas não viram zero silenciosamente; valores arredondados por item; campos de reserva com espaço adequado; consulta de espaços livres distingue identificadores.
- Primeira suíte Debug: 45 verificações aprovadas. Suíte ampliada Release: 51 verificações aprovadas, zero erros/avisos de compilação com MSBuild 14.0. Script scripts/testar.ps1 cria banco único aiRbPG_Test_*, aplica migrações e remove-o em finally. Dados de uso não alterados por testes.
- Verificação visual e documentação final em andamento; não confundir build pelas ferramentas 2015 com teste de Designer/F5 na IDE 2015.

## Revisão visual interrompida
- Painel e primeira versão da tela de reservas observados em banco aiRbPG_UI_* isolado. Detectado layout que ocultava participantes/observações; corrigidas alturas e proporções, versão Release recompilada sem erro/aviso.
- A segunda revisão não foi concluída: o controle do Windows foi interrompido pelo usuário com Escape. Não afirmar validação visual do ajuste nem entrega final.
- Adicionados tests/InterfaceHarness.cs e scripts/verificar-interface.ps1 para QA isolado; o script remove seu banco ao fechar o aplicativo de QA. A segunda instância de QA pode continuar aberta.
- Pendências: concluir revisão de telas, gerar/revisar DOCX/PDF e dois diagramas finais, atualizar requisitos/manual, aplicar migração 002 ao banco de uso e empacotar entrega. Código novo passou 51 verificações em Release; a alteração posterior foi somente de layout e foi recompilada.
- Pasta canônica em Zé/aiRbPG. Originais do professor e print em Zé/referencias. Cópia anterior C:\Meus Projetos\aiRbPG ainda preservada por bloqueio de movimentação; não é a cópia atual de desenvolvimento.

## Correção dos cadastros e validação visual em 22/11/2026
- Banco real aiRbPG estava sem HistoricoReservas; executadas migrações 001 e 002 preservando a conta e os registros existentes. SQL Server está rodando e a conexão integrada abriu com sucesso.
- Cadastro inicia em modo Novo, sem selecionar automaticamente o primeiro registro. Novo limpa a seleção; seleção explícita habilita edição. Proteção contra duplo clique ao salvar. Conexão também consulta a configuração do usuário quando o processo não herdou a variável.
- Adicionados INICIAR.cmd e scripts/iniciar.ps1: carregam a conexão atual, verificam o banco atualizado e abrem o executável Release canônico.
- Verificado pelo Windows em banco QA isolado: cliente criado via Salvar com mensagem de sucesso e novo identificador na lista SQL; reserva criada via formulário; editor carregou os dados da reserva; confirmação refletiu na lista; usuários e relatório de recebimentos carregaram sem erro.
- Corrigida atualização de estoque que zerava acessórios no editor. Nova revisão confirmou quantidade 2 antes e depois de Atualizar estoque. Participantes, observações e botões visíveis em janela restaurada e maximizada.
- Capturas verificadas em docs/imagens: clientes.png, reservas.png, operacoes.png e relatorios.png. Bancos de QA removidos ao fechar as instâncias de teste. Não houve automação de senha da conta de uso.
- Suíte Release repetida após correção do cadastro: 51 verificações aprovadas; ajuste posterior de preservação dos itens recompilado sem erros e validado visualmente.
- Conteúdo da antiga cópia local movido para Zé/arquivo-anterior, preservado. No caminho antigo ficou apenas LEIA-ME.txt apontando para a versão atual.
- Instalador oficial VS2015 Community Update 3 (14.0.25420.1, assinatura Microsoft válida) tentou instalar a IDE, mas retornou 1603 por pré-requisito indisponível. Tentativa por mídia offline em andamento; não declarar a IDE instalada antes de conferir.

## Revisão final do código e documentação
- Solução ordenada com WinForms como primeiro projeto C#, facilitando a inicialização em uma cópia nova. Rebuild Debug final: 51 verificações aprovadas; Release atualizado sem erros/avisos. Log em Zé/.revisao/testes/testes-debug.txt.
- Documentação DOCX e PDF com 14 páginas, requisitos consolidados, regras, dois diagramas UML, dados, instalação, manual e conferência do enunciado. Todas as páginas renderizadas pelo Word e inspecionadas. O renderizador padrão falhou por ausência de LibreOffice; exportação nativa do Word usada, sem alterar o original do professor.
- Corrigida borda herdada do estilo de título; versão final reexportada. Comparação dos PNGs mostrou alteração somente nas páginas 1 e 6, reinspecionadas. Diagramas PNG e fontes PlantUML coerentes; fontes UML iniciais preservadas em Zé/referencias/uml-inicial.
- Requisitos distinguem funcionalidades implementadas de kits opcionais, e deixam claros os limites: sem estorno, cobrança online, recuperação de senha, multa automática ou teste de carga. Conferência cobre cada exigência do professor.
- Mídia oficial VS2015 Update 3 completa, 7.617.847.296 bytes, montada somente leitura em F:. Instalador 14.0.25420.1 com assinatura Microsoft válida; instalação offline iniciada. Ainda aguardando resultado.

## Entrega final e IDE 2015 confirmadas
- Visual Studio Community 2015 Update 3 instalado pela mídia oficial offline; instalador terminou com código 0 e sem reinicialização. devenv.exe 14.0.25420.1 presente; vswhere -legacy confirmou VisualStudio.14.0.
- Rebuild da solução pela própria IDE via devenv.com: 3 projetos aprovados, 0 falhas. Log em Zé/.revisao/testes/build-ide-vs2015.txt. A IDE informou licença inicial de 30 dias; eventual entrada/ativação da Community deve ser feita pelo usuário, sem automação de autenticação. Designer e F5 não foram homologados.
- Documento final de 14 páginas atualizado com a instalação real e reexportado pelo Word. Páginas 13 e 14 alteradas foram reinspecionadas; demais imagens permaneceram idênticas à revisão aprovada.
- Pacote Zé/entrega/aiRbPG-Entrega.zip criado: 83 arquivos, cerca de 2,2 MB, pasta principal com os três nomes completos, integrantes.txt, código, SQL, aplicativo Release, DOCX/PDF, dois diagramas e evidências. Sem banco local, contas, senhas, .git, .vs, obj ou instaladores.
- ZIP extraído em pasta separada: 82 hashes do manifesto confirmados e todos os arquivos obrigatórios presentes. Solução extraída reconstruída pela própria IDE 2015: 3 projetos aprovados, 0 falhas. Maior caminho da pasta de entrega: 205 caracteres, abaixo do limite clássico do Windows.
- Início nesta máquina: Zé/INICIAR-aiRbPG.cmd. Código na IDE instalada: Zé/ABRIR-NO-VS2015.cmd. Banco de uso permanece aiRbPG, com a conta anterior preservada. A versão antiga está em arquivo-anterior e não é o ponto de desenvolvimento.
- Trabalho concluído no escopo de entrega documentado. Envio à plataforma e apresentação são realizados pelo grupo. Não foi feito push ao GitHub.
- A IDE2015 expandiu automaticamente Forms\*.cs para a lista explícita dos oito formulários no csproj WinForms. Alteração revisada e preservada; alvo 4.6.1 e linguagem 6 inalterados. O pacote já contém essa forma explícita, idêntica ao projeto atual.
- Iniciador final executado com sucesso: processo da versão canônica Release aberto na tela aiRbPG — Acesso, conexão real conferida e tabela HistoricoReservas presente.

## Validação no PC da faculdade em 24/11/2026

- A pasta em Downloads veio sem .git. Foi vinculada a origin/main na base e807f22, com árvore limpa antes das alterações. A IDE estava aberta em uma segunda cópia no Desktop e foi reaberta na solução de Downloads.
- IDE Visual Studio 2015 Update 3 e MSBuild 14.0, ambos 14.0.25420.1, com referências do .NET Framework 4.6.1 disponíveis. O ActivityLog registrava erro 80131500 ao carregar ErrorListPackage e CSharpPackage, cache de componentes desatualizado e falha de carregamento de Microsoft.VisualStudio.Workspaces.Contracts 14.0. Os três projetos usam referências do Framework e do projeto, sem pacotes NuGet. A Lista de Erros consultada inicialmente retornou zero itens apesar dos erros no ActivityLog.
- O cache ComponentModelCache foi renomeado como cópia de segurança e regenerado após reiniciar a IDE. Na reabertura, as falhas de cache e carregamento de pacotes anteriores não reapareceram no ActivityLog consultado. Na solução correta, a Lista de Erros retornou zero itens. A saída de devenv.com /Rebuild em Release/Any CPU registrou três projetos aprovados, zero falhas e zero ignorados. Designer e F5 não foram exercitados.
- SQL Server Enterprise, serviço MSSQLSERVER, acessível em localhost com a conta Windows. Também existe LocalDB MSSQLLocalDB. Não havia AIRBPG_SQL_CONNECTION nem banco aiRbPG visível na instância padrão. O iniciador selecionou a instância padrão existente, criou o banco e confirmou dbo.HistoricoReservas. Nenhuma nova instância ou ferramenta do sistema foi instalada.
- O INICIAR.cmd foi executado com diretório atual diferente da raiz, compilou Release com MSBuild 14.0 e abriu aiRbPG — Primeiro acesso. O segundo clique detectou a janela já aberta. O banco de uso permaneceu sem conta criada pelo teste.
- A suíte Release executada em banco temporário na instância padrão passou nas 51 verificações, incluindo primeiro administrador, login válido e inválido, cadastro e edição de cliente, reserva, conflito de agenda e operações. O banco temporário foi removido. Esses testes exercitaram as camadas de domínio e dados. Pela interface WinForms foi confirmada a abertura da tela de primeiro acesso.
