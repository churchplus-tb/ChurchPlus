using Analise.Enuns;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analise.Models
{
    public class SaidaModel
    {
        public int Id { get; set; }
        public int TipoDespesaId { get; set; }
        public TipoDespesaModel TipoDespesa { get; set; }
        [NotMapped]
        public List<SelectListItem> ListaDespesas { get; set; }
        public int? LinhaId { get; set; }
        public LinhaModel Linha { get; set; }
        [NotMapped]
        public List<SelectListItem> ListaLinhas { get; set; }
        public int ContaId { get; set; }
        public ContaModel Conta { get; set; }
        [NotMapped]
        public List<SelectListItem> ListaContas { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTimeOffset DataReferencia { get; set; }

        [Required(ErrorMessage = "Deixe a descrição")]
        public string Descricao { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Required(ErrorMessage = "Digite o valor")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Digite a quantidade")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantidade { get; set; }
        [NotMapped]
        public decimal Total => Quantidade * Valor;
        public int CadastroId { get; set; }
        public CadastroModel Cadastro { get; set; }
        [NotMapped]
        public List<SelectListItem> ListaCadastros { get; set; }
        public int? TipoServicoId { get; set; }
        public TipoServicoModel TipoServico { get; set; }
        [NotMapped]
        public List<SelectListItem> ListaTipoServicos { get; set; }
        public DateTimeOffset DataCadastro { get; set; }
        public int AgenciaId { get; set; }
        public AgenciaModel Agencia { get; set; }
        [NotMapped]
        public List<SelectListItem> ListaAgencias { get; set; }
      
        public int UsuarioId { get; set; }
        public UsuarioModel Usuario { get; set; }
    }
}
