# aiRbPG - reservas de espaços e acessórios para RPG

Projeto de Linguagem de Programação VIII, Fatec Itapetininga. C# 6, Windows Forms, .NET Framework 4.6.1 e SQL Server. Integrantes: Alessandro Silva Vieira, Felipe Gabriel Leite Moreira e Mayzon Rezende do Nascimento.

## Abrir nesta máquina

Abra **INICIAR.cmd** na raiz desta cópia. O iniciador compila em Release com MSBuild 14.0, verifica ou prepara o banco aiRbPG e abre o aplicativo. No laboratório, a conexão usa a conta Windows na instância padrão em localhost. Contas e dados existentes são preservados.

Em banco novo, a tela **Primeiro acesso** pede a criação de um administrador com senha de pelo menos 10 caracteres. Entre depois com a conta criada.

Para trabalhar no código, abra **aiRbPG.sln** no Visual Studio 2015 Update 2 ou posterior com o Developer Pack 4.6.1. Se necessário, defina **aiRbPG.WinForms** como projeto de inicialização. Se o VS2015 mostrar erros ao carregar pacotes da própria IDE, reinicie-o com os arquivos salvos. Se persistirem, confira o ActivityLog do Visual Studio e solicite à TI o reparo dos componentes indicados.

## Preparar em outra máquina

1. Faça um clone do repositório ou extraia o ZIP inteiro para uma pasta gravável. Se o Windows marcar um ZIP confiável como vindo da Internet e o PowerShell bloquear os scripts sob RemoteSigned, use **Propriedades → Desbloquear** no ZIP original antes de extrair, ou use um clone Git. Em caso de restrição institucional, consulte a TI.
2. Para compilar, instale o **Microsoft Build Tools 2015 (MSBuild 14.0)** ou o **Visual Studio 2015 Update 2/3** com C#, e o **Developer Pack do .NET Framework 4.6.1**. O iniciador informa qual componente falta. As ferramentas de compilação são suficientes para executar o projeto pelo iniciador.
3. Use uma instância SQL Server ou LocalDB disponível. Se precisar iniciar um serviço, instalar o SQL Server Express/LocalDB ou obter permissões SQL, peça à TI. A conta que prepara um banco precisa poder criá-lo ou migrá-lo. Para um banco já preparado, são necessárias as permissões de uso do aplicativo.
4. Dê duplo clique em **INICIAR.cmd**. O iniciador respeita AIRBPG_SQL_CONNECTION quando configurada. Na detecção automática, dá preferência ao banco aiRbPG existente e, na ausência de um banco, ao único serviço SQL acessível, se houver. Para escolher a instância explicitamente, execute no Prompt de Comando: **INICIAR.cmd -Servidor "SERVIDOR\INSTANCIA"**. Esse modo usa autenticação Windows e salva a conexão escolhida para o usuário.
5. Para usar F5, reabra o Visual Studio após preparar a conexão. Feche o aplicativo antes de recompilar pela IDE.


## O que está entregue

- Login, perfis e administração de usuários.
- Cadastros de clientes, espaços, sistemas e acessórios com inativação lógica.
- Consulta de disponibilidade e reservas com validação de agenda, capacidade e estoque.
- Alteração, confirmação e cancelamento com preservação de histórico.
- Pagamentos parciais manuais, desconto administrativo, retirada integral e devolução parcial com danos/perdas.
- Finalização com itens resolvidos e saldo quitado; relatórios por período.
- Documentação DOCX/PDF, dois diagramas UML, fontes dos diagramas e testes reproduzíveis.

Em cadastros, a tela abre em **Novo**: preencha e salve. Para editar, selecione uma linha. Para outra inclusão, clique **Novo**. A base demonstrativa tem dados fictícios do script.

A política de demonstração cobra espaço e acessórios por hora; a tarifa opcional por participante/hora pode ser zero. O total inclui multas manuais e descontos. Alteração/cancelamento são permitidos antes de retirada e sem pagamento; não há estorno. Kits, associações de catálogo, cobrança online e recuperação de senha não têm interface nesta versão.

## Documentação e evidências

- [Documento editável](docs/documentacao/aiRbPG-Documentacao.docx) e [PDF](docs/documentacao/aiRbPG-Documentacao.pdf).
- [Requisitos consolidados](docs/requisitos.md), [diagramas](docs/uml) e [progresso com evidências](docs/PROGRESSO.md).
- `powershell -NoProfile -File scripts\testar.ps1 -Servidor localhost -Configuracao Release` compila com MSBuild 14.0 e executa 51 verificações em banco SQL temporário. Requer permissão para criar/remover esse banco; não altera o banco de uso.

A compilação com as ferramentas 2015 e o estado de instalação da IDE são registrados separadamente em PROGRESSO.md. A execução não depende de possuir Visual Studio 2022.

## Entrega acadêmica

O pacote está em `../entrega`, com os nomes dos três integrantes, fontes, SQL, integrantes.txt, aplicação compilada e documentação. O PDF do professor e a captura da plataforma permanecem em `../referencias`. Prazo indicado: **24/11/2026 às 23h59**. Um integrante envia e todos participam da demonstração. Nenhum envio à plataforma é automático.
