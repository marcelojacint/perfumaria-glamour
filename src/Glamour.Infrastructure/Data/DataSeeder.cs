using Glamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Glamour.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedDadosDemoAsync(GlamourDbContext db)
    {
        if (await db.Categorias.AnyAsync()) return;

        var feminino = new Categoria("Feminino", "feminino", 1);
        var masculino = new Categoria("Masculino", "masculino", 2);
        var unissex = new Categoria("Unissex", "unissex", 3);
        var kits = new Categoria("Kits e Presentes", "kits-presentes", 4);

        await db.Categorias.AddRangeAsync(feminino, masculino, unissex, kits);
        await db.SaveChangesAsync();

        var produtos = new List<Produto>
        {
            CriarProduto("La Vie Est Belle", "la-vie-est-belle", feminino.Id,
                "Lancôme", "100ml", "Feminino",
                "Um perfume floral-gourmand que celebra a felicidade. Notas de íris, patchouli e baunilha.",
                389.90m, 299.90m, 15, true,
                "https://images.unsplash.com/photo-1541643600914-78b084683702?w=600&q=80"),

            CriarProduto("Black Opium", "black-opium", feminino.Id,
                "Yves Saint Laurent", "90ml", "Feminino",
                "Intenso e sedutor. Notas de café negro, baunilha branca e jasmim branco.",
                459.90m, null, 8, true,
                "https://images.unsplash.com/photo-1592945403244-b3fbafd7f539?w=600&q=80"),

            CriarProduto("Coco Mademoiselle", "coco-mademoiselle", feminino.Id,
                "Chanel", "100ml", "Feminino",
                "Elegância atemporal com notas de laranja bergamota, rosa e patchouli.",
                620.00m, 549.90m, 5, false,
                "https://images.unsplash.com/photo-1523293182086-7651a899d37f?w=600&q=80"),

            CriarProduto("Bombshell", "bombshell", feminino.Id,
                "Victoria's Secret", "100ml", "Feminino",
                "Floral e frutado. Peônia, flor de maracujá e orquídea se encontram neste irresistível.",
                199.90m, 159.90m, 20, false,
                "https://images.unsplash.com/photo-1588776814546-daab30f310ce?w=600&q=80"),

            CriarProduto("Bleu de Chanel", "bleu-de-chanel", masculino.Id,
                "Chanel", "100ml", "Masculino",
                "Frescor mediterrâneo com notas de limão siciliano, gengibre e cedro.",
                630.00m, null, 10, true,
                "https://images.unsplash.com/photo-1610461888750-10bfc601b4a6?w=600&q=80"),

            CriarProduto("Sauvage", "sauvage", masculino.Id,
                "Dior", "100ml", "Masculino",
                "Selvagem e nobre. Bergamota de Calabria e ambroxano em harmonia perfeita.",
                580.00m, 499.90m, 12, true,
                "https://images.unsplash.com/photo-1563170351-be82bc888aa4?w=600&q=80"),

            CriarProduto("Acqua di Gio", "acqua-di-gio", masculino.Id,
                "Giorgio Armani", "100ml", "Masculino",
                "Inspirado no mar da Calábria. Fresco, aquático e absolutamente mediterrâneo.",
                430.00m, 369.90m, 18, false,
                "https://images.unsplash.com/photo-1587017539504-67cfbddac569?w=600&q=80"),

            CriarProduto("Invictus", "invictus", masculino.Id,
                "Paco Rabanne", "100ml", "Masculino",
                "Audacioso como um campeão. Toranja, néroli marinho e guáiaco.",
                399.90m, null, 7, false,
                "https://images.unsplash.com/photo-1595425959377-a15c3e0b7b7f?w=600&q=80"),

            CriarProduto("Oud Wood", "oud-wood", unissex.Id,
                "Tom Ford", "50ml", "Unissex",
                "A madeira rara oud encontra cardamomo e sândalo em uma composição única.",
                890.00m, null, 4, true,
                "https://images.unsplash.com/photo-1594736797933-d0401ba2fe65?w=600&q=80"),

            CriarProduto("Libre", "libre", unissex.Id,
                "Yves Saint Laurent", "90ml", "Unissex",
                "Liberdade em forma de fragrância. Lavanda, açafrão e cedro.",
                499.90m, 439.90m, 9, true,
                "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=600&q=80"),

            CriarProduto("Kit Presente Luxury", "kit-presente-luxury", kits.Id,
                "Glamour Selection", "Variado", "Unissex",
                "Kit presente especial com 3 miniaturas exclusivas + nécessaire de couro. Ideal para presentear.",
                299.90m, 249.90m, 25, true,
                "https://images.unsplash.com/photo-1549187774-b4e9b0445b41?w=600&q=80"),
        };

        await db.Produtos.AddRangeAsync(produtos);
        await db.SaveChangesAsync();
    }

    private static Produto CriarProduto(string nome, string slug, Guid categoriaId,
        string marca, string volume, string genero, string descricao,
        decimal preco, decimal? precoPromo, int estoque, bool destaque, string urlImagem)
    {
        var p = new Produto(nome, slug, descricao, preco, estoque, categoriaId, marca, volume, genero);
        p.Atualizar(nome, slug, descricao, preco, precoPromo, categoriaId, marca, volume, genero, destaque);
        p.AdicionarImagem(new ProdutoImagem(p.Id, urlImagem, 1, true));
        return p;
    }
}
