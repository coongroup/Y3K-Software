using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Engine
{
    public interface IMSDataScan : IMassSpectrum
    {
        int SpectrumNumber { get; }
        int MsnOrder { get; }
        double RetentionTime { get; }
        Polarity Polarity { get; }
        MZAnalyzerType MzAnalyzer { get; }
        DoubleRange MzRange { get; }
    }

    public interface IMSDataScan<out TSpectrum> : IMSDataScan
        where TSpectrum : ISpectrum
    {
        new TSpectrum MassSpectrum { get; }
    }
}
