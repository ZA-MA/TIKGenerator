using System;
using System.Linq;
using System.Numerics;
using TIKGenerator.Models;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace TIKGenerator.Services
{
    public interface ISignalProcessingService
    {
        double[] ProcessSignal(double[] signal, SignalProcessingModel model, double sampleRate);

        double[] ApplyNone(double[] signal);
        double[] ApplyLowPass(double[] signal, double cutoffHz, double sampleRate);
        double[] ApplyHighPass(double[] signal, double cutoffHz, double sampleRate);
        double[] ApplyBandPass(double[] signal, double lowHz, double highHz, double sampleRate);
        double[] ApplyBandStop(double[] signal, double lowHz, double highHz, double sampleRate);
        double[] ApplyMovingAverage(double[] signal, int window);
        double[] ApplyExponentialSmoothing(double[] signal, double alpha);
        double[] ApplyFFT(double[] signal, double sampleRate);
        double[] ApplyNormalization(double[] signal);
        double[] ApplyOverlay(double[][] signals);
    }

    public class SignalProcessingService : ISignalProcessingService
    {
        public double[] ProcessSignal(double[] signal, SignalProcessingModel model, double sampleRate)
        {
            if (signal == null || signal.Length == 0) return signal;

            return model.SelectedMethod switch
            {
                ProcessingMethod.None => ApplyNone(signal),
                ProcessingMethod.LowPass when model.LowPassCutoff.HasValue => ApplyLowPass(signal, model.LowPassCutoff.Value, sampleRate),
                ProcessingMethod.HighPass when model.HighPassCutoff.HasValue => ApplyHighPass(signal, model.HighPassCutoff.Value, sampleRate),
                ProcessingMethod.BandPass when model.BandPassLow.HasValue && model.BandPassHigh.HasValue => ApplyBandPass(signal, model.BandPassLow.Value, model.BandPassHigh.Value, sampleRate),
                ProcessingMethod.BandStop when model.BandStopLow.HasValue && model.BandStopHigh.HasValue => ApplyBandStop(signal, model.BandStopLow.Value, model.BandStopHigh.Value, sampleRate),
                ProcessingMethod.MovingAverage when model.MovingAverageWindow.HasValue => ApplyMovingAverage(signal, model.MovingAverageWindow.Value),
                ProcessingMethod.ExponentialSmoothing when model.ExponentialAlpha.HasValue => ApplyExponentialSmoothing(signal, model.ExponentialAlpha.Value),
                ProcessingMethod.FFT => ApplyFFT(signal, sampleRate),
                ProcessingMethod.Normalization => ApplyNormalization(signal),
                _ => signal
            };
        }

        public double[] ApplyNone(double[] signal) => signal;

        public double[] ApplyLowPass(double[] signal, double cutoffHz, double sampleRate)
        {
            if (signal == null || signal.Length == 0) return signal;

            double[] result = new double[signal.Length];
            double rc = 1.0 / (2 * Math.PI * cutoffHz);
            double dt = 1.0 / sampleRate;
            double alpha = dt / (rc + dt);

            result[0] = signal[0];
            for (int i = 1; i < signal.Length; i++)
            {
                result[i] = alpha * signal[i] + (1 - alpha) * result[i - 1];
            }
            return result;
        }

        public double[] ApplyHighPass(double[] signal, double cutoffHz, double sampleRate)
        {
            if (signal == null || signal.Length == 0) return signal;

            double[] result = new double[signal.Length];
            double rc = 1.0 / (2 * Math.PI * cutoffHz);
            double dt = 1.0 / sampleRate;
            double alpha = rc / (rc + dt);

            result[0] = signal[0];
            for (int i = 1; i < signal.Length; i++)
            {
                result[i] = alpha * (result[i - 1] + signal[i] - signal[i - 1]);
            }
            return result;
        }


        public double[] ApplyBandPass(double[] signal, double lowHz, double highHz, double sampleRate)
        {
            if (signal == null || signal.Length == 0) return signal;
            if (lowHz >= highHz) throw new ArgumentException("Low frequency must be less than high frequency.");

            var highPassed = ApplyHighPass(signal, lowHz, sampleRate);

            var bandPassed = ApplyLowPass(highPassed, highHz, sampleRate);

            return bandPassed;
        }

        public double[] ApplyBandStop(double[] signal, double lowHz, double highHz, double sampleRate)
        {
            if (signal == null || signal.Length == 0) return signal;
            if (lowHz >= highHz) throw new ArgumentException("Low frequency must be less than high frequency.");

            var bandPass = ApplyBandPass(signal, lowHz, highHz, sampleRate);

            double[] result = new double[signal.Length];
            for (int i = 0; i < signal.Length; i++)
            {
                result[i] = signal[i] - bandPass[i];
            }
            return result;
        }

        public double[] ApplyMovingAverage(double[] signal, int window)
        {
            if (window <= 1) return signal;
            double[] result = new double[signal.Length];
            double sum = 0;
            for (int i = 0; i < signal.Length; i++)
            {
                sum += signal[i];
                if (i >= window) sum -= signal[i - window];
                result[i] = sum / Math.Min(window, i + 1);
            }
            return result;
        }

        public double[] ApplyExponentialSmoothing(double[] signal, double alpha)
        {
            if (signal.Length == 0) return signal;
            double[] result = new double[signal.Length];
            result[0] = signal[0];
            for (int i = 1; i < signal.Length; i++)
                result[i] = alpha * signal[i] + (1 - alpha) * result[i - 1];
            return result;
        }

        public double[] ApplyFFT(double[] signal, double sampleRate)
        {
            if (signal == null || signal.Length == 0) return signal;

            Complex[] complexSignal = signal.Select(v => new Complex(v, 0)).ToArray();

            Fourier.Forward(complexSignal, FourierOptions.Matlab);

            double[] magnitude = new double[complexSignal.Length / 2];
            for (int i = 0; i < magnitude.Length; i++)
            {
                magnitude[i] = 2.0 / signal.Length * complexSignal[i].Magnitude;
            }

            return magnitude;
        }

        public double[] ApplyNormalization(double[] signal)
        {
            if (signal.Length == 0) return signal;
            double max = signal.Max();
            double min = signal.Min();
            if (Math.Abs(max - min) < 1e-10) return signal;
            return signal.Select(v => (v - min) / (max - min)).ToArray();
        }

        public double[] ApplyOverlay(double[][] signals)
        {
            if (signals == null || signals.Length == 0)
                return Array.Empty<double>();

            int length = signals[0].Length;
            double[] result = new double[length];

            for (int i = 0; i < length; i++)
            {
                double sum = 0;
                for (int s = 0; s < signals.Length; s++)
                    sum += signals[s][i];

                result[i] = sum;
            }

            return result;
        }
    }
}
