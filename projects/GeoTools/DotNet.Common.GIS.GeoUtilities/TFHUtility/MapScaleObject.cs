using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Common.GIS.GeoUtilities.TFHUtility
{
    public class MapScaleObject
    {
        /// <summary>
        /// 比例尺带号
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 比例尺显示名
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// 经差
        /// </summary>
        public double DiffL { get; set; }
        /// <summary>
        /// 纬差
        /// </summary>
        public double DiffB { get; set; }
        /// <summary>
        /// 实际比例尺
        /// </summary>
        public double Scale { get; set; }
        /// <summary>
        /// 图幅数量，即1:100万下各比例尺的图幅数
        /// </summary>
        public int TFNumber { get; set; }
    }
}
