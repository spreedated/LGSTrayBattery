using CommunityToolkit.Mvvm.ComponentModel;
using LGSTrayPrimitives;
using static LGSTrayPrimitives.Constants;

namespace LGSTrayCore
{
    public partial class LogiDevice : ObservableObject
    {
        public const string NOT_FOUND = "NOT FOUND";

        [ObservableProperty]
        private DeviceType _deviceType;

        [ObservableProperty]
        private string _deviceId = NOT_FOUND;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ToolTipString))]
        private string _deviceName = NOT_FOUND;

        [ObservableProperty]
        private bool _hasBattery = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ToolTipString))]
        private double _batteryPercentage = -1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ToolTipString))]
        private double _batteryVoltage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ToolTipString))]
        private double _batteryMileage;


        [ObservableProperty]
        private PowerSupplyStatus _powerSupplyStatus;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ToolTipString))]
        private DateTimeOffset _lastUpdate = DateTimeOffset.MinValue;

        public string ToolTipString
        {
            get
            {
#if DEBUG
                return $"{this.DeviceName}, {this.BatteryPercentage:f2}% - {this.LastUpdate}";
#else
                return $"{this.DeviceName}, {this.BatteryPercentage:f2}%";
#endif
            }
        }

        public Func<Task>? UpdateBatteryFunc;
        public async Task UpdateBatteryAsync()
        {
            if (UpdateBatteryFunc != null)
            {
                await UpdateBatteryFunc.Invoke();
            }
        }

        partial void OnLastUpdateChanged(DateTimeOffset value)
        {
            Console.WriteLine(ToolTipString);
        }

        public string GetXmlData()
        {
            return XAML_DATA_TEMPLATE
                .Replace("##DEVICEID##", this.DeviceId)
                .Replace("###DEVICENAME###", this.DeviceName)
                .Replace("###DEVICETYPE###", this.DeviceType.ToString())
                .Replace("###BATTERYPERCENTAGE###", this.BatteryPercentage.ToString("f2"))
                .Replace("###BATTERYVOLTAGE###", this.BatteryVoltage.ToString("f2"))
                .Replace("###BATTERYMILEAGE###", this.BatteryMileage.ToString("f2"))
                .Replace("###POWERSUPPLYSTATUS###", (this.PowerSupplyStatus == PowerSupplyStatus.POWER_SUPPLY_STATUS_CHARGING).ToString())
                .Replace("###LASTUPDATE###", this.LastUpdate.ToString());
        }
    }
}
