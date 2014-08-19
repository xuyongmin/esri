using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Common.GIS.GeoUtilities.CoordTransUtility
{
    public class Beijing54 : GaussPrjBase
    {
        public Beijing54()
        {
            m_a = 6378245.0;
            m_b = 6356863.0188;
            base.Setfee1();
        }
    }

    public class Xian80 : GaussPrjBase
    {
        public Xian80()
        {
            m_a = 6378140.0;
            m_b = 6356755.2882;
            base.Setfee1();
        }
    }

    public class WGS84 : GaussPrjBase
    {
        public WGS84()
        {
            m_a = 6378137.0;
            m_b = 6356752.3142;
            base.Setfee1();
        }
    }

    public enum Spheroid
    {
        Xian80,
        Beijing54,
        WGS84
    }

    public enum LonWide
    {
        L6 = 6,
        L3 = 3
    }
}
