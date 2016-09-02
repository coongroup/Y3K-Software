using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_GC_Quant
{
    public interface ISpectrum : IEnumerable<MZPeak>
    {
        int Count { get; }

        double FirstMZ { get; }

        double LastMZ { get; }

        double TotalIonCurrent { get; }

        double GetMass(int index);

        double GetIntensity(int index);

        double[] GetMasses();

        double[] GetIntensities();

        double GetBasePeakIntensity();

        double GetTotalIonCurrent();

        bool TryGetIntensities(double minMZ, double maxMZ, out double intensity);

        bool TryGetIntensities(IRange<double> rangeMZ, out double intensity);

        byte[] ToBytes(bool zlibCompressed);

        bool ContainsPeak(double minMZ, double maxMZ);

        bool ContainsPeak(IRange<double> range);

        bool ContainsPeak();

        double[,] ToArray();

        MZPeak GetPeak(int index);

        MZPeak GetClosestPeak(double minMZ, double maxMZ);

        MZPeak GetClosestPeak(IRange<double> rangeMZ);

        ISpectrum Extract(IRange<double> mzRange);

        ISpectrum Extract(double minMZ, double maxMZ);

        ISpectrum FilterByMZ(IEnumerable<IRange<double>> mzRanges);

        ISpectrum FilterByMZ(IRange<double> mzRange);

        ISpectrum FilterByMZ(double minMZ, double maxMZ);

        ISpectrum FilterByIntensity(double minIntensity, double maxIntensity);

        ISpectrum FilterByIntensity(IRange<double> intenistyRange);
    }

    public interface ISpectrum<out TPeak> : ISpectrum
        where TPeak : IPeak
    {
        new TPeak GetPeak(int index);

        new TPeak GetClosestPeak(double minMZ, double maxMZ);

        new TPeak GetClosestPeak(IRange<double> rangeMZ);

        new ISpectrum<TPeak> Extract(IRange<double> mzRange);

        new ISpectrum<TPeak> Extract(double minMZ, double maxMZ);

        new ISpectrum<TPeak> FilterByMZ(IEnumerable<IRange<double>> mzRanges);

        new ISpectrum<TPeak> FilterByMZ(IRange<double> mzRange);

        new ISpectrum<TPeak> FilterByMZ(double minMZ, double maxMZ);

        new ISpectrum<TPeak> FilterByIntensity(double minIntensity, double maxIntensity);

        new ISpectrum<TPeak> FilterByIntensity(IRange<double> intenistyRange);
    }
}
