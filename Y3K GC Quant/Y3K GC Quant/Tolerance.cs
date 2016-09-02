using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Y3K_GC_Quant
{
    public class Tolerance
    {
        private static readonly Regex StringRegex = new Regex(@"(\+-|-\+|±)?\s*([\d.]+)\s*(PPM|DA|MMU)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Tolerance(ToleranceUnit unit, double value, ToleranceType type = ToleranceType.PlusAndMinus)
        {
            Unit = unit;
            Value = value;
            Type = type;
        }

        public Tolerance(ToleranceUnit unit, double experimental, double theoretical, ToleranceType type = ToleranceType.PlusAndMinus)
            : this(unit, GetTolerance(experimental, theoretical, unit), type)
        {
        }

        public Tolerance(string s)
        {
            Match m = StringRegex.Match(s);
            if (!m.Success)
                throw new ArgumentException("Input string is not in the correct format: " + s);
            Type = m.Groups[1].Success ? ToleranceType.PlusAndMinus : ToleranceType.FullWidth;
            Value = double.Parse(m.Groups[2].Value);
            ToleranceUnit type;
            Enum.TryParse(m.Groups[3].Value, true, out type);
            Unit = type;
        }

        public ToleranceUnit Unit { get; set; }

        public double Value { get; set; }

        public ToleranceType Type { get; set; }

        public DoubleRange GetRange(double mean)
        {
            double value = Value * ((Type == ToleranceType.PlusAndMinus) ? 2 : 1);

            double tol;
            switch (Unit)
            {
                case ToleranceUnit.MMU:
                    tol = value / 2000.0;
                    break;

                case ToleranceUnit.PPM:
                    tol = value * mean / 2e6;
                    break;

                default:
                    tol = value / 2.0;
                    break;
            }
            return new DoubleRange(mean - tol, mean + tol);
        }

        public double GetMinimumValue(double mean)
        {
            double value = Value * ((Type == ToleranceType.PlusAndMinus) ? 2 : 1);

            switch (Unit)
            {
                case ToleranceUnit.MMU:
                    return mean - value / 2000.0;

                case ToleranceUnit.PPM:
                    return mean * (1 - (value / 2e6));

                default:
                    return mean - value / 2.0;
            }
        }

        public double GetMaximumValue(double mean)
        {
            double value = Value * ((Type == ToleranceType.PlusAndMinus) ? 2 : 1);

            switch (Unit)
            {
                case ToleranceUnit.MMU:
                    return mean + value / 2000.0;

                case ToleranceUnit.PPM:
                    return mean * (1 + (value / 2e6));

                default:
                    return mean + value / 2.0;
            }
        }

        public bool Within(double experimental, double theoretical)
        {
            double tolerance = Math.Abs(GetTolerance(experimental, theoretical, Unit));
            double value = (Type == ToleranceType.PlusAndMinus) ? Value : Value / 2;
            return tolerance <= value;
        }

        public override string ToString()
        {
            return string.Format("{0}{1:f4} {2}", (Type == ToleranceType.PlusAndMinus) ? "±" : "", Value, Enum.GetName(typeof(ToleranceUnit), Unit));
        }

        public static double GetTolerance(double experimental, double theoretical, ToleranceUnit type)
        {
            switch (type)
            {
                case ToleranceUnit.MMU:
                    return (experimental - theoretical) * 1000.0;

                case ToleranceUnit.PPM:
                    return (experimental - theoretical) / theoretical * 1e6;

                default:
                    return experimental - theoretical;
            }
        }

        public static Tolerance FromPPM(double value, ToleranceType toleranceType = ToleranceType.PlusAndMinus)
        {
            return new Tolerance(ToleranceUnit.PPM, value, toleranceType);
        }

        public static Tolerance FromDA(double value, ToleranceType toleranceType = ToleranceType.PlusAndMinus)
        {
            return new Tolerance(ToleranceUnit.DA, value, toleranceType);
        }

        public static Tolerance FromMMU(double value, ToleranceType toleranceType = ToleranceType.PlusAndMinus)
        {
            return new Tolerance(ToleranceUnit.MMU, value, toleranceType);
        }

        public static Tolerance CalculatePrecursorMassError(double theoreticalMass, double observedMass, out int nominalMassOffset, out double adjustedObservedMass, double difference = Constants.C13C12Difference,
            ToleranceUnit type = ToleranceUnit.PPM)
        {
            double massError = observedMass - theoreticalMass;
            nominalMassOffset = (int)Math.Round(massError / difference);
            double massOffset = nominalMassOffset * difference;
            adjustedObservedMass = observedMass - massOffset;
            return new Tolerance(type, adjustedObservedMass, theoreticalMass);
        }
    }

    public enum ToleranceType
    {
        PlusAndMinus,
        FullWidth
    }

    public enum ToleranceUnit
    {
        PPM,
        DA,
        MMU
    }
}
