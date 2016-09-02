using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Engine
{
    public class ProgressStatusEventArgs
    {
        public double Percent { get; private set; }

        public List<Feature> Features { get; private set; }

        public ProgressStatusEventArgs(double percent)
        {
            Percent = percent;
        }

        public ProgressStatusEventArgs(List<Feature> features)
        {
            Features = features;
        }

        public ProgressStatusEventArgs(List<Feature> features, double percent)
        {
            Features = features;
            Percent = percent;
        }
    }
}
