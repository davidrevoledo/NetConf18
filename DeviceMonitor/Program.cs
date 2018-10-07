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
        private static void Main(string[] args)
        {
            MainAsync()
                .GetAwaiter()
                .GetResult();

            Console.WriteLine("Hello World!");
        }

        private static async Task MainAsync()
        {
            var options = new DeviceOptions
            {
                MeasurementSecondsInterval = 0.1m
            };

            var devices = new List<IDevice>();

            for (var i = 0; i < 5000; i++)
                devices.Add(DeviceFactory.Create(options));

            // set event handler
            foreach (var device in devices.AsParallel())
                device.TemperaturaMeasurement += Device_TemperaturaMeasurement;

            var starts = devices.Select(c => c.On());

            await Task.WhenAll(starts);
        }

        private static void Device_TemperaturaMeasurement(object sender, DeviceMeasurement e)
        {
            Interlocked.Increment(ref _events);
            Console.WriteLine($"A temperature of {e.Temperature} has been recorded {e.Date:hh:mm:ss}" +
                              $" for the device id : {e.DeviceId}");

            //if ((_events % 10000) == 0)
            //{
            //    Console.WriteLine($"{_events} have been registred");
            //    Thread.Sleep(2000);
            //}
        }
    }
}