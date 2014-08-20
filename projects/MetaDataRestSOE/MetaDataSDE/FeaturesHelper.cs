using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessor;

namespace MetaDataSDE
{
    public class FeaturesHelper
    {
        private IWorkspaceFactory pWorkspaceFactory = null;
        private IWorkspace pWorkspace = null;
        private IWorkspace2 pWorkspace2 = null;
        private IFeatureWorkspace pFeatureWorkspace = null;
        private IWorkspaceEdit pWorkspaceEdit = null;

        /// <summary>
        /// 开启SDE连接
        /// </summary>
        private void OpenSDE(ConnectionJSObject connObj)
        {
            try
            {
                if (pWorkspace == null)
                {
                    if (connObj != null)
                    {
                        //设置连接属性!
                        IPropertySet pPropertySet = new PropertySetClass();
                        pPropertySet.SetProperty("SERVER", connObj.server);
                        pPropertySet.SetProperty("INSTANCE", connObj.instance);
                        pPropertySet.SetProperty("DATABASE", connObj.database);
                        pPropertySet.SetProperty("USER", connObj.user);
                        pPropertySet.SetProperty("PASSWORD", connObj.password);
                        pPropertySet.SetProperty("VERSION", connObj.version);

                        //打开SDE空间数据库!
                        pWorkspaceFactory = new SdeWorkspaceFactoryClass();
                        pWorkspace = pWorkspaceFactory.Open(pPropertySet, 0);
                        if (pWorkspace != null)
                        {
                            pWorkspace2 = (IWorkspace2)pWorkspace;
                            pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;
                            pWorkspaceEdit = (IWorkspaceEdit)pWorkspace;
                        }
                        else
                        {
                            pWorkspace = null;
                            pWorkspace2 = null;
                            pFeatureWorkspace = null;
                            pWorkspaceEdit = null;
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("输入的连接对象不正确！"));
                    }
                }
            }
            catch (Exception ex)
            {
                CloseSDE();
                throw new Exception(string.Format("打开SDE空间数据库失败:{0}!", ex.Message));
            }
        }

        /// <summary>
        /// 关闭SDE连接
        /// </summary>
        private void CloseSDE()
        {
            if (pWorkspaceFactory != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactory);
                pWorkspaceFactory = null;
            }
            if (pWorkspace != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace);
                pWorkspace = null;
            }
            if (pWorkspace2 != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace2);
                pWorkspace2 = null;
            }
            if (pFeatureWorkspace != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureWorkspace);
                pFeatureWorkspace = null;
            }
            if (pWorkspaceEdit != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceEdit);
                pWorkspaceEdit = null;
            }
        }

        /// <summary>
        /// 验证是否版本号编辑
        /// </summary>
        /// <param name="targetFeatureClass"></param>
        /// <returns></returns>
        private bool CheckVersionEdit(IFeatureClass targetFeatureClass)
        {
            bool result = false;
            IVersionedObject pVersionedObject = (IVersionedObject)targetFeatureClass;
            if (pVersionedObject.IsRegisteredAsVersioned)
            {
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 如果是Polygon，进行自重叠检查
        /// </summary>
        /// <returns></returns>
        public bool CheckSelfOverlap(List<FeatureObject> featureList, out string outMsg)
        {
            bool result = false;
            outMsg = string.Empty;

            try
            {

                for (int i = 0; i < featureList.Count; i++)
                {
                    FeatureObject foi = featureList[i];
                    if (foi.geometry == null || foi.geometry.IsEmpty)
                    {
                        continue;
                    }

                    ITopologicalOperator pITop = (ITopologicalOperator)foi.geometry;

                    for (int j = i + 1; j < featureList.Count; j++)
                    {
                        FeatureObject foj = featureList[j];
                        if (foj.geometry == null || foj.geometry.IsEmpty)
                        {
                            continue;
                        }

                        IGeometry overlapGeo = pITop.Intersect(foj.geometry, esriGeometryDimension.esriGeometry2Dimension);
                        if (overlapGeo != null && !overlapGeo.IsEmpty)
                        {
                            result = true;
                            int i_int = i + 1;
                            int j_int = j + 1;
                            outMsg += string.Format("传入的地块集合中第{0}和第{1}个地块是相互重叠的;", i_int, j_int);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                outMsg += ex.Message;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 如果是Polygon，与库中数据重叠检查
        /// </summary>
        /// <returns></returns>
        public bool CheckOverlap(List<FeatureObject> featureList, IFeatureClass featureClass, string expFilterWhere, out string outMsg)
        {
            bool result = false;
            outMsg = string.Empty;

            try
            {
                ISpatialFilter spatialFilter = new SpatialFilterClass();
                spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                foreach (FeatureObject fo in featureList)
                {
                    spatialFilter.Geometry = fo.geometry;
                    if (!string.IsNullOrEmpty(expFilterWhere))
                    {
                        spatialFilter.WhereClause = expFilterWhere;
                    }
                    IFeatureCursor featureCursor = featureClass.Search(spatialFilter, false);
                    int count = 0;
                    IFeature ff = featureCursor.NextFeature();
                    while (ff != null)
                    {
                        count++;
                        ff = featureCursor.NextFeature();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);

                    if (count > 0)
                    {
                        result = true;
                        outMsg += "地块有重叠！";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                outMsg += ex.Message;
                result = true;
            }


            return result;
        }

        /// <summary>
        /// 保存Features
        /// </summary>
        /// <param name="connObj">连接对象</param>
        /// <param name="fcObj">表对象</param>
        /// <param name="featureList">几何对象</param>
        /// <param name="count">返回影响行数</param>
        /// <param name="outMsg">返回必要信息</param>
        /// <returns></returns>
        public bool SaveFeatures(ConnectionJSObject connObj, FeatureClassJSObject fcObj, List<FeatureObject> featureList, out int count, out string outMsg)
        {
            bool result = false;
            count = 0;
            outMsg = string.Empty;
            try
            {
                if (fcObj != null)
                {
                    OpenSDE(connObj);

                    if (pWorkspace != null)
                    {
                        IFeatureClass featureClass = null;
                        //指定表名是否存在
                        if (pWorkspace2.get_NameExists(esriDatasetType.esriDTFeatureClass, fcObj.name))
                        {
                            featureClass = pFeatureWorkspace.OpenFeatureClass(fcObj.name);

                            bool isVersion = CheckVersionEdit(featureClass);
                            if (isVersion)
                            {
                                pWorkspaceEdit.StartEditing(true);
                            }
                            if (isVersion)
                            {
                                pWorkspaceEdit.StartEditOperation();
                            }

                            //解析几何数据
                            foreach (FeatureObject fo in featureList)
                            {
                                IFeature pFeature;
                                if (fo.filterWhere == null || string.IsNullOrEmpty(fo.filterWhere.filter))//没有过滤条件，默认为新增
                                {
                                    pFeature = featureClass.CreateFeature();
                                }
                                else
                                {
                                    //有过滤条件，如果能找到，默认为修改，否则默认为新增，并且默认情况下只修改第一条符合条件的数据，如果有多条，不提示。

                                    //查找地块
                                    IQueryFilter pQueryFilter = new QueryFilterClass();
                                    pQueryFilter.WhereClause = fo.filterWhere.filter;
                                    IFeatureCursor pFeatureCursor = featureClass.Update(pQueryFilter, false);

                                    pFeature = pFeatureCursor.NextFeature();
                                }


                                if (pFeature != null)
                                {
                                    pFeature.Shape = fo.geometry;

                                    foreach (AttributeJSObject aObj in fo.attributes)
                                    {
                                        int index = featureClass.Fields.FindField(aObj.name);
                                        if (index != -1)
                                        {
                                            IField f = featureClass.Fields.get_Field(index);
                                            if (f.Editable)
                                            {
                                                pFeature.set_Value(index, aObj.value);
                                            }
                                        }
                                    }

                                    //保存
                                    pFeature.Store();
                                    count++;
                                }
                            }

                            if (isVersion)
                            {
                                pWorkspaceEdit.StopEditOperation();
                            }

                            result = true;
                        }
                        else
                        {
                            outMsg = string.Format("数据源中不存在表名[{0}]。", fcObj.name);
                        }

                    }
                }
                else
                {
                    outMsg = string.Format("操作表参数错误!");
                }
            }
            catch (Exception ex)
            {
                result = false;
                outMsg = ex.Message;
            }
            finally
            {
                CloseSDE();
            }

            return result;
        }

        /// <summary>
        /// 删除Features
        /// </summary>
        /// <param name="connObj">连接对象</param>
        /// <param name="fcObj">表对象</param>
        /// <param name="fwObj">删除条件</param>
        /// <param name="count">返回影响行数</param>
        /// <param name="outMsg">返回必要信息</param>
        /// <returns></returns>
        public bool DeleteFeatures(ConnectionJSObject connObj, FeatureClassJSObject fcObj, FilterWhereJSObject fwObj, out int count, out string outMsg)
        {
            bool result = false;
            count = 0;
            outMsg = string.Empty;
            try
            {
                if (fcObj != null)
                {
                    OpenSDE(connObj);

                    if (pWorkspace != null)
                    {
                        IFeatureClass featureClass = null;
                        //指定表名是否存在
                        if (pWorkspace2.get_NameExists(esriDatasetType.esriDTFeatureClass, fcObj.name))
                        {
                            featureClass = pFeatureWorkspace.OpenFeatureClass(fcObj.name);

                            bool isVersion = CheckVersionEdit(featureClass);
                            if (isVersion)
                            {
                                pWorkspaceEdit.StartEditing(true);
                            }
                            if (isVersion)
                            {
                                pWorkspaceEdit.StartEditOperation();
                            }
                            //查找地块
                            IQueryFilter pQueryFilter = new QueryFilterClass();
                            pQueryFilter.WhereClause = fwObj.filter;
                            IFeatureCursor pFeatureCursor = featureClass.Update(pQueryFilter, false);
                            IFeature feature = pFeatureCursor.NextFeature();

                            while (feature != null)
                            {
                                feature.Delete();
                                feature = pFeatureCursor.NextFeature();
                                count++;
                            }

                            if (isVersion)
                            {
                                pWorkspaceEdit.StopEditOperation();
                            }

                            result = true;
                        }
                        else
                        {
                            outMsg = string.Format("数据源中不存在表名[{0}]。", fcObj.name);
                        }
                    }
                }
                else
                {
                    outMsg = string.Format("操作表参数错误!");
                }
            }
            catch (Exception ex)
            {
                result = false;
                outMsg = ex.Message;
            }
            finally
            {
                CloseSDE();
            }

            return result;
        }

        public IFeature GetTestFeatures(ConnectionJSObject connObj, FeatureClassJSObject fcObj, FilterWhereJSObject fwObj, out string outMsg)
        {
            IFeature result = null;

            outMsg = string.Empty;
            try
            {
                if (fcObj != null)
                {
                    OpenSDE(connObj);

                    if (pWorkspace != null)
                    {
                        IFeatureClass featureClass = null;
                        //指定表名是否存在
                        if (pWorkspace2.get_NameExists(esriDatasetType.esriDTFeatureClass, fcObj.name))
                        {
                            featureClass = pFeatureWorkspace.OpenFeatureClass(fcObj.name);

                            //查找地块
                            IQueryFilter pQueryFilter = new QueryFilterClass();
                            pQueryFilter.WhereClause = fwObj.filter;
                            IFeatureCursor pFeatureCursor = featureClass.Search(pQueryFilter, false);
                            result = pFeatureCursor.NextFeature();
                        }
                        else
                        {
                            outMsg = string.Format("数据源中不存在表名[{0}]。", fcObj.name);
                        }
                    }
                }
                else
                {
                    outMsg = string.Format("操作表参数错误!");
                }
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
            }
            finally
            {
                CloseSDE();
            }

            return result;
        }

        /// <summary>
        /// 删除Features
        /// </summary>
        /// <param name="connObj">连接对象</param>
        /// <param name="fcObj">表对象</param>
        /// <param name="fwObj">查询条件</param>
        /// <param name="outMsg">返回必要信息</param>
        /// <returns></returns>
        public List<FeatureObject> GetFeatures(ConnectionJSObject connObj, FeatureClassJSObject fcObj, FilterWhereJSObject fwObj, out string outMsg)
        {
            List<FeatureObject> result = new List<FeatureObject>();
            outMsg = string.Empty;
            try
            {
                if (fcObj != null)
                {
                    OpenSDE(connObj);

                    if (pWorkspace != null)
                    {
                        IFeatureClass featureClass = null;
                        //指定表名是否存在
                        if (pWorkspace2.get_NameExists(esriDatasetType.esriDTFeatureClass, fcObj.name))
                        {
                            featureClass = pFeatureWorkspace.OpenFeatureClass(fcObj.name);

                            //查找地块
                            IQueryFilter pQueryFilter = new QueryFilterClass();
                            pQueryFilter.WhereClause = fwObj.filter;
                            IFeatureCursor pFeatureCursor = featureClass.Update(pQueryFilter, false);
                            IFeature feature = pFeatureCursor.NextFeature();
                            while (feature != null)
                            {
                                //TODO:解析
                                FeatureObject fo = new FeatureObject();
                                List<AttributeJSObject> attrs = new List<AttributeJSObject>();

                                for (int i = 0; i < featureClass.Fields.FieldCount; i++)
                                {
                                    IField field = featureClass.Fields.get_Field(i);
                                    if (field != null && field.Editable && field.Type != esriFieldType.esriFieldTypeGeometry)
                                    {
                                        AttributeJSObject ajso = new AttributeJSObject();
                                        ajso.name = field.Name;
                                        ajso.value = feature.get_Value(i);
                                        attrs.Add(ajso);
                                    }
                                }
                                fo.attributes = attrs;
                                fo.geometry = feature.ShapeCopy;

                                result.Add(fo);

                                feature = pFeatureCursor.NextFeature();
                            }
                        }
                        else
                        {
                            outMsg = string.Format("数据源中不存在表名[{0}]。", fcObj.name);
                        }
                    }
                }
                else
                {
                    outMsg = string.Format("操作表参数错误!");
                }
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
            }
            finally
            {
                CloseSDE();
            }

            return result;
        }


        public IPolygon DissolvePolygon(List<IPolygon> polygons)
        {
            IPolygon result = null;

            IFeatureClass outFeatureClass = null;

            Geoprocessor gp = new Geoprocessor();
            ESRI.ArcGIS.DataManagementTools.Dissolve disTool = new ESRI.ArcGIS.DataManagementTools.Dissolve();
            disTool.in_features = polygons;
            disTool.out_feature_class = outFeatureClass;

            gp.Execute(disTool, null);

            return result;
        }

        public IWorkspace GetFGDBWorkspace(string sFilePath)
        {
            if (!System.IO.Directory.Exists(sFilePath))
            {
                return null;
            }
            try
            {
                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                return factory.OpenFromFile(sFilePath, 0);
            }
            catch
            {
                return null;
            }
        }

        public IWorkspace GetShapefileWorkspace(string sFilePath)
        {
            if (!File.Exists(sFilePath))
            {
                return null;
            }
            try
            {
                IWorkspaceFactory factory = new ShapefileWorkspaceFactoryClass();
                sFilePath = System.IO.Path.GetDirectoryName(sFilePath);
                return factory.OpenFromFile(sFilePath, 0);
            }
            catch
            {
                return null;
            }
        }  
    }
}
