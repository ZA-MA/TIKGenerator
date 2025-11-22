using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIKGenerator.Models
{
    public class SignalGroup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public List<Signal> Signals { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
