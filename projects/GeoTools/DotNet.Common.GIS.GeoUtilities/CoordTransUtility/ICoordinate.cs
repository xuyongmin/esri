using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Common.GIS.GeoUtilities.CoordTransUtility
{

    public interface ICoordinate
    {
        double a { get; }
        double b { get; }

        /// <summary>
        /// 求两点之间的距离(根据经纬度)
        /// </summary>
        /// <param name="L1">经度1</param>
        /// <param name="B1">纬度1</param>
        /// <param name="L2">经度2</param>
        /// <param name="B2">纬度2</param>
        /// <param name="gs">高斯投影中所选用的参考椭球</param>
        /// <returns>两点间距离(单位:meters)</returns>
        double DistanceOfTwoPointsLB(double L1, double B1, double L2, double B2);

        /// <summary>
        /// 求两点之间的距离(大地坐标)
        /// </summary>
        /// <param name="Ye1"></param>
        /// <param name="Xn1"></param>
        /// <param name="Ye2"></param>
        /// <param name="Xn2"></param>
        /// <returns>单位为meters</returns>
        double DistanceOfTwoPointsXY(double Ye1, double Xn1, double Ye2, double Xn2);

        /// <summary>
        /// 高期投影正算
        /// 由经纬度（单位：Decimal Degree）正算到大地坐标（单位：Metre，含带号）
        /// </summary>
        /// <param name="L">经度</param>
        /// <param name="B">纬度</param>
        /// <param name="delno">带号</param>
        /// <param name="lw">分度</param>
        void GaussPrjCalculate(double L, double B, LonWide lw, int delno, out double Ye, out double Xn);

        /// <summary>
        /// 高斯投影反算
        /// 大地坐标（单位：Metre，含带号）反算到经纬度坐标（单位，Decimal Degree）
        /// </summary>
        /// <param name="Ye">大地坐标X值</param>
        /// <param name="Xn">大地坐标Y值</param>
        /// <param name="delno">带号</param>
        /// <param name="lw">分度</param>
        void GaussPrjInvCalculate(double Ye, double Xn, LonWide lw, int delno, out double L, out double B);

        /// <summary>
        /// (B,L,h)->(Xn,Ye,Z)
        /// </summary>
        /// <param name="L">经度</param>
        /// <param name="B">纬度</param>
        /// <param name="h">高度</param>
        /// <param name="Ye">大地坐标X值</param>
        /// <param name="Xn">大地坐标Y值</param>
        /// <param name="Z">Z</param>
        void BLhToXYZ(double L, double B, double h, out double Ye, out double Xn, out double Z);

        /// <summary>
        /// (Xn,Ye,Z)->(B,L,h)
        /// </summary>
        /// <param name="Ye">大地坐标X值</param>
        /// <param name="Xn">大地坐标Y值</param>
        /// <param name="Z">Z</param>
        /// <param name="L">经度</param>
        /// <param name="B">纬度</param>
        /// <param name="h">高度</param>
        void XZYToBLh(double Ye, double Xn, double Z, out double L, out double B, out double h);
    }
}
