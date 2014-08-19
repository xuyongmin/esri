using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DotNet.Common.GIS.GeoUtilities.TFHUtility
{
    public class TFHHelper
    {
        private static List<MapScaleObject> m_TFHMapScaleList = null;
        public static List<MapScaleObject> TFHMapScaleList
        {
            get
            {
                if (m_TFHMapScaleList == null)
                {
                    m_TFHMapScaleList = GetMapScaleList();
                }
                return m_TFHMapScaleList;
            }
        }

        public static MapScaleObject TFH100WMapScaleObject
        {
            get
            {
                return TFHMapScaleList[0];
            }
        }

        private static List<MapScaleObject> GetMapScaleList()
        {
            List<MapScaleObject> result = new List<MapScaleObject>();

            double diffb = 0;
            double diffl = 0;
            //1:100万
            MapScaleObject a = new MapScaleObject();
            a.Key = "A";
            a.Label = "1:100万";
            CommonUtility.DDDMSHelper.DMS2DD(4, 0, 0, out diffb);
            a.DiffB = diffb;
            CommonUtility.DDDMSHelper.DMS2DD(6, 0, 0, out diffl);
            a.DiffL = diffl;
            a.Scale = 1000000;
            a.TFNumber = 1;
            result.Add(a);
            //1:50万
            MapScaleObject b = new MapScaleObject();
            b.Key = "B";
            b.Label = "1:50万";
            CommonUtility.DDDMSHelper.DMS2DD(2, 0, 0, out diffb);
            b.DiffB = diffb;
            CommonUtility.DDDMSHelper.DMS2DD(3, 0, 0, out diffl);
            b.DiffL = diffl;
            b.Scale = 500000;
            b.TFNumber = 4;
            result.Add(b);
            //1:25万
            MapScaleObject c = new MapScaleObject();
            c.Key = "C";
            c.Label = "1:25万";
            CommonUtility.DDDMSHelper.DMS2DD(1, 0, 0, out diffb);
            c.DiffB = diffb;
            CommonUtility.DDDMSHelper.DMS2DD(1, 30, 0, out diffl);
            c.DiffL = diffl;
            c.Scale = 250000;
            c.TFNumber = 16;
            result.Add(c);
            //1:10万
            MapScaleObject d = new MapScaleObject();
            d.Key = "D";
            d.Label = "1:10万";
            CommonUtility.DDDMSHelper.DMS2DD(0, 20, 0, out diffb);
            d.DiffB = diffb;
            CommonUtility.DDDMSHelper.DMS2DD(0, 30, 0, out diffl);
            d.DiffL = diffl;
            d.Scale = 100000;
            d.TFNumber = 144;
            result.Add(d);
            //1:5万
            MapScaleObject e = new MapScaleObject();
            e.Key = "E";
            e.Label = "1:5万";
            CommonUtility.DDDMSHelper.DMS2DD(0, 10, 0, out diffb);
            e.DiffB = diffb;
            CommonUtility.DDDMSHelper.DMS2DD(0, 15, 0, out diffl);
            e.DiffL = diffl;
            e.Scale = 50000;
            e.TFNumber = 576;
            result.Add(e);
            //1:2.5万
            MapScaleObject f = new MapScaleObject();
            f.Key = "F";
            f.Label = "1:2.5万";
            CommonUtility.DDDMSHelper.DMS2DD(0, 5, 0, out diffb);
            f.DiffB = diffb;
            CommonUtility.DDDMSHelper.DMS2DD(0, 7, 30, out diffl);
            f.DiffL = diffl;
            f.Scale = 25000;
            f.TFNumber = 2304;
            result.Add(f);
            //1:1万
            MapScaleObject g = new MapScaleObject();
            g.Key = "G";
            g.Label = "1:1万";
            CommonUtility.DDDMSHelper.DMS2DD(0, 2, 30, out diffb);
            g.DiffB = diffb;
            CommonUtility.DDDMSHelper.DMS2DD(0, 3, 45, out diffl);
            g.DiffL = diffl;
            g.Scale = 10000;
            g.TFNumber = 9216;
            result.Add(g);
            //1:5000
            MapScaleObject h = new MapScaleObject();
            h.Key = "H";
            h.Label = "1:5000";
            CommonUtility.DDDMSHelper.DMS2DD(0, 1, 15, out diffb);
            h.DiffB = diffb;
            CommonUtility.DDDMSHelper.DMS2DD(0, 1, 52.5, out diffl);
            h.DiffL = diffl;
            h.Scale = 5000;
            h.TFNumber = 36864;
            result.Add(h);

            return result;
        }

        public static List<TFHObject> GetTFHList(string startTFH, string endTFH, out string msg)
        {
            List<TFHObject> result = new List<TFHObject>();
            msg = string.Empty;
            if (startTFH.Length == endTFH.Length)
            {
                if (startTFH.Length == 3 || startTFH.Length == 10)
                {
                    if (startTFH.ToUpper().Equals(endTFH.ToUpper()))
                    {
                        TFHObject obj = GetTFHObject(startTFH);
                        result.Add(obj);
                    }
                    else
                    {
                        string hStr100wStart = startTFH.Substring(0, 1);
                        string hStr100wEnd = endTFH.Substring(0, 1);
                        string lStr100wStart = startTFH.Substring(1, 2);
                        string lStr100wEnd = endTFH.Substring(1, 2);
                        int hstart = (int)(Enum.Parse(typeof(Scale100w), hStr100wStart));
                        int hend = (int)(Enum.Parse(typeof(Scale100w), hStr100wEnd));
                        int lstart = int.Parse(lStr100wStart);
                        int lend = int.Parse(lStr100wEnd);
                        if (startTFH.Length == 3)
                        {
                            for (int i = hend; i <= hstart; i++)
                            {
                                for (int j = lstart; j <= lend; j++)
                                {
                                    TFHObject obj = GetTFHObject(i, j, string.Empty, 0, 0);
                                    result.Add(obj);
                                }
                            }
                        }
                        else//获取四点的图幅号 左上：hs ls B h1s l1s 左下：he ls B h1e l1s 右上：hs le B h1s l1e 右下：he le B h1e l1e
                        {
                            string startTFHScaleStr = startTFH.Substring(3, 1);
                            string endTFHScaleStr = endTFH.Substring(3, 1);
                            int h1start = int.Parse(startTFH.Substring(4, 3));
                            int l1start = int.Parse(startTFH.Substring(7, 3));
                            int h1end = int.Parse(endTFH.Substring(4, 3));
                            int l1end = int.Parse(endTFH.Substring(7, 3));
                            if (startTFHScaleStr.ToUpper().Equals(endTFHScaleStr.ToUpper()))
                            {
                                MapScaleObject tmpMapScaleObj = null;
                                foreach (MapScaleObject m in TFHMapScaleList)
                                {
                                    if (m.Key.ToUpper().Equals(startTFHScaleStr.ToUpper()))
                                    {
                                        tmpMapScaleObj = m;
                                        break;
                                    }
                                }
                                if (tmpMapScaleObj != null)
                                {
                                    int tfnumberHorL = (int)Math.Sqrt(tmpMapScaleObj.TFNumber);

                                    if (hstart == hend && lstart == lend)//hstart==hend && lstart==lend 百万比例尺：同行同列
                                    {
                                        for (int i = h1start; i <= h1end; i++)
                                        {
                                            for (int j = l1start; j <= l1end; j++)
                                            {
                                                TFHObject obj = GetTFHObject(hstart, lstart, startTFHScaleStr, i, j);
                                                result.Add(obj);
                                            }
                                        }
                                    }
                                    else if (hstart == hend && lstart != lend)//hstart==hend && lstart!=lend 百万比例尺：同行不同列
                                    {
                                        //获取四点的图幅号 左上：hs ls B h1s l1s 左下：he ls B h1e l1s 右上：hs le B h1s l1e 右下：he le B h1e l1e
                                        for (int i = lstart; i <= lend; i++)
                                        {
                                            if (i == lstart)//第一格
                                            {
                                                for (int j = h1start; j <= h1end; j++)
                                                {
                                                    for (int k = l1start; k <= tfnumberHorL; k++)
                                                    {
                                                        TFHObject obj = GetTFHObject(hstart, i, startTFHScaleStr, j, k);
                                                        result.Add(obj);
                                                    }
                                                }
                                            }
                                            else if (i == lend)//最后一格
                                            {
                                                for (int j = h1start; j <= h1end; j++)
                                                {
                                                    for (int k = 1; k <= l1end; k++)
                                                    {
                                                        TFHObject obj = GetTFHObject(hstart, i, startTFHScaleStr, j, k);
                                                        result.Add(obj);
                                                    }
                                                }
                                            }
                                            else//中间
                                            {
                                                for (int j = h1start; j <= h1end; j++)
                                                {
                                                    for (int k = 1; k <= tfnumberHorL; k++)
                                                    {
                                                        TFHObject obj = GetTFHObject(hstart, i, startTFHScaleStr, j, k);
                                                        result.Add(obj);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (hstart != hend && lstart == lend)//hstart!=hend && lstart==lend 百万比例尺：同列不同行
                                    {
                                        //获取四点的图幅号 左上：hs ls B h1s l1s 左下：he ls B h1e l1s 右上：hs le B h1s l1e 右下：he le B h1e l1e
                                        for (int i = hend; i <= hstart; i++)
                                        {
                                            if (i == hstart)//在第一格
                                            {
                                                for (int j = h1start; j <= tfnumberHorL; j++) //循环行号
                                                {
                                                    for (int k = l1start; k <= l1end; k++)
                                                    {
                                                        TFHObject obj = GetTFHObject(i, lstart, startTFHScaleStr, j, k);
                                                        result.Add(obj);
                                                    }
                                                }
                                            }
                                            else if (i == hend)//最后一格
                                            {
                                                for (int j = 1; j <= h1end; j++)
                                                {
                                                    for (int k = l1start; k <= l1end; k++)
                                                    {
                                                        TFHObject obj = GetTFHObject(i, lstart, startTFHScaleStr, j, k);
                                                        result.Add(obj);
                                                    }
                                                }
                                            }
                                            else//中间
                                            {
                                                for (int j = 1; j <= tfnumberHorL; j++)
                                                {
                                                    for (int k = l1start; k <= l1end; k++)
                                                    {
                                                        TFHObject obj = GetTFHObject(i, lstart, startTFHScaleStr, j, k);
                                                        result.Add(obj);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else//hstart!=hend && lstart!=lend 百万比例尺：不同行不同列
                                    {
                                        //获取四点的图幅号 左上：hs ls B h1s l1s 左下：he ls B h1e l1s 右上：hs le B h1s l1e 右下：he le B h1e l1e
                                        for (int i = hend; i <= hstart; i++)
                                        {
                                            if (i == hstart)
                                            {
                                                for (int j = lstart; j <= lend; j++)
                                                {
                                                    if (j == lstart)
                                                    {
                                                        for (int k = h1start; k <= tfnumberHorL; k++)
                                                        {
                                                            for (int l = l1start; l <= tfnumberHorL; l++)
                                                            {
                                                                TFHObject obj = GetTFHObject(i, j, startTFHScaleStr, k, l);
                                                                result.Add(obj);
                                                            }
                                                        }
                                                    }
                                                    else if (j == lend)
                                                    {
                                                        for (int k = h1start; k <= tfnumberHorL; k++)
                                                        {
                                                            for (int l = 1; l <= l1end; l++)
                                                            {
                                                                TFHObject obj = GetTFHObject(i, j, startTFHScaleStr, k, l);
                                                                result.Add(obj);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int k = h1start; k <= tfnumberHorL; k++)
                                                        {
                                                            for (int l = 1; l <= tfnumberHorL; l++)
                                                            {
                                                                TFHObject obj = GetTFHObject(i, j, startTFHScaleStr, k, l);
                                                                result.Add(obj);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (i == hend)
                                            {
                                                for (int j = lstart; j <= lend; j++)
                                                {
                                                    if (j == lstart)
                                                    {
                                                        for (int k = 1; k <= h1end; k++)
                                                        {
                                                            for (int l = l1start; l <= tfnumberHorL; l++)
                                                            {
                                                                TFHObject obj = GetTFHObject(i, j, startTFHScaleStr, k, l);
                                                                result.Add(obj);
                                                            }
                                                        }
                                                    }
                                                    else if (j == lend)
                                                    {
                                                        for (int k = 1; k <= h1end; k++)
                                                        {
                                                            for (int l = 1; l <= l1end; l++)
                                                            {
                                                                TFHObject obj = GetTFHObject(i, j, startTFHScaleStr, k, l);
                                                                result.Add(obj);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int k = 1; k <= h1end; k++)
                                                        {
                                                            for (int l = 1; l <= tfnumberHorL; l++)
                                                            {
                                                                TFHObject obj = GetTFHObject(i, j, startTFHScaleStr, k, l);
                                                                result.Add(obj);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                for (int j = lstart; j <= lend; j++)
                                                {
                                                    if (j == lstart)
                                                    {
                                                        for (int k = 1; k <= tfnumberHorL; k++)
                                                        {
                                                            for (int l = l1start; l <= tfnumberHorL; l++)
                                                            {
                                                                TFHObject obj = GetTFHObject(i, j, startTFHScaleStr, k, l);
                                                                result.Add(obj);
                                                            }
                                                        }
                                                    }
                                                    else if (j == lend)
                                                    {
                                                        for (int k = 1; k <= tfnumberHorL; k++)
                                                        {
                                                            for (int l = 1; l <= l1end; l++)
                                                            {
                                                                TFHObject obj = GetTFHObject(i, j, startTFHScaleStr, k, l);
                                                                result.Add(obj);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int k = 1; k <= tfnumberHorL; k++)
                                                        {
                                                            for (int l = 1; l <= tfnumberHorL; l++)
                                                            {
                                                                TFHObject obj = GetTFHObject(i, j, startTFHScaleStr, k, l);
                                                                result.Add(obj);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    msg = string.Format(string.Format("找不到对应比例尺 请检查输入图幅号：{0}——{1}", startTFH, endTFH));
                                }
                            }
                            else
                            {
                                msg = string.Format(string.Format("图幅号比例尺不对 请检查输入图幅号：{0}——{1}", startTFH, endTFH));
                            }
                        }
                    }
                }
                else
                {
                    msg = string.Format(string.Format("图幅号长度不对 请检查输入图幅号：{0}——{1}", startTFH, endTFH));
                }
            }
            else
            {
                msg = string.Format(string.Format("图幅号长度不一致 请检查输入图幅号：{0}——{1}", startTFH, endTFH));
            }

            //排序
            var orders = from tfh in result orderby tfh.H descending, tfh.H1 ascending, tfh.L ascending, tfh.L1 ascending select tfh;
            result = orders.ToList<TFHObject>();

            return result;
        }

        public static TFHObject GetTFHObject(string tfh)
        {
            TFHObject result = new TFHObject();
            result.TFH = tfh;

            MapScaleObject mapscaleObj = null;
            double ltl, rbl, rbb, ltb;
            GetLTRBPoints(result.TFH, out mapscaleObj, out ltl, out ltb, out rbl, out rbb);
            string extent = string.Format("{0},{1},{2},{3}", ltl, ltb, rbl, rbb);
            result.Extent = extent;

            return result;
        }

        public static TFHObject GetTFHObject(int h, int l, string scaleKey, int h1, int l1)
        {
            TFHObject result = null;
            string tfh = string.Empty;

            if (string.IsNullOrEmpty(scaleKey))
            {
                tfh = string.Format("{0}{1}", ((Scale100w)h).ToString(), string.Format("{0:##00}", l));
            }
            else
            {
                tfh = string.Format("{0}{1}{2}{3}{4}"
                    , ((Scale100w)h).ToString()
                    , string.Format("{0:##00}", l)
                    , scaleKey
                    , string.Format("{0:###000}", h1)
                    , string.Format("{0:###000}", l1));
            }

            result = GetTFHObject(tfh);

            return result;
        }

        /// <summary>
        /// 获取图幅号
        /// </summary>
        /// <param name="L">经度</param>
        /// <param name="B">纬度</param>
        /// <returns></returns>
        public static string GetTFH(double L, double B, MapScaleObject scaleObj)
        {
            string result = string.Empty;

            //计算1:100万的横和列 我国的百万比例尺计算公式
            //h(横)=[B/diffb]+1; []为取整
            //l(列)=[L/diffl]+31;[]为取整
            double diffb = TFH100WMapScaleObject.DiffB;
            double diffl = TFH100WMapScaleObject.DiffL;
            int h, l;
            h = (int)Math.Floor(B / diffb) + 1;
            l = (int)Math.Floor(L / diffl) + 31;
            if (!scaleObj.Scale.Equals(TFH100WMapScaleObject.Scale))
            {
                //获取指定比例尺的行列号
                //h1(横)=diffb/scaleObj.DiffB-[(B/diffb)/scaleObj.DiffB] []取整 ()取余数
                //l1(列)=[(L/diffl)/scaleObj.DiffL]+1 []取整 ()取余数
                int h1, l1;
                h1 = (int)(diffb / scaleObj.DiffB - Math.Floor((B % diffb) / scaleObj.DiffB));
                l1 = (int)(Math.Floor((L % diffl) / scaleObj.DiffL) + 1);

                result = string.Format("{0}{1}{2}{3}{4}", ((Scale100w)h).ToString(), string.Format("{0:##00}", l), scaleObj.Key, string.Format("{0:###000}", h1), string.Format("{0:###000}", l1));
            }
            else
            {
                result = string.Format("{0}{1}", ((Scale100w)h).ToString(), string.Format("{0:##00}", l));
            }
            return result;
        }

        /// <summary>
        /// 根据图幅号获取左上角坐标
        /// </summary>
        /// <param name="TFH"></param>
        /// <param name="LTL"></param>
        /// <param name="LTB"></param>
        /// <param name="RBL"></param>
        /// <param name="RBB"></param>
        public static void GetLTRBPoints(string TFH, out MapScaleObject scaleObj, out double LTL, out double LTB, out double RBL, out double RBB)
        {
            LTL = 0;
            LTB = 0;
            RBL = 0;
            RBB = 0;
            scaleObj = null;
            if (TFH.Length == 3 || TFH.Length == 10)
            {
                //计算1:100万的横和列 我国的百万比例尺计算公式
                //h(横)=[B/diffb]+1; []为取整
                //l(列)=[L/diffl]+31;[]为取整
                double diffb = TFH100WMapScaleObject.DiffB;
                double diffl = TFH100WMapScaleObject.DiffL;
                int h, l, h1, l1;
                string tfh100wH = TFH.Substring(0, 1);
                string tfh100wL = TFH.Substring(1, 2);
                h = (int)(Enum.Parse(typeof(Scale100w), tfh100wH));
                l = int.Parse(tfh100wL);
                if (TFH.Length == 10)
                {
                    //B=h*diffb-(h1-1)*scaleObj.DiffB
                    //L=(l-31)*diffl+(l1-1)*scaleObj.DiffL
                    string tfhscale = TFH.Substring(3, 1);
                    string tfhh1 = TFH.Substring(4, 3);
                    h1 = int.Parse(tfhh1);
                    string tfhl1 = TFH.Substring(7, 3);
                    l1 = int.Parse(tfhl1);
                    foreach (MapScaleObject obj in TFHMapScaleList)
                    {
                        if (obj.Key.ToUpper().Equals(tfhscale.ToUpper()))
                        {
                            scaleObj = obj;
                        }
                    }
                    LTB = h * diffb - (h1 - 1) * scaleObj.DiffB;
                    LTL = (l - 31) * diffl + (l1 - 1) * scaleObj.DiffL;
                    RBB = LTB - scaleObj.DiffB;
                    RBL = LTL + scaleObj.DiffL;
                }
                else
                {
                    scaleObj = TFH100WMapScaleObject;
                    //B=(h-1)*diffb;
                    //L=(l-31)*diffl;
                    LTB = (h - 1) * diffb;
                    LTL = (l - 31) * diffl;
                    RBB = LTB - diffb;
                    RBL = LTL + diffl;
                }
            }
        }
    }
}
