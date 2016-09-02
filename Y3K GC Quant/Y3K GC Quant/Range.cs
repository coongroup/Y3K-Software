using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_GC_Quant
{
    public class Range<T> : IRange<T> where T : IComparable<T>, IEquatable<T>
    {
        public Range()
            : this(default(T), default(T))
        {
        }

        public Range(IRange<T> range)
            : this(range.Minimum, range.Maximum)
        {
        }

        public Range(T minimum, T maximum)
        {
            if (maximum.CompareTo(minimum) < 0)
                throw new ArgumentException(minimum + " > " + maximum + ", unable to create negative ranges.");

            Minimum = minimum;
            Maximum = maximum;
        }

        public T Maximum { get; protected set; }

        public T Minimum { get; protected set; }

        public int CompareTo(T item)
        {
            if (Minimum.CompareTo(item) > 0)
                return -1;
            if (Maximum.CompareTo(item) < 0)
                return 1;
            return 0;
        }

        public bool IsSuperRange(IRange<T> other)
        {
            if (other == null)
                return false;

            return Maximum.CompareTo(other.Maximum) >= 0 && Minimum.CompareTo(other.Minimum) <= 0;
        }

        public bool IsSubRange(IRange<T> other)
        {
            if (other == null)
                return false;

            return Maximum.CompareTo(other.Maximum) <= 0 && Minimum.CompareTo(other.Minimum) >= 0;
        }

        public bool IsOverlapping(IRange<T> other)
        {
            if (other == null)
                return false;

            return Maximum.CompareTo(other.Minimum) >= 0 && Minimum.CompareTo(other.Maximum) <= 0;
        }

        public bool Contains(T item)
        {
            return CompareTo(item).Equals(0);
        }

        public override string ToString()
        {
            return string.Format("[{0} - {1}]", Minimum, Maximum);
        }

        public bool Equals(IRange<T> other)
        {
            if (other == null)
                return false;

            return Maximum.Equals(other.Maximum) && Minimum.Equals(other.Minimum);
        }

        public override int GetHashCode()
        {
            return Minimum.GetHashCode() + (Maximum.GetHashCode() << 3);
        }

        public override bool Equals(object obj)
        {
            IRange<T> other = obj as IRange<T>;

            return other != null && Equals(other);
        }
    }
}
