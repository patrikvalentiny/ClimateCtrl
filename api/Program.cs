using System.Net;
using Fleck;
using Serilog;
using WebSocketProxy;
using Host = WebSocketProxy.Host;

StartupClass.Startup(args);
// var tcp = 
// tcp.Start();
// app.Run();

public static class StartupClass
{
    public static  void  Startup(string[] args)
    {
        //setup logger
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            // .WriteTo.Console()
            .CreateLogger();
        
        
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Host.UseSerilog();

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.WebHost.UseUrls("http://*:5000");

        var app = builder.Build();
        app.UseSerilogRequestLogging();
        app.UseForwardedHeaders();
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.MapControllers();
        
        TcpProxyConfiguration proxyConfiguration = new TcpProxyConfiguration()
        {
            PublicHost = new Host()
            {
                IpAddress = IPAddress.Parse("0.0.0.0"),
                Port = 8080
            },
            HttpHost = new Host()
            {
                IpAddress = IPAddress.Loopback,
                Port = 5000
            },
            WebSocketHost = new Host()
            {

                IpAddress = IPAddress.Loopback,
                Port = 8181
            }
        };

        using var websocketServer = new WebSocketServer("ws://0.0.0.0:8181");
        using var tcpProxy = new TcpProxyServer(proxyConfiguration);

        // Initialize Fleck
        websocketServer.Start(socket =>
        {
            socket.OnOpen = () =>
            { 
                Log.Debug("Client connected: {Id}", socket.ConnectionInfo.Id);
            };
            socket.OnClose = () =>
            {
                Log.Debug("Client disconnected: {Id}", socket.ConnectionInfo.Id);
            };
            socket.OnMessage = async message =>
            {
                Log.Debug("Message received: {Message}", message);
                try
                {
                    await socket.Send("Hello from server");
                    // await app.InvokeClientEventHandler(services, socket, message);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error handling message");
                    // e.Handle(socket);
                }
            };
        });
        
        // Initialize the proxy
        tcpProxy.Start();
        
        app.Run();
    }
}