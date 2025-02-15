using Microsoft.EntityFrameworkCore;
using FacilComercio.Models;
using System.Collections.Generic;
using FacilComercio.Models.DTOs;


namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Loja> Lojas { get; set; }
        public DbSet<UsuarioLoja> UsuarioLojas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<LogSistema> LogSistema { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<ItemVenda> ItensVenda { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
        public DbSet<UsuarioListagemDto> UsuarioListagemDtos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<LotesProximosValidadeViewModel> LotesProximosValidadeViewModel { get; set; }
        public DbSet<RelatorioVendasViewModel> RelatorioVendasViewModel { get; set; }
        public DbSet<RelatorioVendasVendedorViewModel> RelatorioVendasVendedorViewModel { get; set; }

        public DbSet<MovimentacaoEstoqueViewModel> MovimentacaoEstoqueViewModel { get; set; }

        public DbSet<ProdutosProximosValidadeViewModel> ProdutosProximosValidadeViewModel { get; set; }
        public DbSet<ItemVenda> ItemVenda { get; set; }
        public DbSet<PagamentosVenda> PagamentosVenda { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ViewModel sem chave primária
            modelBuilder.Entity<LotesProximosValidadeViewModel>().HasNoKey();

            modelBuilder.Entity<EstoqueBaixoViewModel>().HasNoKey();

            // Registrar o RelatorioVendasViewModel
            modelBuilder.Entity<RelatorioVendasViewModel>().HasNoKey();

            modelBuilder.Entity<RelatorioVendasVendedorViewModel>().HasNoKey();

            modelBuilder.Entity<MovimentacaoEstoqueViewModel>().HasNoKey();

            modelBuilder.Entity<ProdutosProximosValidadeViewModel>().HasNoKey();

            // DTO não possui chave primária
            modelBuilder.Entity<UsuarioListagemDto>().HasNoKey();


            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PagamentosVenda>()
                .HasOne(pv => pv.MeioPagamento)
                .WithMany()
                .HasForeignKey(pv => pv.Mpa_Id);

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categoria"); // Nome exato da tabela no banco
                entity.HasKey(c => c.Cat_Id); // Chave primária
                entity.Property(c => c.Loj_Id).IsRequired(false); // Tornar Loj_Id opcional
                entity.Property(c => c.Cat_Titulo).HasMaxLength(255).IsRequired();
                entity.Property(c => c.Cat_Descricao).HasMaxLength(255).IsRequired(false);
                entity.Property(c => c.Cat_CriadoEm).HasDefaultValueSql("GETDATE()");
                entity.Property(c => c.Cat_Status).IsRequired();

                // Relacionamento de navegação entre Categoria e Loja
                entity.HasOne(c => c.Loja)
                    .WithMany()
                    .HasForeignKey(c => c.Loj_Id)
                    .OnDelete(DeleteBehavior.Restrict); // Relacionamento opcional
            });

            // Configuração para a entidade Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Cliente");
                entity.HasKey(e => e.Cli_Id);
                entity.Property(e => e.Cli_Nome).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Cli_Email).HasMaxLength(100);
                entity.Property(e => e.Cli_Telefone).HasMaxLength(20);
                entity.Property(e => e.Cli_Endereco).HasMaxLength(200);

                // Chave estrangeira opcional para Empresa
                entity.HasOne(e => e.Empresa)
                      .WithMany()
                      .HasForeignKey(e => e.Emp_Id)
                      .IsRequired(false);
            });

            // Configuração para a entidade Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(u => u.Usu_Id);
                entity.Property(u => u.Usu_Nome).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Usu_Email).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Usu_SenhaHash).HasMaxLength(200).IsRequired();
                //entity.Property(u => u.Usu_Telefone).HasMaxLength(15).IsRequired(false);
                //entity.Property(u => u.Usu_Status).IsRequired().HasDefaultValue(true);
                //entity.Property(u => u.Usu_CPF).HasMaxLength(14).IsRequired(false);
                //entity.Property(u => u.Usu_Foto).HasMaxLength(500).HasDefaultValue("");
            });

            // Configuração para a entidade Empresa
            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CNPJ).HasMaxLength(18).IsRequired();
                entity.Property(e => e.Nome).HasMaxLength(100);
                entity.Property(e => e.Endereco).HasMaxLength(200);
                entity.Property(e => e.Contato).HasMaxLength(50);
                entity.Property(e => e.CriadoEm).HasDefaultValueSql("GETDATE()");
            });

            // Configuração para a entidade Loja
            modelBuilder.Entity<Loja>(entity =>
            {
                entity.ToTable("Loja"); // Certifique-se de que este nome é o mesmo no banco de dados

                entity.HasKey(l => l.Loj_Id);
                entity.Property(l => l.Loj_Nome).HasMaxLength(100).IsRequired();
                entity.Property(l => l.Loj_Endereco).HasMaxLength(200);
                entity.Property(l => l.Loj_ContatoEm).HasMaxLength(50);
                entity.Property(l => l.Loj_Criado).HasDefaultValueSql("GETDATE()");

                // Relacionamento com Empresa e Usuario
                entity.HasOne(l => l.Empresa)
                      .WithMany()
                      .HasForeignKey(l => l.Loj_EmpresaId)
                      .IsRequired();

                entity.HasOne(l => l.UsuarioCriador)
                      .WithMany()
                      .HasForeignKey(l => l.Loj_CriadoPor)
                      .IsRequired(false);
            });

            // Configuração para a entidade UsuarioLoja
            modelBuilder.Entity<UsuarioLoja>(entity =>
            {
                entity.ToTable("UsuarioLoja");

                // Define a chave primária composta
                entity.HasKey(ul => new { ul.Usu_Id, ul.Loj_Id });

                // Mapeamento explícito das propriedades para as colunas corretas
                entity.Property(ul => ul.Usu_Id)
                      .HasColumnName("Usu_Id"); // Nome da coluna no banco de dados

                entity.Property(ul => ul.Loj_Id)
                      .HasColumnName("Loj_Id"); // Força o mapeamento para a coluna correta

                entity.Property(ul => ul.Permissao)
                      .HasColumnName("Permissao") // Nome explícito da coluna
                      .HasMaxLength(50);

                // Relacionamento com Usuario
                entity.HasOne(ul => ul.Usuario)
                      .WithMany(u => u.UsuarioLojas) // Muitos UsuarioLoja para um Usuario
                      .HasForeignKey(ul => ul.Usu_Id)
                      .OnDelete(DeleteBehavior.Cascade); // Define o comportamento de exclusão em cascata

                // Relacionamento com Loja
                entity.HasOne(ul => ul.Loja)
                      .WithMany(l => l.UsuarioLojas) // Muitos UsuarioLoja para uma Loja
                      .HasForeignKey(ul => ul.Loj_Id)
                      .OnDelete(DeleteBehavior.Cascade); // Define o comportamento de exclusão em cascata
            });


            // Configuração para a entidade LogSistema
            modelBuilder.Entity<LogSistema>(entity =>
            {
                entity.HasKey(log => log.Id);
                entity.Property(log => log.Acao).HasMaxLength(100).IsRequired();
                entity.Property(log => log.Entidade).HasMaxLength(100);
                entity.Property(log => log.IP).HasMaxLength(50);
                entity.Property(log => log.DataHora).HasDefaultValueSql("GETDATE()");

                // Relacionamento com Usuario
                entity.HasOne(log => log.Usuario)
                      .WithMany()
                      .HasForeignKey(log => log.Usu_Id)
                      .IsRequired(false);
            });




            // Configuração para a entidade Produto
            modelBuilder.Entity<Produto>(entity =>
            {
                entity.ToTable("Produto"); // Define o nome da tabela como Produto
                entity.HasKey(p => p.Prod_Id); // Corrigido para refletir a propriedade correta

                // Mapeamento das propriedades
                entity.Property(p => p.Loj_Id).HasColumnName("Loj_Id").IsRequired(); // Relacionamento com a loja
                entity.Property(p => p.Prod_Nome).HasMaxLength(100).IsRequired(); // Nome do produto
                entity.Property(p => p.Prod_Descricao).HasMaxLength(255); // Descrição opcional
                entity.Property(p => p.Prod_Preco).HasColumnType("decimal(10,2)").IsRequired(); // Preço
                entity.Property(p => p.Prod_Estoque).IsRequired(); // Estoque obrigatório
                entity.Property(p => p.Prod_ImagemUrl).HasMaxLength(255); // URL da imagem
                entity.Property(p => p.Prod_CriadoEm).HasDefaultValueSql("GETDATE()"); // Data de criação com valor padrão
                entity.Property(p => p.Prod_EstoqueMinimo).HasDefaultValue(0); // Estoque mínimo com valor padrão
                entity.Property(p => p.Prod_UnidadeMedida).HasMaxLength(50); // Unidade de medida
                entity.Property(p => p.Prod_EAN).HasMaxLength(13); // Código de barras
                entity.Property(p => p.Prod_NCM).HasMaxLength(10); // NCM
                entity.Property(p => p.Prod_CFOP).HasMaxLength(10); // CFOP
                entity.Property(p => p.Prod_Status).HasDefaultValue(true); // Status com valor padrão

                // Configuração da FK
                entity.HasOne(p => p.Loja)
                      .WithMany(l => l.Produtos) // Relacionamento com a Loja
                      .HasForeignKey(p => p.Loj_Id) // Define explicitamente a FK
                      .OnDelete(DeleteBehavior.Restrict); // Sem exclusão em cascata

                // Configuração do relacionamento com a tabela Loja
                entity.HasOne(p => p.Loja)
                      .WithMany()
                      .HasForeignKey(p => p.Loj_Id)
                      .OnDelete(DeleteBehavior.Restrict); // Define DeleteBehavior como Restrict para evitar exclusão em cascata
            });

            // Configuração para a entidade Venda
            modelBuilder.Entity<Venda>(entity =>
            {
                entity.ToTable("Venda");

                // Configuração da chave primária
                entity.HasKey(v => v.Ven_Id);

                // Propriedades da entidade
                entity.Property(v => v.Data)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(v => v.Total)
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                // Relacionamento com Loja
                entity.HasOne(v => v.Loja)
                      .WithMany() // Uma loja pode ter várias vendas
                      .HasForeignKey(v => v.Loj_Id) // Chave estrangeira
                      .IsRequired();

                // Relacionamento com Usuario
                entity.HasOne(v => v.Usuario)
                      .WithMany() // Um usuário pode ter várias vendas
                      .HasForeignKey(v => v.UsuarioId) // Chave estrangeira
                      .IsRequired(false); // Permite nulo

                // Relacionamento com Cliente
                entity.HasOne(v => v.Cliente)
                      .WithMany() // Um cliente pode ter várias vendas
                      .HasForeignKey(v => v.CliId) // Chave estrangeira
                      .IsRequired(false); // Permite nulo
            });


            // Configuração para a entidade ItemVenda
            modelBuilder.Entity<ItemVenda>(entity =>
            {
                entity.ToTable("ItemVenda"); // Nome da tabela no banco de dados
                entity.HasKey(iv => iv.Id);
                entity.Property(iv => iv.VendaId).HasColumnName("Ven_Id");
                entity.Property(iv => iv.ProdutoId).HasColumnName("Prod_Id");
                entity.Property(e => e.Quantidade).HasColumnName("Ite_Quantidade");
               // entity.Property(iv => iv.PrecoUnitario).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(iv => iv.PrecoUnitario).HasColumnName("Ite_PrecoUnitario").HasColumnType("decimal(10,2)").IsRequired(); ;

                // Relacionamento com Venda e Produto
                entity.HasOne(iv => iv.Venda)
                      .WithMany(v => v.ItensVenda) // Supondo que Venda tenha a propriedade ItensVenda
                      .HasForeignKey(iv => iv.VendaId) // Usa VendaId como chave estrangeira
                      .IsRequired();

                entity.HasOne(iv => iv.Produto)
                      .WithMany() // Produto pode não precisar ter uma coleção de ItemVenda
                      .HasForeignKey(iv => iv.ProdutoId) // Usa ProdutoId como chave estrangeira
                      .IsRequired();
            });

            // Configuração para a entidade MovimentacaoEstoque
            modelBuilder.Entity<MovimentacaoEstoque>(entity =>
            {
                entity.HasKey(me => me.Id);
                entity.Property(me => me.Quantidade).IsRequired();
                entity.Property(me => me.TipoMov).HasMaxLength(50).IsRequired();
                entity.Property(me => me.DataMov).HasDefaultValueSql("GETDATE()");

                // Relacionamento com Produto e Loja
                entity.HasOne(me => me.Produto)
                      .WithMany()
                      .HasForeignKey(me => me.Id)
                      .IsRequired();
                entity.HasOne(me => me.Loja)
                      .WithMany()
                      .HasForeignKey(me => me.Id)
                      .IsRequired();
            });
        }
    }
}