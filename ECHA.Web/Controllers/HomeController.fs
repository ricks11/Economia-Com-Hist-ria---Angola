namespace ECHA.Web.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Diagnostics

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open ECHA.Web.Models
open Microsoft.AspNetCore.Authorization

type HomeController (logger : ILogger<HomeController>) =
    inherit Controller()

    member this.Index () =
        if this.User.IsInRole("Editor") || this.User.IsInRole("Admin") || this.User.IsInRole("SuperAdmin") then
            this.RedirectToAction("PainelEditor") :> IActionResult
        else
            this.View() :> IActionResult

    member this.Privacy () =
        this.View()

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error () =
        let reqId = 
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View({ RequestId = reqId })

    [<Authorize(Roles = "Editor,Admin,SuperAdmin")>]
    member this.PainelEditor () =
        this.View()