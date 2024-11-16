WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register the API instance using a factory method
builder.Services.AddSingleton<WebApplication>(sp =>
{
    WebApplicationBuilder apiBuilder = WebApplication.CreateBuilder(args);
    apiBuilder.Logging.ClearProviders(); // Disable logging for the API instance
    WebApplication api = apiBuilder.Build();
    api.MapGet("/", () => "Hello World!");
    api.MapGet("/health", () => Results.Ok("Healthy")); // Health check endpoint
    return api;
});

// Register the hosted service
builder.Services.AddSingleton<EndpointHostedService>(sp =>
{
    WebApplication api = sp.GetRequiredService<WebApplication>();
    return new EndpointHostedService(api);
});
builder.Services.AddHostedService(sp => sp.GetRequiredService<EndpointHostedService>());

// Build the app
WebApplication app = builder.Build();

Console.WriteLine("Hello! Please enter your Bluesky/ATproto handle: ");
string? handle = Console.ReadLine();
Console.WriteLine($"Hello, {handle}!");
Console.WriteLine("Looking up PDS via DNS...");

/**
 * This is where the PDS lookup would happen.
 * 
 * For now, we'll just pretend we found the PDS.
 */

Console.WriteLine("PDS found! Starting API...");
EndpointHostedService hostedService = app.Services.GetRequiredService<EndpointHostedService>();

// Subscribe to the ApiReady event
hostedService.ApiReady += (sender, e) =>
{
    Console.WriteLine($"API is warmed up and accepting requests at {e.Hostname}:{e.Port}.");
};

await hostedService.StartAsync(CancellationToken.None);

Console.WriteLine("Type 'stop' to stop the API...");
while (Console.ReadLine()?.ToLower() != "stop")
{
    // Wait for user to type 'stop'
}

await hostedService.StopAsync(CancellationToken.None);
Console.WriteLine("API stopped. Press Enter key to exit...");
Console.ReadLine();