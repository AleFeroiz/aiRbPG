# Plano de preparação e implementação

## Decisões tomadas nesta preparação

- Interface desktop com C# e Windows Forms, alinhada ao plano da disciplina.
- SQL Server como banco relacional.
- Produto com três perfis: Administrador, Atendente e Cliente sem acesso ao aplicativo.
- O grupo tem até três integrantes e prevê entrega/demonstração em 24/11/2026.
- SDK .NET 10 instalado no notebook Linux Mint 22.3. A solução usa `net10.0-windows`; execução e validação de WinForms devem ser feitas em Windows.
- Nome do projeto aiRbPG; três integrantes confirmados; entrega documental em DOCX e PDF.
- Separar formulários, regras de negócio e acesso a dados para facilitar manutenção e demonstração.
- Repositório local Git com documentação e arquivos de configuração versionáveis; dados locais, builds e credenciais ficam fora do versionamento.

## Decisões ainda pendentes

- Validar compatibilidade do .NET 10 com o Visual Studio disponível na máquina Windows de teste.
- Hospedagem definida em um computador Windows, usando SQL Server. Falta escolher a máquina e a instância concreta (Express local ou servidor do laboratório).
- Método de acesso definido nesta implementação: ADO.NET com `Microsoft.Data.SqlClient`; confirmar se está alinhado ao conteúdo esperado pelo professor.
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

O acesso a dados usa ADO.NET com `Microsoft.Data.SqlClient` e consultas parametrizadas. A interface WinForms compila para Windows; no Linux é possível editar arquivos e preparar o código, mas não executar nem validar visualmente os formulários.

## Etapas sugeridas

1. Completar requisitos com nomes do grupo e confirmar lacunas com professor/integrantes.
2. Revisar os diagramas UML iniciais de casos de uso e classes.
3. Fechar o modelo relacional e preparar scripts SQL Server de criação e carga demonstrativa.
4. Abrir a solução no computador Windows e confirmar o SDK/Visual Studio compatíveis.
5. Preparar a instância SQL Server no Windows, executar o script e configurar `AIRBPG_SQL_CONNECTION`.
6. Validar login/permissões, cadastros e fluxo mínimo de reservas no Windows.
7. Implementar alteração/cancelamento, retirada/devolução, pagamentos, multas e consultas/relatórios priorizados.
8. Preparar dados de demonstração, documentação DOCX/PDF e roteiro para a P2 de 24/11/2026.

## Fluxo Git sugerido

- Commits pequenos com mensagens objetivas, por exemplo `docs: define requisitos iniciais`.
- Usar branches para mudanças maiores quando o grupo começar a trabalhar em paralelo.
- Não versionar senhas, arquivos locais de banco, pastas `.vs`, `bin` ou `obj`.
