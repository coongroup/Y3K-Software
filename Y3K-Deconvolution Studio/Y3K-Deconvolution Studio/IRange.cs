using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Studio
{
    public interface IRange<T> : IEquatable<IRange<T>> where T : IComparable<T>
    {
        T Minimum { get; }
        T Maximum { get; }
        bool Contains(T item);
        int CompareTo(T item);
        bool IsSubRange(IRange<T> other);
        bool IsSuperRange(IRange<T> other);
        bool IsOverlapping(IRange<T> other);
    }
}
