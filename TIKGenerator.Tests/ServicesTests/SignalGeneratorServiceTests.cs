using System;
using TIKGenerator.Models;
using TIKGenerator.Services;
using Xunit;

namespace TIKGenerator.Tests.ServicesTests
{
    public class SignalGeneratorServiceTests
    {
        private readonly SignalGeneratorService _service;

        public SignalGeneratorServiceTests()
        {
            _service = new SignalGeneratorService();
        }

        [Fact]
        public void Generate_ShouldThrow_WhenSignalIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _service.Generate(null, 0.01));
        }

        [Fact]
        public void Generate_ShouldReturnCorrectNumberOfPoints()
        {
            var signal = new Signal
            {
                NumberOfPoints = 5,
                Type = SignalType.SineWave
            };

            double[] result = _service.Generate(signal, 0.1);

            Assert.Equal(5, result.Length);
        }

        [Fact]
        public void Generate_SineWave_ShouldProduceExpectedValues()
        {
            var signal = new Signal
            {
                Type = SignalType.SineWave,
                Amplitude = 1.0,
                Frequency = 1.0,
                Phase = 0,
                TimeStart = 0,
                NumberOfPoints = 3
            };

            double dt = 0.25;
            var result = _service.Generate(signal, dt);

            double epsilon = 1e-6;
            double[] expected = { 0.0, 1.0, 0.0 };

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.True(Math.Abs(result[i] - expected[i]) < epsilon, $"SineWave: i={i} Expected {expected[i]}, got {result[i]}");
            }
        }

        [Fact]
        public void Generate_Meander_ShouldProduceExpectedValues_Classical()
        {
            var signal = new Signal
            {
                Type = SignalType.Meander,
                Amplitude = 2.0,
                Frequency = 1.0,
                Phase = 0,
                TimeStart = 0,
                NumberOfPoints = 4
            };

            double dt = 0.25;
            var result = _service.Generate(signal, dt);

            double[] expected = { 2.0, 2.0, -2.0, -2.0 };
            double epsilon = 1e-6;

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.True(Math.Abs(result[i] - expected[i]) < epsilon,
                    $"Meander: i={i} Expected {expected[i]}, got {result[i]}");
            }
        }

        [Fact]
        public void Generate_Triangular_ShouldProduceExpectedValues()
        {
            var signal = new Signal
            {
                Type = SignalType.Triangular,
                Amplitude = 1.0,
                Frequency = 1.0,
                Phase = 0,
                TimeStart = 0,
                NumberOfPoints = 5
            };

            double dt = 0.25;
            var result = _service.Generate(signal, dt);

            double epsilon = 1e-6;

            double[] expected = new double[5];
            for (int i = 0; i < 5; i++)
            {
                double t = i * dt;
                expected[i] = 2 * signal.Amplitude * Math.Abs(2 * (t * signal.Frequency - Math.Floor(t * signal.Frequency + 0.5))) - signal.Amplitude;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.True(Math.Abs(result[i] - expected[i]) < epsilon, $"Triangular: i={i} Expected {expected[i]}, got {result[i]}");
            }
        }

        [Fact]
        public void Generate_Sawtooth_ShouldProduceExpectedValues()
        {
            var signal = new Signal
            {
                Type = SignalType.Sawtooth,
                Amplitude = 1.0,
                Frequency = 1.0,
                Phase = 0,
                TimeStart = 0,
                NumberOfPoints = 5
            };

            double dt = 0.25;
            var result = _service.Generate(signal, dt);

            double epsilon = 1e-6;

            double[] expected = new double[5];
            for (int i = 0; i < 5; i++)
            {
                double t = i * dt;
                expected[i] = 2 * signal.Amplitude * (t * signal.Frequency - Math.Floor(t * signal.Frequency)) - signal.Amplitude;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.True(Math.Abs(result[i] - expected[i]) < epsilon, $"Sawtooth: i={i} Expected {expected[i]}, got {result[i]}");
            }
        }
    }
}
