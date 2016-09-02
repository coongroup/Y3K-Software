using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Engine
{
    public interface IPeak : IComparable<double>, IComparable<IPeak>, IComparable
    {
        double X { get; }
        double Y { get; }
    }
}
