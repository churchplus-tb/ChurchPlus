using Analise.Enuns;
using System.ComponentModel.DataAnnotations;

namespace Analise.Models
{
    public class EntradaViewModel
    {
        public EntradaModel EntradaNome { get; set; }

        public List<EntradaModel> ListaEntradas { get; set; } = new List<EntradaModel>();

        // Adicione estas listas para os agrupamentos
        public List<EntradaModel> EntradasPorAgencia { get; set; }
        public List<EntradaModel> EntradasPorTipo { get; set; }
        public List<EntradaModel> EntradasPorLinha { get; set; }
        public List<EntradaModel> EntradasPorConta { get; set; }
    }
}
