# Plano de preparação e implementação

## Decisões tomadas nesta preparação

- Interface desktop com C# e Windows Forms, alinhada ao plano da disciplina.
- SQL Server como banco relacional.
- Produto com três perfis: Administrador, Atendente e Cliente sem acesso ao aplicativo.
- O grupo tem até três integrantes e prevê entrega/demonstração em 24/11/2026.
- Compatibilidade solicitada: Visual Studio 2015 Update 2 ou posterior, C# 6 e .NET Framework 4.6.1. Os projetos usam o formato clássico `.csproj`; execução e validação de WinForms devem ser feitas em Windows.
- Nome do projeto aiRbPG; três integrantes confirmados; entrega documental em DOCX e PDF.
- Separar formulários, regras de negócio e acesso a dados para facilitar manutenção e demonstração.
- Repositório local Git com documentação e arquivos de configuração versionáveis; dados locais, builds e credenciais ficam fora do versionamento.

## Decisões ainda pendentes

- Na máquina de teste, instalar o .NET Framework 4.6.1 Developer Pack e compilar a solução no Visual Studio 2015 Update 2 ou posterior.
- Hospedagem definida em um computador Windows, usando SQL Server. Falta escolher a máquina e a instância concreta (Express local ou servidor do laboratório).
- Método de acesso definido nesta implementação: ADO.NET com `System.Data.SqlClient` integrado ao .NET Framework e consultas parametrizadas.
- Fórmula de preço a partir de horas, participantes e acessórios; taxas extras, caução, descontos e política de cancelamento.
- Status e transições; regras de pagamento, retirada, devolução, atraso, dano e perda.
- Dois requisitos não funcionais e quatro regras de negócio ainda não detalhados no resumo recebido.

## Estrutura atual da solução

```text
src/
  aiRbPG.WinForms/            # formulários e interação
  aiRbPG.Domain/              # modelos e regras de negócio
  aiRbPG.Data/                # conexão e repositórios SQL Server
database/
  scripts/                    # criação do banco e carga demonstrativa
docs/
```

O acesso a dados usa ADO.NET com `System.Data.SqlClient` e consultas parametrizadas. A interface WinForms compila para Windows; no Linux é possível editar arquivos e preparar o código, mas não executar nem validar visualmente os formulários.

## Etapas sugeridas

1. Completar requisitos com nomes do grupo e confirmar lacunas com professor/integrantes.
2. Revisar os diagramas UML iniciais de casos de uso e classes.
3. Fechar o modelo relacional e preparar scripts SQL Server de criação e carga demonstrativa.
4. Instalar o Visual Studio 2015 Update 2 (ou posterior) e o .NET Framework 4.6.1 Developer Pack; abrir e compilar a solução no computador Windows.
5. Preparar a instância SQL Server no Windows, executar o script e configurar `AIRBPG_SQL_CONNECTION`.
6. Validar login/permissões, cadastros e fluxo mínimo de reservas no Windows.
7. Implementar alteração/cancelamento, retirada/devolução, pagamentos, multas e consultas/relatórios priorizados.
8. Preparar dados de demonstração, documentação DOCX/PDF e roteiro para a P2 de 24/11/2026.

## Fluxo Git sugerido

- Commits pequenos com mensagens objetivas, por exemplo `docs: define requisitos iniciais`.
- Usar branches para mudanças maiores quando o grupo começar a trabalhar em paralelo.
- Não versionar senhas, arquivos locais de banco, pastas `.vs`, `bin` ou `obj`.
