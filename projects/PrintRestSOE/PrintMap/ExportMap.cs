using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Net;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;

namespace PrintMap
{
    public class ExportMap
    {
        #region 获取本机IP地址或环境变量

        private string PRINT_PATH_KEY = "HOME";
        private string printPath;
        private string printIISRoot;

        public string GetIP()
        {
            string result = "localhost";
            string HostName = Dns.GetHostName(); //得到主机名              
            IPHostEntry IpEntry = Dns.GetHostEntry(HostName); //得到主机IP
            foreach (IPAddress ipadd in IpEntry.AddressList)
            {
                if (ipadd.ToString().Split('.').Length == 4)
                {
                    result = ipadd.ToString();
                    break;
                }
            }
            return result;
        }

        public ExportMap()
        {
            printPath = string.Format(@"{0}\SOEPrintConfig", Environment.GetEnvironmentVariable(PRINT_PATH_KEY));
            printIISRoot = string.Format("http://{0}/TGisPrint/", GetIP());
        }

        #endregion

        #region 删除文件

        public bool DeleteFile(List<string> fileNames, out string errMsg)
        {
            bool result = false;
            errMsg = string.Empty;

            try
            {
                foreach (string fileName in fileNames)
                {
                    string filePath = string.Format(@"{0}\LocalFiles\{1}", printPath, fileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    else
                    {
                        errMsg += string.Format("找不到文件[{0}]！", fileName);
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return result;
        }

        #endregion

        #region 制图


        /// <summary>
        /// 延时函数
        /// </summary>
        /// <param name="delayTime">需要延时多少秒</param>
        /// <returns></returns>
        public bool Delay(int delayTime)
        {
            DateTime now = DateTime.Now;
            int s;
            do
            {
                TimeSpan spand = DateTime.Now - now;
                s = spand.Seconds;
            }
            while (s < delayTime);
            return true;
        }

        public string GetFilePath()
        {
            string result = string.Empty;

            result = Assembly.GetExecutingAssembly().Location;
            //result = System.Environment.CurrentDirectory;

            return result;
        }

        public bool Export(ExportMapInfo info, out string outmsg)
        {
            bool result = false;
            bool hasright = true;
            outmsg = string.Empty;
            //hasright = InitialApp(out outmsg);
            if (hasright)
            {
                #region 地图元素参数设定

                PageSize currentPageSize = PageSize.A4;
                PageOrientation currentPageOri = PageOrientation.Portrait;
                currentPageSize = (PageSize)int.Parse(info.PageSize);
                currentPageOri = (PageOrientation)int.Parse(info.PageOri);

                double left = 0, bottom = 0, right = 0, top = 0;
                double outlineoffset = 0;
                string[] margins = info.PageMargin.Split(',');
                if (margins.Length == 4)
                {
                    left = double.Parse(margins[0]) + outlineoffset;
                    bottom = double.Parse(margins[3]) + outlineoffset;
                    top = double.Parse(margins[1]) + outlineoffset;
                    right = double.Parse(margins[2]) + outlineoffset;
                }

                #endregion

                #region 根据mxd获取相关信息。

                /// <summary>
                /// mxd文档
                /// </summary>
                IMapDocument pMapDocument;

                IMapFrame pPageMapFrame;
                /// <summary>
                /// 制图接口
                /// </summary>
                IPageLayout pPageLayout;
                /// <summary>
                /// 页面
                /// </summary>
                IPage pPage;
                /// <summary>
                /// 当前地图
                /// </summary>
                IMap pMap;
                /// <summary>
                /// 当前数据窗口
                /// </summary>
                IActiveView pDataAV;
                /// <summary>
                /// 当前制图窗口
                /// </summary>
                IActiveView pPageAV;
                IGraphicsContainer pGraphicsContainer;
                IGraphicsContainerSelect pGrapSel;
                IExport docExport;
                IPrintAndExport docPrintExport;

                pMapDocument = new MapDocumentClass();
                info.MxdPath = printPath + "\\Mxds\\";
                string mxdfile = info.MxdPath + (info.TemplateName.ToUpper().EndsWith(".MXD") ? info.TemplateName : info.TemplateName + ".mxd");
                if (File.Exists(mxdfile))
                {
                    pMapDocument.Open(mxdfile, "");
                    pPageLayout = pMapDocument.PageLayout;
                    pPage = pPageLayout.Page;
                    pPageAV = pPageLayout as IActiveView;
                    pDataAV = pPageAV.FocusMap as IActiveView;
                    pMap = pPageAV.FocusMap;
                    pGraphicsContainer = pPageLayout as IGraphicsContainer;
                    pPageMapFrame = pGraphicsContainer.FindFrame(pPageAV.FocusMap) as IMapFrame;

                    pGrapSel = pMapDocument.ActiveView as IGraphicsContainerSelect;

                    docExport = new ExportPDFClass();
                    docPrintExport = new PrintAndExportClass();

                    try
                    {
                        //处理所见所得
                        if (mxdfile.ToUpper().EndsWith("TEMP.MXD"))
                        {
                            ////处理坐标系
                            //PageUtility.ReplaceSR(pPageControl, info.Wkid);
                            //加载要打印的图层
                            if (info.Lyrs != null)
                            {
                                foreach (string lyr in info.Lyrs)
                                {
                                    IMapDocument pLyrDocument = new MapDocumentClass();
                                    pLyrDocument.Open(info.LyrPath + lyr, "");
                                    pMap.AddLayer(pLyrDocument.get_Map(0).get_Layer(0));
                                    //针对所见即所得，如果加载的是影像，现状，规划等大数据量的数据，则需要延时8s处理
                                    Delay(10);
                                    pPageAV.PartialRefresh(esriViewDrawPhase.esriViewAll, null, null);
                                }
                            }
                        }

                        IGeometry dataCenterGeo = EsriWktConverter.ConvertWKTToGeometry(info.DataCenter);

                        IPoint dataCenter = dataCenterGeo as IPoint;
                        PageSizeUtility.SetCenterAndScale(pPageLayout, dataCenter, info.Scale);
                        SetPageTemplate(pPageLayout, dataCenter, left, right, top, bottom, currentPageSize, currentPageOri, info);

                        //业务几何图形处理
                        if (info.BusinessShapes != null && info.BusinessShapes.Count > 0)
                        {
                            foreach (string str in info.BusinessShapes)
                            {
                                string[] strs = str.Split(';');
                                IGeometry shapeGeometry = EsriWktConverter.ConvertWKTToGeometry(strs[0]);
                                IPolygon pPolygon = shapeGeometry as IPolygon;
                                if (pPolygon != null)
                                {
                                    //地块样式
                                    IRgbColor shapeFillRgbColor = new RgbColorClass();
                                    shapeFillRgbColor.NullColor = true;
                                    IRgbColor shapeBorderRGBColor = new RgbColorClass();
                                    shapeBorderRGBColor.Red = 255;
                                    shapeBorderRGBColor.Green = 0;
                                    shapeBorderRGBColor.Blue = 0;
                                    double shapeBorderWidth = 1.5;
                                    // 颜色组织 a,r,g,b
                                    // 业务数据 shapewkt;bordercolor;fillcolor;borderthickness
                                    if (strs.Length == 4)
                                    {
                                        string[] bordercolorargb = strs[1].Split(',');
                                        if (bordercolorargb.Length == 4)
                                        {
                                            int ba = 255, br = 255, bg = 0, bb = 0;
                                            int.TryParse(bordercolorargb[0], out ba);
                                            int.TryParse(bordercolorargb[1], out br);
                                            int.TryParse(bordercolorargb[2], out bg);
                                            int.TryParse(bordercolorargb[3], out bb);
                                            shapeBorderRGBColor.Red = br;
                                            shapeBorderRGBColor.Green = bg;
                                            shapeBorderRGBColor.Blue = bb;
                                            shapeBorderRGBColor.Transparency = (byte)ba;
                                        }
                                        string[] fillcolorargb = strs[2].Split(',');
                                        if (fillcolorargb.Length == 4)
                                        {
                                            int fa = 0, fr = 255, fg = 0, fb = 0;
                                            int.TryParse(fillcolorargb[0], out fa);
                                            int.TryParse(fillcolorargb[1], out fr);
                                            int.TryParse(fillcolorargb[2], out fg);
                                            int.TryParse(fillcolorargb[3], out fb);
                                            shapeFillRgbColor.Red = fr;
                                            shapeFillRgbColor.Green = fg;
                                            shapeFillRgbColor.Blue = fb;
                                            shapeFillRgbColor.Transparency = (byte)fa;
                                        }
                                        string borderwidth = strs[3];
                                        double.TryParse(borderwidth, out shapeBorderWidth);
                                    }

                                    PageElementUtility.AddPolygonElement(pMapDocument, pPolygon, shapeBorderWidth, shapeFillRgbColor, shapeBorderRGBColor);
                                }
                            }
                        }

                        //标注处理
                        if (info.Labels != null && info.Labels.Count > 0)
                        {
                            foreach (string str in info.Labels)
                            {
                                string[] strs = str.Split(';');
                                string name = strs[0];
                                string labelxywkt = strs[1];
                                string labelwkt = strs[2];

                                //注记数据 name;labelxywkt;labelwkt;fontname;fontsize;fontcolor;labelbordercolor;labelfillcolor;labelborderthikness
                                IRgbColor labelFontColor = new RgbColorClass();
                                IRgbColor labelBorderColor = new RgbColorClass();
                                IRgbColor labelFillColor = new RgbColorClass();
                                double labelBorderWidth = 1;
                                int fontSize = 14;
                                string fontName = "宋体";
                                if (strs.Length == 9)
                                {
                                    fontName = string.IsNullOrEmpty(strs[3]) ? "宋体" : strs[3];
                                    int.TryParse(strs[4], out fontSize);
                                    string[] fontcolorargb = strs[5].Split(',');
                                    if (fontcolorargb.Length == 4)
                                    {
                                        int fa = 0, fr = 255, fg = 0, fb = 0;
                                        int.TryParse(fontcolorargb[0], out fa);
                                        int.TryParse(fontcolorargb[1], out fr);
                                        int.TryParse(fontcolorargb[2], out fg);
                                        int.TryParse(fontcolorargb[3], out fb);
                                        labelFontColor.Red = fr;
                                        labelFontColor.Green = fg;
                                        labelFontColor.Blue = fb;
                                        labelFontColor.Transparency = (byte)fa;

                                    }
                                    string[] labelborderargb = strs[6].Split(',');
                                    if (labelborderargb.Length == 4)
                                    {
                                        int ba = 0, br = 255, bg = 0, bb = 0;
                                        int.TryParse(labelborderargb[0], out ba);
                                        int.TryParse(labelborderargb[1], out br);
                                        int.TryParse(labelborderargb[2], out bg);
                                        int.TryParse(labelborderargb[3], out bb);
                                        labelBorderColor.Red = br;
                                        labelBorderColor.Green = bg;
                                        labelBorderColor.Blue = bb;
                                        labelBorderColor.Transparency = (byte)ba;

                                    }
                                    string[] labelfillargb = strs[7].Split(',');
                                    if (labelfillargb.Length == 4)
                                    {
                                        int lfa = 0, lfr = 255, lfg = 0, lfb = 0;
                                        int.TryParse(labelfillargb[0], out lfa);
                                        int.TryParse(labelfillargb[1], out lfr);
                                        int.TryParse(labelfillargb[2], out lfg);
                                        int.TryParse(labelfillargb[3], out lfb);
                                        labelFillColor.Red = lfr;
                                        labelFillColor.Green = lfg;
                                        labelFillColor.Blue = lfb;
                                        labelFillColor.Transparency = (byte)lfa;
                                    }
                                    double.TryParse(strs[8], out labelBorderWidth);
                                }

                                IGeometry labelxyGeometry = EsriWktConverter.ConvertWKTToGeometry(labelxywkt);
                                IPoint pLabelXY = labelxyGeometry as IPoint;
                                double labelX = pLabelXY.X;
                                double labelY = pLabelXY.Y;
                                IGeometry labelGeometry = EsriWktConverter.ConvertWKTToGeometry(labelwkt);
                                IPolygon pLabelPolygon = labelGeometry as IPolygon;
                                PageElementUtility.AddPolygonElement(pMapDocument, pLabelPolygon, labelBorderWidth, labelFillColor, labelBorderColor);
                                PageElementUtility.AddTextElement(pMapDocument, labelX, labelY, name, fontSize, fontName, labelFontColor);
                            }
                        }

                        info.OutPath = printPath + "\\LocalFiles\\";
                        try
                        {
                            foreach (string tmpPath in info.AdditionalImages)
                            {
                                PageElementUtility.AddImageElement(pPageLayout
                                    , info.OutPath + tmpPath
                                    , Guid.NewGuid().ToString()
                                    , ElementPosition.DBL
                                    , 0.05
                                    , 0.05);
                            }
                        }
                        catch
                        {
                        }

                        string tmpfilename = (info.TemplateName.ToUpper().EndsWith(".MXD") ? info.TemplateName.Substring(0, info.TemplateName.Length - 4) : info.TemplateName) + Guid.NewGuid().ToString() + ".pdf";
                        info.OutPath += tmpfilename;
                        switch (info.ExportFormat.Trim().ToUpper())
                        {
                            case "PDF":
                                docExport = new ExportPDFClass();
                                break;
                            case "BMP":
                                docExport = new ExportBMPClass();
                                info.OutPath = info.OutPath.Replace(".pdf", ".bmp");
                                break;
                            case "JPG":
                                docExport = new ExportJPEGClass();
                                info.OutPath = info.OutPath.Replace(".pdf", ".jpg");
                                break;
                            default:
                                docExport = new ExportPDFClass();
                                break;

                        }
                        docExport.ExportFileName = info.OutPath;
                        docPrintExport.Export(pPageAV, docExport, info.DPI, false, null);
                        outmsg = printIISRoot + tmpfilename;
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        outmsg = string.Format("制图错误：坐标系是否一致——{0}", ex.Message);
                    }
                    finally
                    {
                        ReleaseObject(pMapDocument);
                        ReleaseObject(docExport);
                        ReleaseObject(docPrintExport);
                    }
                }
                else
                {
                    result = false;
                    outmsg = "制图模板：" + info.MxdPath + GetFilePath() + "未找到!";
                }

                #endregion
            }
            else
            {
                result = hasright;
            }

            return result;
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        /// <param name="obj"></param>
        public void ReleaseObject(object obj)
        {
            if (obj != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
        }

        private void SetPageTemplate(IPageLayout pPageLayout, IPoint dataCenter, double l, double r, double t, double b, PageSize ps, PageOrientation po, ExportMapInfo info)
        {
            //设定数据 根据中心点和比例尺
            string legendname = "LEGEND";
            IActiveView pPageAV = pPageLayout as IActiveView;
            IActiveView pMapAV = pPageAV.FocusMap as IActiveView;
            IEnvelope pMapFrameExtent = pMapAV.Extent;
            IMapFrame pPageMapFrame = pPageAV.GraphicsContainer.FindFrame(pPageAV.FocusMap) as IMapFrame;
            IElement pPageFrameElement = pPageMapFrame as IElement;
            IEnvelope pPageFrameExtent = pPageFrameElement.Geometry.Envelope;
            double offsetInline = 1.25;
            double offsetOutline = 1.5;

            List<string> ecpnameList = new List<string>();
            ecpnameList.Add(legendname);
            PageElementUtility.RemoveElementExceptNames(pPageLayout, ecpnameList);
            //WEBTXPT-1383
            PageSizeUtility.SetPageSize(pPageLayout, l + offsetOutline, r + offsetOutline + 0.2, t + offsetOutline, b + offsetOutline + 0.2, (esriPageFormID)((int)ps), (short)po);

            IElement legend = PageElementUtility.GetElementByName(pPageLayout, legendname);
            if (legend != null)
            {
                //PageElementUtility.ResizeElement(pPageLayout, legend, ElementPosition.DBR, 0, 0);
                PageElementUtility.MoveElement(pPageLayout, legend, ElementPosition.DBR, 0, 0);
                PageElementUtility.FrontOrBackElement(pPageLayout, legendname, ElementFrontBack.Front);
            }

            string nameInline = Guid.NewGuid().ToString();
            string nameOutline = Guid.NewGuid().ToString();
            PageElementUtility.AddRectElement(pPageLayout, nameInline, 1, offsetInline, offsetInline, offsetInline, offsetInline);
            PageElementUtility.FrontOrBackElement(pPageLayout, nameInline, ElementFrontBack.Back);
            PageElementUtility.AddRectElement(pPageLayout, nameOutline, 2, offsetOutline, offsetOutline, offsetOutline, offsetOutline);
            PageElementUtility.FrontOrBackElement(pPageLayout, nameOutline, ElementFrontBack.Back);

            double linewidth = 0.5;
            PageElementUtility.AddLineElement(pPageLayout, Guid.NewGuid().ToString(), linewidth, LinePosition.Left, 0, offsetInline, offsetInline);
            PageElementUtility.AddLineElement(pPageLayout, Guid.NewGuid().ToString(), linewidth, LinePosition.Top, 0, offsetInline, offsetInline);
            PageElementUtility.AddLineElement(pPageLayout, Guid.NewGuid().ToString(), linewidth, LinePosition.Right, 0, offsetInline, offsetInline);
            PageElementUtility.AddLineElement(pPageLayout, Guid.NewGuid().ToString(), linewidth, LinePosition.Bottom, 0, offsetInline, offsetInline);
            //添加四角的坐标显示
            double offsetXx = 0.1;
            double offsetXy = 0.9;
            double offsetYx = 0.1;
            double offsetYy = 0.3;
            decimal fontSize1 = 7;

            //获取纸张的图形显示数据的四个点纸张坐标，然后转换成地图坐标
            double w, h;
            pPageLayout.Page.QuerySize(out w, out h);
            IPoint ltDataPoint = new PointClass();
            IPoint lbDataPoint = new PointClass();
            IPoint rtDataPoint = new PointClass();
            IPoint rbDataPoint = new PointClass();
            double distanceX = 0;
            double distanceY = 0;
            distanceX = (((w - l - r) / 2) * pPageMapFrame.MapScale) / 100;
            distanceY = (((h - t - b) / 2) * pPageMapFrame.MapScale) / 100;
            ltDataPoint.PutCoords(dataCenter.X - distanceX, dataCenter.Y + distanceY);
            lbDataPoint.PutCoords(dataCenter.X - distanceX, dataCenter.Y - distanceY);
            rtDataPoint.PutCoords(dataCenter.X + distanceX, dataCenter.Y + distanceY);
            rbDataPoint.PutCoords(dataCenter.X + distanceX, dataCenter.Y - distanceY);
            PageElementUtility.AddTextElement(pPageLayout, (ltDataPoint.X / 1000).ToString("f3"), Guid.NewGuid().ToString(), fontSize1, ElementPosition.TL, offsetXx, offsetXy);
            PageElementUtility.AddTextElement(pPageLayout, (ltDataPoint.Y / 1000).ToString("f3"), Guid.NewGuid().ToString(), fontSize1, ElementPosition.LT, offsetYx, -offsetYy);
            PageElementUtility.AddTextElement(pPageLayout, (lbDataPoint.X / 1000).ToString("f3"), Guid.NewGuid().ToString(), fontSize1, ElementPosition.BL, offsetXx, offsetXy);
            PageElementUtility.AddTextElement(pPageLayout, (lbDataPoint.Y / 1000).ToString("f3"), Guid.NewGuid().ToString(), fontSize1, ElementPosition.LB, offsetYx, -offsetYy);
            PageElementUtility.AddTextElement(pPageLayout, (rtDataPoint.X / 1000).ToString("f3"), Guid.NewGuid().ToString(), fontSize1, ElementPosition.TR, offsetXx, offsetXy);
            PageElementUtility.AddTextElement(pPageLayout, (rtDataPoint.Y / 1000).ToString("f3"), Guid.NewGuid().ToString(), fontSize1, ElementPosition.RT, offsetYx, -offsetYy);
            PageElementUtility.AddTextElement(pPageLayout, (rbDataPoint.X / 1000).ToString("f3"), Guid.NewGuid().ToString(), fontSize1, ElementPosition.BR, offsetXx, offsetXy);
            PageElementUtility.AddTextElement(pPageLayout, (rbDataPoint.Y / 1000).ToString("f3"), Guid.NewGuid().ToString(), fontSize1, ElementPosition.RB, offsetYx, -offsetYy);

            //图幅号获取
            string new_tfh_10000 = string.Empty;
            //if ((int)info.Scale == 10000)
            //{
            //    TFBH pTFBH = new TFBH();

            //    #region 数据库获取图幅号

            //    string wkt = string.Format(@"POLYGON  (( {0} {1}, {2} {3}, {4} {5}, {6} {7}, {8} {9}))"
            //        , ltDataPoint.X, ltDataPoint.Y, rtDataPoint.X, rtDataPoint.Y, rbDataPoint.X, rbDataPoint.Y, lbDataPoint.X, lbDataPoint.Y, ltDataPoint.X, ltDataPoint.Y);
            //    string tmp_tfh = pTFBH.GetTFHbySQL(wkt);
            //    #endregion
            //    if (!string.IsNullOrEmpty(tmp_tfh))
            //    {
            //        new_tfh_10000 = string.Format("[{0}]", tmp_tfh);
            //    }
            //    else
            //    {
            //        new_tfh_10000 = string.Format("[{0}]", pTFBH.GetLbToMapNewTFBH(info.DelNo, 0, ltDataPoint.X, ltDataPoint.Y, false));
            //    }
            //    PageElementUtility.AddTextElement(pPageLayout, new_tfh_10000, Guid.NewGuid().ToString(), 14, ElementPosition.TM, 0, offsetOutline + 0.1);
            //}
            PageElementUtility.AddTextElement(pPageLayout, info.MapName, Guid.NewGuid().ToString(), 18, ElementPosition.TM, 0, offsetOutline + 0.7);
            string xmmc = string.IsNullOrEmpty(info.ProjectName) ? "项目名称默认标题" : info.ProjectName;
            PageElementUtility.AddTextElement(pPageLayout, xmmc, Guid.NewGuid().ToString(), ElementPosition.TM, 0, 0.2);
            PageElementUtility.AddTextElement(pPageLayout, string.Format("制图日期：{0}", info.MapDate), Guid.NewGuid().ToString(), ElementPosition.BR, -offsetOutline, offsetOutline + 0.2);
            string mapunit = string.IsNullOrEmpty(info.MapUnit) ? "默认制图单位" : info.MapUnit;
            PageElementUtility.AddTextElement(pPageLayout, mapunit, Guid.NewGuid().ToString(), ElementPosition.BL, -offsetOutline, offsetOutline + 0.2);
            PageElementUtility.AddTextElement(pPageLayout, string.Format("制图人：{0}", info.MapAuthor), Guid.NewGuid().ToString(), ElementPosition.BL, -offsetOutline, offsetOutline + 0.8);
            PageElementUtility.AddTextElement(pPageLayout, string.Format("比例尺： 1:{0}", pPageMapFrame.MapScale.ToString("f0")), Guid.NewGuid().ToString(), ElementPosition.BM, 0, offsetOutline + 0.2);

            PageElementUtility.AddNorthArrow(pPageLayout, Guid.NewGuid().ToString(), ElementPosition.DTR, 0, 0);
        }

        #endregion
    }
}
