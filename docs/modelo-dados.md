# Modelo conceitual de dados

Rascunho baseado no resumo do grupo; deve ser validado antes do script físico. O modelo inclui entidades adicionais sugeridas para representar os fluxos de cobrança e devolução já previstos nos requisitos.

## Entidades principais

### Usuario

Autenticação e autorização dos funcionários.

- `UsuarioId` — chave primária.
- `Nome`, `Login` — obrigatórios; `Login` único.
- `SenhaHash` — hash de senha, nunca texto puro.
- `Perfil` — Administrador ou Atendente.
- `Ativo` — inativação lógica.

### Cliente

- `ClienteId` — chave primária.
- `Nome` — obrigatório.
- `Telefone`, `Email` — dados de contato; obrigatoriedade a confirmar.
- `Ativo` — inativação lógica.

### Espaco

- `EspacoId` — chave primária.
- `Nome`, `Descricao`, `Endereco`.
- `Capacidade` — inteiro positivo.
- `Tarifa` — valor decimal não negativo; unidade (hora, período etc.) a definir.
- `Ativo` — inativação lógica.

### SistemaRpg

- `SistemaRpgId` — chave primária.
- `Nome` — por exemplo, Dungeons & Dragons.
- `Descricao`, `Ativo`.

### Acessorio

- `AcessorioId` — chave primária.
- `SistemaRpgId` — referência opcional/obrigatória ao sistema compatível; confirmar se um acessório pode ser compartilhado por vários sistemas.
- `Nome`, `Descricao`, `QuantidadeEstoque`, `Tarifa`, `Ativo`.
- Quantidade disponível não deve ser negativa; tarifa não negativa.

### Kit e ItemKit (opcional)

`Kit` contém `KitId`, nome, descrição, tarifa opcional e ativo. `ItemKit` associa kit e acessório com quantidade. A associação permite reutilizar o mesmo acessório em vários kits.

### Reserva

- `ReservaId` — chave primária.
- `ClienteId`, `EspacoId`, `UsuarioIdCriacao` — chaves estrangeiras.
- `Inicio`, `Fim` — data e hora; `Fim > Inicio`.
- `NumeroParticipantes` — positivo e não maior que a capacidade.
- `Status` — ciclo de vida a fechar com o grupo.
- `ValorEspaco`, `Desconto`, `Multas`, `ValorTotal` — valores monetários registrados para manter histórico mesmo que tarifas mudem.
- `CriadaEm`, observações e dados de cancelamento, conforme escopo.

### ItemReserva

Associa acessórios à reserva. Campos sugeridos: `ItemReservaId`, `ReservaId`, `AcessorioId`, quantidade, tarifa unitária acordada, quantidades retiradas/devolvidas e quantidades danificadas/perdidas. A regra de estoque deve considerar o período e a situação das reservas.

### Pagamento (sugerido)

Registra pagamentos manuais associados à reserva: `PagamentoId`, `ReservaId`, valor, data/hora, método, observação e usuário que registrou. Permite mais de um pagamento por reserva e consulta do saldo, se o grupo decidir suportar pagamentos parciais.

## Relacionamentos

```text
Cliente 1 ─── N Reserva N ─── 1 Espaco
Usuario 1 ─── N Reserva
Reserva 1 ─── N ItemReserva N ─── 1 Acessorio N ─── 1 SistemaRpg
Kit 1 ─── N ItemKit N ─── 1 Acessorio        (opcional)
Reserva 1 ─── N Pagamento                    (proposto)
```

## Integridade e concorrência

- Não pode haver interseção de intervalos para o mesmo espaço entre reservas em status que bloqueiam agenda. Para intervalos semiabertos, há conflito quando `novoInicio < fimExistente` e `novoFim > inicioExistente`.
- A disponibilidade de acessórios é a quantidade total menos as quantidades reservadas em intervalos sobrepostos com status bloqueador. A gravação precisa verificar e atualizar em transação para impedir que duas reservas concorrentes consumam o mesmo estoque.
- A quantidade de participantes não pode ultrapassar a capacidade do espaço.
- Uma reserva cancelada não deve bloquear agenda/estoque, mas permanece consultável no histórico.
- O valor histórico da reserva deve guardar tarifas aplicadas no momento do acordo; mudanças futuras na tarifa não alteram reservas antigas.
- Consultas e gravações precisam respeitar o perfil do usuário; a regra de autorização também deve ser aplicada fora da interface, no fluxo de negócio.

## Pontos a decidir antes do SQL físico

1. Unidade de cobrança de cada espaço e acessório e política de arredondamento.
2. Status e quais deles bloqueiam disponibilidade.
3. Pagamento total ou parcial e métodos aceitos.
4. Política de reserva/estoque de kits e compatibilidade entre sistemas.
5. Se acessórios podem pertencer a mais de um sistema de RPG.
6. Como representar horário de retirada/devolução e multa por atraso, além de dano/perda.
