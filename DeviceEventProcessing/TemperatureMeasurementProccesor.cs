using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOTDemo;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Newtonsoft.Json;

namespace DeviceEventProcessing
{
    public class TemperatureMeasurementProccesor : IEventProcessor
    {
        private static readonly List<EventData> data = new List<EventData>();

        public Task OpenAsync(PartitionContext context)
        {
            return Task.CompletedTask;
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            return Task.CompletedTask;
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var eventData in messages.AsParallel())
            {
                var payload =
                    Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                var measurement = JsonConvert.DeserializeObject<DeviceMeasurement>(payload);

                // todo : this is not 100% safe
                if (!EventStorage.DeviceMeasurementToProcess.ContainsKey(measurement.DeviceId))
                    EventStorage.DeviceMeasurementToProcess.TryAdd(measurement.DeviceId, new List<DeviceMeasurement>());

                EventStorage.DeviceMeasurementToProcess[measurement.DeviceId].Add(measurement);
            }

            await context.CheckpointAsync()
                .ConfigureAwait(false);
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            return Task.CompletedTask;
        }
    }
}