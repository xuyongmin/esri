using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Common.GIS.GeoUtilities.TFHUtility
{
    public class TFHObject
    {
        private string m_TFH = string.Empty;
        /// <summary>
        /// 图幅号
        /// </summary>
        public string TFH
        {
            get
            {
                return m_TFH;
            }
            set
            {
                m_TFH = value;
                ParseTFH(value);
            }
        }
        /// <summary>
        /// 范围：minL,minB,maxL,maxB
        /// </summary>
        public string Extent { get; set; }
        /// <summary>
        /// 1:100W行
        /// </summary>
        public int L { get; set; }
        /// <summary>
        /// 1:100W列
        /// </summary>
        public int H { get; set; }
        /// <summary>
        /// 对应比例尺行
        /// </summary>
        public int L1 { get; set; }
        /// <summary>
        /// 对应比例尺列
        /// </summary>
        public int H1 { get; set; }

        private void ParseTFH(string tfh)
        {
            int h = 0, l = 0, h1 = 0, l1 = 0;
            if (tfh.Length == 10 || tfh.Length == 3)
            {
                string tfh100wH = TFH.Substring(0, 1);
                string tfh100wL = TFH.Substring(1, 2);
                h = (int)(Enum.Parse(typeof(Scale100w), tfh100wH));
                l = int.Parse(tfh100wL);
                if (TFH.Length == 10)
                {
                    string tfhscale = TFH.Substring(3, 1);
                    string tfhh1 = TFH.Substring(4, 3);
                    h1 = int.Parse(tfhh1);
                    string tfhl1 = TFH.Substring(7, 3);
                    l1 = int.Parse(tfhl1);
                }
            }
            H = h; L = l; H1 = h1; L1 = l1;
        }
    }
}
