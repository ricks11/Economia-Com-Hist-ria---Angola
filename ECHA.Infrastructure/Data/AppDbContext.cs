using Microsoft.EntityFrameworkCore;
using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Utilizador> Utilizadores => Set<Utilizador>();
    public DbSet<Escola> Escolas => Set<Escola>();
    public DbSet<Turma> Turmas => Set<Turma>();
    public DbSet<Conteudo> Conteudos => Set<Conteudo>();
    public DbSet<VersaoConteudo> VersoesConteudo => Set<VersaoConteudo>();
    public DbSet<ConteudoTraducao> TraducoesConteudo => Set<ConteudoTraducao>();
    public DbSet<ConteudoFavorito> Favoritos => Set<ConteudoFavorito>();
    public DbSet<OpcaoResposta> OpcoesResposta => Set<OpcaoResposta>();
    public DbSet<RespostaForum> RespostasForum => Set<RespostaForum>();
    public DbSet<CategoriaForum> CategoriasForum => Set<CategoriaForum>();
    public DbSet<VisualizacaoConteudo> Visualizacoes => Set<VisualizacaoConteudo>();
    public DbSet<SessaoEstudo> SessoesEstudo => Set<SessaoEstudo>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Pergunta> Perguntas => Set<Pergunta>();
    public DbSet<TentativaQuiz> TentativasQuiz => Set<TentativaQuiz>();
    public DbSet<RespostaPergunta> RespostasPerguntas => Set<RespostaPergunta>();
    public DbSet<Ranking> Rankings => Set<Ranking>();
    public DbSet<EntradaRanking> EntradasRanking => Set<EntradaRanking>();
    public DbSet<TopicoForum> TopicosForum => Set<TopicoForum>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Reacao> Reacoes => Set<Reacao>();
    public DbSet<DenunciaConteudo> Denuncias => Set<DenunciaConteudo>();
    public DbSet<Moderacao> Moderacoes => Set<Moderacao>();
    public DbSet<DecisaoModeracao> DecisoesModeração => Set<DecisaoModeracao>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<BadgeConquistado> BadgesConquistados => Set<BadgeConquistado>();
    public DbSet<EventoGamificacao> EventosGamificacao => Set<EventoGamificacao>();
    public DbSet<PlanoEstudo> PlanosEstudo => Set<PlanoEstudo>();
    public DbSet<ModuloProgresso> ModulosProgresso => Set<ModuloProgresso>();
    public DbSet<RelatorioProgresso> Relatorios => Set<RelatorioProgresso>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();
    public DbSet<MetricaDashboard> MetricasDashboard => Set<MetricaDashboard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Utilizador — auto-referência na Turma (professor != aluno)
        modelBuilder.Entity<Turma>()
            .HasOne(t => t.Professor)
            .WithMany()
            .HasForeignKey(t => t.ProfessorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Comentario — auto-referência (respostas aninhadas)
        modelBuilder.Entity<Comentario>()
            .HasOne(c => c.ComentarioPai)
            .WithMany(c => c.Respostas)
            .HasForeignKey(c => c.ComentarioPaiId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RespostaForum>()
            .HasOne(r => r.RespostaPai)
            .WithMany(r => r.RespostasFilhas)
            .HasForeignKey(r => r.RespostaPaiId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reacao>()
            .HasOne(r => r.TopicoForum)
            .WithMany(t => t.Reacoes)
            .HasForeignKey(r => r.TopicoForumId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reacao>()
            .HasOne(r => r.RespostaForum)
            .WithMany(r => r.Reacoes)
            .HasForeignKey(r => r.RespostaForumId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DenunciaConteudo>()
            .HasOne(d => d.TopicoForum)
            .WithMany(t => t.Denuncias)
            .HasForeignKey(d => d.TopicoForumId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DenunciaConteudo>()
            .HasOne(d => d.RespostaForum)
            .WithMany(r => r.Denuncias)
            .HasForeignKey(d => d.RespostaForumId)
            .OnDelete(DeleteBehavior.SetNull);

        // AuditoriaLog — append only, sem cascade delete
        modelBuilder.Entity<AuditoriaLog>()
            .ToTable("AuditoriaLogs", t => t.HasCheckConstraint("CK_AuditoriaLog_Immutable", "1=1"));

        // PlanoEstudo — relação many-to-many com Conteudo via tabela de junção
        modelBuilder.Entity<PlanoEstudo>()
            .HasMany(p => p.ConteudosSugeridos)
            .WithMany()
            .UsingEntity("PlanoEstudoConteudo");

        // Índices de performance
        modelBuilder.Entity<TentativaQuiz>()
            .HasIndex(t => new { t.UtilizadorId, t.QuizId, t.DataHora });
        modelBuilder.Entity<EntradaRanking>()
            .HasIndex(e => new { e.RankingId, e.Posicao });
        modelBuilder.Entity<AuditoriaLog>()
            .HasIndex(a => new { a.UtilizadorId, a.Timestamp });
        modelBuilder.Entity<VisualizacaoConteudo>()
            .HasIndex(v => new { v.UtilizadorId, v.ConteudoId });
    }
}
