# aiRbPG - reservas de espaços e acessórios para RPG

Projeto de Linguagem de Programação VIII, Fatec Itapetininga. C# 6, Windows Forms, .NET Framework 4.6.1 e SQL Server. Integrantes: Alessandro Silva Vieira, Felipe Gabriel Leite Moreira e Mayzon Rezende do Nascimento.

## Abrir nesta máquina

A pasta de trabalho atual é `Zé/aiRbPG`. Abra **INICIAR.cmd** para executar a versão Release e usar a conexão do usuário Windows. Sua conta e os dados já existentes permanecem no banco aiRbPG. A cópia antiga está arquivada fora deste repositório; não a use para continuar o desenvolvimento.

Para o código, abra **aiRbPG.sln**. Use Visual Studio 2015 Update 2 ou posterior com Developer Pack 4.6.1. Não altere o alvo do Framework se precisar manter compatibilidade com o VS2015. Se necessário, defina **aiRbPG.WinForms** como projeto de inicialização.

## Preparar em outra máquina

1. Disponibilize SQL Server (Express pode ser usado) e acesso Windows para criar/usar o banco.
2. Na raiz do código, execute `powershell -NoProfile -File scripts\configurar-banco.ps1`. Para outra instância, acrescente `-Servidor "SERVIDOR\INSTANCIA"`.
3. O script aplica **todos** os SQL em database/scripts, na ordem, e configura AIRBPG_SQL_CONNECTION para o usuário atual. Reabra a IDE.
4. Compile a solução em Release/Any CPU e execute INICIAR.cmd ou use Debug/F5 na IDE.
5. Em banco novo, o aplicativo solicita o primeiro Administrador; escolha login e senha de pelo menos 10 caracteres. Não existe senha padrão.

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
- `powershell -NoProfile -File scripts\testar.ps1 -Configuracao Release` compila com MSBuild 14.0 e executa 51 verificações em banco SQL temporário. Requer permissão para criar/remover esse banco; não altera o banco de uso.

A compilação com as ferramentas 2015 e o estado de instalação da IDE são registrados separadamente em PROGRESSO.md. A execução não depende de possuir Visual Studio 2022.

## Entrega acadêmica

O pacote está em `../entrega`, com os nomes dos três integrantes, fontes, SQL, integrantes.txt, aplicação compilada e documentação. O PDF do professor e a captura da plataforma permanecem em `../referencias`. Prazo indicado: **24/11/2026 às 23h59**. Um integrante envia e todos participam da demonstração. Nenhum envio à plataforma é automático.
