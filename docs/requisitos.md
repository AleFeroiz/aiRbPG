# Requisitos do sistema

Documento inicial a partir do resumo fornecido pelo grupo. As regras e prioridades ainda devem ser confirmadas por todos os integrantes e pelo professor.

## Identificação e entrega

- Disciplina: Linguagem de Programação VIII — Fatec Itapetininga.
- Grupo: até três integrantes.
- Entrega e demonstração: **24/11/2026 (P2)**.
- Integrantes: preencher com os nomes completos antes de gerar a versão final para entrega.

## Perfis

- **Administrador:** gerencia cadastros e tarifas, além de acessar todas as operações e relatórios financeiros.
- **Atendente:** opera reservas, pagamentos manuais, retiradas e devoluções; não altera tarifas nem acessa o faturamento.
- **Cliente:** não acessa o aplicativo; é cadastrado pelo funcionário e associado às reservas.

## Requisitos funcionais

Prioridade **Alta** indica o MVP planejado para a demonstração. As prioridades não informadas ficam para validação do grupo.

| ID | Requisito | Prioridade |
|---|---|---|
| RF01 | Permitir que usuários façam login. | Alta |
| RF02 | Restringir as operações conforme o perfil de Administrador ou Atendente. | Alta |
| RF03 | Cadastrar, consultar, alterar e inativar clientes. | Alta |
| RF04 | Cadastrar, consultar, alterar e inativar espaços com capacidade e tarifa. | Alta |
| RF05 | Cadastrar, consultar, alterar e inativar sistemas de RPG. | Média |
| RF06 | Cadastrar, consultar, alterar e inativar acessórios com quantidade em estoque e tarifa. | Alta |
| RF07 | Cadastrar kits temáticos compostos por acessórios. | Baixa / opcional |
| RF08 | Consultar espaços e estoque de acessórios disponíveis para um período. | Alta |
| RF09 | Criar reserva de espaço para um cliente, indicando período e participantes. | Alta |
| RF10 | Alterar uma reserva e validar novamente agenda, estoque e capacidade. | Média |
| RF11 | Cancelar reserva preservando seu histórico. | Média |
| RF12 | Incluir acessórios e quantidades na reserva. | Alta |
| RF13 | Calcular o total da reserva a partir de espaço, acessórios, multas e descontos. | Alta |
| RF14 | Registrar a retirada dos itens reservados. | Média |
| RF15 | Registrar devolução e multa por dano ou perda de acessórios. | Média |
| RF16 | Registrar pagamento manual associado à reserva. | Média |
| RF17 | Controlar e exibir o status da reserva durante seu ciclo de vida. | Alta |
| RF18 | Consultar reservas com filtros. | Alta |
| RF19 | Consultar estoque disponível por data/período. | Alta |
| RF20 | Consultar faturamento por período, somente para Administrador. | Baixa |
| RF21 | Consultar os itens mais alugados. | Baixa |

## Requisitos não funcionais

O resumo do grupo informa doze requisitos não funcionais, mas especifica dez. Os dez requisitos identificados estão abaixo; faltam dois para confirmar.

| ID | Requisito |
|---|---|
| RNF01 | Aplicativo desktop desenvolvido em C# com Windows Forms. |
| RNF02 | Persistir os dados em banco relacional Microsoft SQL Server. |
| RNF03 | Armazenar senhas com algoritmo de hash apropriado, nunca em texto puro. |
| RNF04 | Usar consultas parametrizadas para evitar SQL Injection. |
| RNF05 | Validar os campos e as regras antes de gravar dados. |
| RNF06 | Usar transações nas operações que alterem várias tabelas. |
| RNF07 | Inativar registros em vez de excluí-los fisicamente quando houver histórico relacionado. |
| RNF08 | Organizar o código em camadas, separando interface, regras e acesso a dados. |
| RNF09 | Manter as operações usuais com resposta de até três segundos, conforme massa de dados e ambiente de demonstração a definir. |
| RNF10 | Entregar script para criação e carga do banco com dados de exemplo. |
| RNF11 | Pendente: detalhar com o grupo. |
| RNF12 | Pendente: detalhar com o grupo. |

## Regras de negócio recebidas

1. Não permitir reservas com períodos sobrepostos para o mesmo espaço.
2. Não permitir alugar mais acessórios do que o estoque disponível no período.
3. O número de participantes não pode exceder a capacidade do espaço.
4. O total é calculado por espaço + acessórios + multas − descontos.
5. Somente o Administrador pode alterar tarifas e consultar faturamento.

O resumo menciona nove regras principais, mas detalha cinco. As outras quatro devem ser fornecidas ou aprovadas pelo grupo antes de serem tratadas como requisitos.

## Critérios iniciais do MVP

- Login e aplicação das permissões por perfil.
- Cadastros necessários para operar uma locação.
- Consulta de disponibilidade de espaço e acessórios.
- Criação de reserva com validação de conflito de horário, estoque e capacidade.
- Cálculo do valor e consulta de reservas.

Os diagramas UML iniciais estão em [`uml/casos-de-uso.puml`](uml/casos-de-uso.puml) e [`uml/classes.puml`](uml/classes.puml). São fontes editáveis; confirmar e exportar imagens/PDF para a entrega final.
