using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Common.GIS.GeoUtilities.CoordTransUtility
{
    public class CoordinateFactory
    {
        public static ICoordinate CreateCoordinate(Spheroid s)
        {
            ICoordinate coordinate;
            switch (s)
            {
                case Spheroid.Beijing54:
                    coordinate = new Beijing54();
                    break;
                case Spheroid.WGS84:
                    coordinate = new WGS84();
                    break;
                case Spheroid.Xian80:
                    coordinate = new Xian80();
                    break;
                default:
                    coordinate = null;
                    break;
            }
            return coordinate;
        }

        public static ICoordinate CreateCoordinate()
        {
            return new WGS84();
        }
    }
}
