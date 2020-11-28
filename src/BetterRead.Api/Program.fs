namespace BetterRead.Api

open System
open System.Text
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

module Program =
    let exitCode = 0

    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(fun logging ->
                logging.AddConsole() |> ignore
                logging.AddDebug() |> ignore)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStartup<Startup>() |> ignore)

    [<EntryPoint>]
    let main args =
        Encoding.RegisterProvider CodePagesEncodingProvider.Instance
        Console.OutputEncoding <- Encoding.UTF8
        
        CreateHostBuilder(args).Build().Run()

        exitCode
