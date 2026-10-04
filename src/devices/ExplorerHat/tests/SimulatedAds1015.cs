// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;

namespace Iot.Device.ExplorerHat.Tests
{
    /// <summary>
    /// A simulated ADS1015: writing the configuration register with the OS bit converts the selected single-ended
    /// channel at once, and the 12-bit result goes to the upper part of the conversion register.
    /// </summary>
    internal sealed class SimulatedAds1015 : I2cSimulatedDeviceBase
    {
        private const byte ConversionRegister = 0;
        private const byte ConfigRegister = 1;

        private readonly short[] _channelValues = new short[4];

        public SimulatedAds1015()
            : base(new I2cConnectionSettings(1, 0x48))
        {
            RegisterMap.Add(ConversionRegister, new Register<ushort>(0));
            RegisterMap.Add(ConfigRegister, new Register<ushort>(0x8583, ConfigRegisterHandler, null)); // power-on default
            RegisterMap.Add(2, new Register<ushort>(0x8000));
            RegisterMap.Add(3, new Register<ushort>(0x7FF0));
        }

        public bool IsDisposed { get; private set; }

        public ushort LastConfig { get; private set; } = 0x8583;

        /// <summary>
        /// Sets the voltage of a channel of the converter, as measured with the ±6.144V range
        /// </summary>
        public void SetChannelVoltage(int channel, double volts)
        {
            int value = (int)Math.Round(volts / 6.144 * 2048);
            _channelValues[channel] = (short)(Math.Clamp(value, -2048, 2047) << 4);
        }

        private ushort ConfigRegisterHandler(ushort newValue)
        {
            LastConfig = newValue;
            int mux = (newValue >> 12) & 0x7;
            if ((newValue & 0x8000) != 0 && mux >= 4)
            {
                // Single-ended input: AIN0..AIN3 = multiplexer 4..7. The conversion is finished at once (OS bit = 1).
                RegisterMap[ConversionRegister].WriteRegister((ushort)_channelValues[mux - 4]);
            }

            return (ushort)(newValue | 0x8000);
        }

        public override void WriteRead(byte[] inputBuffer, byte[] outputBuffer)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(SimulatedAds1015));
            }

            if (inputBuffer.Length > 0)
            {
                CurrentRegister = inputBuffer[0];
                if (inputBuffer.Length >= 3 && RegisterMap.TryGetValue(CurrentRegister, out var register))
                {
                    register.WriteRegister(BinaryPrimitives.ReadUInt16BigEndian(inputBuffer.AsSpan(1)));
                }
            }

            if (outputBuffer.Length >= 2 && RegisterMap.TryGetValue(CurrentRegister, out var register2))
            {
                BinaryPrimitives.WriteUInt16BigEndian(outputBuffer, (ushort)register2.ReadRegister());
            }
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
