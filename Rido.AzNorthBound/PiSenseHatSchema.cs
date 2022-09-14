using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rido.AzNorthBound
{
    internal class PiSenseHatSchema : BaseSchema
    {
        public double t1 { get; set; }
        public double t2 { get; set; }
        public double h { get; set; }
        public double p { get; set; }
        public double m { get; set; }
    }
}
