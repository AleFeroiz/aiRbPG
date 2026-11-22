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
