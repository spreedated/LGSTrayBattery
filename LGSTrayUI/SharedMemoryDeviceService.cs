using LGSTrayCore;
using LGSTrayPrimitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LGSTrayUI
{
    public class SharedMemoryDeviceService : IHostedService, IDisposable
    {
        private readonly ILogiDeviceCollection _deviceCollection;
        private MemoryMappedFile? _mmf;
        private CancellationTokenSource? _cts;
        private Task? _backgroundTask;
        private bool disposedValue;
        private const string MapName = "LGSTray_DeviceList";
        private const int MapSize = 4096;
        private readonly IOptions<AppSettings> _appSettings;

        public SharedMemoryDeviceService(ILogiDeviceCollection deviceCollection, IOptions<AppSettings> appSettings)
        {
            this._deviceCollection = deviceCollection;
            _appSettings = appSettings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_appSettings.Value.SharedMemory.Enabled)
            {
                return Task.CompletedTask;
            }

            _mmf = MemoryMappedFile.CreateOrOpen(MapName, MapSize);
            _cts = new();
            _backgroundTask = Task.Run(() => this.RunAsync(_cts.Token), cancellationToken);
            return Task.CompletedTask;
        }

        private async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    IEnumerable<LogiDevice> devices = this._deviceCollection.GetDevices();
                    string json = JsonSerializer.Serialize(devices);
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

                    using var accessor = _mmf!.CreateViewAccessor(0, MapSize);
                    accessor.Write(0, (ushort)bytes.Length);
                    accessor.WriteArray(2, bytes, 0, Math.Min(bytes.Length, MapSize - 2));
                }
                catch
                {
                    //noop
                }

                await Task.Delay(1000, token);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._cts?.Cancel();
            this._backgroundTask?.Wait(cancellationToken);
            this._mmf?.Dispose();
            return Task.CompletedTask;
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this._cts?.Cancel();
                    this._cts?.Dispose();
                    this._mmf?.Dispose();
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
    }
}
