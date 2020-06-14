using System.Threading.Tasks;

namespace PubSub.HubClient.Hub
{
    public interface IPubSubHubServer
    {
        Task Subscribe(string appId);
    }
}