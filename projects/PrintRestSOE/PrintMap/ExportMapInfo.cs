using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintMap
{
    public class ExportMapInfo
    {
        /// <summary>
        /// 制图唯一标识
        /// </summary>
        public string ExportKey { get; set; }
        public int DelNo { get; set; }
        /// <summary>
        /// 地图标题
        /// </summary>
        public string MapName { get; set; }
        /// <summary>
        /// 项目标题
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// 制图单位
        /// </summary>
        public string MapUnit { get; set; }
        /// <summary>
        /// 制图作者
        /// </summary>
        public string MapAuthor { get; set; }
        /// <summary>
        /// 制图日期
        /// </summary>
        public string MapDate { get; set; }
        /// <summary>
        /// mxd位置
        /// </summary>
        public string MxdPath { get; set; }
        public string TemplateName { get; set; }
        /// <summary>
        /// Lyr存放位置
        /// </summary>
        public string LyrPath { get; set; }
        public List<string> Lyrs { get; set; }
        /// <summary>
        /// 页边距：左上右下
        /// </summary>
        public string PageMargin { get; set; }
        /// <summary>
        /// 纸张大小
        /// </summary>
        public string PageSize { get; set; }
        /// <summary>
        /// 打印方向
        /// </summary>
        public string PageOri { get; set; }
        /// <summary>
        /// 制图比例尺
        /// </summary>
        public int Scale { get; set; }
        /// <summary>
        /// 出图精度
        /// </summary>
        public int DPI { get; set; }
        /// <summary>
        /// 出图格式
        /// </summary>
        public string ExportFormat { get; set; }
        /// <summary>
        /// 制图中心点 wkt
        /// </summary>
        public string DataCenter { get; set; }
        /// <summary>
        /// 出图文件保存路径
        /// </summary>
        public string OutPath { get; set; }
        /// <summary>
        /// 额外文字显示信息
        /// </summary>
        public List<string> AdditionalTexts { get; set; }
        /// <summary>
        /// 额外图片显示信息
        /// </summary>
        public List<string> AdditionalImages { get; set; }
        /// <summary>
        /// 颜色组织 a,r,g,b
        /// 业务数据 shapewkt;bordercolor;fillcolor;borderthickness
        /// </summary>
        public List<string> BusinessShapes { get; set; }
        /// <summary>
        /// 注记数据 name;labelxywkt;labelwkt;font;fontsize;fontcolor;labelbordercolor;labelfillcolor;labelborderthikness
        /// </summary>
        public List<string> Labels { get; set; }
    }

    public class PrintResult
    {
        public bool IsPrinted = false;
        public string FileName = string.Empty;
        public string OutMsg = string.Empty;
    }
}
