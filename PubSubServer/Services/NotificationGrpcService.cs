using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PubSub.HubClient;
using PubSub.HubClient.Hub;
using PubSubSignalR.Hubs;

namespace PubSubSignalR.Services
{
    public class NotificationGrpcService : NotificationService.NotificationServiceBase
    {
        private readonly IHubContext<PubSubHub, IPubSubHubClient> _pubSubHub;
        private readonly ILogger<NotificationGrpcService> _logger;

        public NotificationGrpcService(
            IHubContext<PubSubHub, IPubSubHubClient> pubSubHub, 
            ILogger<NotificationGrpcService> logger)
        {
            _pubSubHub = pubSubHub;
            _logger = logger;
        }

        public override async Task<NotificationReply> SendNotification(NotificationRequest request, ServerCallContext context)
        {
            _logger.LogInformation(
                $"Received notification for Group='{request.Topic.Group}';Id='{request.TrackingId}'");

            var notificationReply = new NotificationReply
            {
                TimeStamp = DateTime.UtcNow.ToTimestamp(),
                ErrorResult = new ErrorResult()
            };

            try
            {
                await
                    _pubSubHub.Clients
                        .Group(request.Topic.Group)
                        .NotificationSent(
                            new ClientNotificationEvent
                            {
                                DateTime = DateTime.UtcNow,
                                Message = request.Payload.Message
                            });

                notificationReply.IsSuccess = true;
            }
            catch (Exception e)
            {
                notificationReply.ErrorResult.Errors.Add(
                    new ErrorResult.Types.Error
                    {
                        Message = $"Failed to send message Id='{request.TrackingId}'",
                        Type = e.GetType().ToString()
                    });
            }

            return notificationReply;
        }
    }
}
