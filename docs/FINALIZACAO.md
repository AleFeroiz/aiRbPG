# Finalização - 22/11/2026

## Critério de entrega

Aplicação desktop C# / SQL Server, compilável com ferramentas VS2015, documentação com requisitos e dois diagramas UML, código-fonte e integrantes no pacote. PDF do professor e captura preservados em `../../referencias`.

## Plano

- [x] Ler enunciado e conferir implementação existente.
- [x] Reunir código e referências na pasta Zé; histórico Git preservado.
- [x] Completar ciclo de reserva, pagamento, retirada/devolução e relatórios.
- [x] Gerenciar atendentes e aplicar permissões.
- [x] Testar regras, persistência e interface sem alterar os dados de uso.
- [x] Gerar documentação DOCX/PDF com dois diagramas e manual.
- [x] Revisar e empacotar entrega com fontes, SQL e integrantes.

## Decisões de implementação

- Preservar VS2015 Update 2+, C# 6 e .NET Framework 4.6.1.
- Manter a fórmula horária já implementada como política demonstrativa configurável por tarifas, explicitando-a no manual. Sem taxa de cancelamento ou multa automática inventada.
- Cancelamento somente antes da retirada e sem pagamentos; pagamentos registrados não são apagados. Sem fluxo de estorno nesta entrega.
- Retirada após confirmação; devolução registra quantidades boas, danificadas e perdidas. Multa informada pelo operador com justificativa; itens danificados/perdidos saem do estoque utilizável.
- Alteração de período/itens somente antes de retirada e sem pagamento; revalidar agenda, capacidade e estoque na mesma transação.
- Kits (RF07) permanecem opcionais e fora da entrega operacional. Não afirmar implementação desse módulo.
- Banco e contas de uso preservados; testes em banco temporário separado. Não publicar senhas nem arquivos de banco.
- Nenhum envio à plataforma ou push solicitado nesta etapa.

## Fechamento

VS2015 Community Update 3 instalado e solução reconstruída pela própria IDE, inclusive após extração do ZIP: três projetos aprovados e zero falhas. Entrega em ../../entrega/aiRbPG-Entrega.zip, com 83 arquivos e hashes conferidos. Documento final de 14 páginas renderizado e revisado. Envio e demonstração a cargo do grupo; sem push.
