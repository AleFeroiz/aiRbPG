# Visão geral do projeto

## Contexto da disciplina

O plano de ensino de Linguagem de Programação VIII (2026) descreve o desenvolvimento de aplicações em C#, acesso a SQL Server pelo Visual Studio e uso de formulários Windows. Também menciona variáveis, operadores, estruturas de controle, funções e procedimentos, arrays, menus, controles, depuração e geração de aplicações. O plano informa os temas e ferramentas da disciplina; ele não prescreve o tema nem os requisitos deste produto.

## Problema e proposta

Um espaço voltado a RPG de mesa pode ter salas ou mesas com capacidades e recursos diferentes. A equipe precisa saber quais ambientes estão disponíveis em cada período, registrar locações e consultar os dados de clientes e reservas.

O sistema será um aplicativo administrativo para cadastrar clientes e ambientes, organizar a agenda de locações e evitar que o mesmo ambiente seja reservado em horários sobrepostos.

## Usuários

- Funcionário ou administrador do espaço, responsável por manter cadastros e operar a agenda.
- Cliente, registrado no sistema pelo funcionário para associá-lo a uma locação. Portal ou autoatendimento do cliente ficam fora do escopo inicial.

## Objetivo da primeira versão

Entregar um fluxo demonstrável de cadastro de clientes e ambientes, consulta da disponibilidade e criação, edição, cancelamento e consulta de reservas, com dados guardados no SQL Server.

## Limites do escopo inicial

Incluído: cadastro básico, agenda por data/horário, preço informado ou calculado segundo uma regra simples definida pelo grupo, situação da reserva e prevenção de conflito de horários.

Fora do escopo até decisão do grupo: pagamentos online, emissão fiscal, autenticação e perfis avançados, portal público, campanhas/personagens, estoque de produtos, integração com calendário externo e múltiplas filiais.

## Premissas a validar

- Uma reserva ocupa um único ambiente em uma data e intervalo de horário.
- O horário de término deve ser posterior ao de início.
- Reservas canceladas não bloqueiam disponibilidade.
- Reservas confirmadas ou pendentes bloqueiam horários, salvo regra diferente aprovada pelo grupo.
- O grupo definirá se o preço é por hora, por período fixo ou inserido manualmente.
- O grupo e o professor confirmarão versão do .NET, edição do SQL Server e requisitos de entrega/demonstração.
