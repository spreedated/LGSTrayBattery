﻿using LGSTrayPrimitives.MessageStructs;
using MessagePipe;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace LGSTrayCore.Managers
{
    public class LGsTrayHidManager : IDeviceManager, IHostedService, IDisposable
    {
        #region IDisposable
        private Func<Task>? _diposeSubs;
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _ = _diposeSubs?.Invoke();
                    _diposeSubs = null;
                    _cts?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private readonly CancellationTokenSource _cts = new();
        private CancellationTokenSource? _daemonCts;

        private readonly IDistributedSubscriber<IPCMessageType, IpcMessage> _subscriber;
        private readonly IPublisher<IpcMessage> _deviceEventBus;

        public LGsTrayHidManager(
            IDistributedSubscriber<IPCMessageType, IpcMessage> subscriber,
            IPublisher<IpcMessage> deviceEventBus
        )
        {
            _subscriber = subscriber;
            _deviceEventBus = deviceEventBus;
        }

        private async Task<int> DaemonLoop()
        {
            _daemonCts = new();

            using Process proc = new();
            proc.StartInfo = new()
            {
                RedirectStandardError = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                FileName = Path.Combine(AppContext.BaseDirectory, "LGSTrayHID.exe"),
                Arguments = Environment.ProcessId.ToString(),
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            proc.Start();

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, _daemonCts.Token);
                await proc.WaitForExitAsync(cts.Token);
            }
            catch (Exception)
            {
                if (!proc.HasExited)
                {
                    proc.Kill();
                }
            }
            finally
            {
                _daemonCts.Dispose();
                _daemonCts = null;
            }

            await Task.Delay(1000);
            return proc.ExitCode;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var sub1 = await _subscriber.SubscribeAsync(
                IPCMessageType.INIT,
                x =>
                {
                    var initMessage = (InitMessage)x;
                    _deviceEventBus.Publish(initMessage);
                },
                cancellationToken
            );

            var sub2 = await _subscriber.SubscribeAsync(
                IPCMessageType.UPDATE,
                x =>
                {
                    var updateMessage = (UpdateMessage)x;
                    _deviceEventBus.Publish(updateMessage);
                },
                cancellationToken
            );

            _diposeSubs = async () =>
            {
                await sub1.DisposeAsync();
                await sub2.DisposeAsync();
            };

            _ = Task.Run(async () =>
            {
                int fastFailCount = 0;

                while (!_cts.Token.IsCancellationRequested)
                {
                    DateTime then = DateTime.UtcNow;
                    int ret = await this.DaemonLoop();

                    // Daemon returns -1 on .Kill(), assume its user
                    if ((ret != -1) || (DateTime.UtcNow - then).TotalSeconds < 20)
                    {
                        fastFailCount++;
                    }
                    else
                    {
                        fastFailCount = 0;
                    }

                    if (fastFailCount > 3)
                    {
                        // Notify user?
                        break;
                    }
                }
            }, CancellationToken.None);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();

            return Task.CompletedTask;
        }

        public void RediscoverDevices()
        {
            _daemonCts?.Cancel();
        }
    }
}
