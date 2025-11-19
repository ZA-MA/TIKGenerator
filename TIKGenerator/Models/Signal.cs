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
        public decimal Amplitude { get; set; }
        public decimal Frequency { get; set; }
        public decimal Phase { get; set; }
        public decimal NumberOfPoints { get; set; }
        public decimal TimeStart { get; set; } = 0m;
        public decimal TimeEnd { get; set; } = 1m;
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
