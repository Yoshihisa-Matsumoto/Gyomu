
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common
{
    public static class NumberExtension
    {
        /**
         * RoundUp when the value >=5 , Otherwise RoundDown
         */
        public static decimal ToHalfAdjust(this decimal dValue, int iDigits)
        {
            decimal dCoef = (decimal)System.Math.Pow(10D, iDigits);
            decimal decRemaining = 0.5M;
            return dValue > 0 ? Decimal.Floor(((dValue * dCoef) + decRemaining)) / dCoef :
                                Decimal.Ceiling(((dValue * dCoef) - decRemaining)) / dCoef;
        }
        /**
         * RoundUp when the value >=5 , Otherwise RoundDown
         */
        public static double ToHalfAdjust(this double dValue, int iDigits)
        {
            double dCoef = System.Math.Pow(10, iDigits);

            return dValue > 0 ? System.Math.Floor((dValue * dCoef) + 0.5) / dCoef :
                                System.Math.Ceiling((dValue * dCoef) - 0.5) / dCoef;
        }
        
        public static string OneBasedIndex2ExcelColumn(this int i)
        {
            int digit2 = i / 26;
            int digit1 = i % 26;
            if (digit1 == 0)
                digit2--;
            char char1 = (char)(digit1 + 64);
            char char2 = (char)(digit2 + 64);
            if (digit2 == 0)
                return char1.ToString();
            else
                return char2.ToString() + char1.ToString();
        }
    }
}
