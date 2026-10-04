// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Pimoroni Explorer HAT for Raspberry Pi
    /// </summary>
    public class ExplorerHat : IDisposable
    {
        private const int Ads1015Address = 0x48;

        private readonly bool _shouldDispose;
        private readonly bool _shouldDisposeI2cBus;
        private GpioController _controller;
        private I2cBus? _i2cBus;

        /// <summary>
        /// Explorer HAT DCMotors collection
        /// </summary>
        public Motors Motors { get; private set; }

        /// <summary>
        /// Explorer HAT led array
        /// </summary>
        public Lights Lights { get; private set; }

        /// <summary>
        /// Explorer HAT buffered, 5V tolerant digital inputs. Each pin is opened the first time it is read
        /// </summary>
        public Inputs Inputs { get; private set; }

        /// <summary>
        /// Explorer HAT open collector outputs. Each pin is opened the first time it is switched on or off
        /// </summary>
        public Outputs Outputs { get; private set; }

        /// <summary>
        /// Explorer HAT analog inputs (ADS1015, from 0V to 5V). The I2C device is opened the first time an input is read
        /// </summary>
        public AnalogInputs Analog { get; private set; }

        /// <summary>
        /// Initialize <see cref="ExplorerHat"/> instance
        /// </summary>
        /// <param name="controller">GPIO controller for the motors, lights, inputs and outputs. If null, a new one is created</param>
        /// <param name="shouldDispose">True to dispose the controller when this instance is disposed</param>
        public ExplorerHat(GpioController? controller = null, bool shouldDispose = true)
            : this(controller, null, shouldDispose)
        {
        }

        /// <summary>
        /// Initialize <see cref="ExplorerHat"/> instance
        /// </summary>
        /// <param name="controller">GPIO controller for the motors, lights, inputs and outputs. If null, a new one is created</param>
        /// <param name="i2cBus">I2C bus of the analog inputs. If null, bus 1 is opened the first time an analog input is read</param>
        /// <param name="shouldDispose">True to dispose the controller and the I2C bus when this instance is disposed</param>
        public ExplorerHat(GpioController? controller, I2cBus? i2cBus, bool shouldDispose = true)
        {
            _shouldDispose = shouldDispose || controller is null;
            _controller = controller ?? new GpioController();
            _shouldDisposeI2cBus = shouldDispose || i2cBus is null;
            _i2cBus = i2cBus;

            // The controller belongs to this instance: motors and lights must not dispose it
            Motors = new Motors(_controller, shouldDispose: false);
            Lights = new Lights(_controller, shouldDispose: false);
            Inputs = new Inputs(_controller);
            Outputs = new Outputs(_controller);
            Analog = new AnalogInputs(() => GetI2cBus().CreateDevice(Ads1015Address));
        }

        private I2cBus GetI2cBus()
        {
            _i2cBus ??= I2cBus.Create(1);
            return _i2cBus;
        }

        /// <summary>
        /// Disposes the <see cref="ExplorerHat"/> instance
        /// </summary>
        public void Dispose()
        {
            // Motors first: their software PWM threads write to the controller until they are disposed
            Motors.Dispose();
            Outputs.Dispose();
            Lights.Dispose();
            Inputs.Dispose();
            Analog.Dispose();

            if (_shouldDispose)
            {
                _controller?.Dispose();
            }

            if (_shouldDisposeI2cBus)
            {
                _i2cBus?.Dispose();
            }

            _controller = null!;
            _i2cBus = null;
        }
    }
}
