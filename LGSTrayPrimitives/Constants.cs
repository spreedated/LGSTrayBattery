namespace LGSTrayPrimitives
{
    public static class Constants
    {
        public const string BACKGROUND_SERVICE_NAME = "LGS Background Service";
        public const string MEMORY_MAP_NAME = "LGS_Devices";
        public const string NAMED_PIPE_NAME = "LGSTray";
        public const string AUTOSTART_REG_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        public const string AUTOSTART_REG_KEY_VALUE = "LGSTrayGUI";
        public const string XAML_DATA_TEMPLATE = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<xml>\n<device_id>##DEVICEID##</device_id>\n<device_name>###DEVICENAME###</device_name>\n<device_type>###DEVICETYPE###</device_type>\n<battery_percent>###BATTERYPERCENTAGE###</battery_percent>\n<battery_voltage>###BATTERYVOLTAGE###</battery_voltage>\n<mileage>###BATTERYMILEAGE###</mileage>\n<charging>###POWERSUPPLYSTATUS###</charging>\n<last_update>###LASTUPDATE###</last_update>\n</xml>";
    }
}
