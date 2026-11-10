# Requisitos iniciais

Este é um rascunho de escopo para discussão do grupo. Os requisitos abaixo são propostas derivadas da ideia do espaço de RPG; não são instruções copiadas do plano de ensino.

## Requisitos funcionais

1. Cadastrar, consultar, editar e desativar clientes.
2. Cadastrar, consultar, editar e desativar ambientes, incluindo nome, capacidade, descrição e recursos.
3. Pesquisar disponibilidade por data, horário e ambiente (ou listar os ambientes disponíveis no período).
4. Criar uma reserva associando cliente, ambiente, data, início, término e valor.
5. Consultar reservas por período, cliente, ambiente e situação.
6. Alterar dados e horários de uma reserva, validando novamente a disponibilidade.
7. Cancelar uma reserva sem apagar seu histórico.
8. Exibir e manter a situação da reserva (sugestão inicial: Pendente, Confirmada e Cancelada).

## Regras de negócio propostas

- Data, início, término, cliente e ambiente são obrigatórios.
- Término precisa ser posterior ao início.
- Para o mesmo ambiente, dois intervalos ativos não podem se sobrepor. Considerando intervalos semiabertos, há conflito quando `novoInicio < fimExistente` e `novoFim > inicioExistente`; assim, uma reserva pode começar exatamente quando a anterior termina.
- Cancelada não conta como ativa para a validação de conflito.
- Ao editar uma reserva, a própria reserva não deve ser comparada consigo mesma.
- O valor deve ser não negativo. A forma de cálculo será decidida pelo grupo.
- A reserva deve manter seu registro quando cancelada para permitir consulta posterior.

## Requisitos de qualidade e operação

- Interface em português, construída com Windows Forms.
- Validar entradas na interface e no fluxo de gravação antes de persistir.
- Usar consultas parametrizadas para comunicação com o SQL Server.
- Exibir mensagens claras quando houver dados inválidos, falha de acesso ao banco ou conflito de horário.
- Separar responsabilidades de interface, regras de negócio e acesso a dados em nível adequado ao tamanho do projeto.
- Não armazenar senhas ou credenciais de banco no repositório.

## Critérios de aceite sugeridos para a demonstração

- É possível cadastrar um ambiente e um cliente e encontrá-los depois de fechar e abrir o sistema.
- É possível criar uma reserva e consultá-la por data.
- O sistema rejeita uma segunda reserva sobreposta no mesmo ambiente.
- O sistema permite horários encostados sem sobreposição e permite reservar o mesmo horário em ambientes distintos.
- Cancelar uma reserva libera o horário e preserva o registro cancelado.
