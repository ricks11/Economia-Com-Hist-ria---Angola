using EconomiaComHistoria.Core.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace EconomiaComHistoria.API.Swagger;

public class RankingEntradaDtoExample : IExamplesProvider<RankingEntradaDto>
{
    public RankingEntradaDto GetExamples()
    {
        return new RankingEntradaDto(
            Posicao: 1,
            UtilizadorId: 42,
            NomeUtilizador: "Maria Silva",
            Pontos: 8750,
            QuizzesCompletados: 28,
            EscolaNome: "Colégio Público de Luanda"
        );
    }
}

public class CriarTopicoForumDtoExample : IExamplesProvider<CriarTopicoForumDto>
{
    public CriarTopicoForumDto GetExamples()
    {
        return new CriarTopicoForumDto
        {
            Titulo = "Como estudar História Econômica?",
            Descricao = "Estou com dificuldade em entender os conceitos de economia de mercado. Alguém pode ajudar com recursos ou dicas?",
            CategoriaId = 3
        };
    }
}

public class CriarRespostaForumDtoExample : IExamplesProvider<CriarRespostaForumDto>
{
    public CriarRespostaForumDto GetExamples()
    {
        return new CriarRespostaForumDto
        {
            Conteudo = "Recomendo assistir à série de vídeos sobre economia de mercado no módulo de História Econômica. Os exemplos práticos ajudaram muito!",
            RespostaPaiId = null
        };
    }
}
