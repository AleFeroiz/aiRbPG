# Plano e estado da entrega

## Concluído na aplicação

C# 6, WinForms e .NET Framework 4.6.1 preservados. Cadastros, autenticação/perfis, reservas, disponibilidade, alteração/confirmação/cancelamento, pagamentos manuais, retirada/devolução, danos/perdas, descontos, histórico, usuários e relatórios implementados. Banco de uso atualizado com a migração de histórico, preservando conta e dados.

As regras demonstrativas e os limites são os requisitos consolidados em requisitos.md e especificacao.json. Kits e associações de catálogo continuam opcionais, fora da interface. Não confundir o planejamento inicial com funcionalidades entregues.

## Evidências

51 verificações de integração aprovadas em Release, compiladas com MSBuild 14.0. Cadastro de cliente e reserva, editor, confirmação e relatórios verificados pela interface Windows em banco isolado. Quantidade de acessórios preservada após atualização. Veja PROGRESSO.md.

## Documentação e pacote

Documento DOCX/PDF em documentacao, com requisitos, dois diagramas UML, regras, dados, instalação, manual, verificações e conferência do enunciado. Pacote em ../../entrega. Orientações originais em ../../referencias. Não enviar bancos, senhas, instaladores ou pastas de ferramentas.

## Validação específica da IDE

Ferramentas 2015 instaladas e usadas. IDE Visual Studio Community 2015 Update 3 instalada por mídia oficial; solução original e cópia extraída do ZIP reconstruídas pela própria IDE, com três projetos aprovados e zero falhas em ambas. Designer e F5 continuam sem homologação. Evidências no progresso.

## Estrutura

src/aiRbPG.WinForms: formulários; src/aiRbPG.Domain: regras e modelos; src/aiRbPG.Data: SQL parametrizado e transações; database/scripts: migrações; tests: verificações; scripts: configuração, inicialização e empacotamento.

## Git

Commits locais pequenos, conforme AGENTS.md. Não houve autorização de push nesta finalização. Arquivos de banco, senhas e resultados de build ficam fora do Git.
