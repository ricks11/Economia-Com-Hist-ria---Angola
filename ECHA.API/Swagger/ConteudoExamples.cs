using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace EconomiaComHistoria.API.Swagger;

public class CreateConteudoDtoExample : IExamplesProvider<CreateConteudoDto>
{
    public CreateConteudoDto GetExamples()
    {
        return new CreateConteudoDto
        {
            Titulo = "A Economia Colonial Angolana",
            Resumo = "Análise da estrutura econômica durante o período colonial",
            CorpoTexto = "O período colonial português em Angola teve grande impacto na estrutura econômica do país...",
            VideoUrl = "https://example.com/videos/economia-colonial.mp4",
            AudioUrl = "https://example.com/audios/economia-colonial.mp3",
            ThumbnailUrl = "https://example.com/thumbnails/economia-colonial.jpg",
            Tema = "História Econômica",
            Nivel = NivelDificuldade.Intermedio,
            Regiao = "Luanda",
            Tipo = TipoConteudo.Video,
            IsJindungo = false,
            ReferenciaFactual = "Baseado em fontes históricas do Instituto Nacional de Estatística de Angola"
        };
    }
}

public class ConteudoResponseDtoExample : IExamplesProvider<ConteudoResponseDto>
{
    public ConteudoResponseDto GetExamples()
    {
        return new ConteudoResponseDto(
            Id: 1,
            Titulo: "A Economia Colonial Angolana",
            Resumo: "Análise da estrutura econômica durante o período colonial",
            CorpoTexto: "O período colonial português em Angola teve grande impacto na estrutura econômica do país...",
            VideoUrl: "https://example.com/videos/economia-colonial.mp4",
            AudioUrl: "https://example.com/audios/economia-colonial.mp3",
            ThumbnailUrl: "https://example.com/thumbnails/economia-colonial.jpg",
            Tipo: TipoConteudo.Video,
            Nivel: NivelDificuldade.Intermedio,
            Tema: "História Econômica",
            Regiao: "Luanda",
            Estado: EstadoConteudo.Publicado,
            EditorId: 5,
            EditorNome: "Dr. João Silva",
            Visualizacoes: 1250,
            EhFavorito: false,
            IsJindungo: false,
            ReferenciaFactual: "Baseado em fontes históricas do Instituto Nacional de Estatística de Angola",
            DataPublicacao: new DateTime(2024, 1, 15)
        );
    }
}
