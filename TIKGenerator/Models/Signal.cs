using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIKGenerator.Models
{
    public class Signal
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public double Amplitude { get; set; }
        public double Frequency { get; set; }
        public double Phase { get; set; }
        public int NumberOfPoints { get; set; }
        public double TimeStart { get; set; } = 0;
        public double TimeEnd { get; set; } = 10;
        public SignalType Type { get; set; }
    }

    public enum SignalType
    {
        SineWave,
        Meander,
        Triangular,
        Sawtooth
    }
}
