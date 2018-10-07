using System;

namespace IOTDemo
{
    public static class DeviceFactory
    {
        public static IDevice Create(DeviceOptions options)
        {
            // asign default values
            return new Device(options.Identifier ?? Guid.NewGuid(),
                options.MeasurementSecondsInterval ?? 1);
        }

        public static IDevice Create()
        {
            return Create(new DeviceOptions
            {
                Identifier = Guid.NewGuid()
            });
        }
    }
}