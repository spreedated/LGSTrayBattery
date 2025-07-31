using LGSTrayPrimitives.Models;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text.Json;
using static LGSTrayPrimitives.Constants;

namespace LGSSharedMemoryWrapper
{
    public static class DeviceDataReader
    {
        public static string ReadAsJsonString()
        {
            try
            {
                using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(MEMORY_MAP_NAME))
                {
                    using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
                    {
                        ushort length = accessor.ReadUInt16(0);
                        byte[] buffer = new byte[length];
                        accessor.ReadArray(2, buffer, 0, length);
                        return System.Text.Encoding.UTF8.GetString(buffer);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public static IEnumerable<LogitechDevice> ReadAsDevices()
        {
            string json = ReadAsJsonString();
            if (string.IsNullOrEmpty(json))
            {
                return [];
            }

            return JsonSerializer.Deserialize<IEnumerable<LogitechDevice>>(json);
        }
    }
}
