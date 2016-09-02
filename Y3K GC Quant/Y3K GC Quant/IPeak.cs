﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_GC_Quant
{
    public interface IPeak : IComparable<double>, IComparable<IPeak>, IComparable
    {
        double X { get; }
        double Y { get; }
    }
}
