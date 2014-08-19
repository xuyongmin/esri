using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Common.GIS.GeoUtilities.CommonUtility
{
    public class DDDMSHelper
    {
        /// <summary>
        /// 十进制双精度角度转换成度分秒角度格式
        /// </summary>
        /// <param name="Decimal Degree">度，十进制型双精度</param>
        /// <param name="Degree">度，整型</param>
        /// <param name="Minute">分，整型</param>
        /// <param name="Second">秒，双精度型</param>
        public static void DD2DMS(double DecimalDegree, out int Degree, out int Minute, out double Second)
        {
            Degree = (int)DecimalDegree;
            Minute = (int)((DecimalDegree - Degree) * 60.0);
            Second = Math.Round((DecimalDegree * 60 - Degree * 60 - Minute) * 60 * 100) / 100.0;
        }

        /// <summary>
        /// 度分秒角度格式转换成十进制度双精度角度格式
        /// </summary>
        /// <param name="Degree">度，整型</param>
        /// <param name="Minute">分，整型</param>
        /// <param name="Second">秒，双精度型</param>
        /// <param name="Decimal Degree">度，十进制型双精度</param>   
        public static void DMS2DD(int Degree, int Minute, double Second, out double DecimalDegree)
        {
            DecimalDegree = Degree + Minute / 60.0 + Second / 60.0 / 60.0;
        }
    }
}
