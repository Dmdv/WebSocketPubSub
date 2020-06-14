using System;

namespace PubSub.HubClient
{
    public sealed class ClientNotificationEvent
    {
        public string Message { get; set; }

        public DateTime DateTime { get; set; }
    }
}
