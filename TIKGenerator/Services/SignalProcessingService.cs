using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TIKGenerator.Models;

namespace TIKGenerator.Services
{
    public interface ISignalProcessingService
    {
        double[] ProcessSignal(double[] signal, SignalProcessingOptions options, double samplingRate);

        double[] ApplyNone(double[] signal);

        double[] ApplyLowPass(double[] signal, double cutoffHz, double samplingRate);
        double[] ApplyHighPass(double[] signal, double cutoffHz, double samplingRate);
        double[] ApplyBandPass(double[] signal, double lowHz, double highHz, double samplingRate);
        double[] ApplyBandStop(double[] signal, double lowHz, double highHz, double samplingRate);

        double[] ApplyMovingAverage(double[] signal, int window);
        double[] ApplyExponentialSmoothing(double[] signal, double alpha);

        double[] ApplyFFTFilter(double[] signal, double lowHz, double highHz, double samplingRate);
    }

    public class SignalProcessingService : ISignalProcessingService
    {
        public double[] ProcessSignal(double[] signal, SignalProcessingOptions options, double samplingRate)
        {
            if (signal == null || signal.Length == 0)
                return signal;

            double[] result = options.SelectedProcessingType switch
            {
                SignalProcessingType.None => ApplyNone(signal),
                SignalProcessingType.LowPass when options.LowPassCutoff is double lp => ApplyLowPass(signal, lp, samplingRate),
                SignalProcessingType.HighPass when options.HighPassCutoff is double hp => ApplyHighPass(signal, hp, samplingRate),
                SignalProcessingType.BandPass when options.BandPassLow is double bpl && options.BandPassHigh is double bph => ApplyBandPass(signal, bpl, bph, samplingRate),
                SignalProcessingType.BandStop when options.BandStopLow is double bsl && options.BandStopHigh is double bsh => ApplyBandStop(signal, bsl, bsh, samplingRate),
                SignalProcessingType.MovingAverage when options.MovingAverageWindow is int win => ApplyMovingAverage(signal, win),
                SignalProcessingType.ExponentialSmoothing when options.ExponentialAlpha is double alpha => ApplyExponentialSmoothing(signal, alpha),
                _ => signal
            };

            return result;
        }

        public double[] ApplyNone(double[] signal) => signal;

        public double[] ApplyLowPass(double[] signal, double cutoffHz, double samplingRate)
        {
            int n = signal.Length;
            if (n == 0) return signal;

            double[] result = new double[n];

            cutoffHz = Math.Max(0.001, Math.Min(cutoffHz, samplingRate / 2.1));

            double wc = 2.0 * Math.PI * cutoffHz;
            double T = 1.0 / samplingRate;
            double wcT = wc * T;

            double k = Math.Tan(wcT / 2.0);
            double k2 = k * k;
            double sqrt2 = Math.Sqrt(2);

            double norm = 1.0 / (1.0 + sqrt2 * k + k2);
            double b0 = k2 * norm;
            double b1 = 2.0 * b0;
            double b2 = b0;
            double a1 = 2.0 * (k2 - 1.0) * norm;
            double a2 = (1.0 - sqrt2 * k + k2) * norm;

            double x1 = 0, x2 = 0;
            double y1 = 0, y2 = 0;

            for (int i = 0; i < n; i++)
            {
                double x0 = signal[i];

                double y0 = b0 * x0 + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;

                result[i] = y0;

                x2 = x1;
                x1 = x0;
                y2 = y1;
                y1 = y0;
            }

            return result;
        }

        public double[] ApplyHighPass(double[] signal, double cutoffHz, double samplingRate)
        {
            int n = signal.Length;
            if (n == 0) return signal;

            double[] result = new double[n];

            cutoffHz = Math.Max(0.001, Math.Min(cutoffHz, samplingRate / 2.1));

            double wc = 2.0 * Math.PI * cutoffHz;
            double T = 1.0 / samplingRate;
            double wcT = wc * T;

            double k = Math.Tan(wcT / 2.0);
            double k2 = k * k;
            double sqrt2 = Math.Sqrt(2);

            double norm = 1.0 / (1.0 + sqrt2 * k + k2);
            double b0 = 1.0 * norm;
            double b1 = -2.0 * b0;
            double b2 = b0;
            double a1 = 2.0 * (k2 - 1.0) * norm;
            double a2 = (1.0 - sqrt2 * k + k2) * norm;

            double x1 = 0, x2 = 0;
            double y1 = 0, y2 = 0;

            for (int i = 0; i < n; i++)
            {
                double x0 = signal[i];
                double y0 = b0 * x0 + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;

                result[i] = y0;

                x2 = x1;
                x1 = x0;
                y2 = y1;
                y1 = y0;
            }

            return result;
        }

        public double[] ApplyBandPass(double[] signal, double lowHz, double highHz, double samplingRate)
        {
            double[] lowPassed = ApplyLowPass(signal, highHz, samplingRate);
            return ApplyHighPass(lowPassed, lowHz, samplingRate);
        }

        public double[] ApplyBandStop(double[] signal, double lowHz, double highHz, double samplingRate)
        {
            int n = signal.Length;
            if (n == 0) return signal;

            double[] result = new double[n];

            double centerFreq = (lowHz + highHz) / 2.0;
            double bandwidth = highHz - lowHz;

            double[] bandPass = ApplyBandPass(signal, lowHz, highHz, samplingRate);

            for (int i = 0; i < n; i++)
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

        public double[] ApplyFFTFilter(double[] signal, double lowHz, double highHz, double samplingRate)
        {
            int n = signal.Length;
            Complex[] spectrum = new Complex[n];
            for (int i = 0; i < n; i++)
                spectrum[i] = new Complex(signal[i], 0);

            Fourier.Forward(spectrum, FourierOptions.Matlab);

            double freqResolution = samplingRate / n;
            for (int i = 0; i < n; i++)
            {
                double freq = i * freqResolution;
                if (freq > samplingRate / 2)
                    freq = samplingRate - freq;

                if (freq < lowHz || freq > highHz)
                    spectrum[i] = Complex.Zero;
            }

            Fourier.Inverse(spectrum, FourierOptions.Matlab);

            double[] result = new double[n];
            for (int i = 0; i < n; i++)
                result[i] = spectrum[i].Real / n;

            return result;
        }
    }
}