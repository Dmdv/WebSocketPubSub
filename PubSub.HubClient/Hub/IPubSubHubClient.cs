using System.Threading.Tasks;

namespace PubSub.HubClient.Hub
{
    public interface IPubSubHubClient
    {
        Task NotificationSent(ClientNotificationEvent notificationEvent);
    }
}