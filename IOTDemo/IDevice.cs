using System;
using System.Threading.Tasks;

namespace IOTDemo
{
    public interface IDevice
    {
        event EventHandler<DeviceMeasurement> TemperaturaMeasurement;

        Task Off();
        Task On();
    }
}