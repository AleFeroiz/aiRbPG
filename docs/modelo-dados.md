# Modelo de dados inicial

Modelo conceitual proposto para discutir antes de criar o banco físico.

## Entidades

### Cliente

- `ClienteId` — chave primária.
- `Nome` — obrigatório.
- `Telefone` — opcional ou obrigatório, a decidir.
- `Email` — opcional.
- `Ativo` — indica se pode ser selecionado em novas reservas.

### Ambiente

- `AmbienteId` — chave primária.
- `Nome` — obrigatório.
- `Capacidade` — inteiro positivo.
- `Descricao` — opcional.
- `Recursos` — descrição simples (por exemplo: mesa grande, mapas, iluminação); normalização em tabela própria pode ser avaliada depois.
- `Ativo` — indica se pode receber novas reservas.

### Reserva

- `ReservaId` — chave primária.
- `ClienteId` — chave estrangeira para Cliente.
- `AmbienteId` — chave estrangeira para Ambiente.
- `Inicio` e `Fim` — data/hora completos, com `Fim > Inicio`.
- `Valor` — decimal não negativo.
- `Status` — inicialmente Pendente, Confirmada ou Cancelada.
- `Observacao` — opcional.
- `CriadaEm` — data/hora de criação para histórico.

## Relacionamentos

- Um cliente pode ter várias reservas; cada reserva pertence a um cliente.
- Um ambiente pode ter várias reservas ao longo do tempo; cada reserva ocupa um ambiente.

## Integridade e disponibilidade

As chaves estrangeiras preservam as relações entre registros. A aplicação deve validar conflitos para reservas do mesmo ambiente cujos status bloqueiem agenda. A validação deve ocorrer também na operação de gravação, não apenas na tela de consulta, pois a disponibilidade pode mudar entre consultar e salvar.

No SQL Server, uma restrição `CHECK` pode garantir `Fim > Inicio` e `Valor >= 0`. A ausência de sobreposição entre intervalos depende de lógica transacional na aplicação ou de outra estratégia deliberada; uma restrição única simples não cobre esse caso.

## Diagrama textual

```text
Cliente (1) ─────── (N) Reserva (N) ─────── (1) Ambiente
                         │
                         └─ intervalo, valor e status
```

Antes do script SQL, confirmar com o grupo a política de preços, os campos de contato, os status e se períodos atravessando a meia-noite serão aceitos.
