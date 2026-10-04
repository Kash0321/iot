// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ads1115;
using UnitsNet;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Represents one of the analog inputs of the Explorer HAT
    /// </summary>
    /// <remarks>
    /// The inputs are measured by an ADS1015 analog to digital converter (I2C address 0x48), which is powered at 5V:
    /// each input accepts from 0V to 5V. An input with nothing connected does not read 0V: its value is not defined.
    /// </remarks>
    public class AnalogInput
    {
        private readonly AnalogInputs _inputs;
        private readonly InputMultiplexer _channel;

        /// <summary>
        /// Initializes a <see cref="AnalogInput"/> instance
        /// </summary>
        /// <param name="inputs">Collection that owns the converter</param>
        /// <param name="channel">Channel of the ADS1015 connected to this input</param>
        internal AnalogInput(AnalogInputs inputs, InputMultiplexer channel)
        {
            _inputs = inputs;
            _channel = channel;
        }

        /// <summary>
        /// Measures the voltage of the input. The first measurement of any analog input opens the I2C device.
        /// </summary>
        /// <returns>The measured voltage, with a resolution of 3mV</returns>
        public ElectricPotential ReadVoltage() => _inputs.ReadVoltage(_channel);
    }
}
