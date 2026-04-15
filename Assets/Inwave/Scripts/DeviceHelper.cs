using System.Collections.Generic;
using UnityEngine;

namespace Inwave.Production.Helper
{
    public class DeviceHelper
    {
        private static int LowEndDevice_Memory = 2048;
        private static int LowEndDevice_Processor = 3;
        private static int LowEndDevice_GraphicMemory = 128;
        /// <summary>
        /// Check running device is low end or not
        /// </summary>
        /// <returns>true/false</returns>
        public static bool IsLowEnd()
        {
            if (SystemInfo.systemMemorySize <= LowEndDevice_Memory ||
                SystemInfo.processorCount <= LowEndDevice_Processor ||
                SystemInfo.graphicsMemorySize <= LowEndDevice_GraphicMemory)
            {
                return true;
            }
            return false;
        }



        /// <summary>
        /// Run unique one time
        /// </summary>
        public static Dictionary<string,object> GetDeviceDetails()
        {
            var param = new Dictionary<string, object>
            {
                { "IsLowEnd", IsLowEnd() ? "true" : "false" },
                { "DeviceModel", SystemInfo.deviceModel },
                { "Memory", SystemInfo.systemMemorySize.ToString() },
                { "Processor", SystemInfo.processorCount.ToString() },
                { "Graphics", SystemInfo.graphicsMemorySize.ToString() },
                { "Shader", SystemInfo.graphicsShaderLevel.ToString() }
            };

            return param;
        }
    }
}

