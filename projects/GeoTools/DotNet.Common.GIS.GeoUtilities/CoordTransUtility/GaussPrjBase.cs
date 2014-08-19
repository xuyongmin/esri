using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Common.GIS.GeoUtilities.CoordTransUtility
{
    public class GaussPrjBase : ICoordinate
    {
        /// <summary>
        /// 长轴
        /// </summary>
        protected double m_a;
        public double a
        {
            get { return m_a; }
        }
        /// <summary>
        /// 短轴
        /// </summary>
        protected double m_b;
        public double b
        {
            get { return m_b; }
        }
        /// <summary>
        /// 扁率
        /// </summary>
        protected double m_f;

        /// <summary>
        /// 第一偏心率
        /// </summary>
        protected double m_e;

        /// <summary>
        /// 第二偏心率
        /// </summary>
        protected double m_ee;

        protected readonly double PI = 3.14159265353846;

        /// <summary>
        /// 设定参数
        /// </summary>
        public void Setfee1()
        {
            m_f = (m_a - m_b) / m_a;
            m_e = Math.Sqrt(1 - Math.Pow(m_b / m_a, 2.0));
            m_ee = Math.Sqrt(Math.Pow(m_a / m_b, 2.0) - 1);
        }

        /// <summary>
        /// 北纬偏移
        /// FN北半球=0，FN南半球=10000000 米
        /// </summary>
        protected static double FN = 0;
        /// <summary>
        /// 东经偏移
        /// </summary>
        protected static double FE = 500000;

        protected static double k0 = 1;

        #region ICoordinate

        /// <summary>
        /// 求两点之间的距离(根据经纬度)
        /// </summary>
        /// <param name="L1">经度1</param>
        /// <param name="B1">纬度1</param>
        /// <param name="L2">经度2</param>
        /// <param name="B2">纬度2</param>
        /// <param name="gs">高斯投影中所选用的参考椭球</param>
        /// <returns>两点间距离(单位:meters)</returns>
        public double DistanceOfTwoPointsLB(double L1, double B1, double L2, double B2)
        {
            double radLat1 = L1 * Math.PI / 180;
            double radLat2 = L2 * Math.PI / 180;
            double a = radLat1 - radLat2;
            double b = B1 * Math.PI / 180 - B2 * Math.PI / 180;
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
            Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * m_a;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }

        /// <summary>
        /// 求两点之间的距离(大地坐标)
        /// </summary>
        /// <param name="Ye1"></param>
        /// <param name="Xn1"></param>
        /// <param name="Ye2"></param>
        /// <param name="Xn2"></param>
        /// <returns>单位为meters</returns>
        public double DistanceOfTwoPointsXY(double Ye1, double Xn1, double Ye2, double Xn2)
        {
            double a = Ye1 - Ye2;
            double b = Xn1 - Xn2;
            double s = Math.Sqrt(a * a + b * b);
            return s;
        }

        /// <summary>
        /// 高期投影正算
        /// 由经纬度（单位：Decimal Degree）正算到大地坐标（单位：Metre，含带号）
        /// </summary>
        /// <param name="L">经度</param>
        /// <param name="B">纬度</param>
        /// <param name="delno">带号</param>
        /// <param name="lw">分度</param>
        public void GaussPrjCalculate(double L, double B, LonWide lw, int delno, out double Ye, out double Xn)
        {
            double T, C, A, M, N, L0;

            //获取中央经线
            switch (lw)
            {
                case LonWide.L6:
                    L0 = delno * 6 - 3;
                    break;
                case LonWide.L3:
                    L0 = delno * 3;
                    break;
                default:
                    L0 = delno * 6 - 3;
                    break;
            }

            L0 = L0 * Math.PI / 180;

            //弧度计算
            L = L * Math.PI / 180;
            B = B * Math.PI / 180;

            T = Math.Tan(B) * Math.Tan(B);
            C = m_ee * m_ee * Math.Cos(B) * Math.Cos(B);
            A = (L - L0) * Math.Cos(B);
            M = m_a * (
                (1 - Math.Pow(m_e, 2.0) / 4 - 3 * Math.Pow(m_e, 4.0) / 64 - 5 * Math.Pow(m_e, 6) / 256) * B
                - (3 * Math.Pow(m_e, 2.0) / 8 + 3 * Math.Pow(m_e, 4.0) / 32 + 45 * Math.Pow(m_e, 6.0) / 1024) * Math.Sin(2 * B)
                    + (15 * Math.Pow(m_e, 4.0) / 256 + 45 * Math.Pow(m_e, 6.0) / 1024) * Math.Sin(4 * B)
                    - (35 * Math.Pow(m_e, 6.0) / 3072) * Math.Sin(6 * B)
            );
            N = m_a / Math.Sqrt(1 - m_e * m_e * Math.Sin(B) * Math.Sin(B));

            Xn = k0 * (
                M
                + N * Math.Tan(B) * (A * A / 2 + (5 - T + 9 * C + 4 * C * C) * Math.Pow(A, 4.0) / 24)
                + (61 - 58 * T + T * T + 270 * C - 330 * T * C) * Math.Pow(A, 6.0) / 720
                );

            Ye = FE + k0 * N * (A + (1 - T + C) * Math.Pow(A, 3.0) / 6 + (5 - 18 * T + T * T + 14 * C - 58 * T * C) * Math.Pow(A, 5.0) / 120);
        }

        /// <summary>
        /// 高斯投影反算
        /// 大地坐标（单位：Metre，不含带号）反算到经纬度坐标（单位，Decimal Degree）
        /// </summary>
        /// <param name="Ye">大地坐标X值（不含带号）</param>
        /// <param name="Xn">大地坐标Y值</param>
        /// <param name="delno">带号</param>
        /// <param name="lw">分度</param>
        public void GaussPrjInvCalculate(double Ye, double Xn, LonWide lw, int delno, out double L, out double B)
        {
            double Nf, Rf, Bf, fi, Mf, Tf, Cf, D, L0, e1;

            //获取中央经线
            switch (lw)
            {
                case LonWide.L6:
                    L0 = delno * 6 - 3;
                    break;
                case LonWide.L3:
                    L0 = delno * 3;
                    break;
                default:
                    L0 = delno * 6 - 3;
                    break;
            }

            e1 = (1 - m_b / m_a) / (1 + m_b / m_a);

            Mf = (Xn - FN) / k0;
            fi = Mf / (m_a * (1 - m_e * m_e / 4 - 3 * Math.Pow(m_e, 4) / 64 - 5 * Math.Pow(m_e, 6) / 256));
            Bf = fi + (3 * e1 / 2 - 27 * Math.Pow(e1, 3) / 32) * Math.Sin(2 * fi)
                + (21 * e1 * e1 / 16 - 55 * Math.Pow(e1, 4) / 32) * Math.Sin(4 * fi)
                    + (151 * Math.Pow(e1, 3) / 96) * Math.Sin(6 * fi);
            Nf = m_a / Math.Sqrt(1 - m_e * m_e * Math.Sin(Bf) * Math.Sin(Bf));
            Rf = (m_a * (1 - m_e * m_e)) / Math.Sqrt(Math.Pow((1 - m_e * m_e * Math.Sin(Bf) * Math.Sin(Bf)), 3));
            Tf = Math.Tan(Bf) * Math.Tan(Bf);
            Cf = m_ee * m_ee * Math.Cos(Bf) * Math.Cos(Bf);
            D = (Ye - FE) / Nf;

            B = Bf - (Nf * Math.Tan(Bf) / Rf) * (
                D * D / 2 - (5 + 3 * Tf + Cf - 9 * Tf * Cf) * Math.Pow(D, 4) / 24
                + (61 + 90 * Tf + 45 * Tf * Tf) * Math.Pow(D, 6) / 720
                );
            B = B * 180 / Math.PI;
            L = (1 / Math.Cos(Bf)) * (
                D - (1 + 2 * Tf + Cf) * Math.Pow(D, 3) / 6
                + (5 + 28 * Tf + 6 * Cf + 8 * Tf * Cf + 24 * Tf * Tf) * Math.Pow(D, 5) / 120
                );
            L = L * 180 / Math.PI + L0;
        }

        /// <summary>
        /// (B,L,h)->(Xn,Ye,Z)
        /// </summary>
        /// <param name="L">经度</param>
        /// <param name="B">纬度</param>
        /// <param name="h">高度</param>
        /// <param name="Ye">大地坐标X值</param>
        /// <param name="Xn">大地坐标Y值</param>
        /// <param name="Z">Z</param>
        public void BLhToXYZ(double L, double B, double h, out double Ye, out double Xn, out double Z)
        {
            double N;
            B = B * Math.PI / 180;
            L = L * Math.PI / 180;
            N = m_a / Math.Sqrt(1 - m_e * m_e * Math.Sin(B) * Math.Sin(B));

            Xn = (N + h) * Math.Cos(B) * Math.Cos(L);
            Ye = (N + h) * Math.Cos(B) * Math.Sin(L);
            Z = (N * (1 - m_e * m_e)) * Math.Sin(B);
        }

        /// <summary>
        /// (Xn,Ye,Z)->(B,L,h)
        /// </summary>
        /// <param name="Ye">大地坐标X值</param>
        /// <param name="Xn">大地坐标Y值</param>
        /// <param name="Z">Z</param>
        /// <param name="L">经度</param>
        /// <param name="B">纬度</param>
        /// <param name="h">高度</param>
        public void XZYToBLh(double Ye, double Xn, double Z, out double L, out double B, out double h)
        {
            double N0, B0, H0;

            L = Math.Atan(Ye / Xn);
            B0 = Math.Atan(Z / Math.Sqrt(Xn * Xn + Ye * Ye));
            N0 = m_a / Math.Sqrt(1 - m_e * m_e * Math.Sin(B0) * Math.Sin(B0));
            H0 = Z / Math.Sin(B0) - N0 * (1 - m_e * m_e);
            B = Math.Atan(Z * (N0 + H0) / (Math.Sqrt(Xn * Xn + Ye * Ye) * (N0 * (1 - m_e * m_e) + H0)));
            while (Math.Abs(B - B0) < 0.0001)
            {
                N0 = m_a / Math.Sqrt(1 - m_e * m_e * Math.Sin(B) * Math.Sin(B));
                H0 = Z / Math.Sin(B) - N0 * (1 - m_e * m_e);
                B0 = B;
                B = Math.Atan(Z * (N0 + H0) / (Math.Sqrt(Xn * Xn + Ye * Ye) * (N0 * (1 - m_e * m_e) + H0)));
            }

            h = H0;
        }

        #endregion
    }
}
