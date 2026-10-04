// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Linq;
using Iot.Device.DCMotor.Tests;
using Xunit;

namespace Iot.Device.ExplorerHat.Tests
{
    public class AnalogInputsTests
    {
        private static (ExplorerHat Hat, SimulatedAds1015 Adc, SimulatedI2cBus Bus) CreateHat(bool shouldDispose = false)
        {
            SimulatedAds1015 adc = new();
            SimulatedI2cBus bus = new();
            bus.Add(0x48, adc);
            ExplorerHat hat = new(new GpioController(new FakeGpioDriver()), bus, shouldDispose);
            return (hat, adc, bus);
        }

        [Fact]
        public void AnalogInputsReadTheChannelsInTheOrderOfTheHat()
        {
            var (hat, adc, _) = CreateHat();
            using (hat)
            {
                // Analog 1..4 of the HAT are the channels 3, 2, 1 and 0 of the ADS1015
                adc.SetChannelVoltage(3, 1.0);
                adc.SetChannelVoltage(2, 2.0);
                adc.SetChannelVoltage(1, 3.3);
                adc.SetChannelVoltage(0, 5.0);

                Assert.Equal(1.0, hat.Analog.One.ReadVoltage().Volts, 0.003);
                Assert.Equal(2.0, hat.Analog.Two.ReadVoltage().Volts, 0.003);
                Assert.Equal(3.3, hat.Analog.Three.ReadVoltage().Volts, 0.003);
                Assert.Equal(5.0, hat.Analog.Four.ReadVoltage().Volts, 0.003);
                Assert.Equal(new[] { 1.0, 2.0, 3.3, 5.0 }, hat.Analog.Select(input => Math.Round(input.ReadVoltage().Volts, 2)));
            }
        }

        [Fact]
        public void TheConverterIsOpenedTheFirstTimeAnInputIsRead()
        {
            var (hat, _, bus) = CreateHat();
            using (hat)
            {
                Assert.Empty(bus.CreatedAddresses);

                hat.Analog.Two.ReadVoltage();
                hat.Analog.Three.ReadVoltage();

                Assert.Equal(new[] { 0x48 }, bus.CreatedAddresses);
            }
        }

        [Fact]
        public void TheConverterMeasuresOnceWithTheWidestRange()
        {
            var (hat, adc, _) = CreateHat();
            using (hat)
            {
                hat.Analog.One.ReadVoltage();

                // Multiplexer 111 (AIN3 single-ended), gain 000 (±6.144V), single-shot mode, 1600 samples per second
                Assert.Equal(0x7, (adc.LastConfig >> 12) & 0x7);
                Assert.Equal(0x0, (adc.LastConfig >> 9) & 0x7);
                Assert.Equal(0x1, (adc.LastConfig >> 8) & 0x1);
                Assert.Equal(0x4, (adc.LastConfig >> 5) & 0x7);
            }
        }

        [Fact]
        public void DisposeDisposesTheConverterButNotACallerOwnedBus()
        {
            var (hat, adc, bus) = CreateHat(shouldDispose: false);
            hat.Analog.One.ReadVoltage();

            hat.Dispose();

            Assert.True(adc.IsDisposed);
            Assert.False(bus.IsDisposed);
        }

        [Fact]
        public void DisposeDisposesTheBusWhenAsked()
        {
            var (hat, _, bus) = CreateHat(shouldDispose: true);

            hat.Dispose();

            Assert.True(bus.IsDisposed);
            Assert.Empty(bus.CreatedAddresses);
        }

        [Fact]
        public void ReadingAnAnalogInputAfterDisposeThrows()
        {
            var (hat, _, _) = CreateHat();
            hat.Dispose();

            Assert.Throws<ObjectDisposedException>(() => hat.Analog.One.ReadVoltage());
        }
    }
}
