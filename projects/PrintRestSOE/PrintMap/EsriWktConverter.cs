using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using NetTopologySuite.IO;

namespace PrintMap
{

    public class EsriWktConverter
    {
        public static byte[] ConvertGeometryToWKB(IGeometry geometry)
        {
            IWkb wkb = geometry as IWkb;
            ITopologicalOperator oper = geometry as ITopologicalOperator;
            oper.Simplify();

            IGeometryFactory3 factory = new GeometryEnvironment() as IGeometryFactory3;
            byte[] b = factory.CreateWkbVariantFromGeometry(geometry) as byte[];
            return b;
        }

        public static byte[] ConvertWKTToWKB(string wkt)
        {
            WKBWriter writer = new WKBWriter();
            WKTReader reader = new WKTReader();
            return writer.Write(reader.Read(wkt));
        }

        public static string ConvertWKBToWKT(byte[] wkb)
        {
            WKTWriter writer = new WKTWriter();
            WKBReader reader = new WKBReader();
            return writer.Write(reader.Read(wkb));
        }

        public static string ConvertGeometryToWKT(IGeometry geometry)
        {
            byte[] b = ConvertGeometryToWKB(geometry);
            WKBReader reader = new WKBReader();
            GeoAPI.Geometries.IGeometry g = reader.Read(b);
            WKTWriter writer = new WKTWriter();
            return writer.Write(g);
        }

        public static IGeometry ConvertWKTToGeometry(string wkt)
        {
            byte[] wkb = ConvertWKTToWKB(wkt);
            return ConvertWKBToGeometry(wkb);
        }

        public static IGeometry ConvertWKBToGeometry(byte[] wkb)
        {
            IGeometry geom;
            int countin = wkb.GetLength(0);
            IGeometryFactory3 factory = new GeometryEnvironment() as IGeometryFactory3;
            factory.CreateGeometryFromWkbVariant(wkb, out geom, out countin);
            return geom;
        }

        public static IGeometry ConvertGeoAPIToESRI(GeoAPI.Geometries.IGeometry geometry)
        {
            WKBWriter writer = new WKBWriter();
            byte[] bytes = writer.Write(geometry);
            return ConvertWKBToGeometry(bytes);
        }

        public static GeoAPI.Geometries.IGeometry ConvertESRIToGeoAPI(IGeometry geometry)
        {
            byte[] wkb = ConvertGeometryToWKB(geometry);
            WKBReader reader = new WKBReader();
            return reader.Read(wkb);
        }
    }
}
