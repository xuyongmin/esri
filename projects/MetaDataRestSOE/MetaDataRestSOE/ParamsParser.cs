using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SOESupport;

using MetaDataSDE;

namespace MetaDataRestSOE
{
    public class ParamsParser
    {
        /// <summary>
        /// 连接字符串解析
        /// </summary>
        /// <param name="connJson"></param>
        /// <returns></returns>
        public static ConnectionJSObject ParseConn(JsonObject connJson)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ConnectionJSObject>(connJson.ToJson());
        }

        /// <summary>
        /// 条件解析
        /// </summary>
        /// <param name="filterWhereJson"></param>
        /// <returns></returns>
        public static FilterWhereJSObject ParseFilterWhere(JsonObject filterWhereJson)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<FilterWhereJSObject>(filterWhereJson.ToJson());
        }

        /// <summary>
        /// 图形表解析
        /// </summary>
        /// <param name="fcJson"></param>
        /// <returns></returns>
        public static FeatureClassJSObject ParseFeatrureClass(JsonObject fcJson)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<FeatureClassJSObject>(fcJson.ToJson());
        }

        /// <summary>
        /// 几何图形解析
        /// </summary>
        /// <param name="featruesJson"></param>
        /// <returns></returns>
        public static List<FeatureObject> ParseFeatures(object[] featruesJson)
        {
            List<FeatureObject> result = new List<FeatureObject>();

            foreach (object o in featruesJson)
            {
                JsonObject jo = o as JsonObject;
                if (jo != null)
                {
                    FeatureObject fo = ParseFeature(jo);
                    if (fo != null)
                    {
                        result.Add(fo);
                    }
                }
            }

            return result;
        }

        public static FeatureObject ParseFeature(JsonObject featureJson)
        {
            FeatureObject result = null;

            //处理属性 
            object[] attrJson;
            bool found = false;
            found = featureJson.TryGetArray("attribute", out attrJson);
            if (!found && attrJson == null)
            {
                throw new ArgumentNullException("features>attribute  can not parse to jsonobject");
            }

            //处理图形
            JsonObject geoJson;
            found = featureJson.TryGetJsonObject("geometry", out geoJson);
            if (!found && geoJson == null)
            {
                throw new ArgumentNullException("features>geometry  can not parse to jsonobject");
            }

            //解析条件
            JsonObject filterJson;
            found = featureJson.TryGetJsonObject("filterWhere", out filterJson);
            if (!found && filterJson == null)
            {
                throw new ArgumentNullException("features>filterWhere  can not parse to jsonobject");
            }

            result = new FeatureObject();
            result.filterWhere = ParseFilterWhere(filterJson);

            List<AttributeJSObject> attrs = new List<AttributeJSObject>();
            foreach (object o in attrJson)
            {
                JsonObject jo = o as JsonObject;
                if (jo != null)
                {
                    AttributeJSObject attjsObj = ParseAttribute(jo);
                    attrs.Add(attjsObj);
                }
            }
            result.attributes = attrs;

            //解析图形数据，包括点线面
            result.geometry = ParseGeometry(geoJson);

            return result;
        }

        public static IGeometry ParseGeometry(JsonObject geoJson)
        {
            IGeometry result = null;

            object[] ringspathspointsJson;
            bool found = false;
            found = geoJson.TryGetArray("rings", out ringspathspointsJson);
            if (!found)
            {
                found = geoJson.TryGetArray("paths", out ringspathspointsJson);
                if (!found)
                {
                    found = geoJson.TryGetArray("points", out ringspathspointsJson);
                    if (found && ringspathspointsJson != null)//处理points
                    {
                        result = CreatePoint(ringspathspointsJson);
                    }
                }
                else//处理paths
                {
                    if (ringspathspointsJson != null)
                    {
                        result = CreatePolyline(ringspathspointsJson);
                    }
                }
            }
            else//处理rings
            {
                if (ringspathspointsJson != null)
                {
                    result = CreatePolygon(ringspathspointsJson);
                }
            }

            return result;
        }

        public static IPolygon CreatePolygon(object[] rings)
        {
            IPolygon result = null;

            IGeometryCollection pGeomCol = new PolygonClass();
            object objMissing = Type.Missing;
            foreach (object o in rings)//part
            {
                object[] ringpoints = o as object[];
                if (ringpoints != null)
                {
                    ISegmentCollection pSegCol = new RingClass();
                    ISegment pSeg = new LineClass();
                    IPoint pFromPt = new PointClass();
                    IPoint pToPt = new PointClass();
                    IPoint pEndPt = new PointClass();

                    List<PointObject> poList = new List<PointObject>();
                    foreach (object po in ringpoints)
                    {
                        PointObject pObj = new PointObject();
                        object[] ptxya = po as object[];
                        if (ptxya != null)
                        {
                            if (ptxya.Length == 3)
                            {
                                pObj.X = double.Parse(ptxya[0].ToString());
                                pObj.Y = double.Parse(ptxya[1].ToString());
                                pObj.A = int.Parse(ptxya[2].ToString());
                            }
                            else if (ptxya.Length == 2)
                            {
                                pObj.X = double.Parse(ptxya[0].ToString());
                                pObj.Y = double.Parse(ptxya[1].ToString());
                                pObj.A = 0;
                            }
                            else
                            {
                                throw new Exception("坐标串输入错误!");
                            }
                            poList.Add(pObj);
                        }
                    }

                    if (poList.Count < 3)
                    {
                        throw new Exception("至少保证三个点来确定一个面！");
                    }

                    for (int i = 0; i < poList.Count - 1; i++)
                    {
                        if (poList[i].A.Equals(1))//处理狐段
                        {
                            PointObject poF = null;
                            PointObject poT = null;
                            PointObject poE = null;
                            if (i - 1 < 0)
                            {
                                poF = poList[poList.Count - 2];
                            }
                            else
                            {
                                poF = poList[i - 1];
                            }
                            poT = poList[i];
                            poE = poList[i + 1];

                            pFromPt.PutCoords(poF.X, poF.Y);
                            pToPt.PutCoords(poT.X, poT.Y);
                            pEndPt.PutCoords(poE.X, poE.Y);

                            //圆弧
                            ICircularArc cirularArc = new CircularArcClass();
                            IConstructCircularArc constructCircularArc = cirularArc as IConstructCircularArc;
                            constructCircularArc.ConstructThreePoints(pFromPt, pToPt, pEndPt, true);
                            pSeg = cirularArc as ISegment;
                            pSegCol.AddSegment(pSeg, ref objMissing, ref objMissing);
                        }
                        else
                        {
                            if (poList[i + 1].A.Equals(0))//处理直线，否则不处理
                            {
                                pFromPt.PutCoords(poList[i].X, poList[i].Y);
                                pToPt.PutCoords(poList[i + 1].X, poList[i + 1].Y);

                                pSeg = new LineClass();
                                pSeg.FromPoint = pFromPt;
                                pSeg.ToPoint = pToPt;
                                //一根线段
                                pSegCol.AddSegment(pSeg, ref objMissing, ref objMissing);
                            }
                        }
                    }

                    //QI到IRing接口封闭Ring对象，使其有效
                    IRing pRing = pSegCol as IRing;
                    pRing.Close();

                    //一个part
                    pGeomCol.AddGeometry(pSegCol as IGeometry, ref objMissing, ref objMissing);
                }
            }

            result = pGeomCol as IPolygon;
            ITopologicalOperator pTop = result as ITopologicalOperator;
            pTop.Simplify();

            return result;
        }

        public static IPolyline CreatePolyline(object[] paths)
        {
            IPolyline result = null;
            IGeometryCollection pGeomCol = new PolylineClass();
            object objMissing = Type.Missing;
            foreach (object o in paths)//part
            {
                object[] ringpoints = o as object[];
                if (ringpoints != null)
                {
                    ISegmentCollection pSegCol = new PathClass();
                    ISegment pSeg = new LineClass();
                    IPoint pFromPt = new PointClass();
                    IPoint pToPt = new PointClass();
                    IPoint pEndPt = new PointClass();

                    List<PointObject> poList = new List<PointObject>();
                    foreach (object po in ringpoints)
                    {
                        PointObject pObj = new PointObject();
                        object[] ptxya = po as object[];
                        if (ptxya != null)
                        {
                            if (ptxya.Length == 3)
                            {
                                pObj.X = double.Parse(ptxya[0].ToString());
                                pObj.Y = double.Parse(ptxya[1].ToString());
                                pObj.A = int.Parse(ptxya[2].ToString());
                            }
                            else if (ptxya.Length == 2)
                            {
                                pObj.X = double.Parse(ptxya[0].ToString());
                                pObj.Y = double.Parse(ptxya[1].ToString());
                                pObj.A = 0;
                            }
                            else
                            {
                                throw new Exception("坐标串输入错误!");
                            }
                            poList.Add(pObj);
                        }
                    }

                    if (poList.Count < 2)
                    {
                        throw new Exception("至少保证两个点来确定一条线！");
                    }

                    for (int i = 0; i < poList.Count - 1; i++)
                    {
                        if (poList[i].A.Equals(1))//处理狐段
                        {
                            PointObject poF = null;
                            PointObject poT = null;
                            PointObject poE = null;
                            if (i - 1 < 0)
                            {
                                poF = poList[poList.Count - 2];
                            }
                            else
                            {
                                poF = poList[i - 1];
                            }
                            poT = poList[i];
                            poE = poList[i + 1];

                            pFromPt.PutCoords(poF.X, poF.Y);
                            pToPt.PutCoords(poT.X, poT.Y);
                            pEndPt.PutCoords(poE.X, poE.Y);

                            //圆弧
                            ICircularArc cirularArc = new CircularArcClass();
                            IConstructCircularArc constructCircularArc = cirularArc as IConstructCircularArc;
                            constructCircularArc.ConstructThreePoints(pFromPt, pToPt, pEndPt, true);
                            pSeg = cirularArc as ISegment;
                            pSegCol.AddSegment(pSeg, ref objMissing, ref objMissing);
                        }
                        else
                        {
                            if (poList[i + 1].A.Equals(0))//处理直线，否则不处理
                            {
                                pFromPt.PutCoords(poList[i].X, poList[i].Y);
                                pToPt.PutCoords(poList[i + 1].X, poList[i + 1].Y);

                                pSeg = new LineClass();
                                pSeg.FromPoint = pFromPt;
                                pSeg.ToPoint = pToPt;
                                //一根线段
                                pSegCol.AddSegment(pSeg, ref objMissing, ref objMissing);
                            }
                        }
                    }

                    //一个part
                    pGeomCol.AddGeometry(pSegCol as IGeometry, ref objMissing, ref objMissing);
                }
            }

            result = pGeomCol as IPolyline;

            return result;
        }

        public static IPoint CreatePoint(object[] point)
        {
            IPoint result = null;

            if (point.Length == 3 || point.Length == 2)
            {
                PointObject po = new PointObject();
                po.X = double.Parse(point[0].ToString());
                po.Y = double.Parse(point[1].ToString());

                result = new PointClass();
                result.PutCoords(po.X, po.Y);
            }
            else
            {
                throw new Exception("点坐标串错误！");
            }

            return result;
        }

        /// <summary>
        /// 反解析
        /// </summary>
        /// <param name="features"></param>
        /// <returns></returns>
        public static JsonObject FeaturesToJsonObject(List<FeatureObject> features)
        {
            JsonObject result = null;

            //反解析

            return result;
        }

        /// <summary>
        /// 属性解析
        /// </summary>
        /// <param name="attrJson"></param>
        /// <returns></returns>
        public static AttributeJSObject ParseAttribute(JsonObject attrJson)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AttributeJSObject>(attrJson.ToJson());
        }

        /// <summary>
        /// 坐标系解析
        /// </summary>
        /// <param name="srJson"></param>
        /// <returns></returns>
        public static SRJSObject ParseSR(JsonObject srJson)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SRJSObject>(srJson.ToJson());
        }
    }
}
