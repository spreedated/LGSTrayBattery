namespace LGSTrayPrimitives.Models
{
    public record LogitechDevice
    {
        public double BatteryMileage { get; set; }
        public double BatteryPercentage { get; set; }
        public double BatteryVoltage { get; set; }
        public string? DeviceId { get; init; }
        public string? DeviceName { get; init; }
        public DeviceType DeviceType { get; init; }
        public bool HasBattery { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public PowerSupplyStatus PowerSupplyStatus { get; set; }
    }
}
