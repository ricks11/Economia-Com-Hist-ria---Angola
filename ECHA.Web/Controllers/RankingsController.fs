namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open ECHA.Web.Services

[<Authorize>]
type RankingsController(apiClient: ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    member this.Index(tipo: string, periodo: string, escolaId: Nullable<int>, provincia: string, municipio: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    // Valores padrão
                    let tipoParam = if String.IsNullOrEmpty(tipo) then "Nacional" else tipo
                    let periodoParam = if String.IsNullOrEmpty(periodo) then "Semanal" else periodo

                    // Construir query string com filtros opcionais
                    let mutable url = $"/api/ranking?tipo={tipoParam}&periodo={periodoParam}"
                    if escolaId.HasValue then url <- url + $"&escolaId={escolaId.Value}"
                    if not (String.IsNullOrEmpty(provincia)) then url <- url + $"&provincia={provincia}"
                    if not (String.IsNullOrEmpty(municipio)) then url <- url + $"&municipio={municipio}"

                    let! ranking = apiClient.GetRankingByUrlAsync(url, t)
                    match ranking with
                    | Some r ->
                        this.ViewData.["TipoAtual"] <- tipoParam
                        this.ViewData.["PeriodoAtual"] <- periodoParam
                        this.ViewData.["EscolaId"] <- escolaId
                        this.ViewData.["Provincia"] <- provincia
                        this.ViewData.["Municipio"] <- municipio
                        return this.View(r) :> IActionResult
                    | None ->
                        this.TempData["ErrorMessage"] <- "Não foi possível carregar o ranking."
                        return this.View() :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }