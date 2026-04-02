using Analise.Enuns;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analise.Models
{
    public class EntradaModel
    {
        public int Id { get; set; }
        public int TipoEntradaId { get; set; }
        public TipoEntradaModel TipoEntrada { get; set; }
        [NotMapped]
        public List<SelectListItem> ListaTipos { get; set; }
        public int? LinhaId { get; set; }
        public LinhaModel Linha { get; set; }
        [NotMapped]
        public List<SelectListItem> ListaLinhas { get; set; }
        public int ContaId { get; set; }
        public ContaModel Conta { get; set; }
        [NotMapped]
        public List<SelectListItem> ListaContas { get; set; }

        [Required(ErrorMessage = "Deixe a descrição")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "Digite o valor")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }
        public int CadastroId { get; set; }
        public CadastroModel Cadastro { get; set; }
        [NotMapped]
        public List<SelectListItem> ListaCadastros { get; set; }
        public DateTimeOffset DataCadastro { get; set; }
        public int AgenciaId { get; set; }
        public AgenciaModel Agencia { get; set; }
        [NotMapped]
        public List<SelectListItem> ListaAgencias { get; set; }

        public int UsuarioId { get; set; }
        public UsuarioModel Usuario { get; set; }
        
    }
}
