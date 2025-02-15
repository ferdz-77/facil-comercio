using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace FacilComercio.Models
{
    public class CarrinhoVenda
    {
        public List<VendaItem> Itens { get; set; } = new List<VendaItem>(); // Lista de itens no carrinho
        [JsonPropertyName("descontoTotal")] // Nome no JSON
        public decimal DescontoTotal { get; set; }         
        
        public int? ClienteId { get; set; } // ClienteId que pode ser null
        public string? ClienteNome { get; set; } // Nome do cliente, se disponível
        //public decimal Total => Itens.Sum(item => item.SubTotal); // Total calculado automaticamente
        // Total calculado automaticamente, com o desconto
        public decimal Total => Itens.Sum(item => item.SubTotal) - DescontoTotal;


        // Lista de pagamentos realizados no carrinho
        public List<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();

        // Adicionar um item ao carrinho
        public void AdicionarItem(VendaItem item)
        {
            var itemExistente = Itens.FirstOrDefault(i => i.ProdutoId == item.ProdutoId);
            if (itemExistente != null)
            {
                itemExistente.Quantidade += item.Quantidade;
            }
            else
            {
                Itens.Add(item);
            }
        }

        // Remover um item do carrinho
        public void RemoverItem(int produtoId)
        {
            var item = Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
            if (item != null)
            {
                Itens.Remove(item);
            }
        }

        // Limpar o carrinho
        public void LimparCarrinho()
        {
            Itens.Clear();
            DescontoTotal = 0; // Limpar o desconto ao limpar o carrinho
            ClienteId = null; // Limpar o cliente
            ClienteNome = null; // Limpar o nome do cliente
        }


        // Aplicar um desconto ao carrinho
        public void AplicarDesconto(decimal desconto)
        {
            // Você pode adicionar lógica aqui para verificar se o desconto é válido
            DescontoTotal = desconto;
        }

    }
}
