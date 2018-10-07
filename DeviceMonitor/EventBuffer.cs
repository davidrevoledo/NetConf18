using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IOTDemo;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;

namespace DeviceMonitor
{
    public class EventBuffer
    {
        private readonly int _bufferSize;

        private readonly Lazy<EventHubClient> _eventHubClient = new Lazy<EventHubClient>(
            () =>
            {
                const string cs =
                    @"Endpoint=sb://netconfar2018.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=byPgdEE3mUYh9BSlUkcI1fyUBwlTplXcvcvEbyut9M8=";

                var connectionStringBuilder = new EventHubsConnectionStringBuilder(cs)
                {
                    EntityPath = "temperature"
                };

                return EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            });

        private readonly ConcurrentBag<DeviceMeasurement> _measurements = new ConcurrentBag<DeviceMeasurement>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public EventBuffer(int bufferSize)
        {
            _bufferSize = bufferSize;

            _eventHubClient.Value.EventHubName.ToString();
        }

        public void Push(DeviceMeasurement measurement)
        {
            _measurements.Add(measurement);
            if (_measurements.Count == _bufferSize)
                try
                {
                    // todo : this solution is not safe as events can be 
                    _semaphore.Wait();
                    var newCollection = new List<DeviceMeasurement>(_measurements);
                    _measurements.Clear(); // clear the buffer
                    _semaphore.Release();


                    Task.Run(async () => await SendMessages(newCollection));
                }
                finally
                {
                    _semaphore.Release();
                }
        }

        private async Task SendMessages(IEnumerable<DeviceMeasurement> measurements)
        {
            var groupedDevices = measurements.GroupBy(c => c.DeviceId);
            foreach (var groupedDevice in groupedDevices.AsParallel())
            {
                var events = groupedDevice.Select(
                        c => new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(c))))
                    .ToList();

                await _eventHubClient.Value.SendAsync(events, groupedDevice.Key.ToString());
                await Task.Delay(10);
            }
        }
    }
}