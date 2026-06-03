namespace ECHA.Mobile.Models;

public record ConteudoDto(
    Guid Id,
    string Titulo,
    string Descricao,
    string Tipo, // Video, Texto, Podcast
    string ThumbnailUrl,
    int DuracaoMinutos,
    string ConteudoUrl,
    bool IsFavorito
);
