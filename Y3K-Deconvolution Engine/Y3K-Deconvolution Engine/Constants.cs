using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Engine
{
    public static class Constants
    {
        public const double Electron = 0.00054857990946;
        public const double Proton = 1.007276466812;
        public const double Carbon = 12.00000000000;
        public const double Carbon13 = 13.0033548378;
        public const double C13C12Difference = Carbon13 - Carbon;
        public const double Hydrogen = 1.00782503207;
        public const double Deuterium = 2.0141017778;
        public const double Nitrogen = 14.0030740048;
        public const double Nitrogen15 = 15.0001088982;
        public const double Oxygen = 15.99491461956;
        public const double Oxygen18 = 17.9991610;
        public const double Sulfur = 31.97207100;
        public const double Sulfur34 = 33.96786690;
        public const double Water = Hydrogen*2 + Oxygen;
    }
}
