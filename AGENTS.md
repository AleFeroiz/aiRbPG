# Instruções do projeto aiRbPG

## Git

- Após cada implementação concluída, registre as alterações relacionadas em um commit Git local.
- Use uma mensagem curta e descritiva, seguindo o padrão `tipo: descrição` (por exemplo, `feat: adiciona fluxo de reservas`).
- Antes do commit, confira `git status` e revise o diff para não incluir arquivos locais, credenciais ou alterações sem relação com a implementação.

## Contexto e verificações

- Leia README.md, docs/PROGRESSO.md e docs/plano-de-implementacao.md antes de continuar trabalho substancial.
- Preserve compatibilidade com VS2015 Update 2 ou posterior, C# 6 e .NET Framework 4.6.1.
- Atualize docs/PROGRESSO.md com decisões e evidências reais; diferencie compilação pelas ferramentas 2015 de teste dentro da IDE 2015.
- Credenciais, bancos e resultados locais de build não devem ser versionados. Commits locais seguem a regra acima; esta configuração de ambiente não autoriza push.
