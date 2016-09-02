using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Engine
{
    public interface IMsnDataScan : IMSDataScan
    {
        int GetParentSpectrumNumber();
        double GetPrecursorMz();
        DoubleRange GetIsolationRange();
        int GetPrecursorCharge();
        DissociationType GetDissociationType();
        double GetInjectionTime();
    }

    public interface IMsnDataScan<out TSpectrum> : IMsnDataScan
        where TSpectrum : ISpectrum
    {
        new TSpectrum MassSpectrum { get; }
    }
}
