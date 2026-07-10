namespace ECHA.Web.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Diagnostics

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open Microsoft.Extensions.Logging

open ECHA.Web.Models
open ECHA.Web.Services

type HomeController (logger : ILogger<HomeController>, apiClient : ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    member this.Index () =
        task {
            if this.User.Identity.IsAuthenticated then
                let token = this.GetToken()
                if not (isNull token) then
                    try
                        let! perfil = apiClient.GetPerfilAsync(token)
                        let! progresso = apiClient.GetProgressoAsync(token)
                        
                        match perfil with
                        | Some p -> this.ViewData.["Perfil"] <- p
                        | None -> ()
                        
                        match progresso with
                        | Some pr -> this.ViewData.["Progresso"] <- pr
                        | None -> ()
                    with
                    | ex -> logger.LogError(ex, "Erro ao obter dados dinâmicos da página inicial")
            return this.View() :> IActionResult
        }

    member this.Privacy () =
        this.View() :> IActionResult

    [<Authorize(Roles = "Admin,Editor,SuperAdmin")>]
    member this.PainelEditor () =
        this.View() :> IActionResult

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error () =
        let reqId = 
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View({ RequestId = reqId }) :> IActionResult
