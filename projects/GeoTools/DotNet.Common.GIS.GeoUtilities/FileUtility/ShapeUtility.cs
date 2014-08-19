using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI;
using GeoAPI.Geometries;
using NetTopologySuite;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.Features;
using System.IO;

namespace DotNet.Common.GIS.GeoUtilities.FileUtility
{
    public class ShapeUtility
    {
        /// <summary>
        /// xy列表直接输出到shape文件
        /// </summary>
        /// <param name="polygonXYList">x1 y1,x2 y2,x3 y3,x4 y4</param>
        /// <param name="bw"></param>
        public static void ExportShapeFile(string path, List<string> polygonXYList)
        {
            try
            {
                ShapefileDataWriter shapeWrite = new ShapefileDataWriter(path);
                IGeometryFactory pGFactory = new GeometryFactory();
                IList<Feature> featureList = new List<Feature>();
                foreach (string str in polygonXYList)
                {
                    List<Coordinate> coordList = new List<Coordinate>();
                    string[] xys = str.Split(',');
                    string name = xys[0];
                    AttributesTable at = new AttributesTable();
                    at.AddAttribute("TFH", name);
                    for (int i = 1; i < xys.Length; i++)
                    {
                        string[] xandy = xys[i].Split(' ');
                        double x = 0, y = 0;
                        double.TryParse(xandy[0], out x);
                        double.TryParse(xandy[1], out y);
                        Coordinate coord = new Coordinate(x, y);
                        coordList.Add(coord);
                    }
                    IPolygon pPolygon = pGFactory.CreatePolygon(coordList.ToArray());
                    Feature f = new Feature(pPolygon, at);
                    featureList.Add(f);
                }
                System.Collections.IList features = (System.Collections.IList)featureList;
                shapeWrite.Header = ShapefileDataWriter.GetHeader(featureList[0], featureList.Count);
                shapeWrite.Write(features);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
