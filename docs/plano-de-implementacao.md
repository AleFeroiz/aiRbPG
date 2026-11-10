# Plano de preparação e implementação

## Decisões tomadas nesta preparação

- Interface desktop com C# e Windows Forms, alinhada ao plano da disciplina.
- SQL Server como banco relacional.
- Produto com três perfis: Administrador, Atendente e Cliente sem acesso ao aplicativo.
- O grupo tem até três integrantes e prevê entrega/demonstração em 24/11/2026.
- SDK .NET 10.0.114 instalado no notebook Linux Mint 22.3. O design visual e execução WinForms devem ser validados em Windows.
- Separar formulários, regras de negócio e acesso a dados para facilitar manutenção e demonstração.
- Repositório local Git com documentação e arquivos de configuração versionáveis; dados locais, builds e credenciais ficam fora do versionamento.

## Decisões ainda pendentes

- Confirmação do professor sobre a versão do .NET e modelo de projeto aceitos no laboratório.
- SQL Server Express, LocalDB ou outra instalação disponível.
- Método de acesso a dados (ADO.NET direto ou tecnologia apresentada pelo professor).
- Regra de preço (unidade de cobrança, descontos e forma de calcular multas), status e transições.
- Nomes dos integrantes; o resumo recebido informa que são até três.
- Dois requisitos não funcionais e quatro regras de negócio ainda não detalhados no resumo recebido.

## Estrutura de solução sugerida

```text
src/
  ReservasRpg.WinForms/       # formulários, menus e controles
  ReservasRpg.Core/           # entidades e regras de negócio (se o escopo justificar)
  ReservasRpg.Data/           # conexão, consultas e repositórios
database/
  scripts/                    # criação do banco e carga demonstrativa
docs/
```

Se a versão ou o tempo do curso favorecer uma solução menor, os projetos Core e Data podem ser pastas/classes dentro do projeto WinForms. A estrutura não é compromisso com uma arquitetura pesada.

## Etapas sugeridas

1. Completar requisitos com nomes do grupo e confirmar lacunas com professor/integrantes.
2. Revisar os diagramas UML iniciais de casos de uso e classes.
3. Fechar o modelo relacional e preparar scripts SQL Server de criação e carga demonstrativa.
4. Criar solução WinForms no alvo .NET aprovado pelo professor.
5. Implementar primeiro login/permissões, cadastros e fluxo mínimo de reservas.
6. Implementar retirada/devolução, pagamentos, multas e consultas/relatórios priorizados.
7. Abrir e testar formulários em Windows, preparar dados de demonstração e roteiro para a P2 de 24/11/2026.

## Fluxo Git sugerido

- Commits pequenos com mensagens objetivas, por exemplo `docs: define requisitos iniciais`.
- Usar branches para mudanças maiores quando o grupo começar a trabalhar em paralelo.
- Não versionar senhas, arquivos locais de banco, pastas `.vs`, `bin` ou `obj`.
