using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class EndpointHostedService : IHostedService
{
    private readonly WebApplication _api;
    private CancellationTokenSource _cts;

    public event EventHandler<ApiReadyEventArgs> ApiReady;

    public EndpointHostedService(WebApplication api)
    {
        _api = api;
        _cts = new CancellationTokenSource();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            try
            {
                await _api.StartAsync(_cts.Token);

                // Retrieve the URL from the WebApplication instance
                var server = _api.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
                var addressesFeature = server.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();

                if (addressesFeature != null && addressesFeature.Addresses.Any())
                {
                    var address = addressesFeature.Addresses.First();
                    var uri = new Uri(address);
                    OnApiReady(new ApiReadyEventArgs(uri.Host, uri.Port));
                }

                await _api.WaitForShutdownAsync(_cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }

    protected virtual void OnApiReady(ApiReadyEventArgs e)
    {
        ApiReady?.Invoke(this, e);
    }
}

public class ApiReadyEventArgs : EventArgs
{
    public string Hostname { get; }
    public int Port { get; }

    public ApiReadyEventArgs(string hostname, int port)
    {
        Hostname = hostname;
        Port = port;
    }
}