using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOTDemo;

namespace DeviceEventProcessing
{
    public static class EventStorage
    {
        private static int _limitToMonitor;
        private static int _limitThreshold;

        public static ConcurrentDictionary<Guid, List<DeviceMeasurement>> DeviceMeasurementToProcess =
            new ConcurrentDictionary<Guid, List<DeviceMeasurement>>();

        public static ConcurrentDictionary<Guid, List<DeviceMeasurement>> ProcessedDeviceMeasurement =
            new ConcurrentDictionary<Guid, List<DeviceMeasurement>>();

        public static Task Start(int limitToMonitor, int limitThreshold)
        {
            _limitToMonitor = limitToMonitor;
            _limitThreshold = limitThreshold;

            Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var device in DeviceMeasurementToProcess.AsParallel())
                    {
                        // todo : this is not 100% safe
                        if (!ProcessedDeviceMeasurement.ContainsKey(device.Key))
                            ProcessedDeviceMeasurement.TryAdd(device.Key, new List<DeviceMeasurement>());

                        if (ProcessedDeviceMeasurement[device.Key].Count > _limitThreshold)
                            Console.WriteLine($"device {device.Key} has reached the threshold : {_limitToMonitor}");

                        foreach (var measurement in device.Value.ToList())
                        {
                            if (measurement.Temperature >= _limitToMonitor)
                                ProcessedDeviceMeasurement[device.Key].Add(measurement);

                            device.Value.Remove(measurement);
                        }
                    }

                    await Task.Delay(1000);
                }
            });

            return Task.CompletedTask;
        }
    }
}