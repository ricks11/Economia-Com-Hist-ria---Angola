namespace ECHA.Web.Models

open EconomiaComHistoria.Core.DTOs
open EconomiaComHistoria.Core.Helpers

type PerfilVM = {
    Perfil: PerfilResponseDto
    Progresso: ProgressoUtilizadorDto option
    Escolas: EscolaResponseDto list
    Turmas: TurmaResponseDto list
    Favoritos: PagedResult<ConteudoResponseDto>
    TabAtiva: string
}