namespace EconomiaComHistoria.Web.Models

open EconomiaComHistoria.Core.DTOs

type ConteudoDetailsViewModel = {
    Conteudo : ConteudoResponseDto
    IsFavorito : bool
    SolicitacaoStatus : string
    IsAdmin : bool
}