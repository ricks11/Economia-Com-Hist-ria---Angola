using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EconomiaComHistoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForumEModeracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CategoriasForum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icone = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasForum", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Escolas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodigoMEC = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Provincia = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Localizacao = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escolas", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BadgesConquistados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    BadgeId = table.Column<int>(type: "int", nullable: false),
                    DataConquista = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgesConquistados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BadgesConquistados_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Comentarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Texto = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AutorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarios", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Conteudos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titulo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Resumo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Texto = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataPublicacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AutorId = table.Column<int>(type: "int", nullable: false),
                    Tema = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nivel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Regiao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    ImagemCapa = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UrlMedia = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Visualizacoes = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Origem = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataHistorica = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conteudos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Moderacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ConteudoId = table.Column<int>(type: "int", nullable: true),
                    ComentarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moderacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Moderacoes_Comentarios_ComentarioId",
                        column: x => x.ComentarioId,
                        principalTable: "Comentarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Moderacoes_Conteudos_ConteudoId",
                        column: x => x.ConteudoId,
                        principalTable: "Conteudos",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titulo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NivelDificuldade = table.Column<int>(type: "int", nullable: false),
                    Tema = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroPerguntas = table.Column<int>(type: "int", nullable: false),
                    TempoPorPerguntaSegundos = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ConteudoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quizzes_Conteudos_ConteudoId",
                        column: x => x.ConteudoId,
                        principalTable: "Conteudos",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DecisoesModeracao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Decisao = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observacoes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataDecisao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModeracaoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisoesModeracao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecisoesModeracao_Moderacoes_ModeracaoId",
                        column: x => x.ModeracaoId,
                        principalTable: "Moderacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Perguntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QuizId = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrdemAleatorizada = table.Column<int>(type: "int", nullable: false),
                    TempoLimiteSegundos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perguntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Perguntas_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OpcoesRespostas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PerguntaId = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCorrecta = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Explicacao = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcoesRespostas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpcoesRespostas_Perguntas_PerguntaId",
                        column: x => x.PerguntaId,
                        principalTable: "Perguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConteudosFavoritos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ConteudoId = table.Column<int>(type: "int", nullable: false),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    DataAdicionado = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConteudosFavoritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConteudosFavoritos_Conteudos_ConteudoId",
                        column: x => x.ConteudoId,
                        principalTable: "Conteudos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DenunciasConteudo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DenuncianteId = table.Column<int>(type: "int", nullable: false),
                    TopicoId = table.Column<int>(type: "int", nullable: true),
                    RespostaId = table.Column<int>(type: "int", nullable: true),
                    Motivo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataDenuncia = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ConteudoId = table.Column<int>(type: "int", nullable: true),
                    ComentarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DenunciasConteudo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DenunciasConteudo_Comentarios_ComentarioId",
                        column: x => x.ComentarioId,
                        principalTable: "Comentarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DenunciasConteudo_Conteudos_ConteudoId",
                        column: x => x.ConteudoId,
                        principalTable: "Conteudos",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EventosGamificacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataEvento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosGamificacao", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ModulosProgresso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    Tema = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PercentagemCompleta = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataUltimaAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModulosProgresso", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PlanosEstudo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titulo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosEstudo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rankings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    Pontuacao = table.Column<int>(type: "int", nullable: false),
                    Periodo = table.Column<int>(type: "int", nullable: false),
                    EscolaId = table.Column<int>(type: "int", nullable: true),
                    Provincia = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataSnapshot = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rankings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    TopicoId = table.Column<int>(type: "int", nullable: true),
                    RespostaId = table.Column<int>(type: "int", nullable: true),
                    TipoReacao = table.Column<int>(type: "int", nullable: false),
                    ConteudoId = table.Column<int>(type: "int", nullable: true),
                    ComentarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reacoes_Comentarios_ComentarioId",
                        column: x => x.ComentarioId,
                        principalTable: "Comentarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reacoes_Conteudos_ConteudoId",
                        column: x => x.ConteudoId,
                        principalTable: "Conteudos",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RelatoriosProgresso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Resumo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataGeracao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatoriosProgresso", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RespostasForum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TopicoId = table.Column<int>(type: "int", nullable: false),
                    AutorId = table.Column<int>(type: "int", nullable: false),
                    Conteudo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RespostaPaiId = table.Column<int>(type: "int", nullable: true),
                    EstadoResposta = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataEdicao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespostasForum", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespostasForum_RespostasForum_RespostaPaiId",
                        column: x => x.RespostaPaiId,
                        principalTable: "RespostasForum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RespostasPerguntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TentativaQuizId = table.Column<int>(type: "int", nullable: false),
                    PerguntaId = table.Column<int>(type: "int", nullable: false),
                    OpcaoRespostaId = table.Column<int>(type: "int", nullable: false),
                    TempoRespostaMs = table.Column<int>(type: "int", nullable: false),
                    IsCorrecta = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespostasPerguntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespostasPerguntas_OpcoesRespostas_OpcaoRespostaId",
                        column: x => x.OpcaoRespostaId,
                        principalTable: "OpcoesRespostas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RespostasPerguntas_Perguntas_PerguntaId",
                        column: x => x.PerguntaId,
                        principalTable: "Perguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SessoesEstudo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    Inicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Fim = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DuracaoMinutos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessoesEstudo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TentativasQuiz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    QuizId = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Pontuacao = table.Column<int>(type: "int", nullable: false),
                    Completa = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TentativasQuiz", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TentativasQuiz_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TopicosForum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titulo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Conteudo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AutorId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    EstadoTopico = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalDenuncias = table.Column<int>(type: "int", nullable: false),
                    MotivoRejeicao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicosForum", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicosForum_CategoriasForum_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "CategoriasForum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Turmas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ano = table.Column<int>(type: "int", nullable: true),
                    EscolaId = table.Column<int>(type: "int", nullable: false),
                    ProfessorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turmas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turmas_Escolas_EscolaId",
                        column: x => x.EscolaId,
                        principalTable: "Escolas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Utilizadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telemovel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    DataRegisto = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PontosTotais = table.Column<int>(type: "int", nullable: false),
                    StreakAtual = table.Column<int>(type: "int", nullable: false),
                    SuspensoAte = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SuspensaoPermanente = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Provincia = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EscolaId = table.Column<int>(type: "int", nullable: true),
                    TurmaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilizadores_Escolas_EscolaId",
                        column: x => x.EscolaId,
                        principalTable: "Escolas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Utilizadores_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VisualizacoesConteudo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ConteudoId = table.Column<int>(type: "int", nullable: false),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    DataVisualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisualizacoesConteudo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisualizacoesConteudo_Conteudos_ConteudoId",
                        column: x => x.ConteudoId,
                        principalTable: "Conteudos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisualizacoesConteudo_Utilizadores_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BadgesConquistados_BadgeId",
                table: "BadgesConquistados",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_BadgesConquistados_UtilizadorId",
                table: "BadgesConquistados",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_AutorId",
                table: "Comentarios",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Conteudos_AutorId",
                table: "Conteudos",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_ConteudosFavoritos_ConteudoId_UtilizadorId",
                table: "ConteudosFavoritos",
                columns: new[] { "ConteudoId", "UtilizadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConteudosFavoritos_UtilizadorId",
                table: "ConteudosFavoritos",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_DecisoesModeracao_ModeracaoId",
                table: "DecisoesModeracao",
                column: "ModeracaoId");

            migrationBuilder.CreateIndex(
                name: "IX_DenunciasConteudo_ComentarioId",
                table: "DenunciasConteudo",
                column: "ComentarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DenunciasConteudo_ConteudoId",
                table: "DenunciasConteudo",
                column: "ConteudoId");

            migrationBuilder.CreateIndex(
                name: "IX_DenunciasConteudo_DenuncianteId",
                table: "DenunciasConteudo",
                column: "DenuncianteId");

            migrationBuilder.CreateIndex(
                name: "IX_DenunciasConteudo_RespostaId",
                table: "DenunciasConteudo",
                column: "RespostaId");

            migrationBuilder.CreateIndex(
                name: "IX_DenunciasConteudo_TopicoId",
                table: "DenunciasConteudo",
                column: "TopicoId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosGamificacao_UtilizadorId",
                table: "EventosGamificacao",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Moderacoes_ComentarioId",
                table: "Moderacoes",
                column: "ComentarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Moderacoes_ConteudoId",
                table: "Moderacoes",
                column: "ConteudoId");

            migrationBuilder.CreateIndex(
                name: "IX_ModulosProgresso_UtilizadorId_Tema",
                table: "ModulosProgresso",
                columns: new[] { "UtilizadorId", "Tema" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpcoesRespostas_PerguntaId",
                table: "OpcoesRespostas",
                column: "PerguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_Perguntas_QuizId",
                table: "Perguntas",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosEstudo_UtilizadorId",
                table: "PlanosEstudo",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_ConteudoId",
                table: "Quizzes",
                column: "ConteudoId");

            migrationBuilder.CreateIndex(
                name: "IX_Rankings_UtilizadorId",
                table: "Rankings",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reacoes_ComentarioId",
                table: "Reacoes",
                column: "ComentarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Reacoes_ConteudoId",
                table: "Reacoes",
                column: "ConteudoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reacoes_RespostaId",
                table: "Reacoes",
                column: "RespostaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reacoes_TopicoId",
                table: "Reacoes",
                column: "TopicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reacoes_UtilizadorId_TopicoId_RespostaId_TipoReacao",
                table: "Reacoes",
                columns: new[] { "UtilizadorId", "TopicoId", "RespostaId", "TipoReacao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelatoriosProgresso_UtilizadorId",
                table: "RelatoriosProgresso",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasForum_AutorId",
                table: "RespostasForum",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasForum_EstadoResposta",
                table: "RespostasForum",
                column: "EstadoResposta");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasForum_RespostaPaiId",
                table: "RespostasForum",
                column: "RespostaPaiId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasForum_TopicoId",
                table: "RespostasForum",
                column: "TopicoId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasPerguntas_OpcaoRespostaId",
                table: "RespostasPerguntas",
                column: "OpcaoRespostaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasPerguntas_PerguntaId",
                table: "RespostasPerguntas",
                column: "PerguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasPerguntas_TentativaQuizId",
                table: "RespostasPerguntas",
                column: "TentativaQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_SessoesEstudo_UtilizadorId",
                table: "SessoesEstudo",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_TentativasQuiz_QuizId",
                table: "TentativasQuiz",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_TentativasQuiz_UtilizadorId",
                table: "TentativasQuiz",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicosForum_AutorId",
                table: "TopicosForum",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicosForum_CategoriaId",
                table: "TopicosForum",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicosForum_EstadoTopico",
                table: "TopicosForum",
                column: "EstadoTopico");

            migrationBuilder.CreateIndex(
                name: "IX_Turmas_EscolaId",
                table: "Turmas",
                column: "EscolaId");

            migrationBuilder.CreateIndex(
                name: "IX_Turmas_ProfessorId",
                table: "Turmas",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizadores_Email",
                table: "Utilizadores",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilizadores_EscolaId",
                table: "Utilizadores",
                column: "EscolaId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizadores_TurmaId",
                table: "Utilizadores",
                column: "TurmaId");

            migrationBuilder.CreateIndex(
                name: "IX_VisualizacoesConteudo_ConteudoId_UtilizadorId",
                table: "VisualizacoesConteudo",
                columns: new[] { "ConteudoId", "UtilizadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisualizacoesConteudo_UtilizadorId",
                table: "VisualizacoesConteudo",
                column: "UtilizadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_BadgesConquistados_Utilizadores_UtilizadorId",
                table: "BadgesConquistados",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_Utilizadores_AutorId",
                table: "Comentarios",
                column: "AutorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Conteudos_Utilizadores_AutorId",
                table: "Conteudos",
                column: "AutorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConteudosFavoritos_Utilizadores_UtilizadorId",
                table: "ConteudosFavoritos",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DenunciasConteudo_RespostasForum_RespostaId",
                table: "DenunciasConteudo",
                column: "RespostaId",
                principalTable: "RespostasForum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DenunciasConteudo_TopicosForum_TopicoId",
                table: "DenunciasConteudo",
                column: "TopicoId",
                principalTable: "TopicosForum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DenunciasConteudo_Utilizadores_DenuncianteId",
                table: "DenunciasConteudo",
                column: "DenuncianteId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventosGamificacao_Utilizadores_UtilizadorId",
                table: "EventosGamificacao",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModulosProgresso_Utilizadores_UtilizadorId",
                table: "ModulosProgresso",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanosEstudo_Utilizadores_UtilizadorId",
                table: "PlanosEstudo",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rankings_Utilizadores_UtilizadorId",
                table: "Rankings",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reacoes_RespostasForum_RespostaId",
                table: "Reacoes",
                column: "RespostaId",
                principalTable: "RespostasForum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reacoes_TopicosForum_TopicoId",
                table: "Reacoes",
                column: "TopicoId",
                principalTable: "TopicosForum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reacoes_Utilizadores_UtilizadorId",
                table: "Reacoes",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RelatoriosProgresso_Utilizadores_UtilizadorId",
                table: "RelatoriosProgresso",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RespostasForum_TopicosForum_TopicoId",
                table: "RespostasForum",
                column: "TopicoId",
                principalTable: "TopicosForum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RespostasForum_Utilizadores_AutorId",
                table: "RespostasForum",
                column: "AutorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RespostasPerguntas_TentativasQuiz_TentativaQuizId",
                table: "RespostasPerguntas",
                column: "TentativaQuizId",
                principalTable: "TentativasQuiz",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessoesEstudo_Utilizadores_UtilizadorId",
                table: "SessoesEstudo",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TentativasQuiz_Utilizadores_UtilizadorId",
                table: "TentativasQuiz",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicosForum_Utilizadores_AutorId",
                table: "TopicosForum",
                column: "AutorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Turmas_Utilizadores_ProfessorId",
                table: "Turmas",
                column: "ProfessorId",
                principalTable: "Utilizadores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turmas_Utilizadores_ProfessorId",
                table: "Turmas");

            migrationBuilder.DropTable(
                name: "BadgesConquistados");

            migrationBuilder.DropTable(
                name: "ConteudosFavoritos");

            migrationBuilder.DropTable(
                name: "DecisoesModeracao");

            migrationBuilder.DropTable(
                name: "DenunciasConteudo");

            migrationBuilder.DropTable(
                name: "EventosGamificacao");

            migrationBuilder.DropTable(
                name: "ModulosProgresso");

            migrationBuilder.DropTable(
                name: "PlanosEstudo");

            migrationBuilder.DropTable(
                name: "Rankings");

            migrationBuilder.DropTable(
                name: "Reacoes");

            migrationBuilder.DropTable(
                name: "RelatoriosProgresso");

            migrationBuilder.DropTable(
                name: "RespostasPerguntas");

            migrationBuilder.DropTable(
                name: "SessoesEstudo");

            migrationBuilder.DropTable(
                name: "VisualizacoesConteudo");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropTable(
                name: "Moderacoes");

            migrationBuilder.DropTable(
                name: "RespostasForum");

            migrationBuilder.DropTable(
                name: "OpcoesRespostas");

            migrationBuilder.DropTable(
                name: "TentativasQuiz");

            migrationBuilder.DropTable(
                name: "Comentarios");

            migrationBuilder.DropTable(
                name: "TopicosForum");

            migrationBuilder.DropTable(
                name: "Perguntas");

            migrationBuilder.DropTable(
                name: "CategoriasForum");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "Conteudos");

            migrationBuilder.DropTable(
                name: "Utilizadores");

            migrationBuilder.DropTable(
                name: "Turmas");

            migrationBuilder.DropTable(
                name: "Escolas");
        }
    }
}
