using EconomiaComHistoria.Core.Entities;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<ConteudoJindungo> ConteudosJindungo => Set<ConteudoJindungo>();
    public DbSet<VisualizacaoConteudo> VisualizacoesConteudo => Set<VisualizacaoConteudo>();
    public DbSet<ConteudoFavorito> ConteudosFavoritos => Set<ConteudoFavorito>();
    public DbSet<ModuloProgresso> ModulosProgresso => Set<ModuloProgresso>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Pergunta> Perguntas => Set<Pergunta>();
    public DbSet<OpcaoResposta> OpcoesRespostas => Set<OpcaoResposta>();
    public DbSet<TentativaQuiz> TentativasQuiz => Set<TentativaQuiz>();
    public DbSet<RespostaPergunta> RespostasPerguntas => Set<RespostaPergunta>();
    public DbSet<Ranking> Rankings => Set<Ranking>();
    public DbSet<TopicoForum> TopicosForum => Set<TopicoForum>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<RespostaForum> RespostasForum => Set<RespostaForum>();
    public DbSet<Reacao> Reacoes => Set<Reacao>();
    public DbSet<DenunciaConteudo> DenunciasConteudo => Set<DenunciaConteudo>();
    public DbSet<Moderacao> Moderacoes => Set<Moderacao>();
    public DbSet<DecisaoModeracao> DecisoesModeracao => Set<DecisaoModeracao>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<BadgeConquistado> BadgesConquistados => Set<BadgeConquistado>();
    public DbSet<EventoGamificacao> EventosGamificacao => Set<EventoGamificacao>();
    public DbSet<SessaoEstudo> SessoesEstudo => Set<SessaoEstudo>();
    public DbSet<PlanoEstudo> PlanosEstudo => Set<PlanoEstudo>();
    public DbSet<RelatorioProgresso> RelatoriosProgresso => Set<RelatorioProgresso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Utilizador>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Telemovel).HasMaxLength(50);
            entity.Property(x => x.Provincia).HasMaxLength(100);
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasOne(x => x.Escola)
                .WithMany(x => x.Utilizadores)
                .HasForeignKey(x => x.EscolaId);
            entity.HasOne(x => x.Turma)
                .WithMany()
                .HasForeignKey(x => x.TurmaId);
        });

        modelBuilder.Entity<Escola>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CodigoMEC).HasMaxLength(20);
            entity.Property(x => x.Provincia).HasMaxLength(100);
            entity.Property(x => x.Localizacao).HasMaxLength(200);
        });

        modelBuilder.Entity<Turma>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Ano);
            entity.HasOne(x => x.Escola)
                .WithMany(x => x.Turmas)
                .HasForeignKey(x => x.EscolaId);
            entity.HasOne(x => x.Professor)
                .WithMany()
                .HasForeignKey(x => x.ProfessorId);
        });

        modelBuilder.Entity<Conteudo>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Resumo).HasMaxLength(500);
            entity.Property(x => x.Texto).HasColumnType("longtext");
            entity.Property(x => x.Tema).HasMaxLength(100);
            entity.Property(x => x.Nivel).HasMaxLength(50);
            entity.Property(x => x.Regiao).HasMaxLength(100);
            entity.Property(x => x.Tipo).HasMaxLength(50);
            entity.Property(x => x.ImagemCapa).HasMaxLength(500);
            entity.Property(x => x.UrlMedia).HasMaxLength(500);
            entity.HasOne(x => x.Autor)
                .WithMany(x => x.Conteudos)
                .HasForeignKey(x => x.AutorId);
        });

        modelBuilder.Entity<VisualizacaoConteudo>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Conteudo)
                .WithMany(x => x.Visualizacoes_Rastreamento)
                .HasForeignKey(x => x.ConteudoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Utilizador)
                .WithMany()
                .HasForeignKey(x => x.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ConteudoId, x.UtilizadorId }).IsUnique();
        });

        modelBuilder.Entity<ConteudoFavorito>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Conteudo)
                .WithMany(x => x.Favoritos)
                .HasForeignKey(x => x.ConteudoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Utilizador)
                .WithMany()
                .HasForeignKey(x => x.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ConteudoId, x.UtilizadorId }).IsUnique();
        });

        modelBuilder.Entity<ConteudoJindungo>(entity =>
        {
            entity.Property(x => x.Origem).HasMaxLength(200);
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Tema).HasMaxLength(100);
        });

        modelBuilder.Entity<Pergunta>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Texto).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.Quiz)
                .WithMany(x => x.Perguntas)
                .HasForeignKey(x => x.QuizId);
        });

        modelBuilder.Entity<OpcaoResposta>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Texto).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Explicacao).HasMaxLength(1000);
            entity.HasOne(x => x.Pergunta)
                .WithMany(x => x.Opcoes)
                .HasForeignKey(x => x.PerguntaId);
        });

        modelBuilder.Entity<TentativaQuiz>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Utilizador)
                .WithMany(x => x.TentativasQuiz)
                .HasForeignKey(x => x.UtilizadorId);
            entity.HasOne(x => x.Quiz)
                .WithMany(x => x.Tentativas)
                .HasForeignKey(x => x.QuizId);
        });

        modelBuilder.Entity<RespostaPergunta>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Pergunta)
                .WithMany(x => x.Respostas)
                .HasForeignKey(x => x.PerguntaId);
            entity.HasOne(x => x.TentativaQuiz)
                .WithMany(x => x.Respostas)
                .HasForeignKey(x => x.TentativaQuizId);
            entity.HasOne(x => x.OpcaoResposta)
                .WithMany()
                .HasForeignKey(x => x.OpcaoRespostaId);
        });

        modelBuilder.Entity<Ranking>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Provincia).HasMaxLength(100);
            entity.HasOne(x => x.Utilizador)
                .WithMany()
                .HasForeignKey(x => x.UtilizadorId);
        });

        modelBuilder.Entity<TopicoForum>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Conteudo).HasColumnType("longtext");
            entity.HasOne(x => x.Autor)
                .WithMany(x => x.Topicos)
                .HasForeignKey(x => x.AutorId);
        });

        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Texto).HasMaxLength(1000).IsRequired();
            entity.HasOne(x => x.Autor)
                .WithMany(x => x.Comentarios)
                .HasForeignKey(x => x.AutorId);
        });

        modelBuilder.Entity<RespostaForum>(entity =>
        {
            entity.Property(x => x.Texto).HasMaxLength(1000).IsRequired();
            entity.HasOne(x => x.Comentario)
                .WithMany(x => x.Respostas)
                .HasForeignKey(x => x.ComentarioId);
        });

        modelBuilder.Entity<Reacao>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Tipo).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.Utilizador)
                .WithMany(x => x.Reacoes)
                .HasForeignKey(x => x.UtilizadorId);
        });

        modelBuilder.Entity<DenunciaConteudo>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Motivo).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<Moderacao>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<DecisaoModeracao>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Decisao).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Moderacao)
                .WithMany(x => x.Decisoes)
                .HasForeignKey(x => x.ModeracaoId);
        });

        modelBuilder.Entity<Badge>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<BadgeConquistado>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Utilizador)
                .WithMany(x => x.BadgesConquistados)
                .HasForeignKey(x => x.UtilizadorId);
            entity.HasOne(x => x.Badge)
                .WithMany(x => x.BadgesConquistados)
                .HasForeignKey(x => x.BadgeId);
        });

        modelBuilder.Entity<EventoGamificacao>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Descricao).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.Utilizador)
                .WithMany(x => x.EventosGamificacao)
                .HasForeignKey(x => x.UtilizadorId);
        });

        modelBuilder.Entity<SessaoEstudo>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Utilizador)
                .WithMany(x => x.SessoesEstudo)
                .HasForeignKey(x => x.UtilizadorId);
        });

        modelBuilder.Entity<PlanoEstudo>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Utilizador)
                .WithMany(x => x.PlanosEstudo)
                .HasForeignKey(x => x.UtilizadorId);
        });

        modelBuilder.Entity<RelatorioProgresso>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Resumo).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.Utilizador)
                .WithMany(x => x.RelatoriosProgresso)
                .HasForeignKey(x => x.UtilizadorId);
        });

        modelBuilder.Entity<ModuloProgresso>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Tema).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PercentagemCompleta).HasPrecision(5, 2);
            entity.HasOne(x => x.Utilizador)
                .WithMany()
                .HasForeignKey(x => x.UtilizadorId);
            entity.HasIndex(x => new { x.UtilizadorId, x.Tema }).IsUnique();
        });
    }
}
