using System;

namespace IOTDemo
{
    public class DeviceOptions
    {
        /// <summary>
        ///     Time to raise the measurement event default 1 second
        /// </summary>
        public decimal? MeasurementSecondsInterval { get; set; }

        public Guid? Identifier { get; set; }
    }
}