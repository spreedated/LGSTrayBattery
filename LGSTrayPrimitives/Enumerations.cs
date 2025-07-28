namespace LGSTrayPrimitives
{
    public enum DeviceType : byte
    {
        Keyboard = 0,
        Mousepad = 1,
        HeadsetDongle = 2,
        Mouse = 3,
        KeyboardDongle = 4,
        MouseDongle = 5,
        KeyboardHeadsetDongle = 6,
        MouseKeyboardDongle = 7,
        Headset = 8
    }

    public enum PowerSupplyStatus : byte
    {
        POWER_SUPPLY_STATUS_DISCHARGING = 0,
        POWER_SUPPLY_STATUS_CHARGING,
        POWER_SUPPLY_STATUS_FULL,
        POWER_SUPPLY_STATUS_NOT_CHARGING,
        POWER_SUPPLY_STATUS_UNKNOWN
    }
}
