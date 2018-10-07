using System;
using System.Threading.Tasks;

namespace IOTDemo
{
    public class Device : IDevice
    {
        private readonly Guid _identifier;
        private readonly Random _randomizer = new Random();
        private DeviceState _state = DeviceState.Deactivated;
        private int _temperature;
        private readonly decimal _measurementSecondsInterval;

        internal Device(Guid identifier, decimal measurementSecondsInterval)
        {
            _identifier = identifier;
            _measurementSecondsInterval = measurementSecondsInterval;
        }

        public virtual async Task On()
        {
            _state = DeviceState.Activated;

            while (true)
            {
                if (_state == DeviceState.Deactivated)
                    break;

                // asign new temperature
                _temperature = _randomizer.Next(0, 100);

                OnTemperatureMeasurement(null);
                await Task.Delay(Convert.ToInt32(_measurementSecondsInterval * 1000m));
            }
        }

        public virtual Task Off()
        {
            _state = DeviceState.Deactivated;
            return Task.CompletedTask;
        }

        public event EventHandler<DeviceMeasurement> TemperaturaMeasurement;

        protected virtual void OnTemperatureMeasurement(EventArgs e)
        {
            var handler = TemperaturaMeasurement;
            handler?.Invoke(this, new DeviceMeasurement
            {
                DeviceId = _identifier,
                Temperature = _temperature,
                Date = DateTime.Now
            });
        }
    }
}