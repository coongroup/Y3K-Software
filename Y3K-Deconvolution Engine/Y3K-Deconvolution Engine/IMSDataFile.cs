using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Engine
{
    public interface IMSDataFile : IEnumerable<IMSDataScan>, IDisposable, IEquatable<IMSDataFile>
    {
        void Open();
        string Name { get; }
        bool IsOpen { get; }
        int FirstSpectrumNumber { get; }
        int LastSpectrumNumber { get; }
        int GetMsnOrder(int spectrumNumber);
        double GetInjectionTime(int spectrumNumber);
        double GetPrecursorMz(int spectrumNumber, int msnOrder = 2);
        double GetRetentionTime(int spectrumNumber);
        DissociationType GetDissociationType(int spectrumNumber, int msnOrder = 2);
        Polarity GetPolarity(int spectrumNumber);
        ISpectrum GetSpectrum(int spectrumNumber);
        IMSDataScan this[int spectrumNumber] { get; }
    }

    public interface IMSDataFile<out TSpectrum> : IMSDataFile, IEnumerable<IMSDataScan<TSpectrum>>
        where TSpectrum : ISpectrum
    {
        new TSpectrum GetSpectrum(int spectrumNumber);
        new IMSDataScan<TSpectrum> this[int spectrumNumber] { get; }
    }
}
