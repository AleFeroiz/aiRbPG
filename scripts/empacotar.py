"""Monta o pacote acadêmico somente com fontes, SQL, documentação e binários Release.
Não copia .git, .vs, bancos, segredos, instaladores ou dados da máquina.
"""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import hashlib, shutil, json

root=Path(__file__).resolve().parents[1]
delivery=root.parent/'entrega'
folder='aiRbPG - ALESSANDRO SILVA VIEIRA - FELIPE GABRIEL LEITE MOREIRA - MAYZON REZENDE DO NASCIMENTO'
target=delivery/folder
if target.exists():
    raise SystemExit('Pasta de entrega já existe; preserve-a antes de gerar outra versão.')
target.mkdir(parents=True)
files=[]
def copy(source,dest):
    source=Path(source);dest=target/dest
    if not source.is_file():raise RuntimeError('Arquivo ausente: '+str(source))
    if len(str(dest))>=250:raise RuntimeError('Caminho longo para VS2015: '+str(dest))
    dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,dest);files.append(dest)
def write(relative,text):
    dest=target/relative;dest.parent.mkdir(parents=True,exist_ok=True);dest.write_text(text,encoding='utf-8');files.append(dest)

for name in ['src','database','scripts','tests']:
    for path in sorted((root/name).rglob('*')):
        relative=path.relative_to(root)
        if not path.is_file() or any(p in {'bin','obj','.vs','__pycache__'} for p in relative.parts):continue
        if path.suffix.lower() in {'.mdf','.ldf','.bak','.log','.tmp','.local.json'}:continue
        copy(path,Path('CodigoFonte')/relative)
for path in ['README.md','AGENTS.md','.gitignore','aiRbPG.sln','INICIAR.cmd','integrantes.txt','docs/requisitos.md','docs/especificacao.json']:
    copy(root/path,Path('CodigoFonte')/path)
for name in ['uml','imagens']:
    for path in (root/'docs'/name).glob('*'):
        if path.is_file():copy(path,Path('CodigoFonte')/path.relative_to(root))
for path in (root/'docs/documentacao').glob('aiRbPG-Documentacao.*'):
    copy(path,Path('Documentacao')/path.name)
for name in ['casos-de-uso.png','classes.png','casos-de-uso.puml','classes.puml']:
    copy(root/'docs/uml'/name,Path('Documentacao/Diagramas')/name)
for name in ['aiRbPG.WinForms.exe','aiRbPG.Domain.dll','aiRbPG.Data.dll']:
    copy(root/'src/aiRbPG.WinForms/bin/Release'/name,Path('Aplicativo')/name)
copy(root/'integrantes.txt','integrantes.txt')
copy(root.parent/'referencias/Trabalho 2 Sem 2026 - Mat - LPVIII.pdf','Referencias/Enunciado-original.pdf')
copy(root.parent/'referencias/instrucoes-plataforma.png','Referencias/instrucoes-plataforma.png')
copy(root.parent/'.revisao/testes/testes-debug.txt','Verificacoes/testes-debug.txt')
write('INICIAR.cmd','@echo off\npowershell.exe -NoProfile -File "%~dp0CodigoFonte\\scripts\\iniciar.ps1"\nif errorlevel 1 pause\n')
write('CONFIGURAR-BANCO.cmd','@echo off\npowershell.exe -NoProfile -File "%~dp0CodigoFonte\\scripts\\configurar-banco.ps1"\npause\n')
write('ABRIR-PROJETO.cmd','@echo off\nif exist "%ProgramFiles(x86)%\\Microsoft Visual Studio 14.0\\Common7\\IDE\\devenv.exe" (\n start "" "%ProgramFiles(x86)%\\Microsoft Visual Studio 14.0\\Common7\\IDE\\devenv.exe" "%~dp0CodigoFonte\\aiRbPG.sln"\n) else (\n start "" "%~dp0CodigoFonte\\aiRbPG.sln"\n)\n')
write('LEIA-ME.txt', '''aiRbPG - Linguagem de Programação VIII

1. Extraia o ZIP antes de executar. Use uma pasta curta, por exemplo C:\\Projetos.
2. Configure uma instância Microsoft SQL Server. O exemplo usa localhost\\SQLEXPRESS e autenticação Windows.
3. Execute CONFIGURAR-BANCO.cmd. Para outra instância, consulte Documentacao/aiRbPG-Documentacao.pdf.
4. Abra INICIAR.cmd. A aplicação compilada está em Aplicativo e não exige Visual Studio para executar.
5. Em banco novo, crie o primeiro administrador. Não existe senha padrão. O banco de uso não acompanha este pacote.
6. Para código e depuração, use ABRIR-PROJETO.cmd, VS2015 Update 2+ e Developer Pack 4.6.1. Defina aiRbPG.WinForms como inicialização.

Cadastros abrem no modo Novo: preencha e clique em Salvar. Para editar, selecione uma linha. Para cadastrar outro registro, clique Novo.
O sistema implementa atendimento, reservas, pagamentos manuais, retirada/devolução e relatórios. Kits são opcionais e não possuem interface.

Pasta CodigoFonte: código completo, migrações SQL, requisitos, fontes UML e testes.
Pasta Documentacao: documento em DOCX/PDF e dois diagramas UML.
Pasta Verificacoes: resultado da suíte final Debug (51 verificações). Release também foi validado.
Pasta Referencias: enunciado e captura da plataforma.

O pacote não inclui senhas, contas do banco local, arquivos MDF/LDF, ferramentas ou instaladores.
Todos os integrantes devem participar da demonstração; somente um faz o envio e confere os arquivos na plataforma.
''')
write('CONFERENCIA-ENTREGA.md','''# Conferência de entrega

| Solicitação do professor | Arquivo ou evidência |
|---|---|
| Aplicação C# | CodigoFonte/aiRbPG.sln e src |
| Persistência SQL Server | CodigoFonte/database/scripts, configurar-banco.ps1 e testes |
| Requisitos funcionais e não funcionais | Documentacao/aiRbPG-Documentacao.pdf, seções 2 e 3 |
| Dois diagramas UML | Documentacao/Diagramas e seções 5 e 6 do documento |
| Documentação junto do projeto | DOCX e PDF em Documentacao |
| Pasta com nomes dos integrantes | Nome da pasta principal e integrantes.txt |
| Demonstração breve | Seção 13 do documento |

51 verificações de integração aprovadas. Cadastro de cliente e reserva, edição, confirmação e consultas administrativas verificados no Windows com SQL Server em banco isolado. Banco de uso atualizado sem apagar a conta existente.

O VS2015 é o alvo de compatibilidade; os testes usam MSBuild 14.0 e C# 6. O pacote não afirma validação do Designer da IDE. Kits, estorno e pagamentos online não fazem parte da versão operacional.

Prazo informado na plataforma: 24/11/2026 às 23h59. Envio e apresentação são feitos pelo grupo.
''')
manifest=[]
for path in sorted(files):manifest.append(hashlib.sha256(path.read_bytes()).hexdigest()+'  '+path.relative_to(target).as_posix())
(target/'manifesto-sha256.txt').write_text('\n'.join(manifest)+'\n',encoding='utf-8')
archive=delivery/'aiRbPG-Entrega.zip'
with ZipFile(archive,'w',ZIP_DEFLATED,compresslevel=6) as z:
    for path in sorted(target.rglob('*')):
        if path.is_file():z.write(path,path.relative_to(delivery).as_posix())
with ZipFile(archive) as z:
    assert z.testzip() is None
    assert all(not any(part in {'.git','.vs','obj'} for part in Path(name).parts) for name in z.namelist())
    assert not any(Path(name).suffix.lower() in {'.mdf','.ldf','.bak','.iso'} for name in z.namelist())
summary={'pasta':str(target),'zip':str(archive),'arquivos':len(manifest)+1,'bytes_zip':archive.stat().st_size,'maior_caminho':max(len(str(p)) for p in target.rglob('*'))}
(delivery/'verificacao-pacote.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(summary,ensure_ascii=True))
