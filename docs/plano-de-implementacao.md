# Plano de preparação e implementação

## Decisões tomadas nesta preparação

- Interface desktop com C# e Windows Forms, alinhada ao plano da disciplina.
- SQL Server como banco relacional.
- Separar formulários, regras de negócio e acesso a dados para facilitar manutenção e demonstração.
- Repositório local Git com documentação e arquivos de configuração versionáveis; dados locais, builds e credenciais ficam fora do versionamento.

## Decisões ainda pendentes

- Versão do .NET e modelo de projeto aceitos no laboratório do curso.
- SQL Server Express, LocalDB ou outra instalação disponível.
- Método de acesso a dados (ADO.NET direto ou tecnologia apresentada pelo professor).
- Regra de preço e se reservas começam pendentes ou confirmadas.
- Login/perfis, se forem exigidos pelo professor.
- Entregáveis, prazo, tamanho do grupo e critérios específicos de avaliação do projeto prático.

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

1. Confirmar requisitos, regras e critérios com o grupo e o professor.
2. Fechar modelo relacional e desenhar protótipo simples das telas.
3. Criar solução WinForms e script versionado para criar o banco e dados demonstrativos.
4. Implementar cadastros de clientes e ambientes.
5. Implementar consulta de disponibilidade e ciclo de vida da reserva, incluindo prevenção de sobreposição.
6. Preparar dados de demonstração, instruções de execução e roteiro de apresentação.

## Fluxo Git sugerido

- Commits pequenos com mensagens objetivas, por exemplo `docs: define requisitos iniciais`.
- Usar branches para mudanças maiores quando o grupo começar a trabalhar em paralelo.
- Não versionar senhas, arquivos locais de banco, pastas `.vs`, `bin` ou `obj`.
