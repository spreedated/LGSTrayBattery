using Microsoft.Extensions.DependencyInjection;
using static LGSTrayPrimitives.Constants;

namespace LGSTrayPrimitives.IPC
{
    public static class MessagePipeHelper
    {
        public static void AddLGSMessagePipe(this IServiceCollection services, bool hostAsServer = false)
        {
            services.AddMessagePipe(options =>
            {
                options.EnableCaptureStackTrace = true;
            }).
            AddNamedPipeInterprocess(NAMED_PIPE_NAME, config =>
            {
                config.HostAsServer = hostAsServer;
            });
        }
    }
}
