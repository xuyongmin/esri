using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Geometry;

namespace MetaDataSDE
{
    /// <summary>
    /// 几何图形解析对象
    /// </summary>
    public class FeatureObject
    {
        public IList<AttributeJSObject> attributes { get; set; }

        public IGeometry geometry { get; set; }

        public FilterWhereJSObject filterWhere { get; set; }
    }

    /// <summary>
    /// 坐标系解析对象
    /// </summary>
    public class SRJSObject
    {
        public int wkid { get; set; }
    }

    ///// <summary>
    ///// 面解析对象
    ///// </summary>
    //public class RingsJSObject
    //{
    //    public IList<PointsJSObject> rings { get; set; }
    //}

    ///// <summary>
    ///// 线解析对象
    ///// </summary>
    //public class PathsJSObject
    //{
    //    public IList<PointsJSObject> paths { get; set; }
    //}

    ///// <summary>
    ///// 点解析对象
    ///// </summary>
    //public class PointsJSObject
    //{
    //    public IList<string> points { get; set; }
    //}

    /// <summary>
    /// 点对象
    /// </summary>
    public class PointObject
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int A { get; set; }

        //public PointObject Parse(string xya)
        //{
        //    char split = ',';
        //    return Parse(xya, split);
        //}

        //public PointObject Parse(string xya, char split)
        //{
        //    PointObject result = null;

        //    string[] xyas = xya.Split(split);
        //    if (xyas.Length == 2)
        //    {
        //        X = double.Parse(xyas[0]);
        //        Y = double.Parse(xyas[1]);
        //        A = 0;
        //    }
        //    else if (xyas.Length == 3)
        //    {
        //        X = double.Parse(xyas[0]);
        //        Y = double.Parse(xyas[1]);
        //        A = int.Parse(xyas[2]);
        //    }

        //    return result;
        //}

        //public override string ToString()
        //{
        //    string split = " ";
        //    return ToString(split);
        //}

        //public string ToString(string split)
        //{
        //    return string.Format("{0}{3}{1}{3}{2}", X, Y, A, split);
        //}
    }

    /// <summary>
    /// SDE连接字符串解析对象
    /// </summary>
    public class ConnectionJSObject
    {
        public string server { get; set; }
        public string instance { get; set; }
        public string database { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public string version { get; set; }
    }

    /// <summary>
    /// 属性对象
    /// </summary>
    public class AttributeJSObject
    {
        public string name { get; set; }
        public object value { get; set; }
    }

    /// <summary>
    /// 字段属性解析对象
    /// </summary>
    public class FiledJSObject
    {
        public string name { get; set; }
        public string alias { get; set; }
    }

    /// <summary>
    /// 图形表解析对象
    /// </summary>
    public class FeatureClassJSObject
    {
        public string name { get; set; }
    }

    /// <summary>
    /// 过滤条件解析对象
    /// </summary>
    public class FilterWhereJSObject
    {
        public string filter { get; set; }
    }
}
