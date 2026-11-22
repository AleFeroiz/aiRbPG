"""Gera documentação editável e diagramas; execute com o Python do runtime Codex.
O PDF é exportado do DOCX e revisado separadamente. Não altera fontes C# nem banco.
"""
from pathlib import Path
import json, math
from PIL import Image, ImageDraw, ImageFont
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'docs'/'documentacao'
UML=ROOT/'docs'/'uml'
OUT.mkdir(parents=True,exist_ok=True)
UML.mkdir(exist_ok=True)
AUTHORS=['Alessandro Silva Vieira','Felipe Gabriel Leite Moreira','Mayzon Rezende do Nascimento']
RF=[
('RF01','Autenticar usuário ativo por login e senha.','Implementado'),
('RF02','Restringir operações pelos perfis Administrador e Atendente.','Implementado'),
('RF03','Cadastrar, consultar, alterar e inativar clientes.','Implementado'),
('RF04','Manter espaços, capacidade, endereço e tarifas.','Implementado'),
('RF05','Manter o catálogo de sistemas de RPG.','Implementado'),
('RF06','Manter acessórios, quantidade em estoque e tarifa.','Implementado'),
('RF07','Montar kits temáticos compostos por acessórios.','Opcional futuro'),
('RF08','Consultar espaços livres e acessórios disponíveis por período.','Implementado'),
('RF09','Criar reserva com cliente, espaço, período e participantes.','Implementado'),
('RF10','Alterar reserva antes da retirada e sem pagamento, revalidando disponibilidade.','Implementado'),
('RF11','Cancelar reserva pendente ou confirmada, sem pagamento, preservando o histórico.','Implementado'),
('RF12','Adicionar acessórios e suas quantidades à reserva.','Implementado'),
('RF13','Calcular valor por horas, pessoas e acessórios, acrescido de multas e menos descontos.','Implementado'),
('RF14','Registrar retirada integral dos itens de reserva confirmada.','Implementado'),
('RF15','Registrar devoluções parciais, danos, perdas e multa manual justificada.','Implementado'),
('RF16','Registrar pagamentos manuais parciais, sem ultrapassar o saldo.','Implementado'),
('RF17','Controlar os estados até cancelamento ou finalização.','Implementado'),
('RF18','Consultar reservas por nome do cliente ou espaço e por estado.','Implementado'),
('RF19','Consultar estoque comprometido e disponível no intervalo escolhido.','Implementado'),
('RF20','Consultar recebimentos por data e método, somente como Administrador.','Implementado'),
('RF21','Consultar acessórios mais reservados em um período.','Implementado'),
('RF22','Cadastrar usuários e ativar ou inativar contas, como Administrador.','Implementado'),
('RF23','Consultar histórico de ações e operador responsável em cada reserva.','Implementado'),
]
RNF=[
('RNF01','Executar em Windows com interface C# Windows Forms.'),
('RNF02','Persistir os dados em Microsoft SQL Server com acesso ADO.NET.'),
('RNF03','Armazenar senha com PBKDF2 HMAC SHA256, salt aleatório e 210 mil iterações.'),
('RNF04','Usar parâmetros SQL nas entradas de dados.'),
('RNF05','Validar campos obrigatórios, limites, capacidade, período e quantidades.'),
('RNF06','Usar transações nas operações de reserva, pagamento e devolução.'),
('RNF07','Inativar cadastros e preservar referências e histórico.'),
('RNF08','Separar domínio, dados e interface em projetos da solução.'),
('RNF09','Buscar resposta de até três segundos nas operações usuais da demonstração. Meta dependente do ambiente; não homologada para grande carga.'),
('RNF10','Fornecer scripts de criação, migração e carga demonstrativa.'),
('RNF11','Manter formato clássico de projeto, C# 6 e .NET Framework 4.6.1 para VS2015 Update 2 ou posterior.'),
('RNF12','Fornecer configuração reproduzível da conexão e pacote sem senhas ou arquivos de banco.'),
]
RULES=[
('Agenda','Um espaço não admite reservas abertas com períodos sobrepostos. Intervalos adjacentes são permitidos. Uma locação ainda em andamento pode continuar bloqueando a agenda após o término previsto.'),
('Estoque','A soma dos acessórios comprometidos no período não pode exceder o estoque utilizável. Devoluções liberam itens; danos e perdas reduzem o estoque. A redução administrativa é conservadora e não pode ficar abaixo do total comprometido em reservas abertas.'),
('Capacidade','O período deve ter término posterior ao início, pelo menos um participante e participantes dentro da capacidade do espaço.'),
('Preço','O total soma espaço por hora, participantes por hora quando houver tarifa, acessórios por unidade e hora, multas e descontos. Os subtotais são arredondados para centavos; as tarifas aplicadas ficam gravadas na reserva.'),
('Permissões','Somente Administrador altera catálogos de espaços, acessórios e sistemas, tarifas, descontos, usuários e relatórios financeiros. Atendente mantém clientes e opera as reservas.'),
('Alteração e cancelamento','Só são permitidos antes da retirada, em Pendente ou Confirmada e sem pagamento. A alteração reaplica tarifas atuais e zera o desconto anterior. O cancelamento não apaga dados. Não há estorno no aplicativo.'),
('Pagamento','O valor deve ser positivo, ter até duas casas decimais e não exceder o saldo. Dinheiro, Pix, Cartão e Transferência são registros manuais; não movimentam contas bancárias.'),
('Movimentação','A retirada é integral e exige confirmação. A devolução pode ser parcial e separa boas, danificadas e perdidas. Quantidades não podem exceder a retirada em aberto. Multa é informada e justificada pelo operador.'),
('Encerramento','A finalização exige resolução de todos os itens retirados e saldo financeiro zero. Reservas canceladas ou finalizadas não recebem novos pagamentos.'),
]

def font(size=20,bold=False):
    return ImageFont.truetype(str(Path('C:/Windows/Fonts')/('arialbd.ttf' if bold else 'arial.ttf')),size)
S=2
def canvas(w,h):
    return Image.new('RGB',(w*S,h*S),'white')
def draw_text(d,xy,text,size=20,bold=False,anchor=None):
    d.multiline_text((xy[0]*S,xy[1]*S),text,font=font(size*S,bold),fill='#182431',anchor=anchor,align='center',spacing=5*S)
def line(d,points,width=2): d.line([(x*S,y*S) for x,y in points],fill='#354656',width=width*S)
def rect(d,box,fill='white'):d.rectangle(tuple(v*S for v in box),fill=fill,outline='#354656',width=2*S)

def use_case_diagram():
    image=canvas(1000,930); d=ImageDraw.Draw(image)
    rect(d,(170,105,830,865)); draw_text(d,(500,128),'aiRbPG',23,True,'mm')
    def actor(x,y,label):
        d.ellipse(((x-15)*S,(y-65)*S,(x+15)*S,(y-35)*S),outline='#182431',width=2*S)
        line(d,[(x,y-35),(x,y+15)]);line(d,[(x-25,y-18),(x+25,y-18)])
        line(d,[(x,y+15),(x-22,y+45)]);line(d,[(x,y+15),(x+22,y+45)])
        draw_text(d,(x,y+70),label,19,True,'mm')
    actor(65,450,'Atendente');actor(935,450,'Administrador')
    # Generalization: Administrator inherits the Attendant's operations.
    line(d,[(935,385),(935,55),(65,55),(65,365)])
    d.polygon([(53*S,365*S),(77*S,365*S),(65*S,388*S)],fill='white',outline='#354656',width=2*S)
    draw_text(d,(500,34),'herda as operações',18,False,'mm')
    common=['Fazer login','Manter clientes','Consultar disponibilidade','Criar alterar e cancelar\nreservas','Consultar reservas','Registrar pagamentos','Registrar retirada\ne devolução','Confirmar e finalizar']
    for i,label in enumerate(common):
        y=200+i*82
        line(d,[(92,430),(240,y)])
        d.ellipse((240*S,(y-30)*S,505*S,(y+30)*S),fill='#F4F7FA',outline='#354656',width=2*S)
        draw_text(d,(372,y),label,17,False,'mm')
    admin=['Manter catálogos\ne tarifas','Gerenciar usuários','Consultar relatórios\ne conceder desconto']
    for i,label in enumerate(admin):
        y=270+i*200
        line(d,[(908,430),(790,y)])
        d.ellipse((535*S,(y-45)*S,790*S,(y+45)*S),fill='#F4F7FA',outline='#354656',width=2*S)
        draw_text(d,(662,y),label,18,False,'mm')
    draw_text(d,(500,902),'O cliente é cadastrado pelo funcionário e não acessa o aplicativo.',18,False,'mm')
    image.save(UML/'casos-de-uso.png')

def class_diagram():
    image=canvas(1000,970); d=ImageDraw.Draw(image)
    boxes={
        'Cliente':(30,60,280,220,['+ id: int','+ nome: string','+ contato: string','+ ativo: bool']),
        'Usuario':(375,60,625,220,['+ id: int','+ login: string','+ perfil: PerfilUsuario','+ ativo: bool']),
        'Espaco':(720,60,970,220,['+ id: int','+ capacidade: int','+ tarifaHora: decimal','+ tarifaPessoa: decimal']),
        'Reserva':(330,360,670,570,['+ id: int','+ inicio e fim: DateTime','+ pessoas: int','+ status: StatusReserva','+ total: decimal','+ multas e desconto: decimal']),
        'HistoricoReserva':(735,375,985,535,['+ id: int','+ dataHora: DateTime','+ acao: string','+ observacao: string']),
        'Acessorio':(30,720,280,880,['+ id: int','+ nome: string','+ estoque: int','+ tarifaHora: decimal']),
        'ItemReserva':(375,720,625,910,['+ id: int','+ reservados: int','+ retirados: int','+ devolvidos: int','+ danos e perdas: int']),
        'Pagamento':(720,720,970,880,['+ id: int','+ dataHora: DateTime','+ valor: decimal','+ metodo: string']),
    }
    # Associations and multiplicities, routed outside the class boxes.
    for pts,a,b,pa,pb in [
        ([(155,220),(155,300),(350,300),(350,360)],'1','0..*',(172,238),(360,335)),
        ([(500,220),(500,360)],'1','0..*',(515,240),(515,335)),
        ([(845,220),(845,300),(650,300),(650,360)],'1','0..*',(858,240),(602,335)),
        ([(500,570),(500,720)],'1','0..*',(515,590),(515,689)),
        ([(670,520),(700,520),(700,640),(845,640),(845,720)],'1','0..*',(678,540),(857,690)),
        ([(280,800),(375,800)],'1','0..*',(294,773),(328,773)),
        ([(670,430),(735,430)],'1','0..*',(680,400),(700,453)),
        ([(625,145),(690,145),(690,260),(970,260),(970,375)],'1','0..*',(640,112),(919,345)),
    ]:
        line(d,pts);draw_text(d,pa,a,17);draw_text(d,pb,b,17)
    # Filled diamond means composition of a reservation's items/payments/history.
    for x,y,vertical in [(500,570,True),(670,520,False),(670,430,False)]:
        pts=[(x,y),(x-7,y+12),(x,y+24),(x+7,y+12)] if vertical else [(x,y),(x+12,y-7),(x+24,y),(x+12,y+7)]
        d.polygon([(a*S,b*S) for a,b in pts],fill='#354656')
    for name,(x1,y1,x2,y2,attrs) in boxes.items():
        rect(d,(x1,y1,x2,y2));rect(d,(x1,y1,x2,y1+40),'#EAF0F5')
        draw_text(d,((x1+x2)/2,y1+20),name,20,True,'mm')
        for i,attr in enumerate(attrs):draw_text(d,(x1+12,y1+51+i*25),attr,17)
    draw_text(d,(500,946),'Modelo lógico de classes persistentes; detalhes auxiliares foram omitidos.',18,False,'mm')
    image.save(UML/'classes.png')

use_case_diagram();class_diagram()

document=Document()
section=document.sections[0]
section.page_width=Inches(8.5);section.page_height=Inches(11)
section.top_margin=section.bottom_margin=Inches(.72)
section.left_margin=section.right_margin=Inches(.85)
normal=document.styles['Normal'];normal.font.name='Calibri';normal.font.size=Pt(11)
normal.paragraph_format.space_after=Pt(7);normal.paragraph_format.line_spacing=1.12
for name,size in [('Title',30),('Subtitle',16),('Heading 1',18),('Heading 2',13)]:
    style=document.styles[name];style.font.name='Calibri';style.font.size=Pt(size);style.font.color.rgb=RGBColor(0,0,0)
    style.paragraph_format.space_before=Pt(10);style.paragraph_format.space_after=Pt(9)
footer=section.footer.paragraphs[0];footer.alignment=WD_ALIGN_PARAGRAPH.RIGHT
field=OxmlElement('w:fldSimple');field.set(qn('w:instr'),'PAGE');footer._p.append(field)
document.core_properties.title='aiRbPG Documentação do sistema'
document.core_properties.author='; '.join(AUTHORS)
document.core_properties.subject='Linguagem de Programação VIII - C# e SQL Server'

def p(text,style=None):return document.add_paragraph(text,style)
def h(text):document.add_heading(text,1)
def page(title):document.add_page_break();h(title)
def table(headers,rows,widths):
    t=document.add_table(rows=1,cols=len(headers));t.alignment=WD_TABLE_ALIGNMENT.CENTER;t.autofit=False
    for i,width in enumerate(widths):t.columns[i].width=Inches(width)
    for c,text in zip(t.rows[0].cells,headers):c.text=text
    repeat=OxmlElement('w:tblHeader');t.rows[0]._tr.get_or_add_trPr().append(repeat)
    for row in rows:
        cells=t.add_row().cells
        for i,text in enumerate(row):cells[i].text=str(text)
    for ri,row in enumerate(t.rows):
        for i,cell in enumerate(row.cells):
            cell.width=Inches(widths[i]);cell.vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.CENTER
            props=cell._tc.get_or_add_tcPr();border=OxmlElement('w:tcBorders')
            for edge in ['top','left','bottom','right']:
                e=OxmlElement('w:'+edge);e.set(qn('w:val'),'single');e.set(qn('w:sz'),'4');e.set(qn('w:color'),'D9D9D9');border.append(e)
            props.append(border)
            shade=OxmlElement('w:shd');shade.set(qn('w:fill'),'20364B' if ri==0 else ('F2F5F8' if ri%2==0 else 'FFFFFF'));props.append(shade)
            margins=OxmlElement('w:tcMar')
            for edge in ['top','left','bottom','right']:
                e=OxmlElement('w:'+edge);e.set(qn('w:w'),'85');e.set(qn('w:type'),'dxa');margins.append(e)
            props.append(margins)
            for paragraph in cell.paragraphs:
                paragraph.paragraph_format.space_after=Pt(3);paragraph.paragraph_format.space_before=Pt(3);paragraph.paragraph_format.line_spacing=1.03
                if i==0 or (len(headers)==3 and i==2):paragraph.alignment=WD_ALIGN_PARAGRAPH.CENTER
                for run in paragraph.runs:
                    run.font.size=Pt(10.5);run.bold=ri==0
                    if ri==0:run.font.color.rgb=RGBColor(255,255,255)
    p('')
    return t
def picture(path,width=6.6):
    para=document.add_paragraph();para.alignment=WD_ALIGN_PARAGRAPH.CENTER;para.add_run().add_picture(str(path),width=Inches(width))
def bullets(values):
    for text in values:p(text,'List Bullet')

p('aiRbPG','Title');p('Sistema de reservas de espaços e acessórios para RPG','Subtitle')
p('Documentação do sistema e manual de demonstração')
p('Linguagem de Programação VIII\nFatec Itapetininga\nSegundo semestre de 2026')
p('Integrantes','Heading 2')
for author in AUTHORS:p(author)
p('1 Apresentação','Heading 1')
p('O aiRbPG é um aplicativo desktop para organizar a locação de espaços destinados a sessões de RPG de mesa e reservar acessórios no mesmo atendimento. Funcionários cadastram clientes, consultam a disponibilidade e acompanham a reserva até sua conclusão.')
p('A entrega contém aplicação C# Windows Forms, persistência Microsoft SQL Server, código-fonte, scripts de banco, requisitos funcionais e não funcionais e dois diagramas UML. O Administrador gerencia os cadastros e a equipe; o Atendente executa o atendimento. O cliente não entra no sistema.')
p('O documento apresenta as regras adotadas na demonstração, a estrutura de dados, a instalação, o uso e as verificações realizadas. Kits temáticos são opcionais e não integram os fluxos operacionais desta versão.')
p('Versão de entrega preparada em 22 de novembro de 2026. Prazo indicado na plataforma: 24 de novembro de 2026, às 23h59.')

page('2 Requisitos funcionais')
p('Os requisitos abaixo descrevem o comportamento entregue. As restrições de alteração, cancelamento e encerramento constam também nas regras de negócio. O único requisito opcional listado fora da implementação é RF07.')
table(['ID','Comportamento','Situação'],RF[:12],[.65,4.9,1.25])
page('2 Requisitos funcionais continuação')
table(['ID','Comportamento','Situação'],RF[12:],[.65,4.9,1.25])
p('O relatório de recebimentos considera pagamentos efetivamente registrados no período. A consulta de acessórios considera o início das reservas no período e exclui cancelamentos. Não representa integração bancária ou emissão fiscal.')

page('3 Requisitos não funcionais')
p('Os requisitos consolidam as decisões técnicas da entrega. A meta de desempenho é um objetivo para o ambiente de demonstração; a suíte funcional não equivale a ensaio de carga.')
table(['ID','Requisito'],RNF,[.85,5.95])

page('4 Regras de negócio e arquitetura')
for title,text in RULES:
    para=p('');r=para.add_run(title+'. ');r.bold=True;para.add_run(text)
p('Arquitetura','Heading 2')
p('aiRbPG.WinForms contém os formulários. aiRbPG.Domain reúne modelos, autenticação e cálculo de preço. aiRbPG.Data executa consultas parametrizadas e transações SQL Server. A sessão identifica o operador; a camada de dados verifica permissões nas operações administrativas.')

page('5 Diagrama UML de casos de uso')
p('O Administrador especializa o Atendente e herda suas operações. Os casos agrupam os fluxos do atendimento; manter catálogos abrange espaços, sistemas de RPG e acessórios.')
picture(UML/'casos-de-uso.png',6.7)
p('Figura 1 - Casos de uso da versão operacional. Fontes editáveis e imagem em docs/uml.')

page('6 Diagrama UML de classes')
p('O modelo lógico destaca as entidades persistidas e suas multiplicidades. Uma reserva pertence a um cliente e espaço, registra o operador e reúne itens, pagamentos e histórico. Um acessório pode participar de vários itens de reserva.')
picture(UML/'classes.png',6.7)
p('Figura 2 - Modelo lógico das classes persistentes centrais. O diagrama é conceitual e não reproduz todos os métodos e DTOs do código C#.')

page('7 Banco de dados e integridade')
p('A aplicação usa Microsoft SQL Server. O ambiente local foi preparado com Express 2022, instância localhost\\SQLEXPRESS e autenticação integrada do Windows. A versão do servidor é independente da versão do Visual Studio usada para compilar o cliente.')
table(['Tabela ou grupo','Finalidade'],[
('Usuarios','Login, perfil, situação da conta, salt e hash da senha.'),
('Clientes','Identificação e contato dos clientes atendidos.'),
('Espacos','Capacidade, endereço e tarifas configuráveis.'),
('SistemasRpg e Acessorios','Catálogos e estoque utilizável de acessórios.'),
('Reservas','Período, participantes, estado e valores aplicados.'),
('ItensReserva','Quantidade reservada, retirada, devolvida, danificada e perdida.'),
('Pagamentos','Valor, método, data e operador do registro manual.'),
('HistoricoReservas','Criação, alteração e movimentações auditadas por operador.'),
('SistemasAcessorios, Kits e ItensKit','Estruturas previstas no SQL original, sem interface operacional de associação ou kits nesta entrega.'),
],[2.05,4.75])
p('A primeira migração cria a estrutura original e dados demonstrativos. A segunda adiciona o histórico. O script configurar-banco.ps1 aplica os arquivos em ordem e pode ser executado novamente sem duplicar a carga demonstrativa. Não apaga contas existentes.')
p('Chaves estrangeiras mantêm os vínculos. Operações de locação, pagamento e devolução usam transações. Cadastros são inativados para preservar o histórico. A conexão não é gravada no código-fonte nem no pacote com senha.')

page('8 Instalação e execução')
bullets([
'Use Windows e Visual Studio 2015 Update 2 ou posterior, com .NET Framework 4.6.1 Developer Pack. Os projetos usam C# 6 e formato clássico. Não aceite redirecionamento automático para outra versão do Framework se precisar manter compatibilidade com a faculdade.',
'Instale ou disponibilize uma instância Microsoft SQL Server e conceda ao usuário Windows acesso para criar e usar o banco. O exemplo local utiliza SQL Server Express com autenticação Windows.',
'Extraia o pacote e abra um PowerShell na pasta do código-fonte. Execute: powershell -NoProfile -File scripts\\configurar-banco.ps1. Para outra instância, acrescente -Servidor "SERVIDOR\\INSTANCIA".',
'O script cria/atualiza aiRbPG e salva AIRBPG_SQL_CONNECTION para o usuário Windows atual. Feche e reabra o Visual Studio após configurar a variável.',
'Abra aiRbPG.sln, selecione Release e Any CPU, defina aiRbPG.WinForms como projeto de inicialização e compile. Para depuração, use Debug e F5.',
'Após compilar Release, abra INICIAR.cmd. O iniciador confere a conexão e a migração de histórico antes de executar a aplicação.',
'Em banco novo, crie o primeiro Administrador com nome, login e senha de pelo menos 10 caracteres. Em banco já usado, entre com a conta cadastrada. Não há senha padrão no pacote.',
'Cadastre os demais operadores em Usuários e relatórios. Para repetir verificações automatizadas, execute powershell -NoProfile -File scripts\\testar.ps1 -Configuracao Release; isso cria e remove um banco temporário separado.',
])
p('Diagnóstico','Heading 2')
p('Se a conexão falhar, confirme o serviço SQL Server, o nome da instância e as permissões Windows. Se o iniciador indicar banco desatualizado, execute configurar-banco.ps1. Não execute a cópia antiga arquivada. O pacote não instala o servidor SQL automaticamente e não contém arquivos MDF/LDF.')

page('9 Manual dos cadastros')
p('Entre em Clientes, Espaços, Acessórios ou Sistemas de RPG pelo menu. A tela abre em Novo cadastro. Preencha os campos e clique em Salvar; o registro aparece na lista. Para alterar, selecione uma linha e edite. Para cadastrar outro, clique em Novo.')
picture(ROOT/'docs/imagens/clientes.png',6.5)
p('Figura 3 - Cliente criado pelo botão Salvar e exibido pela consulta ao SQL Server, em banco isolado de teste.')
p('Nome é obrigatório. Capacidade deve ser positiva; estoque e tarifas não podem ser negativos. A tarifa por pessoa pode ser zero. Inativar oculta o registro nas novas operações sem apagar os vínculos anteriores. Espaços, acessórios e sistemas exigem perfil Administrador; clientes podem ser mantidos pelo Atendente.')

page('10 Manual das reservas')
picture(ROOT/'docs/imagens/reservas.png',6.7)
p('Figura 4 - Lista, filtros, período, participantes e acessórios no formulário de reservas.')
bullets([
'Selecione cliente e espaço ativos, início, término e participantes. Use Espaços livres para filtrar o catálogo pelo intervalo. Ao mudar o intervalo, consulte a disponibilidade novamente.',
'Na coluna Qtd., informe inteiros entre zero e o disponível para cada acessório. A estimativa acompanha as tarifas. Salvar reserva valida novamente tudo no banco; a consulta prévia não garante a vaga até a gravação.',
'Use a busca por nome do cliente ou espaço e o filtro de estado para localizar uma reserva. Carregar para editar recupera os dados; a alteração é limitada a reservas pendentes/confirmadas sem pagamentos.',
'Operar selecionada abre o ciclo de confirmação, pagamento e movimentação. Nova / limpar sai da edição e limpa a seleção de acessórios.',
])

page('11 Pagamentos e devoluções')
picture(ROOT/'docs/imagens/operacoes.png',6.5)
p('Figura 5 - Itens, saldo, pagamentos e histórico de uma reserva em banco de demonstração.')
bullets([
'Confirme a reserva. Registrar pagamento permite pagamentos parciais manuais, limitados ao saldo. Não é cobrança automática de cartão ou Pix.',
'Retirada integral registra todas as unidades reservadas. Na devolução, preencha Boas, Danos e Perdas com as quantidades desta operação; depois clique em Registrar devolução e informe eventual multa e justificativa.',
'Finalizar / quitar verifica todos os itens e o saldo; o botão não realiza um pagamento. Registre o saldo em Registrar pagamento antes de finalizar.',
'Cancelamento exige motivo, estado anterior à retirada e ausência de pagamento. Desconto é exclusivo do Administrador. O histórico identifica as ações e o operador.',
])

page('12 Verificações e rastreabilidade da entrega')
p('A suíte executável usa um banco temporário aiRbPG_Test_ com nome único, aplica os SQL e o remove ao terminar. A execução Release com ferramentas MSBuild 14.0 concluiu 51 verificações funcionais sem falhas. Não depende de pacotes de teste externos.')
table(['Grupo','Evidência'],[
('Autenticação e perfis','Primeiro administrador, senha curta/incorreta, usuário inativo e restrições administrativas.'),
('Cadastros','Inclusão/edição de cliente; catálogo de sistema alterado/inativado; bloqueio de estoque comprometido.'),
('Reserva','Preço, sobreposição, adjacência, capacidade, edição, duplicação de itens e duas criações simultâneas.'),
('Financeiro','Desconto, pagamento parcial, sobrepagamento, bloqueios de edição e cancelamento com pagamento.'),
('Movimentação','Retirada, devolução excessiva/duplicada, dano no estoque e finalização com pendências.'),
('Histórico e relatórios','Registro de ações, total de recebimentos, acessórios mais reservados e preservação após inativação.'),
('Interface Windows','Cliente e reserva criados por formulário; editor, confirmação, usuários e relatórios observados; quantidade preservada ao atualizar.'),
],[1.65,5.15])
p('A compilação foi verificada com as ferramentas reais de 2015 e .NET Framework 4.6.1. O teste do Designer e F5 dentro da IDE completa deve ser distinguido da compilação por MSBuild. As evidências e o estado da instalação da IDE ficam em PROGRESSO.md.')
p('Limites da versão','Heading 2')
p('Kits e associações entre acessórios e sistemas não possuem interface. Não há estorno, nota fiscal, pagamento online, multa automática por atraso, recuperação de senha ou cadastro público de clientes. Não houve ensaio de carga nem homologação do ambiente do laboratório. A demonstração usa dados fictícios.')

page('13 Conferência e roteiro de demonstração')
table(['Exigência do professor','Atendimento no pacote'],[
('Aplicação em C#','Solução com três projetos e código-fonte de Windows Forms.'),
('Acesso ao SQL Server','Repositórios ADO.NET, scripts SQL e configuração de conexão.'),
('Requisitos funcionais e não funcionais','Seções 2 e 3 deste documento e docs/requisitos.md.'),
('Dois diagramas UML','Casos de uso e classes nas seções 5 e 6; imagens e fontes em docs/uml.'),
('Documentação junto do código','DOCX e PDF na pasta Documentacao do pacote.'),
('Até três integrantes e identificação','Três nomes na pasta de entrega e no arquivo integrantes.txt.'),
('Demonstração breve','Roteiro abaixo, com participação de todos os integrantes.'),
],[2.25,4.55])
p('Roteiro sugerido','Heading 2')
bullets([
'Apresentar o problema, os perfis e o SQL Server utilizado.',
'Entrar como Administrador e cadastrar um cliente e um espaço.',
'Criar reserva com acessórios, mostrar o cálculo e tentar um conflito.',
'Confirmar, registrar um pagamento parcial e executar a retirada.',
'Registrar a devolução, quitar o saldo e finalizar.',
'Mostrar histórico, relatório por período e restrições do Atendente.',
])
p('Antes do envio, confira a extração do ZIP, a abertura da solução, a configuração do banco e o carregamento dos arquivos na plataforma. Somente um integrante faz o envio; todos devem estar presentes na demonstração. O envio à plataforma é realizado pelo grupo.')
p('Referência da atividade','Heading 2')
p('Trabalho 2 Sem 2026 - Mat - LPVIII.pdf, sete páginas, fornecido pelo professor. Captura da plataforma preservada em referencias/instrucoes-plataforma.png. Prazo informado: 24/11/2026 às 23h59.')

for part in [document._element,document.styles.element]:
    for border in part.xpath('.//w:pBdr'):
        border.getparent().remove(border)
path=OUT/'aiRbPG-Documentacao.docx'
document.save(path)
spec={'integrantes':AUTHORS,'requisitos_funcionais':RF,'requisitos_nao_funcionais':RNF,'regras':RULES}
(ROOT/'docs/especificacao.json').write_text(json.dumps(spec,ensure_ascii=False,indent=2),encoding='utf-8')
md='# Requisitos consolidados do aiRbPG\n\nVersão operacional de entrega. Compatibilidade: C# 6, .NET Framework 4.6.1 e VS2015 Update 2+.\n\n## Perfis\n\nAdministrador mantém catálogos, tarifas, usuários e relatórios e também realiza o atendimento. Atendente mantém clientes e opera reservas. Cliente não acessa o aplicativo.\n\n## Requisitos funcionais\n\n| ID | Comportamento | Situação |\n|---|---|---|\n'
md+=''.join('| '+' | '.join(row)+' |\n' for row in RF)
md+='\n## Requisitos não funcionais\n\n| ID | Requisito |\n|---|---|\n'+''.join('| '+' | '.join(row)+' |\n' for row in RNF)
md+='\n## Regras adotadas para demonstração\n\n'+''.join(f'{i+1}. **{title}:** {text}\n' for i,(title,text) in enumerate(RULES))
md+='\n## Validação e limites\n\n51 verificações de integração aprovadas em Release com MSBuild 14.0. Cadastro de cliente e reserva, confirmação e consulta administrativa verificados na interface em SQL isolado. Consulte PROGRESSO.md para evidências. Kits permanecem opcionais. Não há pagamento online, estorno, recuperação de senha ou multa automática por atraso. A meta de desempenho não equivale a teste de carga.\n'
(ROOT/'docs/requisitos.md').write_text(md,encoding='utf-8')
print(path)
