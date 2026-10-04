// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.I2c;
using Iot.Device.Ads1115;
using UnitsNet;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Represents the Explorer HAT analog inputs collection
    /// </summary>
    /// <remarks>
    /// The ADS1015 converter is used in single-shot mode with a full scale range of ±6.144V. The I2C device is opened the
    /// first time an input is read. The numbers of the HAT do not match the channels of the converter: analog input 1 is
    /// channel 3, input 2 is channel 2, input 3 is channel 1 and input 4 is channel 0.
    /// </remarks>
    public class AnalogInputs : IDisposable, IEnumerable<AnalogInput>
    {
        // The ADS1015 has the same registers as the ADS1115, with 12 bits in the upper part of the conversion register.
        // DataRate.SPS128 is the ADS1015 power-on data rate, 1600 samples per second.
        private const MeasuringRange Range = MeasuringRange.FS6144;
        private const DataRate Rate = DataRate.SPS128;

        private readonly Func<I2cDevice> _createDevice;
        private readonly List<AnalogInput> _inputArray;
        private Ads1115.Ads1115? _adc;
        private bool _disposed;

        /// <summary>
        /// Analog input #1 (ADS1015 channel 3)
        /// </summary>
        public AnalogInput One => _inputArray[0];

        /// <summary>
        /// Analog input #2 (ADS1015 channel 2)
        /// </summary>
        public AnalogInput Two => _inputArray[1];

        /// <summary>
        /// Analog input #3 (ADS1015 channel 1)
        /// </summary>
        public AnalogInput Three => _inputArray[2];

        /// <summary>
        /// Analog input #4 (ADS1015 channel 0)
        /// </summary>
        public AnalogInput Four => _inputArray[3];

        /// <summary>
        /// Initializes a <see cref="AnalogInputs"/> instance
        /// </summary>
        /// <param name="createDevice">Creates the I2C device of the ADS1015 the first time an input is read</param>
        internal AnalogInputs(Func<I2cDevice> createDevice)
        {
            _createDevice = createDevice;
            _inputArray = new List<AnalogInput>()
            {
                new(this, InputMultiplexer.AIN3),
                new(this, InputMultiplexer.AIN2),
                new(this, InputMultiplexer.AIN1),
                new(this, InputMultiplexer.AIN0)
            };
        }

        internal ElectricPotential ReadVoltage(InputMultiplexer channel)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AnalogInputs));
            }

            _adc ??= new Ads1115.Ads1115(_createDevice(), channel, Range, Rate, DeviceMode.PowerDown);
            return _adc.ReadVoltage(channel);
        }

        /// <summary>
        /// Disposes the converter and its I2C device, if an input was read
        /// </summary>
        public void Dispose()
        {
            _adc?.Dispose();
            _adc = null;
            _disposed = true;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of analog inputs
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of analog inputs</returns>
        public IEnumerator<AnalogInput> GetEnumerator() => _inputArray.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection of analog inputs
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of analog inputs</returns>
        IEnumerator IEnumerable.GetEnumerator() => _inputArray.GetEnumerator();
    }
}
