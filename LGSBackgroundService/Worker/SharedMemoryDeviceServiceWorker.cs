using LGSTrayPrimitives.MessageStructs;
using LGSTrayPrimitives.Models;
using MessagePipe;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LGSBackgroundService.Worker
{
    public class SharedMemoryDeviceServiceWorker : IHostedService, IDisposable
    {
        private readonly SemaphoreSlim updateQueue = new(1, 1);
        private MemoryMappedFile mmf;
        private CancellationTokenSource _cts;
        private bool disposedValue;
        private const string MapName = "LGS_Devices";
        private const int MapSize = 4096;
        private readonly ISubscriber<IpcMessage> subscriber;
        private readonly ILogger logger;
        public List<LogitechDevice> Devices { get; } = [];

        public SharedMemoryDeviceServiceWorker(ISubscriber<IpcMessage> subscriber)
        {
            this.logger = new SerilogLoggerProvider().CreateLogger("SharedMemoryDeviceService");
            this.subscriber = subscriber;
            this.subscriber.Subscribe(this.OnDeviceMessage);
        }
        private void OnDeviceMessage(IpcMessage msg)
        {
            if (msg is InitMessage init)
            {
                if (!this.Devices.Any(x => x.DeviceId == init.deviceId))
                {
                    this.Devices.Add(new() { DeviceId = init.deviceId, DeviceName = init.deviceName, DeviceType = init.deviceType });
                    this.logger?.LogTrace("New device discoverd \"{DeviceName}\"", init.deviceName);
                }
            }
            else if (msg is UpdateMessage update)
            {
                LogitechDevice d = this.Devices.FirstOrDefault(x => x.DeviceId == update.deviceId);

                if (d == null)
                {
                    return;
                }

                d.BatteryMileage = update.Mileage;
                d.BatteryPercentage = update.batteryPercentage;
                d.BatteryVoltage = update.batteryMVolt;
                d.PowerSupplyStatus = update.powerSupplyStatus;
                d.LastUpdate = update.updateTime;

                this.logger?.LogTrace("Device \"{DeviceName}\" updated: {BatteryPercentage:f2}%, {PowerSupplyStatus}, {BatteryVoltage}mV, {Mileage:f2}km, LastUpdate: {LastUpdate}",
                    d.DeviceName, d.BatteryPercentage, d.PowerSupplyStatus, d.BatteryVoltage, d.BatteryMileage, d.LastUpdate);
            }

            Task.Run(() => this.RunAsync());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            mmf = MemoryMappedFile.CreateOrOpen(MapName, MapSize);
            _cts = new();

            return Task.CompletedTask;
        }

        private async Task RunAsync()
        {
            await this.updateQueue.WaitAsync();

            try
            {
                string json = JsonSerializer.Serialize(this.Devices);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

                using var accessor = mmf!.CreateViewAccessor(0, MapSize);
                accessor.Write(0, (ushort)bytes.Length);
                accessor.WriteArray(2, bytes, 0, Math.Min(bytes.Length, MapSize - 2));
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Error while updating shared memory");
            }

            this.logger?.LogTrace("Shared memory updated");
            this.updateQueue.Release();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._cts?.Cancel();
            this.updateQueue?.Dispose();
            this.mmf?.Dispose();

            this.logger?.LogTrace("SharedMemoryDeviceService stopped");

            return Task.CompletedTask;
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.updateQueue?.Dispose();
                    this._cts?.Cancel();
                    this._cts?.Dispose();
                    this.mmf?.Dispose();
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
