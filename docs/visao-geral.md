# Visão geral do projeto — aiRbPG

## Contexto da disciplina

O plano de ensino de Linguagem de Programação VIII (2026) descreve o desenvolvimento de aplicações em C#, acesso a SQL Server pelo Visual Studio e uso de formulários Windows. Também menciona variáveis, operadores, estruturas de controle, funções e procedimentos, arrays, menus, controles, depuração e geração de aplicações. O plano informa os temas e ferramentas da disciplina; o tema, os perfis e os requisitos deste produto vêm do grupo.

## Problema e proposta

O **aiRbPG** (“Airbnb de RPG”) permite que um cliente alugue um espaço adequado à sua sessão e reserve acessórios compatíveis com o sistema de RPG escolhido. Os espaços podem incluir casas, galpões ou cômodos; os itens podem incluir livros, miniaturas, fantasias e dados.

O sistema será um aplicativo administrativo para cadastrar clientes, espaços, sistemas de RPG e acessórios; organizar reservas de espaço e itens; controlar retirada, devolução, pagamento e multas; e evitar conflitos de agenda e estoque.

## Usuários

- Administrador, que gerencia cadastros e tarifas e consulta todas as operações, inclusive faturamento.
- Atendente, que opera reservas, pagamentos manuais, retiradas e devoluções.
- Cliente, registrado por um funcionário e associado à reserva; não acessa o sistema.

## Objetivo da primeira versão

Entregar para a P2 de 24/11/2026 um fluxo demonstrável de login/permissões, cadastros essenciais, disponibilidade de espaços e acessórios, criação de reserva com validação de horário/estoque/capacidade, cálculo do valor e consulta de reservas, com dados guardados no SQL Server. O grupo é composto por Alessandro Silva Vieira, Felipe Gabriel Leite Moreira e Mayzon Rezende do Nascimento.

## Limites do escopo inicial

Incluído no escopo do produto: login e perfis, cadastro de clientes/espaços/sistemas/acessórios, reserva de espaço e acessórios, cálculo de preço, retirada/devolução, pagamentos manuais, multas, consultas e relatórios. O MVP para a demonstração prioriza login, cadastros, reserva com validação de horário e estoque, cálculo de valor e consulta.

Fora do escopo inicial: acesso de cliente, pagamentos online, emissão fiscal, integração com calendário externo, campanhas/personagens e múltiplas filiais. Kits temáticos são opcionais.

## Premissas a validar

- Uma reserva está associada a um cliente e a um espaço, além de poder conter itens/acessórios.
- O grupo precisa fechar os status e as regras de bloqueio, cancelamento e transição entre reserva, retirada e devolução.
- O preço depende das horas selecionadas, da quantidade de participantes e dos acessórios. A fórmula exata, taxas adicionais, descontos/caução e política de cancelamento ainda precisam ser definidos; o Administrador gerencia as tarifas.
- O grupo solicitou compatibilidade com Visual Studio 2015 Update 2 ou posterior; por isso a aplicação usa C# 6 e .NET Framework 4.6.1. SQL Server é a escolha confirmada para o banco; falta definir em qual máquina/servidor ficará a instância.
- A documentação final será entregue em DOCX e PDF. Os nomes completos do grupo estão em [`../integrantes.txt`](../integrantes.txt).
