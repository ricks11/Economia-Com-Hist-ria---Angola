namespace ECHA.Mobile.Models;

public record CategoriaDto(Guid Id, string Nome);

public record RespostaForumDto(
    Guid Id, 
    Guid TopicoId, 
    string Conteudo, 
    string AutorNome, 
    DateTime DataCriacao,
    int Reacoes,
    List<RespostaForumDto>? RespostasAninhadas
);

public record TopicoDto(
    Guid Id, 
    string Titulo, 
    string Descricao, 
    Guid CategoriaId, 
    string AutorNome, 
    DateTime DataCriacao,
    bool Aprovado
);
