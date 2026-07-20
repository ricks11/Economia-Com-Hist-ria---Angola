namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.Rendering
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open ECHA.Web.Services
open EconomiaComHistoria.Core.Enums

[<Authorize(Roles = "Admin,Editor,SuperAdmin")>]
type RelatoriosController (apiClient: ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    // Carrega dropdowns de escolas e turmas (opcionalmente filtrando por escola)
    member private this.CarregarDropdownsAsync (token: string, ?escolaId: int) =
        task {
            let! escolas = apiClient.ListEscolasAsync(token)
            this.ViewData.["Escolas"] <- SelectList(escolas, "Id", "Nome")

            let! turmas = apiClient.ListTurmasAsync(token, ?escolaId = escolaId)
            this.ViewData.["Turmas"] <- SelectList(turmas, "Id", "Nome")
        }

    [<HttpGet>]
    member this.Index (escolaId: Nullable<int>) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let escolaIdOpt = if escolaId.HasValue then Some escolaId.Value else None
                let! lista = apiClient.ListarRelatoriosAsync(t, ?escolaId = escolaIdOpt)
                let listaDotNet = new System.Collections.Generic.List<RelatorioListaDto>(lista)
                return this.View(listaDotNet) :> IActionResult
        }

    [<HttpGet>]
    member this.Criar (escolaId: Nullable<int>) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let escolaIdOpt = if escolaId.HasValue then Some escolaId.Value else None

                do! this.CarregarDropdownsAsync(t, ?escolaId = escolaIdOpt)

                let dto = new SolicitarRelatorioDto("", "CSV", escolaId, Nullable<int>(), Nullable<DateTime>(), Nullable<DateTime>())
                return this.View(dto) :> IActionResult
        }

    [<HttpPost>]
    member this.Criar (request: SolicitarRelatorioDto) : Task<IActionResult> =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let escolaIdOpt = if request.EscolaId.HasValue then Some request.EscolaId.Value else None

                if String.IsNullOrWhiteSpace(request.Titulo) then
                    this.TempData["ErrorMessage"] <- "Título é obrigatório."
                    do! this.CarregarDropdownsAsync(t, ?escolaId = escolaIdOpt)
                    return this.View(request) :> IActionResult
                else
                    let! result = apiClient.SolicitarRelatorioAsync(request, t)
                    match result with
                    | Some status ->
                        this.TempData["SuccessMessage"] <- "Relatório solicitado com sucesso! A geração pode demorar alguns minutos."
                        // ✅ CORREÇÃO: usar objeto anónimo para redirecionamento
                        return this.RedirectToAction("Status", {| id = status.Id |}) :> IActionResult
                    | None ->
                        this.TempData["ErrorMessage"] <- "Erro ao solicitar relatório."
                        do! this.CarregarDropdownsAsync(t, ?escolaId = escolaIdOpt)
                        return this.View(request) :> IActionResult
        }

    [<HttpGet>]
    member this.Status (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! status = apiClient.GetRelatorioStatusAsync(id, t)
                match status with
                | Some s -> return this.View(s) :> IActionResult
                | None -> return this.NotFound() :> IActionResult
        }

    [<HttpGet>]
    member this.Download (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                // 1. Procura o estado do relatório para saber o formato (PDF/CSV) e se está concluído
                let! statusOpt = apiClient.GetRelatorioStatusAsync(id, t)
                match statusOpt with
                | None -> return this.NotFound() :> IActionResult
                | Some status ->
                    if status.Estado <> EstadoRelatorio.Concluido then
                        this.TempData["ErrorMessage"] <- "O relatório ainda não está pronto para download."
                        return this.RedirectToAction("Status", {| id = id |}) :> IActionResult
                    else
                        // 2. Procura os bytes do ficheiro através do ApiClient
                        // Nota: Certifique-se de que o seu ApiClient tem o método DownloadRelatorioAsync implementado
                        let! dadosOpt = apiClient.DownloadRelatorioAsync(id, t)
                        match dadosOpt with
                        | None -> 
                            this.TempData["ErrorMessage"] <- "Ficheiro não encontrado no servidor de API."
                            return this.RedirectToAction("Status", {| id = id |}) :> IActionResult
                        | Some (bytes: byte[]) ->
                            let extensao = if status.Tipo.ToLower() = "csv" then "csv" else "pdf"
                            let contentType = if extensao = "csv" then "text/csv" else "application/pdf"
                            let nomeFicheiro = sprintf "relatorio_%d.%s" id extensao
                            
                            return this.File(bytes, contentType, nomeFicheiro) :> IActionResult
        }