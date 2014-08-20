using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;

using stdole;

namespace PrintMap
{

    #region 打图枚举值

    /// <summary>
    /// 纸张大小
    /// </summary>
    public enum PageSize
    {
        A5 = 6,
        A4 = 7,
        A3 = 8,
        A2 = 9,
        A1 = 10,
        A0 = 11,
        CUSTOM = 12
    }

    /// <summary>
    /// 纸张方向
    /// </summary>
    public enum PageOrientation
    {
        /// <summary>
        /// 纵向
        /// </summary>
        Portrait = 1,
        /// <summary>
        /// 横向
        /// </summary>
        Landscape = 2
    }

    /// <summary>
    /// 元素布局位置
    /// </summary>
    public enum ElementPosition
    {
        /// <summary>
        /// 顶部居左
        /// </summary>
        TL,
        /// <summary>
        /// 顶部居中
        /// </summary>
        TM,
        /// <summary>
        /// 顶部居右
        /// </summary>
        TR,
        /// <summary>
        /// 底部居左
        /// </summary>
        BL,
        /// <summary>
        /// 底部居中
        /// </summary>
        BM,
        /// <summary>
        /// 底部居右
        /// </summary>
        BR,
        /// <summary>
        /// 左侧顶部
        /// </summary>
        LT,
        /// <summary>
        /// 左侧居中
        /// </summary>
        LM,
        /// <summary>
        /// 左侧底部
        /// </summary>
        LB,
        /// <summary>
        /// 右侧顶部
        /// </summary>
        RT,
        /// <summary>
        /// 右侧居中
        /// </summary>
        RM,
        /// <summary>
        /// 右侧底部
        /// </summary>
        RB,
        /// <summary>
        /// 地图框内上左
        /// </summary>
        DTL,
        /// <summary>
        /// 地图框内上右
        /// </summary>
        DTR,
        /// <summary>
        /// 地图框内下左
        /// </summary>
        DBL,
        /// <summary>
        /// 地图框内下右
        /// </summary>
        DBR
    }

    /// <summary>
    /// 线条位置
    /// </summary>
    public enum LinePosition
    {
        Left,
        Right,
        Top,
        Bottom
    }

    /// <summary>
    /// 元素前后显示
    /// </summary>
    public enum ElementFrontBack
    {
        Front,
        Forward,
        Back,
        Backward
    }

    #endregion

    public class DataItem
    {
        string m_Key = string.Empty;

        public string Key
        {
            get { return m_Key; }
            set { m_Key = value; }
        }
        string m_Value = string.Empty;

        public string Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }
    }

    /// <summary>
    /// 制图元素位置控制
    /// </summary>
    public class ElementPositionUtility
    {
        /// <summary>
        /// 返回元素布局位置：以MapFrame为参考进行设置，偏移量也是也此为参考
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="pElement"></param>
        /// <param name="ep"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="rX">out X偏移</param>
        /// v<param name="rY">out Y偏移</param>
        /// <returns></returns>
        public static void GetElementOffset(IPageLayout pageLayout, IElement pElement, ElementPosition ep, double offsetX, double offsetY, out double rX, out double rY)
        {
            rX = 0.0;
            rY = 0.0;

            IPolygon pElementPolygon = new PolygonClass();
            IEnvelope pElementExtent = null;
            IActiveView pActiveView = pageLayout as IActiveView;
            pElement.QueryOutline(pActiveView.ScreenDisplay, pElementPolygon);
            pElementExtent = pElementPolygon.Envelope;
            IMapFrame pMapFrame = pActiveView.GraphicsContainer.FindFrame(pActiveView.FocusMap) as IMapFrame;
            IElement pFrameElement = pMapFrame as IElement;
            IEnvelope pFrameExtent = pFrameElement.Geometry.Envelope;
            //图幅框中心点
            double centerFrameX = pFrameExtent.XMin + (pFrameExtent.XMax - pFrameExtent.XMin) / 2;
            double centerFrameY = pFrameExtent.YMin + (pFrameExtent.YMax - pFrameExtent.YMin) / 2;
            //Element中心点
            double centerElementX = pElementExtent.XMin + (pElementExtent.XMax - pElementExtent.XMin) / 2;
            double centerElementY = pElementExtent.YMin + (pElementExtent.YMax - pElementExtent.YMin) / 2;

            switch (ep)
            {
                case ElementPosition.TL: //顶部居左 mapFrame
                    rX = pFrameExtent.XMin - pElementExtent.XMin + offsetX;
                    rY = pFrameExtent.YMax - pElementExtent.YMin + offsetY;
                    break;
                case ElementPosition.TM: //顶部居中
                    rX = centerFrameX - centerElementX + offsetX;
                    rY = pFrameExtent.YMax - pElementExtent.YMin + offsetY;
                    break;
                case ElementPosition.TR:
                    rX = pFrameExtent.XMax - pElementExtent.XMax - offsetX;
                    rY = pFrameExtent.YMax - pElementExtent.YMin + offsetY;
                    break;
                case ElementPosition.BL:
                    rX = pFrameExtent.XMin - pElementExtent.XMin + offsetX;
                    rY = pFrameExtent.YMin - pElementExtent.YMax - offsetY;
                    break;
                case ElementPosition.BM:
                    rX = centerFrameX - centerElementX + offsetX;
                    rY = pFrameExtent.YMin - pElementExtent.YMax - offsetY;
                    break;
                case ElementPosition.BR:
                    rX = pFrameExtent.XMax - pElementExtent.XMax - offsetX;
                    rY = pFrameExtent.YMin - pElementExtent.YMax - offsetY;
                    break;
                case ElementPosition.LT:
                    rX = pFrameExtent.XMin - pElementExtent.XMax - offsetX;
                    rY = pFrameExtent.YMax - pElementExtent.YMax - offsetY;
                    break;
                case ElementPosition.LM:
                    rX = pFrameExtent.XMin - pElementExtent.XMax - offsetX;
                    rY = centerFrameY - centerElementY - offsetY;
                    break;
                case ElementPosition.LB:
                    rX = pFrameExtent.XMin - pElementExtent.XMax - offsetX;
                    rY = pFrameExtent.YMin - pElementExtent.YMin + offsetY;
                    break;
                case ElementPosition.RT:
                    rX = pFrameExtent.XMax - pElementExtent.XMin + offsetX;
                    rY = pFrameExtent.YMax - pElementExtent.YMax - offsetY;
                    break;
                case ElementPosition.RM:
                    rX = pFrameExtent.XMax - pElementExtent.XMin + offsetX;
                    rY = centerFrameY - centerElementY - offsetY;
                    break;
                case ElementPosition.RB:
                    rX = pFrameExtent.XMax - pElementExtent.XMin + offsetX;
                    rY = pFrameExtent.YMin - pElementExtent.YMin + offsetY;
                    break;
                case ElementPosition.DTL:
                    rX = pFrameExtent.XMin - pElementExtent.XMin + offsetX;
                    rY = pFrameExtent.YMax - pElementExtent.YMax - offsetY;
                    break;
                case ElementPosition.DTR:
                    rX = pFrameExtent.XMax - pElementExtent.XMax - offsetX;
                    rY = pFrameExtent.YMax - pElementExtent.YMax - offsetY;
                    break;
                case ElementPosition.DBL:
                    rX = pFrameExtent.XMin - pElementExtent.XMin + offsetX;
                    rY = pFrameExtent.YMin - pElementExtent.YMin + offsetY;
                    break;
                case ElementPosition.DBR:
                    rX = pFrameExtent.XMax - pElementExtent.XMax - offsetX;
                    rY = pFrameExtent.YMin - pElementExtent.YMin + offsetY;
                    break;
                default:
                    break;
            }
        }
    }
    
    /// <summary>
    /// 制图纸张大小控制
    /// </summary>
    public class PageSizeUtility
    {
        public static void SetCenterAndScale(IPageLayout pageLayout, IPoint dataCenter, double scale)
        {
            IActiveView pageActiveView = pageLayout as IActiveView;
            IActiveView dataActiveView = pageActiveView.FocusMap as IActiveView;
            IGraphicsContainer pGraphicsContainer = pageLayout as IGraphicsContainer;
            IMapFrame pPageMapFrame = pGraphicsContainer.FindFrame(pageActiveView.FocusMap) as IMapFrame;
            pPageMapFrame.ExtentType = esriExtentTypeEnum.esriExtentScale;
            pPageMapFrame.MapScale = scale;
            IEnvelope pEnv = dataActiveView.Extent;
            pEnv.CenterAt(dataCenter);
            dataActiveView.Extent = pEnv;
            pageActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, null);
        }

        /// <summary>
        /// 返回纸张宽度和高度，单位为mm
        /// </summary>
        /// <param name="size"></param>
        /// <param name="ori"></param>
        /// <param name="worh">W,H</param>
        /// <returns></returns>
        public static double GetPageWORHMM(PageSize size, PageOrientation ori, string worh)
        {
            double margin = 30;
            double result = 0;

            double w = 297, h = 210;
            switch (size)
            {
                case PageSize.A5:
                    w = 210;
                    h = 148;
                    break;
                case PageSize.A4:
                    w = 297;
                    h = 210;
                    break;
                case PageSize.A3:
                    w = 420;
                    h = 297;
                    break;
                case PageSize.A2:
                    w = 594;
                    h = 420;
                    break;
                case PageSize.A1:
                    w = 841;
                    h = 594;
                    break;
                case PageSize.A0:
                    w = 1189;
                    h = 841;
                    break;
                case PageSize.CUSTOM:
                    break;
                default:
                    break;
            }

            switch (ori)
            {
                case PageOrientation.Portrait:
                    double t = w;
                    w = h;
                    h = t;
                    break;
                case PageOrientation.Landscape:
                    break;
                default:
                    break;
            }

            if (worh.ToUpper().Equals("W"))
            {
                result = w - margin * 2;
            }
            else
            {
                result = h - margin * 2;
            }

            return result;
        }

        public static double GetPageWCM(PageSize size, PageOrientation ori)
        {
            return GetPageWORHMM(size, ori, "W") / 10;
        }

        public static double GetPageHCM(PageSize size, PageOrientation ori)
        {
            return GetPageWORHMM(size, ori, "H") / 10;
        }

        public static double GetPageWIn(PageSize size, PageOrientation ori)
        {
            return GetPageWORHMM(size, ori, "W") / (10 * 2.54);
        }

        public static double GetPageHIn(PageSize size, PageOrientation ori)
        {
            return GetPageWORHMM(size, ori, "H") / (10 * 2.54);
        }

        /// <summary>
        /// 设定纸张
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <param name="t"></param>
        /// <param name="b"></param>
        /// <param name="pageform"></param>
        /// <param name="oi"></param>
        public static void SetPageSize(IPageLayout pageLayout, double l, double r, double t, double b, esriPageFormID pageform, short oi)
        {
            pageLayout.Page.FormID = pageform;
            pageLayout.Page.Orientation = oi;
            SetPageSize(pageLayout, l, r, t, b);
        }

        /// <summary>
        /// 设定纸张
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <param name="t"></param>
        /// <param name="b"></param>
        /// <param name="oi"></param>
        public static void SetPageSize(IPageLayout pageLayout, double l, double r, double t, double b, short oi)
        {
            pageLayout.Page.Orientation = oi;
            SetPageSize(pageLayout, l, r, t, b);
        }

        /// <summary>
        /// 设定纸张
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <param name="t"></param>
        /// <param name="b"></param>
        /// <param name="pageform"></param>
        public static void SetPageSize(IPageLayout pageLayout, double l, double r, double t, double b, esriPageFormID pageform)
        {
            pageLayout.Page.FormID = pageform;
            SetPageSize(pageLayout, l, r, t, b);
        }

        /// <summary>
        /// 设定纸张
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <param name="t"></param>
        /// <param name="b"></param>
        public static void SetPageSize(IPageLayout pageLayout, double l, double r, double t, double b)
        {
            double w, h;
            pageLayout.Page.QuerySize(out w, out h);
            IActiveView pAV = pageLayout as IActiveView;
            IGraphicsContainer pGraphicsContainer = pageLayout as IGraphicsContainer;
            IGraphicsContainerSelect pGraphicsContainerSelect = (IGraphicsContainerSelect)pGraphicsContainer;
            pGraphicsContainer.Reset();
            IElement pElement = pGraphicsContainer.Next();
            while (pElement != null)
            {
                if (pElement is IMapFrame)
                {
                    IMapFrame pMapFrame = pElement as IMapFrame;
                    IEnvelope pEnvelope = new EnvelopeClass();
                    pEnvelope.PutCoords(l, b, w - r, h - t);//left,bottom,right,top
                    pElement.Geometry = pEnvelope;
                    //pageLayoutControl.Refresh();
                    pGraphicsContainerSelect.SelectElement(pElement);
                    IEnumElement pEnumElement = (IEnumElement)pGraphicsContainerSelect.SelectedElements;
                    pEnumElement.Reset();
                    pGraphicsContainer.BringForward(pEnumElement);
                    pGraphicsContainerSelect.UnselectElement(pElement);
                    pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                }
                pElement = pGraphicsContainer.Next();
            }
        }
    }

    /// <summary>
    /// 制图元素生成
    /// </summary>
    public class PageElementUtility
    {
        /// <summary>
        /// 获取元素外边框
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="pElement"></param>
        /// <param name="eleExtend"></param>
        public static void GetElementExtend(IPageLayout pageLayout, IElement pElement, out IEnvelope eleExtend)
        {
            IActiveView pActiveView = pageLayout as IActiveView;
            IPolygon pElementPolygon = new PolygonClass();
            pElement.QueryOutline(pActiveView.ScreenDisplay, pElementPolygon);
            eleExtend = pElementPolygon.Envelope;
        }

        /// <summary>
        /// 测试MapSurround的大小调整
        /// </summary>
        /// <param name="pageLayout"></param>
        public static void ResizeMapSurroundElement(IPageLayout pageLayout)
        {
            IGraphicsContainer GC = pageLayout as IGraphicsContainer;
            IActiveView pAV = pageLayout as IActiveView;
            GC.Reset();
            IElement element = GC.Next();
            while (element != null)
            {
                IElementProperties2 pEleProperties = element as IElementProperties2;
                if (element is IMapSurroundFrame || element is IGroupElement || element is IPictureElement)
                {
                    IEnvelope pEnvelope = new EnvelopeClass();
                    pEnvelope.PutCoords(0, 0, 5, 5);  //新的页面大小
                    element.Geometry = pEnvelope as IGeometry;
                }
                element = GC.Next();
            }
            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        public static void ResizeGroupElement(IPageLayout pageLayout, IGroupElement pGElement, double rate)
        {
            IElement pElement = pGElement as IElement;
            ResizeElement(pageLayout, pElement, rate);

            for (int i = 0; i < pGElement.ElementCount; i++)
            {
                IElement tmpElement = pGElement.get_Element(i);
                ResizeElement(pageLayout, tmpElement, rate);
                IGroupElement tmpPGElement = tmpElement as IGroupElement;
                if (tmpPGElement != null)
                {
                    ResizeGroupElement(pageLayout, tmpPGElement, rate);
                }
            }
        }

        /// <summary>
        /// 默认为地图数据框的一半
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="pElement"></param>
        /// <param name="ep"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public static void ResizeElement(IPageLayout pageLayout, IElement pElement, ElementPosition ep, double offsetX, double offsetY)
        {
            IActiveView pActiveView = pageLayout as IActiveView;
            IPolygon pElementPolygon = new PolygonClass();
            pElement.QueryOutline(pActiveView.ScreenDisplay, pElementPolygon);
            IEnvelope pElementExtend = pElementPolygon.Envelope;
            IMapFrame pMapFrame = pActiveView.GraphicsContainer.FindFrame(pActiveView.FocusMap) as IMapFrame;
            IElement pFrameElement = pMapFrame as IElement;
            IEnvelope pFrameExtent = pFrameElement.Geometry.Envelope;
            double rate = ((pFrameExtent.XMax - pFrameExtent.XMin) / 2) / (pElementExtend.XMax - pElementExtend.XMin);

            ResizeElement(pageLayout, pElement, rate);
            MoveElement(pageLayout, pElement, ep, offsetX, offsetY);
        }

        public static void ResizeElement(IPageLayout pageLayout, IElement pElement, double rate)
        {
            IEnvelope pElementExtend;
            IElementProperties2 pElementProperties = pElement as IElementProperties2;

            IActiveView pActiveView = pageLayout as IActiveView;
            IGraphicsContainer pGraphicsContainer = pActiveView as IGraphicsContainer;
            GetElementExtend(pageLayout, pElement, out pElementExtend);
            //左下角不动调整大小
            double wo, ho;//源宽度高度
            double wn, hn;//现宽度高度
            double wd, hd;//差值
            wo = (pElementExtend.XMax - pElementExtend.XMin);
            ho = (pElementExtend.YMax - pElementExtend.YMin);
            wn = wo * rate;
            hn = ho * rate;
            wd = wn - wo;
            hd = hn - ho;

            double xMin, xMax, yMin, yMax;
            xMin = pElementExtend.XMin;
            yMin = pElementExtend.YMin;
            xMax = pElementExtend.XMax + wd;
            yMax = pElementExtend.YMax + hd;
            IEnvelope pEnv = new EnvelopeClass();
            pEnv.PutCoords(xMin, yMin, xMax, yMax);
            pElement.Geometry = pEnv;
            pGraphicsContainer.UpdateElement(pElement);

            //pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        public static void RemoveElementByNames(IPageLayout pageLayout, List<string> names)
        {
            foreach (string n in names)
            {
                if (!string.IsNullOrEmpty(n))
                {
                    RemoveElementByName(pageLayout, n);
                }
            }
        }

        public static void RemoveElementExceptNames(IPageLayout pageLayout, List<string> excpNames)
        {
            IActiveView pAV = pageLayout as IActiveView;
            IGraphicsContainer pGraphicsContainer = pageLayout as IGraphicsContainer;
            pGraphicsContainer.Reset();
            List<string> delNameList = new List<string>();
            IElement pElement = pGraphicsContainer.Next();
            while (pElement != null)
            {
                IElementProperties2 pElementProperties2 = pElement as IElementProperties2;
                IMapFrame pMapFrame = pElement as IMapFrame;
                if (pMapFrame == null)
                {
                    var ns = from n in excpNames where pElementProperties2.Name.ToUpper().EndsWith(n.ToUpper()) select n;
                    List<string> nsList = ns.ToList<string>();

                    if (nsList.Count == 0)
                    {
                        delNameList.Add(pElementProperties2.Name);
                    }
                }
                pElement = pGraphicsContainer.Next();
            }

            RemoveElementByNames(pageLayout, delNameList);
            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        /// <summary>
        /// 根据特定名称删除Element
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageLayout"></param>
        public static void RemoveElementByName(IPageLayout pageLayout, string name)
        {
            IActiveView pAV = pageLayout as IActiveView;
            IGraphicsContainer pGraphicsContainer = pageLayout as IGraphicsContainer;
            pGraphicsContainer.Reset();
            IElement pElement = pGraphicsContainer.Next();
            while (pElement != null)
            {
                IElementProperties2 pElementProperties2 = pElement as IElementProperties2;
                if (pElementProperties2.Name.ToUpper().EndsWith(name.ToUpper()))
                {
                    pGraphicsContainer.DeleteElement(pElement);
                    break;
                }
                pElement = pGraphicsContainer.Next();
            }
            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        public static IGroupElement GetGroupElementByName(IPageLayout pageLayout, string name)
        {
            IGroupElement result = null;
            IActiveView pActiveView = pageLayout as IActiveView;
            IGraphicsContainer pGraphicsContainer = pActiveView as IGraphicsContainer;
            pGraphicsContainer.Reset();
            IMap pMap = pActiveView.FocusMap;
            IElement pElement = pGraphicsContainer.Next();
            while (pElement != null)
            {
                result = pElement as IGroupElement;
                if (result != null)
                {
                    IElementProperties2 pElementProperties2 = pElement as IElementProperties2;
                    if (pElementProperties2.Name.ToUpper().EndsWith(name.ToUpper()))
                    {
                        break;
                    }
                }
                pElement = pGraphicsContainer.Next();
            }

            return result;
        }

        public static IMapSurroundFrame GetMapSurroundElementByName(IPageLayout pageLayout, string name)
        {
            IMapSurroundFrame result = null;
            IActiveView pActiveView = pageLayout as IActiveView;
            IGraphicsContainer pGraphicsContainer = pActiveView as IGraphicsContainer;
            pGraphicsContainer.Reset();
            IMap pMap = pActiveView.FocusMap;
            IElement pElement = pGraphicsContainer.Next();
            while (pElement != null)
            {
                result = pElement as IMapSurroundFrame;
                if (result != null)
                {
                    IElementProperties2 pElementProperties2 = pElement as IElementProperties2;
                    if (pElementProperties2.Name.ToUpper().EndsWith(name.ToUpper()))
                    {
                        break;
                    }
                }
                pElement = pGraphicsContainer.Next();
            }

            return result;
        }

        public static T GetElementByName<T>(IPageLayout pageLayout, string name)
        {
            T result = default(T);

            IActiveView pActiveView = pageLayout as IActiveView;
            IGraphicsContainer pGraphicsContainer = pActiveView as IGraphicsContainer;
            pGraphicsContainer.Reset();
            IMap pMap = pActiveView.FocusMap;
            IElement pElement = pGraphicsContainer.Next();
            while (pElement != null)
            {
                System.Type t = typeof(T);

                //result = (T)pElement;
                //if (result != null)
                //{
                IElementProperties2 pElementProperties2 = pElement as IElementProperties2;
                if (pElementProperties2.Name.ToUpper().EndsWith(name.ToUpper()))
                {
                    break;
                }
                //}
                pElement = pGraphicsContainer.Next();
            }

            return result;
        }

        /// <summary>
        /// 根据名称获取元素
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IElement GetElementByName(IPageLayout pageLayout, string name)
        {
            IElement result = null;
            IActiveView pActiveView = pageLayout as IActiveView;
            IGraphicsContainer pGraphicsContainer = pActiveView as IGraphicsContainer;
            pGraphicsContainer.Reset();
            IElement pElement = pGraphicsContainer.Next();
            while (pElement != null)
            {
                IElementProperties2 pElementProperties2 = pElement as IElementProperties2;
                if (pElementProperties2.Name.ToUpper().EndsWith(name.ToUpper()))
                {
                    result = pElement;
                    break;
                }
                pElement = pGraphicsContainer.Next();
            }

            return result;
        }

        /// <summary>
        /// 根据屏幕坐标获取地图坐标
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <param name="activeView"></param>
        /// <returns></returns>
        public static ESRI.ArcGIS.Geometry.IPoint GetMapCoordinatesFromScreenCoordinates(IPoint screenPoint, IActiveView activeView)
        {

            if (screenPoint == null || screenPoint.IsEmpty || activeView == null)
            {
                return null;
            }

            ESRI.ArcGIS.Display.IScreenDisplay screenDisplay = activeView.ScreenDisplay;
            ESRI.ArcGIS.Display.IDisplayTransformation displayTransformation =
                screenDisplay.DisplayTransformation;
            return displayTransformation.ToMapPoint((System.Int32)screenPoint.X,
                (System.Int32)screenPoint.Y); // Explicit cast.
        }

        /// <summary>
        /// 移动元素
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="pElement"></param>
        /// <param name="ep"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public static void MoveElement(IPageLayout pageLayout, IElement pElement, ElementPosition ep, double offsetX, double offsetY)
        {
            double rX, rY;
            IActiveView pAV = pageLayout as IActiveView;
            ElementPositionUtility.GetElementOffset(pageLayout, pElement, ep, offsetX, offsetY, out rX, out rY);
            ITransform2D transObj = pElement as ITransform2D;
            transObj.Move(rX, rY);
            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        public static void MoveElement(IPageLayout pageLayout, IElement pElement, double rX, double rY)
        {
            IActiveView pAV = pageLayout as IActiveView;
            ITransform2D transObj = pElement as ITransform2D;
            transObj.Move(rX, rY);
            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        public static void FrontOrBackElement(IPageLayout pageLayout, string name, ElementFrontBack efb)
        {

            IElement pElement = GetElementByName(pageLayout, name);
            if (pElement != null)
            {
                IGraphicsContainer pGraphicsContainer;
                IGraphicsContainerSelect pGraphicsContainerSelect;
                IActiveView pActiveView = pageLayout as IActiveView;
                pGraphicsContainer = pActiveView as IGraphicsContainer;
                pGraphicsContainerSelect = (IGraphicsContainerSelect)pGraphicsContainer;

                pGraphicsContainerSelect.SelectElement(pElement);
                IEnumElement pEnumElement = (IEnumElement)pGraphicsContainerSelect.SelectedElements;
                pEnumElement.Reset();
                switch (efb)
                {
                    case ElementFrontBack.Front:
                        pGraphicsContainer.BringToFront(pEnumElement);
                        break;
                    case ElementFrontBack.Forward:
                        pGraphicsContainer.BringForward(pEnumElement);
                        break;
                    case ElementFrontBack.Back:
                        pGraphicsContainer.SendToBack(pEnumElement);
                        break;
                    case ElementFrontBack.Backward:
                        pGraphicsContainer.SendBackward(pEnumElement);
                        break;
                    default:
                        pGraphicsContainer.BringForward(pEnumElement);
                        break;
                }
                pGraphicsContainerSelect.UnselectElement(pElement);
                pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
        }

        public static void AddPolygonElement(IMapDocument pMapDocument, IPolygon pPolygon, double borderWidth, IRgbColor fillRgbColor, IRgbColor borderRgbColor)
        {
            ISimpleFillSymbol simpleFillSymbol = new SimpleFillSymbolClass();
            simpleFillSymbol.Color = fillRgbColor;

            ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbolClass();
            simpleLineSymbol.Color = borderRgbColor;
            simpleLineSymbol.Width = borderWidth;

            simpleFillSymbol.Outline = simpleLineSymbol;
            IElement pElement;
            IPolygonElement pPolygonElement = new PolygonElementClass();

            IFillShapeElement fillShapeElement = pPolygonElement as IFillShapeElement;
            fillShapeElement.Symbol = simpleFillSymbol;

            IActiveView pAV = pMapDocument.ActiveView as IActiveView;
            IGraphicsContainer pMapGraphicsContainer = (IGraphicsContainer)pMapDocument.ActiveView.FocusMap.BasicGraphicsLayer;

            pElement = pPolygonElement as IElement;
            pElement.Geometry = (IGeometry)pPolygon;

            pMapGraphicsContainer.AddElement(pElement, 0);
            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        public static void AddTextElement(IMapDocument pMapDocument, double startx, double starey, string text, decimal fontSize, string fontName, IRgbColor fontColor)
        {
            IActiveView pAV = pMapDocument.ActiveView as IActiveView;
            IGraphicsContainer pMapGraphicsContainer = (IGraphicsContainer)pMapDocument.ActiveView.FocusMap.BasicGraphicsLayer;

            IPoint pPoint = new PointClass();
            IElement pTElement = new TextElementClass();
            TextSymbolClass pTextSymbol = new TextSymbolClass();
            IFontDisp pFont = new StdFont() as IFontDisp;
            pFont.Name = fontName;
            pFont.Size = fontSize;
            pTextSymbol.Font = pFont;
            pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;
            pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVATop;
            pTextSymbol.Color = fontColor;
            ((ITextElement)pTElement).Symbol = pTextSymbol;
            ((ITextElement)pTElement).Text = text;
            //该点和TextSymbol的Alignment属性有关
            pPoint.X = startx;
            pPoint.Y = starey;
            pTElement.Geometry = pPoint;

            pMapGraphicsContainer.AddElement(pTElement, 0);

            //IEnvelope tmpPEnvelope;
            //GetElementExtend(pMapDocument.PageLayout, pTElement, out tmpPEnvelope);

            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        /// <summary>
        /// 添加几何，
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="name"></param>
        /// <param name="borderwidth">线宽度 像素</param>
        /// <param name="rL">距离DataFrame距离左</param>
        /// <param name="rR">距离DataFrame距离右</param>
        /// <param name="rT">距离DataFrame距离上</param>
        /// <param name="rB">距离DataFrame距离下</param>
        public static void AddRectElement(IPageLayout pageLayout, string name, double borderwidth, double rL, double rR, double rT, double rB)
        {
            IGraphicsContainer pGraphicsContainer;
            IGraphicsContainerSelect pGraphicsContainerSelect;
            IMapFrame pMapFrame;
            IElement pFrameElement;
            IEnvelope pFrameExtent;
            IElementProperties2 pElementPropertie2;
            IActiveView pActiveView = pageLayout as IActiveView;
            pGraphicsContainer = pActiveView as IGraphicsContainer;
            pGraphicsContainerSelect = (IGraphicsContainerSelect)pGraphicsContainer;
            pMapFrame = pActiveView.GraphicsContainer.FindFrame(pActiveView.FocusMap) as IMapFrame;
            pFrameElement = pMapFrame as IElement;
            pFrameExtent = pFrameElement.Geometry.Envelope;
            RectangleElement rectElement1 = new RectangleElementClass();
            IEnvelope rect1Enve = new EnvelopeClass();
            rect1Enve.XMax = pFrameExtent.XMax + rR;
            rect1Enve.XMin = pFrameExtent.XMin - rL;
            rect1Enve.YMin = pFrameExtent.YMin - rB;
            rect1Enve.YMax = pFrameExtent.YMax + rT;
            rectElement1.Geometry = (IGeometry)rect1Enve;
            IFillShapeElement pFillShapeElement = (IFillShapeElement)rectElement1;
            ISymbol rectSymbol1 = CreateSimpleFillSymbol(borderwidth, esriSimpleFillStyle.esriSFSSolid);
            pFillShapeElement.Symbol = (IFillSymbol)rectSymbol1;

            pElementPropertie2 = pFillShapeElement as IElementProperties2;
            pElementPropertie2.Name = name;

            pGraphicsContainer.AddElement((IElement)pFillShapeElement, 0);
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        /// <summary>
        /// 添加线元素
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="name"></param>
        /// <param name="linewidth">线宽度，单位像素</param>
        /// <param name="lp">左上右下</param>
        /// <param name="r1">lp为左右，居左或右，lp为上下，居上或下</param>
        /// <param name="r2">lp为上下，则左右距离，lp为左右，则上下距离</param>
        /// <param name="r3"></param>
        public static void AddLineElement(IPageLayout pageLayout, string name, double linewidth, LinePosition lp, double r1, double r2, double r3)
        {
            IGraphicsContainer pGraphicsContainer;
            IMapFrame pMapFrame;
            IElement pFrameElement;
            IEnvelope pFrameExtent;
            IElementProperties2 pElementPropertie2;
            ISimpleLineSymbol pSimpleLineSymbol;
            IActiveView pActiveView = pageLayout as IActiveView;
            pGraphicsContainer = pActiveView as IGraphicsContainer;
            pMapFrame = pActiveView.GraphicsContainer.FindFrame(pActiveView.FocusMap) as IMapFrame;
            pFrameElement = pMapFrame as IElement;
            pFrameExtent = pFrameElement.Geometry.Envelope;

            pSimpleLineSymbol = new SimpleLineSymbol();
            pSimpleLineSymbol.Width = 0.5;
            pSimpleLineSymbol.Color = GetColor(0, 0, 0);
            pSimpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;

            IElement lineElement1 = new LineElementClass();
            IPolyline line1 = new PolylineClass();
            IPoint fromPt = new PointClass();
            IPoint toPt = new PointClass();
            switch (lp)
            {
                case LinePosition.Left: //纵线居左
                    fromPt.X = pFrameExtent.XMin - r1;
                    fromPt.Y = pFrameExtent.YMax + r2;
                    toPt.X = pFrameExtent.XMin - r1;
                    toPt.Y = pFrameExtent.YMin - r3;
                    break;
                case LinePosition.Right: //纵线
                    fromPt.X = pFrameExtent.XMax + r1;
                    fromPt.Y = pFrameExtent.YMax + r2;
                    toPt.X = pFrameExtent.XMax + r1;
                    toPt.Y = pFrameExtent.YMin - r3;
                    break;
                case LinePosition.Top: //横线
                    fromPt.X = pFrameExtent.XMin - r2;
                    fromPt.Y = pFrameExtent.YMax + r1;
                    toPt.X = pFrameExtent.XMax + r3;
                    toPt.Y = pFrameExtent.YMax + r1;
                    break;
                case LinePosition.Bottom: //横线
                    fromPt.X = pFrameExtent.XMin - r2;
                    fromPt.Y = pFrameExtent.YMin - r1;
                    toPt.X = pFrameExtent.XMax + r3;
                    toPt.Y = pFrameExtent.YMin - r1;
                    break;
                default:
                    fromPt.X = pFrameExtent.XMin - r1;
                    fromPt.Y = pFrameExtent.YMax + r2;
                    toPt.X = pFrameExtent.XMin - r1;
                    toPt.Y = pFrameExtent.YMin - r3;
                    break;
            }

            line1.FromPoint = fromPt;
            line1.ToPoint = toPt;
            lineElement1.Geometry = line1;
            ILineElement pLineElement = (ILineElement)lineElement1;
            pLineElement.Symbol = (ILineSymbol)pSimpleLineSymbol;

            pElementPropertie2 = pLineElement as IElementProperties2;
            pElementPropertie2.Name = name;

            pGraphicsContainer.AddElement((IElement)pLineElement, 0);
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        /// <summary>
        /// 获取symbol
        /// </summary>
        /// <param name="oLineWidth"></param>
        /// <param name="fillStyle"></param>
        /// <returns></returns>
        public static ISymbol CreateSimpleFillSymbol(double oLineWidth, esriSimpleFillStyle fillStyle)
        {
            ISimpleFillSymbol pSimpleFillSymbol;
            pSimpleFillSymbol = new SimpleFillSymbol();
            pSimpleFillSymbol.Style = fillStyle;
            pSimpleFillSymbol.Color = GetColor(255, 255, 255);
            pSimpleFillSymbol.Outline = (ILineSymbol)CreateSimpleLineSymbol(oLineWidth, esriSimpleLineStyle.esriSLSSolid);
            return (ISymbol)pSimpleFillSymbol;

        }

        public static ISymbol CreateSimpleLineSymbol(double width, esriSimpleLineStyle style)
        {
            ISimpleLineSymbol pSimpleLineSymbol;
            pSimpleLineSymbol = new SimpleLineSymbol();
            pSimpleLineSymbol.Width = width;
            pSimpleLineSymbol.Color = GetColor(0, 0, 0);
            pSimpleLineSymbol.Style = style;
            return (ISymbol)pSimpleLineSymbol;

        }

        /// <summary>
        /// 获取颜色rgb
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IRgbColor GetColor(int r, int g, int b)
        {
            RgbColor color = new RgbColor();
            color.Red = r;
            color.Green = g;
            color.Blue = b;
            return color;
        }

        /// <summary>
        /// 创建指定位置的文本元素
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="msg"></param>
        /// <param name="name"></param>
        /// <param name="ep"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public static void AddTextElement(IPageLayout pageLayout, string msg, string name, decimal fontSize, ElementPosition ep, double offsetX, double offsetY)
        {
            AddTextElement(pageLayout, msg, name, fontSize);
            IElement pElement = GetElementByName(pageLayout, name);
            if (pElement != null)
            {
                MoveElement(pageLayout, pElement, ep, offsetX, offsetY);
            }
        }

        /// <summary>
        /// 创建指定位置的文本元素
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="msg"></param>
        /// <param name="name"></param>
        /// <param name="ep"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public static void AddTextElement(IPageLayout pageLayout, string msg, string name, ElementPosition ep, double offsetX, double offsetY)
        {
            AddTextElement(pageLayout, msg, name);
            IElement pElement = GetElementByName(pageLayout, name);
            if (pElement != null)
            {
                MoveElement(pageLayout, pElement, ep, offsetX, offsetY);
            }
        }

        /// <summary>
        /// 添加TextElement
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="msg"></param>
        /// <param name="name"></param>
        public static void AddTextElement(IPageLayout pageLayout, string msg, string name, decimal fontSize)
        {
            IRgbColor fontColor = new RgbColorClass();
            fontColor.Red = 0;
            fontColor.Blue = 0;
            fontColor.Green = 0;

            AddTextElement(pageLayout, msg, name, fontSize, fontColor);
        }

        /// <summary>
        /// 添加TextElement
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="msg"></param>
        /// <param name="name"></param>
        public static void AddTextElement(IPageLayout pageLayout, string msg, string name)
        {
            IRgbColor fontColor = new RgbColorClass();
            fontColor.Red = 0;
            fontColor.Blue = 0;
            fontColor.Green = 0;
            decimal fontSize = 13;

            AddTextElement(pageLayout, msg, name, fontSize);
        }

        /// <summary>
        /// 添加TextElement
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="msg"></param>
        /// <param name="name"></param>
        public static void AddTextElement(IPageLayout pageLayout, string msg, string name, decimal fontSize, IRgbColor fontColor)
        {
            IActiveView pAV;
            IGraphicsContainer pGraphicsContainer;
            IPoint pPoint;
            ITextElement pTextElement;
            IElementProperties2 pElementPropertie2;
            IElement pElement;
            ITextSymbol pTextSymbol;
            pAV = (IActiveView)pageLayout;
            pGraphicsContainer = (IGraphicsContainer)pageLayout;
            pTextElement = new TextElementClass();

            IFontDisp pFont = new StdFontClass() as IFontDisp;
            pFont.Bold = true;
            pFont.Name = "宋体";
            pFont.Size = fontSize;

            pTextSymbol = new TextSymbolClass();
            pTextSymbol.Color = (IColor)fontColor;
            pTextSymbol.Font = pFont;
            pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
            pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;

            pTextElement.Text = string.IsNullOrEmpty(msg) ? "该值为空！" : msg;
            pTextElement.Symbol = pTextSymbol;

            pElementPropertie2 = pTextElement as IElementProperties2;
            pElementPropertie2.Name = name;

            pPoint = new PointClass();
            pPoint.X = 0;
            pPoint.Y = 0;
            pElement = (IElement)pTextElement;
            pElement.Geometry = (ESRI.ArcGIS.Geometry.IGeometry)pPoint;
            pGraphicsContainer.AddElement(pElement, 0);

            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        #region 添加图片元素

        /// <summary>
        /// 添加图片元素
        /// </summary>
        /// <param name="pageLayoutControl"></param>
        /// <param name="imgpath"></param>
        /// <param name="name"></param>
        /// <param name="ep"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public static void AddImageElement(IPageLayout pageLayout, string imgpath, string name, ElementPosition ep, double offsetX, double offsetY)
        {
            AddImageElement(pageLayout, imgpath, name);
            IElement pElement = GetElementByName(pageLayout, name);
            if (pElement != null)
            {
                MoveElement(pageLayout, pElement, ep, offsetX, offsetY);
            }
        }

        /// <summary>
        /// 添加图片元素
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="imgpath"></param>
        /// <param name="name"></param>
        public static void AddImageElement(IPageLayout pageLayout, string imgpath, string name)
        {
            IActiveView pAV;
            IGraphicsContainer pGraphicsContainer;
            IPictureElement pPicElement;
            IElementProperties2 pElementPropertie2;
            IElement pElement;

            pAV = (IActiveView)pageLayout;
            pGraphicsContainer = (IGraphicsContainer)pageLayout;
            pPicElement = new ESRI.ArcGIS.Carto.PictureElementClass();
            pPicElement.ImportPictureFromFile(imgpath);
            pPicElement.MaintainAspectRatio = true;
            pPicElement.SavePictureInDocument = false;

            pElementPropertie2 = pPicElement as IElementProperties2;
            pElementPropertie2.Name = name;

            IEnvelope pEnv = new EnvelopeClass();
            double w = 6, h = 5;
            Size jpgSize;
            float wpx, hpx;
            try
            {
                GetJpgSize(imgpath, out jpgSize, out wpx, out hpx);
                w = (jpgSize.Width / wpx) * 2.54;
                h = (jpgSize.Height / hpx) * 2.54;
            }
            catch
            {

            }
            pEnv.PutCoords(0, 0, w, h);
            pElement = (IElement)pPicElement;
            pElement.Geometry = pEnv as IGeometry;
            pGraphicsContainer.AddElement(pElement, 0);
            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        //C#快速获取JPG图片大小及英寸分辨率
        public static int GetJpgSize(string FileName, out Size JpgSize, out float Wpx, out float Hpx)
        {//C#快速获取JPG图片大小及英寸分辨率
            JpgSize = new Size(0, 0);
            Wpx = 0; Hpx = 0;
            int rx = 0;
            if (!File.Exists(FileName)) return rx;
            System.IO.FileStream F_Stream = File.OpenRead(FileName);
            int ff = F_Stream.ReadByte();
            int type = F_Stream.ReadByte();
            if (ff != 0xff || type != 0xd8)
            {//非JPG文件
                F_Stream.Close();
                return rx;
            }
            long ps = 0;
            do
            {
                do
                {
                    ff = F_Stream.ReadByte();
                    if (ff < 0) //文件结束
                    {
                        F_Stream.Close();
                        return rx;
                    }
                } while (ff != 0xff);

                do
                {
                    type = F_Stream.ReadByte();
                } while (type == 0xff);

                //MessageBox.Show(ff.ToString() + "," + type.ToString(), F_Stream.Position.ToString());
                ps = F_Stream.Position;
                switch (type)
                {
                    case 0x00:
                    case 0x01:
                    case 0xD0:
                    case 0xD1:
                    case 0xD2:
                    case 0xD3:
                    case 0xD4:
                    case 0xD5:
                    case 0xD6:
                    case 0xD7:
                        break;
                    case 0xc0: //SOF0段
                        ps = F_Stream.ReadByte() * 256;
                        ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度

                        F_Stream.ReadByte(); //丢弃精度数据
                        //高度
                        JpgSize.Height = F_Stream.ReadByte() * 256;
                        JpgSize.Height = JpgSize.Height + F_Stream.ReadByte();
                        //宽度
                        JpgSize.Width = F_Stream.ReadByte() * 256;
                        JpgSize.Width = JpgSize.Width + F_Stream.ReadByte();
                        //后面信息忽略
                        if (rx != 1 && rx < 3) rx = rx + 1;
                        break;
                    case 0xe0: //APP0段
                        ps = F_Stream.ReadByte() * 256;
                        ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度

                        F_Stream.Seek(5, SeekOrigin.Current); //丢弃APP0标记(5bytes)
                        F_Stream.Seek(2, SeekOrigin.Current); //丢弃主版本号(1bytes)及次版本号(1bytes)
                        int units = F_Stream.ReadByte(); //X和Y的密度单位,units=0：无单位,units=1：点数/英寸,units=2：点数/厘米

                        //水平方向(像素/英寸)分辨率
                        Wpx = F_Stream.ReadByte() * 256;
                        Wpx = Wpx + F_Stream.ReadByte();
                        if (units == 2) Wpx = (float)(Wpx * 2.54); //厘米变为英寸
                        //垂直方向(像素/英寸)分辨率
                        Hpx = F_Stream.ReadByte() * 256;
                        Hpx = Hpx + F_Stream.ReadByte();
                        if (units == 2) Hpx = (float)(Hpx * 2.54); //厘米变为英寸
                        //后面信息忽略
                        if (rx != 2 && rx < 3) rx = rx + 2;
                        break;

                    default: //别的段都跳过////////////////
                        ps = F_Stream.ReadByte() * 256;
                        ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度
                        break;
                }
                if (ps + 1 >= F_Stream.Length) //文件结束
                {
                    F_Stream.Close();
                    return rx;
                }
                F_Stream.Position = ps; //移动指针
            } while (type != 0xda); // 扫描行开始
            F_Stream.Close();
            return rx;
        }

        #endregion

        /// <summary>
        /// 创建指定位置的添加指北针
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="name"></param>
        public static void AddNorthArrow(IPageLayout pageLayout, string name, ElementPosition ep, double offsetX, double offsetY)
        {
            AddNorthArrow(pageLayout, name);
            IElement pElement = GetElementByName(pageLayout, name);
            if (pElement != null)
            {
                MoveElement(pageLayout, pElement, ep, offsetX, offsetY);
            }
        }

        /// <summary>
        /// 添加指北针
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="name"></param>
        public static void AddNorthArrow(IPageLayout pageLayout, string name)
        {
            double xMin, xMax, yMin, yMax;
            IActiveView pAV = pageLayout as IActiveView;
            IMapFrame pMapFrame = pAV.GraphicsContainer.FindFrame(pAV.FocusMap) as IMapFrame;

            xMin = 0;//指北针的宽度为2，高度为3
            xMax = 1;
            yMin = 0;
            yMax = 1;
            IEnvelope pEnv = new EnvelopeClass();
            pEnv.PutCoords(xMin, yMin, xMax, yMax);
            UID pID = new UIDClass();
            pID.Value = "esriCarto.MarkerNorthArrow";
            IMapSurround pMapSurround = CreateSurround(pID, pEnv, name, pMapFrame, pageLayout);
            IMarkerNorthArrow pMarkerNorthArrow = pMapSurround as IMarkerNorthArrow; //QI  
            ICharacterMarkerSymbol pCharacterMarkerSymbol = pMarkerNorthArrow.MarkerSymbol as ICharacterMarkerSymbol; //clones the symbol  
            pCharacterMarkerSymbol.CharacterIndex = 167;//设置North Arrow的特征值，即显示风格
            ESRI.ArcGIS.Display.IRgbColor pRGBColor = new ESRI.ArcGIS.Display.RgbColorClass();
            pRGBColor.Red = 0;
            pRGBColor.Green = 0;
            pRGBColor.Blue = 0;
            pCharacterMarkerSymbol.Color = pRGBColor;
            pCharacterMarkerSymbol.Angle = 0;
            pCharacterMarkerSymbol.Size = 80;
            pMarkerNorthArrow.MarkerSymbol = pCharacterMarkerSymbol;//'set it back   

            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        /// <summary>
        /// 创建指定位置的设定图例
        /// </summary>
        /// <param name="pageLayout"></param>
        public static void AddLegend(IPageLayout pageLayout, string name, double w, double h, ElementPosition ep, double offsetX, double offsetY)
        {
            AddLegend(pageLayout, name, w, h);
            IElement pElement = GetElementByName(pageLayout, name);
            if (pElement != null)
            {
                MoveElement(pageLayout, pElement, ep, offsetX, offsetY);
            }
        }

        /// <summary>
        /// 设定图例
        /// </summary>
        /// <param name="pageLayout"></param>
        public static void AddLegend(IPageLayout pageLayout, string name, double w, double h)
        {
            double xMin, xMax, yMin, yMax;
            IActiveView pAV = pageLayout as IActiveView;
            IMapFrame pMapFrame = pAV.GraphicsContainer.FindFrame(pAV.FocusMap) as IMapFrame;

            xMin = 0;//图例的宽度为2，高度为5
            xMax = w;
            yMin = 0;
            yMax = h;
            IEnvelope pEnv = new EnvelopeClass();
            pEnv.PutCoords(xMin, yMin, xMax, yMax);
            UID pID = new UIDClass();
            pID.Value = "esriCarto.Legend";//增加legend
            IMapSurround pMapSurround = CreateSurround(pID, pEnv, name, pMapFrame, pageLayout); //Refresh the graphics 
            ILegend pLegend = pMapSurround as ILegend;
            pLegend.Title = "图例";

            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        /// <summary>
        /// 创建指定位置的添加比例尺
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="name"></param>
        public static void AddScaleBar(IPageLayout pageLayout, string name, double w, double h, ElementPosition ep, double offsetX, double offsetY)
        {
            AddScaleBar(pageLayout, name, w, h);
            IElement pElement = GetElementByName(pageLayout, name);
            if (pElement != null)
            {
                MoveElement(pageLayout, pElement, ep, offsetX, offsetY);
            }
        }

        /// <summary>
        /// 添加比例尺
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="name"></param>
        public static void AddScaleBar(IPageLayout pageLayout, string name, double w, double h)
        {
            double xMin, xMax, yMin, yMax;
            IActiveView pAV = pageLayout as IActiveView;
            IMapFrame pMapFrame = pAV.GraphicsContainer.FindFrame(pAV.FocusMap) as IMapFrame;

            xMin = 0;//比例尺的宽度为4，高度为2，位置距离地图最上角0.1 0.1
            xMax = w;
            yMin = 0;
            yMax = h;
            IEnvelope pEnv = new EnvelopeClass();
            pEnv.PutCoords(xMin, yMin, xMax, yMax);
            UID pID = new UIDClass();
            pID.Value = "esriCarto.ScaleLine";//增加ScaleLine
            IMapSurround pMapSurround = CreateSurround(pID, pEnv, name, pMapFrame, pageLayout); //Refresh the graphics
            IScaleBar pScaleBar = pMapSurround as IScaleBar;
            pScaleBar.Units = esriUnits.esriMeters;
            IScaleMarks pScaleMarks = pScaleBar as IScaleMarks;
            pScaleMarks.MarkFrequency = esriScaleBarFrequency.esriScaleBarOne;
            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        /// <summary>
        /// 添加MapSurround类型的Element
        /// </summary>
        /// <param name="pID"></param>
        /// <param name="pEnv"></param>
        /// <param name="strName"></param>
        /// <param name="pMapFrame"></param>
        /// <param name="pPageLayout"></param>
        /// <returns></returns>
        public static IMapSurround CreateSurround(UID pID, IEnvelope pEnv, string strName, IMapFrame pMapFrame, IPageLayout pPageLayout)
        {
            IGraphicsContainer pGraphicsContainer = pPageLayout as IGraphicsContainer;
            IActiveView pActiveView = pPageLayout as IActiveView;
            IMapSurroundFrame pMapSurroundFrame = pMapFrame.CreateSurroundFrame(pID, null);//MapSurroundFrames are related to MapFrames MapFrames hold Maps 
            //pMapSurroundFrame.MapSurround.Name = this.ElementNamePrefix + strName; //Set the geometry of the MapSurroundFrame to give it a location  
            pMapSurroundFrame.MapSurround.Name = pID.Value + "_" + strName; //Set the geometry of the MapSurroundFrame to give it a location  
            //Activate it and add it to the PageLayout's graphics container
            IElement pElement = pMapSurroundFrame as IElement;  //MapSurrounds are held in a MapSurroundFram
            pElement.Geometry = pEnv;
            pElement.Activate(pActiveView.ScreenDisplay); //'Allow the legend frame size to be altered after the legend has been  'added to the GraphicsContainer  
            ITrackCancel PTrack = new CancelTrackerClass();
            pElement.Draw(pActiveView.ScreenDisplay, PTrack);
            pGraphicsContainer.AddElement(pElement, 0);
            //Re-apply the change to the Legend MapSurroundFrame Geometry  
            pElement.Geometry = pEnv;

            //设置背景
            ISymbolBackground pBackground = new SymbolBackgroundClass();
            IFillSymbol pFillSymbol = new SimpleFillSymbol();
            IColor pColor;
            pColor = GetColor(250, 250, 250);
            pFillSymbol.Color = pColor;
            pBackground.FillSymbol = pFillSymbol;
            pMapSurroundFrame.Background = pBackground;
            return pMapSurroundFrame.MapSurround;
        }

        #region 地理点坐标转换成纸张点坐标

        /// <summary>
        /// 地理点坐标转换成纸张点坐标
        /// </summary>
        /// <param name="dataPoint">要转化的点</param>
        /// <param name="pagePoint">out</param>
        /// <param name="pageLayout"></param>
        public static void DataPointToPagePoint(IPageLayout pageLayout, IPoint dataPoint, out IPoint pagePoint)
        {
            IActiveView pageActiveView = pageLayout as IActiveView;
            IActiveView dataActiveView = pageActiveView.FocusMap as IActiveView;
            IDisplayTransformation pageDT = pageActiveView.ScreenDisplay.DisplayTransformation;
            IDisplayTransformation dataDT = dataActiveView.ScreenDisplay.DisplayTransformation;

            int screenX, screenY;
            dataDT.FromMapPoint(dataPoint, out screenX, out screenY);
            pagePoint = pageDT.ToMapPoint(screenX, screenY);
        }

        #endregion

        #region 纸张点坐标转换成地理点坐标

        /// <summary>
        /// 纸张点坐标转换成地理点坐标
        /// </summary>
        /// <param name="pageLayout"></param>
        /// <param name="pagePoint"></param>
        /// <param name="dataPoint"></param>
        public static void PagePointToDataPoint(IPageLayout pageLayout, IPoint pagePoint, out IPoint dataPoint)
        {
            IActiveView pageActiveView = pageLayout as IActiveView;
            IDisplayTransformation pageDT = pageActiveView.ScreenDisplay.DisplayTransformation;
            IActiveView dataActiveView = pageActiveView.FocusMap as IActiveView;
            IDisplayTransformation dataDT = dataActiveView.ScreenDisplay.DisplayTransformation;

            int screenX, screenY;
            pageDT.FromMapPoint(pagePoint, out screenX, out screenY);
            dataPoint = dataDT.ToMapPoint(screenX, screenY);
        }

        #endregion
    }
}
