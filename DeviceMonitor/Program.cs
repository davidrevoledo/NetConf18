using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOTDemo;

namespace DeviceMonitor
{
    internal class Program
    {
        private static int _events;
        private static readonly EventBuffer _buffer = new EventBuffer(500);

        private static void Main(string[] args)
        {
            MainAsync()
                .GetAwaiter()
                .GetResult();

            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            var options = new DeviceOptions
            {
                MeasurementSecondsInterval = 1m
            };

            var devices = new List<IDevice>();

            for (var i = 0; i < 1000; i++)
                devices.Add(DeviceFactory.Create(options));

            // set event handler
            foreach (var device in devices.AsParallel())
                device.TemperaturaMeasurement += Device_TemperaturaMeasurement;

            var starts = devices.Select(c => c.On());
            await Task.WhenAll(starts);
        }

        private static async void Device_TemperaturaMeasurement(object sender, DeviceMeasurement @event)
        {
            Interlocked.Increment(ref _events);
            Console.WriteLine($"A temperature of {@event.Temperature} has been recorded" +
                              $" {@event.Date:hh:mm:ss}" +
                              $" for the device id : {@event.DeviceId}");

            await _buffer.Push(@event);
        }
    }
}