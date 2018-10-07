using System;

namespace IOTDemo
{
    public struct DeviceMeasurement
    {
        public int Temperature { get; set; }

        public Guid DeviceId { get; set; }

        public DateTime Date { get; set; }
    }
}