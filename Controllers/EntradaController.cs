using Analise.Data;
using Analise.Filters;
using Analise.Helper;
using Analise.Models;
using Analise.Repositorio;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Analise.Controllers
{
    public class EntradaController : Controller
    {
        private readonly BancoContext _context;
        private readonly IContasCobrarRepositorio _contasCobrarRepositorio;
        private readonly IContasPagarRepositorio _contasPagarRepositorio;
        private readonly IEntradaRepositorio _entradaRepositorio;
        private readonly ICadastroRepositorio _cadastroRepositorio;
        private readonly IAgenciaRepositorio _agenciaRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly ITipoEntradaRepositorio _tipoEntradaRepositorio;
        private readonly ILinhaRepositorio _linhaRepositorio;
        private readonly IContaRepositorio _contaRepositorio;
        private readonly IWebHostEnvironment _env;
        public EntradaController(IEntradaRepositorio cargoRepositorio,
                                    IContasCobrarRepositorio contasCobrarRepositorio,
                                    IContasPagarRepositorio contasPagarRepositorio,
                                    ICadastroRepositorio cadastroRepositorio,
                                    ITipoEntradaRepositorio tipoEntradaRepositorio,
                                    IUsuarioRepositorio usuarioRepositorio,
                                    ILinhaRepositorio linhaRepositorio,
                                    IAgenciaRepositorio agenciaRepositorio,
                                    IContaRepositorio contaRepositorio,
                                    BancoContext bancoContext,
                                    IWebHostEnvironment env)
        {
            _entradaRepositorio = cargoRepositorio;
            _contasCobrarRepositorio = contasCobrarRepositorio;
            _contasPagarRepositorio = contasPagarRepositorio;
            _cadastroRepositorio = cadastroRepositorio;
            _tipoEntradaRepositorio = tipoEntradaRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _linhaRepositorio = linhaRepositorio;
            _agenciaRepositorio = agenciaRepositorio;
            _contaRepositorio = contaRepositorio;
            this._context = bancoContext;
            _env = env;
        }
        public IActionResult Criar(
            DateTime? data,
            int? agenciaId,
            int? linhaId,
            int? contaId)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            int agenciaSessaoId = HttpContext.Session.GetInt32("AgenciaId") ?? 0;

            var tipoEntradas = _tipoEntradaRepositorio.BuscarTodos()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Nome
                }).ToList();

            var tabernaculos = _agenciaRepositorio.BuscarTodos()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Nome,
                    Selected = agenciaId.HasValue && m.Id == agenciaId
                }).ToList();

            var linhas = _linhaRepositorio.BuscarTodos()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Nome,
                    Selected = linhaId.HasValue && m.Id == linhaId
                }).ToList();

            var contas = _contaRepositorio.BuscarTodos()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Nome,
                    Selected = contaId.HasValue && m.Id == contaId
                }).ToList();

            var cadastros = _cadastroRepositorio.BuscarTodos()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Nome
                }).ToList();

            var entradas = _entradaRepositorio.BuscarTodos().AsQueryable();

            if (data.HasValue)
                entradas = entradas.Where(e => e.DataCadastro.Date == data.Value.Date);

            if (agenciaId.HasValue)
                entradas = entradas.Where(e => e.AgenciaId == agenciaId);

            if (linhaId.HasValue)
                entradas = entradas.Where(e => e.LinhaId == linhaId);

            if (contaId.HasValue)
                entradas = entradas.Where(e => e.ContaId == contaId);

            var pedidoModel = new EntradaModel
            {
                ListaAgencias = tabernaculos,
                ListaTipos = tipoEntradas,
                ListaLinhas = linhas,
                ListaContas = contas,
                ListaCadastros = cadastros
            };

            var viewModel = new EntradaViewModel
            {
                EntradaNome = pedidoModel,
                ListaEntradas = entradas?.ToList() ?? new List<EntradaModel>()
            };

            return View(viewModel);
        }
                
        public IActionResult Editar(int id)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            int agenciaId = HttpContext.Session.GetInt32("AgenciaId") ?? 0;

            var cadastroSelecionado = _entradaRepositorio.ListarPorId(id);

            if (cadastroSelecionado == null)
            {
                TempData["MensagemErro"] = "Não encontrado.";
                return RedirectToAction("Criar");
            }

            var viewModel = new EntradaViewModel
            {
                EntradaNome = cadastroSelecionado,
                ListaEntradas = _entradaRepositorio.BuscarTodos()
            };

            return View(viewModel);
        }

        private bool DiaEstaAberto(int agenciaId)
        {
            var hoje = DateTime.Today;

            return _context.Fechos.Any(f =>
                f.AgenciaId == agenciaId &&
                f.EstadoId == 1 &&
                f.DataCadastro.Date == hoje
            );
        }

        [HttpPost]
        public IActionResult Criar(EntradaViewModel entrada)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            int agenciaId = HttpContext.Session.GetInt32("AgenciaId") ?? 0;
            try
            {
                // 🔒 BLOQUEIO DO DIA
                if (!DiaEstaAberto(agenciaId))
                {
                    TempData["MensagemErro"] = "O dia está fechado. Não é possível lançar entradas.";
                    return RedirectToAction("Criar");
                }

                if (ModelState.IsValid)
                {
                    entrada.ListaEntradas = _entradaRepositorio.BuscarTodos();

                    TempData["MensagemErro"] = "Dados inválidos! Corrija os erros e tente novamente.";
                    return View(entrada);
                }
                 //Actualizar saldo Agencia
                var agencia = _context.Agencias
                              .FirstOrDefault(a => a.Id == entrada.EntradaNome.AgenciaId);

                if (agencia == null)
                {
                    TempData["MensagemErro"] = "Tabernáculo não encontrado!";
                    return RedirectToAction("Criar");
                }

                agencia.Credito += entrada.EntradaNome.Valor;

                //Actualizar saldo Linha
                var linha = _context.Linhas
                              .FirstOrDefault(a => a.Id == entrada.EntradaNome.LinhaId);

                if (linha == null)
                {
                    TempData["MensagemErro"] = "Linha não encontrada!";
                    return RedirectToAction("Criar");
                }

                linha.Credito += entrada.EntradaNome.Valor;

                //Actualizar saldo Conta
                var conta = _context.Contas
                              .FirstOrDefault(a => a.Id == entrada.EntradaNome.ContaId);

                if (conta == null)
                {
                    TempData["MensagemErro"] = "Conta não encontrada!";
                    return RedirectToAction("Criar");
                }

                conta.Credito += entrada.EntradaNome.Valor;

                ExtratoModel extrato = new ExtratoModel()
                {
                    AgenciaId = entrada.EntradaNome.AgenciaId,
                    Debito = 0,
                    CanalId = entrada.EntradaNome.ContaId,
                    LinhaId = entrada.EntradaNome.LinhaId,
                    Credito = entrada.EntradaNome.Valor,
                    UsuarioId = usuarioId,
                    DataCadastro = DateTime.Now,
                    Descricao = entrada.EntradaNome.Descricao
                };

                _context.Extratos.Add(extrato);

                if (entrada.EntradaNome.TipoEntradaId == 6)
                {
                    ContasPagarModel contasPagar = new ContasPagarModel()
                    {
                        AgenciaId = entrada.EntradaNome.AgenciaId,
                        CadastroId = entrada.EntradaNome.CadastroId,
                        Valor = entrada.EntradaNome.Valor,
                        Pago = 0,
                        UsuarioId = usuarioId,
                        DataCadastro = DateTime.Now
                    };
                    _context.PagarContas.Add(contasPagar);
                }

                if (entrada.EntradaNome.TipoEntradaId == 5)
                {
                    //Actualizar saldo Conta
                    var cobrar = _context.CobrarContas
                                  .FirstOrDefault(a => a.Id == entrada.EntradaNome.CadastroId);

                    if (cobrar == null)
                    {
                        TempData["MensagemErro"] = "Dados não encontrados!";
                        return RedirectToAction("Criar");
                    }

                    cobrar.Pago += entrada.EntradaNome.Valor;
                }

                if (entrada.EntradaNome.TipoEntradaId == 7)
                {
                    entrada.EntradaNome.LinhaId = 2;
                }
                else
                {
                    entrada.EntradaNome.LinhaId = 1;
                }
                _context.SaveChanges();

                // ✅ Se chegou aqui → ModelState válido
                entrada.EntradaNome.UsuarioId = usuarioId;
                _entradaRepositorio.Adicionar(entrada.EntradaNome);

                TempData["MensagemSucesso"] = "Registado com sucesso!";
                return RedirectToAction("Criar");
            }
            catch (Exception erro)
            {
                Console.WriteLine("Erro: " + erro.InnerException?.Message);

                TempData["MensagemErro"] = $"Ops, erro ao registar! Detalhes: {erro.InnerException?.Message ?? erro.Message}";

                entrada.ListaEntradas = _entradaRepositorio.BuscarTodos();

                return View(entrada);
            }
        }


        // POST: Editar
        [HttpPost]
        public IActionResult Editar(EntradaViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    //viewModel.CadastroNome.UsuarioId = usuarioId;
                    _entradaRepositorio.Actualizar(viewModel.EntradaNome);
                    TempData["MensagemSucesso"] = "Actualizado com sucesso!";
                    return RedirectToAction("Criar");
                }
                viewModel.ListaEntradas = _entradaRepositorio.BuscarTodos();
                return View(viewModel);
            }
            catch (Exception erro)
            {
                TempData["MensagemErro"] = $"Ops, erro ao actualizar. Detalhes: {erro.Message}";
                return RedirectToAction("Criar");
            }
        }

        public IActionResult RelatorioEntradas(
             DateTime? dataInicio,
             DateTime? dataFim,
             int? agenciaId,
             int? tipoId,
             int? cadastroId,
             int? linhaId,
             int? contaId)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            int agenciaSessaoId = HttpContext.Session.GetInt32("AgenciaId") ?? 0;

            var tipoEntradas = _tipoEntradaRepositorio.BuscarTodos()
                 .Select(m => new SelectListItem
                 {
                     Value = m.Id.ToString(),
                     Text = m.Nome,
                     Selected = tipoId.HasValue && m.Id == tipoId
                 }).ToList();

            var tabernaculos = _agenciaRepositorio.BuscarTodos()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Nome,
                    Selected = agenciaId.HasValue && m.Id == agenciaId
                }).ToList();

            var linhas = _linhaRepositorio.BuscarTodos()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Nome,
                    Selected = linhaId.HasValue && m.Id == linhaId
                }).ToList();

            var contas = _contaRepositorio.BuscarTodos()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Nome,
                    Selected = contaId.HasValue && m.Id == contaId
                }).ToList();

            var cadastros = _cadastroRepositorio.BuscarTodos()
                 .Select(m => new SelectListItem
                 {
                     Value = m.Id.ToString(),
                     Text = m.Nome,
                     Selected = cadastroId.HasValue && m.Id == cadastroId
                 }).ToList();

            var entradas = _entradaRepositorio.BuscarTodosRelatorio().AsQueryable();

            // 🔹 FILTRO POR INTERVALO DE DATAS
            if (dataInicio.HasValue)
                entradas = entradas.Where(e => e.DataCadastro >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                entradas = entradas.Where(e => e.DataCadastro < dataFim.Value.Date.AddDays(1));

            if (agenciaId.HasValue)
                entradas = entradas.Where(e => e.AgenciaId == agenciaId);

            if (tipoId.HasValue)
                entradas = entradas.Where(e => e.TipoEntradaId == tipoId);

            if (cadastroId.HasValue)
                entradas = entradas.Where(e => e.CadastroId == cadastroId);

            if (linhaId.HasValue)
                entradas = entradas.Where(e => e.LinhaId == linhaId);

            if (contaId.HasValue)
                entradas = entradas.Where(e => e.ContaId == contaId);

            var pedidoModel = new EntradaModel
            {
                ListaAgencias = tabernaculos,
                ListaTipos = tipoEntradas,
                ListaLinhas = linhas,
                ListaContas = contas,
                ListaCadastros = cadastros
            };

            var viewModel = new EntradaViewModel
            {
                EntradaNome = pedidoModel,
                ListaEntradas = entradas.ToList()
            };

            return View(viewModel);
        }

        public IActionResult ImprimirEntradas(DateTime? dataInicio,
            DateTime? dataFim,
            int? agenciaId,
            int? tipoId,
            int? cadastroId,
            int? linhaId,
            int? contaId)
        {
            var entradas = _context.Entradas
                .Include(e => e.Agencia)
                .Include(e => e.TipoEntrada)
                .Include(e => e.Cadastro)
                .ToList();
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            var lista = _entradaRepositorio.BuscarTodosRelatorio().AsQueryable();

            if (dataInicio.HasValue)
                lista = lista.Where(e => e.DataCadastro >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                lista = lista.Where(e => e.DataCadastro < dataFim.Value.Date.AddDays(1));

            if (agenciaId.HasValue)
                lista = lista.Where(e => e.AgenciaId == agenciaId);

            if (tipoId.HasValue)
                lista = lista.Where(e => e.TipoEntradaId == tipoId);

            if (cadastroId.HasValue)
                lista = lista.Where(e => e.CadastroId == cadastroId);

            if (linhaId.HasValue)
                lista = lista.Where(e => e.LinhaId == linhaId);

            if (contaId.HasValue)
                lista = lista.Where(e => e.ContaId == contaId);

            var dadosFiltrados = lista.ToList();
            decimal totalGeral = dadosFiltrados.Sum(x => (decimal?)x.Valor) ?? 0;

            using MemoryStream stream = new MemoryStream();

            float pxPorMm = 72 / 25.5f;
            iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4,
                15 * pxPorMm, 15 * pxPorMm, 60 * pxPorMm, 25 * pxPorMm); // Margem topo maior para cabeçalho, rodapé

            PdfWriter writer = PdfWriter.GetInstance(doc, stream);

            // Adicionar evento de cabeçalho/rodapé
            string logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
            string usuario = "Desconhecido";
            if (usuarioId > 0)
            {
                usuario = _context.Usuarios
                    .Where(u => u.Id == usuarioId)
                    .Select(u => u.Login)
                    .FirstOrDefault() ?? "Desconhecido";
            }
            string empresa = "IGREJA CRENTES DA BÍBLIA EM MOÇAMBIQUE\nTABERNÁCULO DE MAPUTO\nInterdenominacional";
            string escritura ="_____________________________________________________________________________________________________________________________________________________" +
                               "\n\nMALAQUIAS 4:5-6                                                                          LUCAS 17:30                                                                          APOCALIPSE 10:7";
            string titulo = "\nEntradas";

            writer.PageEvent = new PdfHeaderFooter(logoPath, empresa, escritura, titulo, usuario);

            doc.Open();

            // 🔹 RESUMO DOS FILTROS
            iTextSharp.text.Font fonteFiltro =
                FontFactory.GetFont(FontFactory.HELVETICA, 7, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            Paragraph filtros = new Paragraph();
            filtros.SpacingAfter = 10;

            string periodo = (dataInicio.HasValue || dataFim.HasValue)
                ? $"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}"
                : "Período: Todo período";

            string tabernaculo = agenciaId.HasValue
                ? "Tabernáculo: " + _context.Agencias
                    .Where(a => a.Id == agenciaId)
                    .Select(a => a.Nome)
                    .FirstOrDefault()
                : "Tabernáculo: Todos";
            
            string tipo = tipoId.HasValue
                ? "Tipo: " + _context.TipoEntradas
                    .Where(t => t.Id == tipoId)
                    .Select(t => t.Nome)
                    .FirstOrDefault()
                : "Tipo: Todos";

            string linha = linhaId.HasValue
                ? "Linha: " + _context.Linhas
                    .Where(t => t.Id == linhaId)
                    .Select(t => t.Nome)
                    .FirstOrDefault()
                : "Linha: Todas";

            string conta = contaId.HasValue
                ? "Conta: " + _context.Contas
                    .Where(t => t.Id == contaId)
                    .Select(t => t.Nome)
                    .FirstOrDefault()
                : "Conta: Todas";

            string contribuinte = cadastroId.HasValue
                ? "Contribuinte: " + _context.Cadastros
                    .Where(c => c.Id == cadastroId)
                    .Select(c => c.Nome)
                    .FirstOrDefault()
                : "Contribuinte: Todos";

            filtros.Add(new Phrase(periodo + "\n", fonteFiltro));
            filtros.Add(new Phrase(tabernaculo + " | ", fonteFiltro));
            filtros.Add(new Phrase(tipo + " | ", fonteFiltro));
            filtros.Add(new Phrase(linha + " | ", fonteFiltro));
            filtros.Add(new Phrase(conta + " | ", fonteFiltro));
            filtros.Add(new Phrase(contribuinte + "\n\n", fonteFiltro));

            doc.Add(filtros);

            // Fontes e cores
            BaseColor corCabecalho = new BaseColor(240, 240, 240);
            iTextSharp.text.Font fonteCabecalho = FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            iTextSharp.text.Font fonteDados = FontFactory.GetFont(FontFactory.COURIER, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            BaseColor corLinha = new BaseColor(200, 200, 200);

            // Tabela
            PdfPTable tabela = new PdfPTable(7);
            tabela.WidthPercentage = 100;
            tabela.SetWidths(new float[] { 7f, 10f, 13f, 18f, 30f, 11f, 11f });
            tabela.HeaderRows = 1; // Repete cabeçalho automaticamente

            // Cabeçalhos
            AddHeader(tabela, "Id", fonteCabecalho, corCabecalho);
            AddHeader(tabela, "Tabern.", fonteCabecalho, corCabecalho);
            AddHeader(tabela, "Tipo", fonteCabecalho, corCabecalho);
            AddHeader(tabela, "Contribuinte", fonteCabecalho, corCabecalho);
            AddHeader(tabela, "Descrição", fonteCabecalho, corCabecalho);
            AddHeader(tabela, "Valor", fonteCabecalho, corCabecalho);
            AddHeader(tabela, "Data", fonteCabecalho, corCabecalho);

            // Dados com linhas horizontais finas
            foreach (var item in dadosFiltrados)
            {
                PdfPCell AddCell(string valor, int alinhamento = Element.ALIGN_LEFT)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(valor, fonteDados))
                    {
                        Border = PdfPCell.BOTTOM_BORDER,
                        BorderColor = corLinha,
                        HorizontalAlignment = alinhamento
                    };
                    return cell;
                }

                tabela.AddCell(AddCell(item.Id.ToString()));
                tabela.AddCell(AddCell(item.Agencia?.Nome ?? ""));
                tabela.AddCell(AddCell(item.TipoEntrada?.Nome ?? ""));
                tabela.AddCell(AddCell(item.Cadastro?.Nome ?? ""));
                tabela.AddCell(AddCell(item.Descricao ?? ""));
                tabela.AddCell(AddCell(item.Valor.ToString("N2", new CultureInfo("pt-PT")), Element.ALIGN_RIGHT));
                tabela.AddCell(AddCell(item.DataCadastro.ToString("dd/MM/yyyy"), Element.ALIGN_CENTER));
            }
            // 🔹 TOTAL GERAL
            Paragraph total = new Paragraph(
                "\nTotal: " + totalGeral.ToString("N2", new CultureInfo("pt-PT")),
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)
            );

            total.Alignment = Element.ALIGN_RIGHT;

            doc.Add(total);
            doc.Add(tabela);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "Entradas.pdf");
        }
        private void AddHeader(PdfPTable table, string texto, iTextSharp.text.Font fonte, BaseColor cor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(texto, fonte));

            cell.BackgroundColor = cor;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.BorderWidthBottom = 2;
            cell.BorderWidthTop = 0;
            cell.BorderWidthLeft = 0;
            cell.BorderWidthRight = 0;
            cell.FixedHeight = 18;

            table.AddCell(cell);
        }


        public IActionResult RelatorioEntradaAgrupadoTab(
           DateTime? dataInicio,
           DateTime? dataFim,
           int? agenciaId,
           int? tipoId,
           int? cadastroId,
           int? linhaId,
           int? contaId)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            // Todas agências (para incluir as que não têm entradas)
            var todasAgencias = _context.Agencias.ToList();

            // Entradas filtradas
            var lista = _entradaRepositorio.BuscarTodosRelatorio().AsQueryable();

            if (dataInicio.HasValue)
                lista = lista.Where(e => e.DataCadastro >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                lista = lista.Where(e => e.DataCadastro < dataFim.Value.Date.AddDays(1));

            if (agenciaId.HasValue)
                lista = lista.Where(e => e.AgenciaId == agenciaId);

            // Agrupar, somar e ordenar
            var entradasAgrupadas = todasAgencias
                .GroupJoin(
                    lista,
                    a => a.Id,
                    e => e.AgenciaId,
                    (a, entradasAgencia) => new EntradaModel
                    {
                        Agencia = new AgenciaModel { Nome = a.Nome },
                        Valor = entradasAgencia.Sum(x => (decimal?)x.Valor) ?? 0
                    }
                )
                .OrderByDescending(x => x.Valor)
                .ToList();

            // Adicionar Total Geral
            var totalGeral = entradasAgrupadas.Sum(x => x.Valor);
            entradasAgrupadas.Add(new EntradaModel
            {
                Agencia = new AgenciaModel { Nome = "Soma de todos" },
                Valor = totalGeral
            });

            var viewModel = new EntradaViewModel
            {
                ListaEntradas = entradasAgrupadas
            };

            return View(viewModel);
        }

        public IActionResult ImprimirEntradaAgrupadoTab(
            DateTime? dataInicio,
            DateTime? dataFim,
            int? agenciaId,
            int? tipoId,
            int? cadastroId,
            int? linhaId,
            int? contaId)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var todasAgencias = _context.Agencias.ToList();
            var lista = _entradaRepositorio.BuscarTodosRelatorio().AsQueryable();

            if (dataInicio.HasValue)
                lista = lista.Where(e => e.DataCadastro >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                lista = lista.Where(e => e.DataCadastro < dataFim.Value.Date.AddDays(1));

            if (agenciaId.HasValue)
                lista = lista.Where(e => e.AgenciaId == agenciaId);

            var dadosAgrupados = todasAgencias
                .GroupJoin(
                    lista,
                    a => a.Id,
                    e => e.AgenciaId,
                    (a, entradasAgencia) => new
                    {
                        AgenciaNome = a.Nome,
                        ValorTotal = entradasAgencia.Sum(x => (decimal?)x.Valor) ?? 0
                    }
                )
                .OrderByDescending(x => x.ValorTotal)
                .ToList();

            // Adicionar linha Total Geral
            var totalGeral = dadosAgrupados.Sum(x => x.ValorTotal);
            dadosAgrupados.Add(new { AgenciaNome = "Soma de todos", ValorTotal = totalGeral });

            using MemoryStream stream = new MemoryStream();
            float pxPorMm = 72 / 25.5f;
            iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4,
                15 * pxPorMm, 15 * pxPorMm, 60 * pxPorMm, 25 * pxPorMm);

            PdfWriter writer = PdfWriter.GetInstance(doc, stream);

            string logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
            string usuario = usuarioId > 0
                ? _context.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.Login).FirstOrDefault() ?? "Desconhecido"
                : "Desconhecido";

            string empresa = "IGREJA CRENTES DA BÍBLIA EM MOÇAMBIQUE\nTABERNÁCULO DE MAPUTO\nInterdenominacional";
            string escritura = "_____________________________________________________________________________________________________________________________________________________" +
                               "\n\nMALAQUIAS 4:5-6                                                                          LUCAS 17:30                                                                          APOCALIPSE 10:7";
            string titulo = "\nEntradas agrupadas por Tabernáculo";
            writer.PageEvent = new PdfHeaderFooter(logoPath, empresa, escritura, titulo, usuario);

            doc.Open();

            var fonteFiltro = FontFactory.GetFont(FontFactory.HELVETICA, 7, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Paragraph filtros = new Paragraph { SpacingAfter = 10 };
            string periodo = (dataInicio.HasValue || dataFim.HasValue)
                ? $"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}"
                : "Período: Todo período";
            filtros.Add(new Phrase(periodo + "\n", fonteFiltro));
            doc.Add(filtros);

            BaseColor corCabecalho = new BaseColor(240, 240, 240);
            BaseColor corLinha = new BaseColor(200, 200, 200);
            var fonteCabecalho = FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            var fonteDados = FontFactory.GetFont(FontFactory.COURIER, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            PdfPTable tabela = new PdfPTable(2) { WidthPercentage = 100 };
            tabela.SetWidths(new float[] { 70f, 30f });
            tabela.HeaderRows = 1;
            AddHeader(tabela, "Tabernáculo", fonteCabecalho, corCabecalho);
            AddHeader(tabela, "Valor", fonteCabecalho, corCabecalho);

            foreach (var item in dadosAgrupados)
            {
                PdfPCell AddCell(string valor, int alinhamento = Element.ALIGN_LEFT)
                {
                    return new PdfPCell(new Phrase(valor, fonteDados))
                    {
                        Border = PdfPCell.BOTTOM_BORDER,
                        BorderColor = corLinha,
                        HorizontalAlignment = alinhamento
                    };
                }

                tabela.AddCell(AddCell(item.AgenciaNome));
                tabela.AddCell(AddCell(item.ValorTotal.ToString("N2", new CultureInfo("pt-PT")), Element.ALIGN_RIGHT));
            }

            doc.Add(tabela);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "EntradaAgrupadoTab.pdf");
        }

        public IActionResult RelatorioEntradaAgrupadoTipo(
          DateTime? dataInicio,
          DateTime? dataFim,
          int? agenciaId,
          int? tipoId,
          int? cadastroId,
          int? linhaId,
          int? contaId)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            // Todas agências (para incluir as que não têm entradas)
            var todasTipos = _context.TipoEntradas.ToList();

            // Entradas filtradas
            var lista = _entradaRepositorio.BuscarTodosRelatorio().AsQueryable();

            if (dataInicio.HasValue)
                lista = lista.Where(e => e.DataCadastro >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                lista = lista.Where(e => e.DataCadastro < dataFim.Value.Date.AddDays(1));

            if (tipoId.HasValue)
                lista = lista.Where(e => e.TipoEntradaId == tipoId);

            // Agrupar, somar e ordenar
            var entradasAgrupadas = todasTipos
                .GroupJoin(
                    lista,
                    a => a.Id,
                    e => e.TipoEntradaId,
                    (a, entradasTipo) => new EntradaModel
                    {
                        TipoEntrada = new TipoEntradaModel { Nome = a.Nome },
                        Valor = entradasTipo.Sum(x => (decimal?)x.Valor) ?? 0
                    }
                )
                .OrderByDescending(x => x.Valor)
                .ToList();

            // Adicionar Total Geral
            var totalGeral = entradasAgrupadas.Sum(x => x.Valor);
            entradasAgrupadas.Add(new EntradaModel
            {
                TipoEntrada = new TipoEntradaModel { Nome = "Soma de todos" },
                Valor = totalGeral
            });

            var viewModel = new EntradaViewModel
            {
                ListaEntradas = entradasAgrupadas
            };

            return View(viewModel);
        }

        public IActionResult ImprimirEntradaAgrupadoTipo(
            DateTime? dataInicio,
            DateTime? dataFim,
            int? agenciaId,
            int? tipoId,
            int? cadastroId,
            int? linhaId,
            int? contaId)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var todasTipos = _context.TipoEntradas.ToList();
            var lista = _entradaRepositorio.BuscarTodosRelatorio().AsQueryable();

            if (dataInicio.HasValue)
                lista = lista.Where(e => e.DataCadastro >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                lista = lista.Where(e => e.DataCadastro < dataFim.Value.Date.AddDays(1));

            if (tipoId.HasValue)
                lista = lista.Where(e => e.TipoEntradaId == tipoId);

            var dadosAgrupados = todasTipos
                .GroupJoin(
                    lista,
                    a => a.Id,
                    e => e.TipoEntradaId,
                    (a, entradasTipo) => new
                    {
                        TipoEntrada = a.Nome,
                        ValorTotal = entradasTipo.Sum(x => (decimal?)x.Valor) ?? 0
                    }
                )
                .OrderByDescending(x => x.ValorTotal)
                .ToList();

            // Adicionar linha Total Geral
            var totalGeral = dadosAgrupados.Sum(x => x.ValorTotal);
            dadosAgrupados.Add(new { TipoEntrada = "Soma de todos", ValorTotal = totalGeral });

            using MemoryStream stream = new MemoryStream();
            float pxPorMm = 72 / 25.5f;
            iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4,
                15 * pxPorMm, 15 * pxPorMm, 60 * pxPorMm, 25 * pxPorMm);

            PdfWriter writer = PdfWriter.GetInstance(doc, stream);

            string logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
            string usuario = usuarioId > 0
                ? _context.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.Login).FirstOrDefault() ?? "Desconhecido"
                : "Desconhecido";

            string empresa = "IGREJA CRENTES DA BÍBLIA EM MOÇAMBIQUE\nTABERNÁCULO DE MAPUTO\nInterdenominacional";
            string escritura = "_____________________________________________________________________________________________________________________________________________________" +
                               "\n\nMALAQUIAS 4:5-6                                                                          LUCAS 17:30                                                                          APOCALIPSE 10:7";
            string titulo = "\nEntradas agrupadas por Tipo";
            writer.PageEvent = new PdfHeaderFooter(logoPath, empresa, escritura, titulo, usuario);

            doc.Open();

            var fonteFiltro = FontFactory.GetFont(FontFactory.HELVETICA, 7, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Paragraph filtros = new Paragraph { SpacingAfter = 10 };
            string periodo = (dataInicio.HasValue || dataFim.HasValue)
                ? $"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}"
                : "Período: Todo período";
            filtros.Add(new Phrase(periodo + "\n", fonteFiltro));
            doc.Add(filtros);

            BaseColor corCabecalho = new BaseColor(240, 240, 240);
            BaseColor corLinha = new BaseColor(200, 200, 200);
            var fonteCabecalho = FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            var fonteDados = FontFactory.GetFont(FontFactory.COURIER, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            PdfPTable tabela = new PdfPTable(2) { WidthPercentage = 100 };
            tabela.SetWidths(new float[] { 70f, 30f });
            tabela.HeaderRows = 1;
            AddHeader(tabela, "Tipo de entrada", fonteCabecalho, corCabecalho);
            AddHeader(tabela, "Valor", fonteCabecalho, corCabecalho);

            foreach (var item in dadosAgrupados)
            {
                PdfPCell AddCell(string valor, int alinhamento = Element.ALIGN_LEFT)
                {
                    return new PdfPCell(new Phrase(valor, fonteDados))
                    {
                        Border = PdfPCell.BOTTOM_BORDER,
                        BorderColor = corLinha,
                        HorizontalAlignment = alinhamento
                    };
                }

                tabela.AddCell(AddCell(item.TipoEntrada));
                tabela.AddCell(AddCell(item.ValorTotal.ToString("N2", new CultureInfo("pt-PT")), Element.ALIGN_RIGHT));
            }

            doc.Add(tabela);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "EntradaAgrupadoTipo.pdf");
        }


        public IActionResult RelatorioEntradaAgrupadoConta(
          DateTime? dataInicio,
          DateTime? dataFim,
          int? agenciaId,
          int? tipoId,
          int? cadastroId,
          int? linhaId,
          int? contaId)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            // Todas agências (para incluir as que não têm entradas)
            var todasContas = _context.Contas.ToList();

            // Entradas filtradas
            var lista = _entradaRepositorio.BuscarTodosRelatorio().AsQueryable();

            if (dataInicio.HasValue)
                lista = lista.Where(e => e.DataCadastro >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                lista = lista.Where(e => e.DataCadastro < dataFim.Value.Date.AddDays(1));

            if (contaId.HasValue)
                lista = lista.Where(e => e.ContaId == contaId);

            // Agrupar, somar e ordenar
            var entradasAgrupadas = todasContas
                .GroupJoin(
                    lista,
                    a => a.Id,
                    e => e.ContaId,
                    (a, entradasConta) => new EntradaModel
                    {
                        Conta = new ContaModel { Nome = a.Nome },
                        Valor = entradasConta.Sum(x => (decimal?)x.Valor) ?? 0
                    }
                )
                .OrderByDescending(x => x.Valor)
                .ToList();

            // Adicionar Total Geral
            var totalGeral = entradasAgrupadas.Sum(x => x.Valor);
            entradasAgrupadas.Add(new EntradaModel
            {
                Conta = new ContaModel { Nome = "Soma de todos" },
                Valor = totalGeral
            });

            var viewModel = new EntradaViewModel
            {
                ListaEntradas = entradasAgrupadas
            };

            return View(viewModel);
        }

        public IActionResult ImprimirEntradaAgrupadoConta(
            DateTime? dataInicio,
            DateTime? dataFim,
            int? agenciaId,
            int? tipoId,
            int? cadastroId,
            int? linhaId,
            int? contaId)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var todasContas = _context.Contas.ToList();
            var lista = _entradaRepositorio.BuscarTodosRelatorio().AsQueryable();

            if (dataInicio.HasValue)
                lista = lista.Where(e => e.DataCadastro >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                lista = lista.Where(e => e.DataCadastro < dataFim.Value.Date.AddDays(1));

            if (contaId.HasValue)
                lista = lista.Where(e => e.ContaId == contaId);

            var dadosAgrupados = todasContas
                .GroupJoin(
                    lista,
                    a => a.Id,
                    e => e.ContaId,
                    (a, entradasConta) => new
                    {
                        Conta = a.Nome,
                        ValorTotal = entradasConta.Sum(x => (decimal?)x.Valor) ?? 0
                    }
                )
                .OrderByDescending(x => x.ValorTotal)
                .ToList();

            // Adicionar linha Total Geral
            var totalGeral = dadosAgrupados.Sum(x => x.ValorTotal);
            dadosAgrupados.Add(new { Conta = "Soma de todos", ValorTotal = totalGeral });

            using MemoryStream stream = new MemoryStream();
            float pxPorMm = 72 / 25.5f;
            iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4,
                15 * pxPorMm, 15 * pxPorMm, 60 * pxPorMm, 25 * pxPorMm);

            PdfWriter writer = PdfWriter.GetInstance(doc, stream);

            string logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
            string usuario = usuarioId > 0
                ? _context.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.Login).FirstOrDefault() ?? "Desconhecido"
                : "Desconhecido";

            string empresa = "IGREJA CRENTES DA BÍBLIA EM MOÇAMBIQUE\nTABERNÁCULO DE MAPUTO\nInterdenominacional";
            string escritura = "_____________________________________________________________________________________________________________________________________________________" +
                               "\n\nMALAQUIAS 4:5-6                                                                          LUCAS 17:30                                                                          APOCALIPSE 10:7";
            string titulo = "\nEntradas agrupadas por Conta";
            writer.PageEvent = new PdfHeaderFooter(logoPath, empresa, escritura, titulo, usuario);

            doc.Open();

            var fonteFiltro = FontFactory.GetFont(FontFactory.HELVETICA, 7, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Paragraph filtros = new Paragraph { SpacingAfter = 10 };
            string periodo = (dataInicio.HasValue || dataFim.HasValue)
                ? $"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}"
                : "Período: Todo período";
            filtros.Add(new Phrase(periodo + "\n", fonteFiltro));
            doc.Add(filtros);

            BaseColor corCabecalho = new BaseColor(240, 240, 240);
            BaseColor corLinha = new BaseColor(200, 200, 200);
            var fonteCabecalho = FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            var fonteDados = FontFactory.GetFont(FontFactory.COURIER, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            PdfPTable tabela = new PdfPTable(2) { WidthPercentage = 100 };
            tabela.SetWidths(new float[] { 70f, 30f });
            tabela.HeaderRows = 1;
            AddHeader(tabela, "Conta", fonteCabecalho, corCabecalho);
            AddHeader(tabela, "Valor", fonteCabecalho, corCabecalho);

            foreach (var item in dadosAgrupados)
            {
                PdfPCell AddCell(string valor, int alinhamento = Element.ALIGN_LEFT)
                {
                    return new PdfPCell(new Phrase(valor, fonteDados))
                    {
                        Border = PdfPCell.BOTTOM_BORDER,
                        BorderColor = corLinha,
                        HorizontalAlignment = alinhamento
                    };
                }

                tabela.AddCell(AddCell(item.Conta));
                tabela.AddCell(AddCell(item.ValorTotal.ToString("N2", new CultureInfo("pt-PT")), Element.ALIGN_RIGHT));
            }

            doc.Add(tabela);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "EntradaAgrupadoConta.pdf");
        }


        public IActionResult RelatorioEntradaAgrupadoLinha(
          DateTime? dataInicio,
          DateTime? dataFim,
          int? agenciaId,
          int? tipoId,
          int? cadastroId,
          int? linhaId,
          int? contaId)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            // Todas agências (para incluir as que não têm entradas)
            var todasLinhas = _context.Linhas.ToList();

            // Entradas filtradas
            var lista = _entradaRepositorio.BuscarTodosRelatorio().AsQueryable();

            if (dataInicio.HasValue)
                lista = lista.Where(e => e.DataCadastro >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                lista = lista.Where(e => e.DataCadastro < dataFim.Value.Date.AddDays(1));

            if (linhaId.HasValue)
                lista = lista.Where(e => e.LinhaId == linhaId);

            // Agrupar, somar e ordenar
            var entradasAgrupadas = todasLinhas
                .GroupJoin(
                    lista,
                    a => a.Id,
                    e => e.LinhaId,
                    (a, entradasLinha) => new EntradaModel
                    {
                        Linha = new LinhaModel { Nome = a.Nome },
                        Valor = entradasLinha.Sum(x => (decimal?)x.Valor) ?? 0
                    }
                )
                .OrderByDescending(x => x.Valor)
                .ToList();

            // Adicionar Total Geral
            var totalGeral = entradasAgrupadas.Sum(x => x.Valor);
            entradasAgrupadas.Add(new EntradaModel
            {
                Linha = new LinhaModel { Nome = "Soma de todos" },
                Valor = totalGeral
            });

            var viewModel = new EntradaViewModel
            {
                ListaEntradas = entradasAgrupadas
            };

            return View(viewModel);
        }

        public IActionResult ImprimirEntradaAgrupadoLinha(
            DateTime? dataInicio,
            DateTime? dataFim,
            int? agenciaId,
            int? tipoId,
            int? cadastroId,
            int? linhaId,
            int? contaId)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var todasLinhas = _context.Linhas.ToList();
            var lista = _entradaRepositorio.BuscarTodosRelatorio().AsQueryable();

            if (dataInicio.HasValue)
                lista = lista.Where(e => e.DataCadastro >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                lista = lista.Where(e => e.DataCadastro < dataFim.Value.Date.AddDays(1));

            if (linhaId.HasValue)
                lista = lista.Where(e => e.LinhaId == linhaId);

            var dadosAgrupados = todasLinhas
                .GroupJoin(
                    lista,
                    a => a.Id,
                    e => e.LinhaId,
                    (a, entradasLinha) => new
                    {
                        Linha = a.Nome,
                        ValorTotal = entradasLinha.Sum(x => (decimal?)x.Valor) ?? 0
                    }
                )
                .OrderByDescending(x => x.ValorTotal)
                .ToList();

            // Adicionar linha Total Geral
            var totalGeral = dadosAgrupados.Sum(x => x.ValorTotal);
            dadosAgrupados.Add(new { Linha = "Soma de todos", ValorTotal = totalGeral });

            using MemoryStream stream = new MemoryStream();
            float pxPorMm = 72 / 25.5f;
            iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4,
                15 * pxPorMm, 15 * pxPorMm, 60 * pxPorMm, 25 * pxPorMm);

            PdfWriter writer = PdfWriter.GetInstance(doc, stream);

            string logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
            string usuario = usuarioId > 0
                ? _context.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.Login).FirstOrDefault() ?? "Desconhecido"
                : "Desconhecido";

            string empresa = "IGREJA CRENTES DA BÍBLIA EM MOÇAMBIQUE\nTABERNÁCULO DE MAPUTO\nInterdenominacional";
            string escritura = "_____________________________________________________________________________________________________________________________________________________" +
                               "\n\nMALAQUIAS 4:5-6                                                                          LUCAS 17:30                                                                          APOCALIPSE 10:7";
            string titulo = "\nEntradas agrupadas por Linha";
            writer.PageEvent = new PdfHeaderFooter(logoPath, empresa, escritura, titulo, usuario);

            doc.Open();

            var fonteFiltro = FontFactory.GetFont(FontFactory.HELVETICA, 7, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Paragraph filtros = new Paragraph { SpacingAfter = 10 };
            string periodo = (dataInicio.HasValue || dataFim.HasValue)
                ? $"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}"
                : "Período: Todo período";
            filtros.Add(new Phrase(periodo + "\n", fonteFiltro));
            doc.Add(filtros);

            BaseColor corCabecalho = new BaseColor(240, 240, 240);
            BaseColor corLinha = new BaseColor(200, 200, 200);
            var fonteCabecalho = FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            var fonteDados = FontFactory.GetFont(FontFactory.COURIER, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            PdfPTable tabela = new PdfPTable(2) { WidthPercentage = 100 };
            tabela.SetWidths(new float[] { 70f, 30f });
            tabela.HeaderRows = 1;
            AddHeader(tabela, "Linha", fonteCabecalho, corCabecalho);
            AddHeader(tabela, "Valor", fonteCabecalho, corCabecalho);

            foreach (var item in dadosAgrupados)
            {
                PdfPCell AddCell(string valor, int alinhamento = Element.ALIGN_LEFT)
                {
                    return new PdfPCell(new Phrase(valor, fonteDados))
                    {
                        Border = PdfPCell.BOTTOM_BORDER,
                        BorderColor = corLinha,
                        HorizontalAlignment = alinhamento
                    };
                }

                tabela.AddCell(AddCell(item.Linha));
                tabela.AddCell(AddCell(item.ValorTotal.ToString("N2", new CultureInfo("pt-PT")), Element.ALIGN_RIGHT));
            }

            doc.Add(tabela);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "EntradaAgrupadoLinha.pdf");
        }
    }
}
