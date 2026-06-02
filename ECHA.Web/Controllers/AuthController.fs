namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open System.Threading.Tasks
open ECHA.Core.DTOs

type AuthController () =
    inherit Controller()

    [<HttpGet>]
    member this.Login () =
        this.View()

    [<HttpPost>]
    member this.Login (request: LoginRequestDto) =
        // TODO: Implement actual API call to ECHA.API/api/auth/login
        // For now, redirect to Home/Index to allow basic flow testing
        this.RedirectToAction("Index", "Home")
