using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PubSub.HubClient.Hub;

namespace PubSubSignalR.Hubs
{
    public class PubSubHub : Hub<IPubSubHubClient>, IPubSubHubServer
    {
        private readonly ILogger<PubSubHub> _logger;

        public PubSubHub(ILogger<PubSubHub> logger) => _logger = logger;

        public Task Subscribe(string appId)
        {
            _logger.LogInformation($"User '{Context.ConnectionId}'");
            return Groups.AddToGroupAsync(Context.ConnectionId, appId);
        }
    }
}