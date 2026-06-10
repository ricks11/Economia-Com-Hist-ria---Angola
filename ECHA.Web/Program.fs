namespace ECHA.Web

#nowarn "20"

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)

        builder
            .Services
            .AddControllersWithViews()
            .AddRazorRuntimeCompilation()

        builder.Services
            .AddAuthentication(fun options ->
                options.DefaultAuthenticateScheme <- "CookieAuthentication"
                options.DefaultChallengeScheme <- "CookieAuthentication"
                options.DefaultSignInScheme <- "CookieAuthentication")
            .AddCookie("CookieAuthentication", (fun options ->
                options.LoginPath <- Microsoft.AspNetCore.Http.PathString("/Auth/Login")
                options.LogoutPath <- Microsoft.AspNetCore.Http.PathString("/Auth/Logout")
                options.Cookie.HttpOnly <- true
                options.Cookie.SecurePolicy <- Microsoft.AspNetCore.Http.CookieSecurePolicy.Always))

        builder.Services.AddRazorPages()

        builder.Services.AddHttpClient<Services.ApiClient>(fun client ->
            client.BaseAddress <- System.Uri(builder.Configuration["ApiSettings:BaseUrl"]))

        let app = builder.Build()

        if not (builder.Environment.IsDevelopment()) then
            app.UseExceptionHandler("/Home/Error")
            app.UseHsts() |> ignore // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

        app.UseHttpsRedirection()

        app.UseStaticFiles()
        app.UseRouting()
        app.UseAuthentication()
        app.UseAuthorization()

        app.MapControllerRoute(name = "default", pattern = "{controller=Home}/{action=Index}/{id?}")

        app.MapRazorPages()

        app.Run()

        exitCode
