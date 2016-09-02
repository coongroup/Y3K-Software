using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;

namespace Y3K_Deconvolution_Studio
{
    public static class CollectionExtension
    {
        public static double[] BoxCarSmooth(this double[] data, int points)
        {
            points = points - (1 - points % 2);

            int count = data.Length;

            if (points <= 0 || points > count)
            {
                return null;
            }

            int newCount = count - points + 1;

            double[] smoothedData = new double[newCount];

            for (int i = 0; i < newCount; i++)
            {
                double value = 0;

                for (int j = i; j < i + points; j++)
                {
                    value += data[j];
                }

                smoothedData[i] = value / points;
            }
            return smoothedData;
        }

        public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static double Median(this List<double> values)
        {
            int length = values.Count;

            if (length == 0)
                return 0;

            values.Sort();

            int mid = length / 2;
            if (length % 2 != 0)
            {
                return values[mid];
            }

            return (values[mid] + values[mid - 1]) / 2.0;
        }

        public static double StdDev(this IList<double> values)
        {
            int length = values.Count;

            if (length == 0)
                return 0;

            double mean = values.Average();
            double stdDev = values.Sum(value => (value - mean) * (value - mean));
            return Math.Sqrt(stdDev / values.Count);
        }

        public static byte[] GetBytes(this double[] values)
        {
            if (values == null)
                return null;
            var result = new byte[values.Length * sizeof(double)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }

        public static double[] GetDoubles(this byte[] bytes)
        {
            var result = new double[bytes.Length / sizeof(double)];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
            return result;
        }

        public static string ToCsvString(this IEnumerable<string> values, char delimiter = ',')
        {
            StringBuilder sb = new StringBuilder();
            foreach (string value in values)
            {
                if (value.Contains(delimiter))
                {
                    sb.Append("\"" + value + "\"");
                }
                else
                {
                    sb.Append(value);
                }
                sb.Append(',');
            }
            if (sb.Length > 1)
                sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static int[] Histogram(this IList<double> values, int numberOfBins, out double min, out double max, out double binSize)
        {
            max = values.Max();
            min = values.Min();
            double range = max - min;
            binSize = range / numberOfBins;
            int[] bins = new int[numberOfBins];

            foreach (double value in values)
            {
                int binnedValue = (int)((value - min) / binSize); // (int)Math.Floor((value - min) / binSize);
                if (binnedValue == numberOfBins)
                    binnedValue--;
                bins[binnedValue]++;
            }
            return bins;
        }

        public static int[] Histogram(this IList<double> values, int numberOfBins, double min, double max, out double binSize)
        {
            double range = max - min;
            binSize = range / numberOfBins;
            int[] bins = new int[numberOfBins];

            foreach (double value in values)
            {
                if (value < min || value > max)
                    continue;
                int binnedValue = (int)((value - min) / binSize);
                if (binnedValue == numberOfBins)
                    binnedValue--;
                bins[binnedValue]++;
            }
            return bins;
        }

        public static int MaxIndex<TSource>(this IEnumerable<TSource> items) where TSource : IComparable<TSource>
        {
            TSource maxItem;
            return MaxIndex(items, o => o, out maxItem);
        }

        public static int MaxIndex<TSource>(this IEnumerable<TSource> items, out TSource maxItem) where TSource : IComparable<TSource>
        {
            return MaxIndex(items, o => o, out maxItem);
        }

        public static int MaxIndex<TSource, TResult>(this IEnumerable<TSource> items, Func<TSource, TResult> selectFunc) where TResult : IComparable<TResult>
        {
            TSource maxItem;
            return MaxIndex(items, selectFunc, out maxItem);
        }

        public static int MaxIndex<TSource, TResult>(this IEnumerable<TSource> items, Func<TSource, TResult> selectFunc, out TSource maxItem) where TResult : IComparable<TResult>
        {
            // From: http://stackoverflow.com/questions/462699/how-do-i-get-the-index-of-the-highest-value-in-an-array-using-linq
            int maxIndex = -1;
            TResult maxValue = default(TResult);
            maxItem = default(TSource);

            int index = 0;
            foreach (TSource item in items)
            {
                TResult value = selectFunc(item);

                if (value.CompareTo(maxValue) > 0 || maxIndex == -1)
                {
                    maxIndex = index;
                    maxItem = item;
                    maxValue = value;
                }
                index++;
            }
            return maxIndex;
        }

        public static int BinarySearch<T>(this IList<T> list, int index, int length, T value, IComparer<T> comparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (index < 0 || length < 0)
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "length");
            if (list.Count - index < length)
                throw new ArgumentException();

            int lower = index;
            int upper = (index + length - 1);
            while (lower <= upper)
            {
                int adjustedIndex = lower + ((upper - lower) >> 1);
                int comparison = comparer.Compare(list[adjustedIndex], value);
                if (comparison == 0)
                    return adjustedIndex;
                if (comparison < 0)
                    lower = adjustedIndex + 1;
                else
                    upper = adjustedIndex - 1;
            }
            return ~lower;
        }

        public static int BinarySearch<T>(this IList<T> list, T value, IComparer<T> comparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            return list.BinarySearch(0, list.Count, value, comparer);
        }

        public static int BinarySearch<T>(this IList<T> list, T value) where T : IComparer<T>
        {
            return list.BinarySearch(value, Comparer<T>.Default);
        }
    }

    public static class ByteArrayExtension
    {
        public static byte[] Compress(this byte[] bytes)
        {
            var compressedStream = new MemoryStream();
            using (var stream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                new MemoryStream(bytes).CopyTo(stream);
            }
            return compressedStream.ToArray();
        }

        public static byte[] Decompress(this byte[] bytes)
        {
            var bigStreamOut = new MemoryStream();
            new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress).CopyTo(bigStreamOut);
            return bigStreamOut.ToArray();
        }

        public static bool IsCompressed(this byte[] bytes)
        {
            // From http://stackoverflow.com/questions/19364497/how-to-tell-if-a-byte-array-is-gzipped
            return bytes.Length >= 2 && bytes[0] == 31 && bytes[1] == 139;
        }
    }

    [Serializable]
    public abstract class Spectrum<TPeak, TSpectrum> : ISpectrum<TPeak>
        where TPeak : MZPeak
        where TSpectrum : Spectrum<TPeak, TSpectrum>
    {
        protected double[] Masses;

        protected double[] Intensities;

        public int Count { get; protected set; }

        public double FirstMZ
        {
            get { return Count == 0 ? 0 : Masses[0]; }
        }

        public double LastMZ
        {
            get { return Count == 0 ? 0 : Masses[Count - 1]; }
        }

        public double TotalIonCurrent
        {
            get { return Count == 0 ? 0 : GetTotalIonCurrent(); }
        }

        protected Spectrum(double[] mz, double[] intensities, bool shouldCopy = true)
        {
            Count = mz.Length;
            Masses = CopyData(mz, shouldCopy);
            Intensities = CopyData(intensities, shouldCopy);
        }

        protected Spectrum()
        {
            Count = 0;
        }

        protected Spectrum(ISpectrum spectrum)
            : this(spectrum.GetMasses(), spectrum.GetIntensities())
        {
        }

        protected Spectrum(double[,] mzintensities)
            : this(mzintensities, mzintensities.GetLength(1))
        {
        }

        protected Spectrum(double[,] mzintensities, int count)
        {
            int length = mzintensities.GetLength(1);

            Masses = new double[count];
            Intensities = new double[count];
            Buffer.BlockCopy(mzintensities, 0, Masses, 0, sizeof(double) * count);
            Buffer.BlockCopy(mzintensities, sizeof(double) * length, Intensities, 0, sizeof(double) * count);
            Count = count;
        }

        protected Spectrum(byte[] mzintensities)
        {
            Count = mzintensities.Length / (sizeof(double) * 2);
            int size = sizeof(double) * Count;
            Masses = new double[Count];
            Intensities = new double[Count];
            Buffer.BlockCopy(mzintensities, 0, Masses, 0, size);
            Buffer.BlockCopy(mzintensities, size, Intensities, 0, size);
        }

        public TPeak this[int index]
        {
            get { return GetPeak(index); }
        }

        public virtual double GetBasePeakIntensity()
        {
            return Count == 0 ? 0 : Intensities.Max();
        }

        /// <summary>
        /// Gets the full m/z range of this spectrum
        /// </summary>
        /// <returns></returns>
        public virtual MzRange GetMzRange()
        {
            return new MzRange(FirstMZ, LastMZ);
        }

        /// <summary>
        /// Gets a copy of the underlying m/z array
        /// </summary>
        /// <returns></returns>
        public virtual double[] GetMasses()
        {
            return CopyData(Masses);
        }

        /// <summary>
        /// Gets a copy of the underlying intensity array
        /// </summary>
        /// <returns></returns>
        public virtual double[] GetIntensities()
        {
            return CopyData(Intensities);
        }

        /// <summary>
        /// Converts the spectrum into a multi-dimensional array of doubles
        /// </summary>
        /// <returns></returns>
        public virtual double[,] ToArray()
        {
            double[,] data = new double[2, Count];
            const int size = sizeof(double);
            Buffer.BlockCopy(Masses, 0, data, 0, size * Count);
            Buffer.BlockCopy(Intensities, 0, data, size * Count, size * Count);
            return data;
        }

        /// <summary>
        /// Calculates the total ion current of this spectrum
        /// </summary>
        /// <returns>The total ion current of this spectrum</returns>
        public virtual double GetTotalIonCurrent()
        {
            return Count == 0 ? 0 : Intensities.Sum();
        }

        /// <summary>
        /// Gets the m/z value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual double GetMass(int index)
        {
            return Masses[index];
        }

        /// <summary>
        /// Gets the intensity value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual double GetIntensity(int index)
        {
            return Intensities[index];
        }

        /// <summary>
        /// Checks if this spectrum contains any peaks
        /// </summary>
        /// <returns></returns>
        public virtual bool ContainsPeak()
        {
            return Count > 0;
        }

        /// <summary>
        /// Checks if this spectrum contains any peaks within the range
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public virtual bool ContainsPeak(IRange<double> range)
        {
            return ContainsPeak(range.Minimum, range.Maximum);
        }

        /// <summary>
        /// Checks if this spectrum contains any peaks within the range
        /// </summary>
        /// <param name="minMZ">The minimum m/z (inclusive)</param>
        /// <param name="maxMZ">The maximum m/z (inclusive)</param>
        /// <returns></returns>
        public virtual bool ContainsPeak(double minMZ, double maxMZ)
        {
            if (Count == 0)
                return false;

            int index = Array.BinarySearch(Masses, minMZ);
            if (index >= 0)
                return true;

            index = ~index;

            return index < Count && Masses[index] <= maxMZ;
        }

        public virtual bool TryGetIntensities(IRange<double> rangeMZ, out double intensity)
        {
            return TryGetIntensities(rangeMZ.Minimum, rangeMZ.Maximum, out intensity);
        }

        public virtual bool TryGetIntensities(double minMZ, double maxMZ, out double intensity)
        {
            intensity = 0;

            if (Count == 0)
                return false;

            int index = GetPeakIndex(minMZ);

            while (index < Count && Masses[index] <= maxMZ)
                intensity += Intensities[index++];

            return intensity > 0.0;
        }

        public virtual TPeak GetBasePeak()
        {
            return GetPeak(Intensities.MaxIndex());
        }

        public virtual TPeak GetClosestPeak(IRange<double> massRange)
        {
            double mean = (massRange.Maximum + massRange.Minimum) / 2.0;
            double width = massRange.Maximum - massRange.Minimum;
            return GetClosestPeak(mean, width);
        }

        public virtual TPeak GetClosestPeak(double mean, double tolerance)
        {
            int index = GetClosestPeakIndex(mean, tolerance);

            return index >= 0 ? GetPeak(index) : default(TPeak);
        }

        public virtual bool TryGetPeaks(IRange<double> rangeMZ, out List<TPeak> peaks)
        {
            return TryGetPeaks(rangeMZ.Minimum, rangeMZ.Maximum, out peaks);
        }

        public virtual bool TryGetPeaks(double minMZ, double maxMZ, out List<TPeak> peaks)
        {
            peaks = new List<TPeak>();

            if (Count == 0)
                return false;

            int index = GetPeakIndex(minMZ);

            while (index < Count && Masses[index] <= maxMZ)
            {
                peaks.Add(GetPeak(index++));
            }

            return peaks.Count > 0;
        }

        public virtual string ToBase64String(bool zlibCompressed = false)
        {
            return Convert.ToBase64String(ToBytes(zlibCompressed));
        }

        public virtual byte[] ToBytes(bool zlibCompressed = false)
        {
            return ToBytes(zlibCompressed, Masses, Intensities);
        }

        /// <summary>
        /// Creates a clone of this spectrum with each mass transformed by some function
        /// </summary>
        /// <param name="convertor">The function to convert each mass by</param>
        /// <returns>A cloned spectrum with masses corrected</returns>
        public virtual TSpectrum CorrectMasses(Func<double, double> convertor)
        {
            TSpectrum newSpectrum = Clone();
            for (int i = 0; i < newSpectrum.Count; i++)
                newSpectrum.Masses[i] = convertor(newSpectrum.Masses[i]);
            return newSpectrum;
        }

        public virtual TSpectrum Extract(IRange<double> mzRange)
        {
            return Extract(mzRange.Minimum, mzRange.Maximum);
        }

        public virtual TSpectrum FilterByMZ(IRange<double> mzRange)
        {
            return FilterByMZ(mzRange.Minimum, mzRange.Maximum);
        }

        public virtual TSpectrum FilterByIntensity(IRange<double> intensityRange)
        {
            return FilterByIntensity(intensityRange.Minimum, intensityRange.Maximum);
        }

        #region ISpectrum

        MZPeak ISpectrum.GetClosestPeak(IRange<double> massRange)
        {
            return GetClosestPeak(massRange);
        }

        MZPeak ISpectrum.GetClosestPeak(double mean, double tolerance)
        {
            return GetClosestPeak(mean, tolerance);
        }

        ISpectrum ISpectrum.Extract(double minMZ, double maxMZ)
        {
            return Extract(minMZ, maxMZ);
        }

        ISpectrum ISpectrum.Extract(IRange<double> mzRange)
        {
            return Extract(mzRange.Minimum, mzRange.Maximum);
        }

        ISpectrum ISpectrum.FilterByMZ(IEnumerable<IRange<double>> mzRanges)
        {
            return FilterByMZ(mzRanges);
        }

        ISpectrum ISpectrum.FilterByMZ(IRange<double> mzRange)
        {
            return FilterByMZ(mzRange.Minimum, mzRange.Maximum);
        }

        ISpectrum ISpectrum.FilterByMZ(double minMZ, double maxMZ)
        {
            return FilterByMZ(minMZ, maxMZ);
        }

        ISpectrum ISpectrum.FilterByIntensity(double minIntensity, double maxIntensity)
        {
            return FilterByIntensity(minIntensity, maxIntensity);
        }

        ISpectrum ISpectrum.FilterByIntensity(IRange<double> intenistyRange)
        {
            return FilterByIntensity(intenistyRange.Minimum, intenistyRange.Maximum);
        }

        #endregion

        #region Abstract

        public abstract TPeak GetPeak(int index);

        public abstract TSpectrum Extract(double minMZ, double maxMZ);
        public abstract TSpectrum FilterByMZ(IEnumerable<IRange<double>> mzRanges);
        public abstract TSpectrum FilterByMZ(double minMZ, double maxMZ);
        public abstract TSpectrum FilterByIntensity(double minIntensity = 0, double maxIntensity = double.MaxValue);

        /// <summary>
        /// Returns a new deep clone of this spectrum.
        /// </summary>
        /// <returns></returns>
        public abstract TSpectrum Clone();

        #endregion Abstract

        #region Protected Methods

        protected double[] FromBytes(byte[] data, int index)
        {
            if (data.IsCompressed())
                data = data.Decompress();
            Count = data.Length / (sizeof(double) * 2);
            int size = sizeof(double) * Count;
            double[] outArray = new double[Count];
            Buffer.BlockCopy(data, index * size, outArray, 0, size);
            return outArray;
        }

        protected byte[] ToBytes(bool zlibCompressed, params double[][] arrays)
        {
            int length = Count * sizeof(double);
            int arrayCount = arrays.Length;
            byte[] bytes = new byte[length * arrayCount];
            int i = 0;
            foreach (double[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, bytes, length * i++, length);
            }

            if (zlibCompressed)
            {
                bytes = bytes.Compress();
            }

            return bytes;
        }

        /// <summary>
        /// Copies the source array to the destination array
        /// </summary>
        /// <typeparam name="TArray"></typeparam>
        /// <param name="sourceArray">The source array to copy from</param>
        /// <param name="deepCopy">If true, a new array will be generate, else references are copied</param>
        protected TArray[] CopyData<TArray>(TArray[] sourceArray, bool deepCopy = true) where TArray : struct
        {
            if (sourceArray == null)
                return null;
            if (sourceArray.Length != Count)
                throw new ArgumentException("Mismatched array size");
            if (!deepCopy)
            {
                return sourceArray;
            }
            int count = sourceArray.Length;
            TArray[] dstArray = new TArray[Count];
            Type type = typeof(TArray);
            Buffer.BlockCopy(sourceArray, 0, dstArray, 0, count * Marshal.SizeOf(type));
            return dstArray;
        }

        protected int GetClosestPeakIndex(double meanMZ, double tolerance)
        {
            if (Count == 0)
                return -1;

            int index = Array.BinarySearch(Masses, meanMZ);

            if (index >= 0)
                return index;

            index = ~index;

            int indexm1 = index - 1;

            double minMZ = meanMZ - tolerance;
            double maxMZ = meanMZ + tolerance;
            if (index >= Count)
            {
                // only the indexm1 peak can be closer

                if (indexm1 >= 0 && Masses[indexm1] >= minMZ)
                {
                    return indexm1;
                }

                return -1;
            }
            if (index == 0)
            {
                // only the index can be closer
                if (Masses[index] <= maxMZ)
                {
                    return index;
                }

                return -1;
            }

            double p1 = Masses[indexm1];
            double p2 = Masses[index];

            if (p2 > maxMZ)
            {
                if (p1 >= minMZ)
                    return indexm1;
                return -1;
            }
            if (p1 >= minMZ)
            {
                if (meanMZ - p1 > p2 - meanMZ)
                    return index;
                return indexm1;
            }
            return index;
        }

        protected int GetPeakIndex(double mz)
        {
            int index = Array.BinarySearch(Masses, mz);

            if (index >= 0)
                return index;

            return ~index;
        }

        #endregion Protected Methods

        public override string ToString()
        {
            return string.Format("{0} (Peaks {1})", GetMzRange(), Count);
        }

        public IEnumerator<TPeak> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return GetPeak(i);
            }
        }

        ISpectrum<TPeak> ISpectrum<TPeak>.Extract(IRange<double> mzRange)
        {
            return Extract(mzRange);
        }

        ISpectrum<TPeak> ISpectrum<TPeak>.Extract(double minMZ, double maxMZ)
        {
            return Extract(minMZ, maxMZ);
        }

        ISpectrum<TPeak> ISpectrum<TPeak>.FilterByMZ(IEnumerable<IRange<double>> mzRanges)
        {
            return FilterByMZ(mzRanges);
        }

        ISpectrum<TPeak> ISpectrum<TPeak>.FilterByMZ(IRange<double> mzRange)
        {
            return FilterByMZ(mzRange);
        }

        ISpectrum<TPeak> ISpectrum<TPeak>.FilterByMZ(double minMZ, double maxMZ)
        {
            return FilterByMZ(minMZ, maxMZ);
        }

        ISpectrum<TPeak> ISpectrum<TPeak>.FilterByIntensity(double minIntensity, double maxIntensity)
        {
            return FilterByIntensity(minIntensity, maxIntensity);
        }

        ISpectrum<TPeak> ISpectrum<TPeak>.FilterByIntensity(IRange<double> intenistyRange)
        {
            return FilterByIntensity(intenistyRange);
        }

        MZPeak ISpectrum.GetPeak(int index)
        {
            return GetPeak(index);
        }

        IEnumerator<MZPeak> IEnumerable<MZPeak>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
