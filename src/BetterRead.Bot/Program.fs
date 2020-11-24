namespace BetterRead.Bot

open System
open System.Text
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting

module Program =
    let exitCode = 0

    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStartup<Startup>() |> ignore
            )

    [<EntryPoint>]
    let main args =
        Encoding.RegisterProvider CodePagesEncodingProvider.Instance
        Console.OutputEncoding <- Encoding.UTF8
        
        CreateHostBuilder(args).Build().Run()

        exitCode
