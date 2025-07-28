using LGSTrayPrimitives.MessageStructs;
using MessagePipe;
using Microsoft.Extensions.Hosting;

namespace LGSTrayHID
{
    public class HidppManagerService : IHostedService
    {
        private readonly IDistributedPublisher<IPCMessageType, IpcMessage> _publisher;

        public HidppManagerService(IDistributedPublisher<IPCMessageType, IpcMessage> publisher)
        {
            _publisher = publisher;

            HidppManagerContext.Instance.HidppDeviceEvent += async (type, message) =>
            {
#if DEBUG
                if (message is InitMessage initMessage)
                {
                    Console.WriteLine(initMessage.deviceName);
                }
#endif

                await _publisher.PublishAsync(type, message);
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            HidppManagerContext.Instance.Start(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
