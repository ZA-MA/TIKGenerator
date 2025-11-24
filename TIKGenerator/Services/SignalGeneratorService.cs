using System;
using TIKGenerator.Models;

namespace TIKGenerator.Services
{
    public interface ISignalGeneratorService
    {
        double[] Generate(Signal s, double dt);
    }

    public class SignalGeneratorService : ISignalGeneratorService
    {
        public double[] Generate(Signal s, double dt)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            int points = Math.Max(1, s.NumberOfPoints);
            double[] data = new double[points];

            for (int i = 0; i < points; i++)
            {
                double t = s.TimeStart + i * dt;

                switch (s.Type)
                {
                    case SignalType.SineWave:
                        data[i] = s.Amplitude * Math.Sin(2 * Math.PI * s.Frequency * t + s.Phase);
                        break;

                    case SignalType.Meander:
                        data[i] = s.Amplitude * ((Math.Floor(2 * s.Frequency * t) % 2 == 0) ? 1 : -1);
                        break;

                    case SignalType.Triangular:
                        data[i] = 2 * s.Amplitude * Math.Abs(2 * (t * s.Frequency - Math.Floor(t * s.Frequency + 0.5))) - s.Amplitude;
                        break;

                    case SignalType.Sawtooth:
                        data[i] = 2 * s.Amplitude * (t * s.Frequency - Math.Floor(t * s.Frequency)) - s.Amplitude;
                        break;

                    default:
                        data[i] = 0;
                        break;
                }

            }


            return data;
        }
    }
}
