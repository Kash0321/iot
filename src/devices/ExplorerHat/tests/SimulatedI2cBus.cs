// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;

namespace Iot.Device.ExplorerHat.Tests
{
    /// <summary>
    /// An I2C bus that returns simulated devices and records which addresses were opened
    /// </summary>
    internal sealed class SimulatedI2cBus : I2cBus
    {
        private readonly Dictionary<int, I2cDevice> _devices = new();

        public List<int> CreatedAddresses { get; } = new();

        public bool IsDisposed { get; private set; }

        public void Add(int address, I2cDevice device) => _devices[address] = device;

        public override I2cDevice CreateDevice(int deviceAddress)
        {
            if (!_devices.TryGetValue(deviceAddress, out I2cDevice? device))
            {
                throw new ArgumentException($"No simulated device at 0x{deviceAddress:X2}", nameof(deviceAddress));
            }

            CreatedAddresses.Add(deviceAddress);
            return device;
        }

        public override void RemoveDevice(int deviceAddress)
        {
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
