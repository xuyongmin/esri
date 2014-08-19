using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DotNet.Common.GIS.GeoUtilities;
using DotNet.Common.GIS.GeoUtilities.CommonUtility;
using DotNet.Common.GIS.GeoUtilities.CoordTransUtility;
using DotNet.Common.GIS.GeoUtilities.TFHUtility;
using DotNet.Common.GIS.GeoUtilities.FileUtility;

namespace DotNet.Common.GIS.MainForm
{
    public partial class frmMain : Form
    {
        private Spheroid m_CurrentSpheroid = Spheroid.Xian80;
        private ICoordinate m_CoordHelper = null;
        private List<TFHObject> m_AllTFHS = new List<TFHObject>();

        public frmMain()
        {
            InitializeComponent();
            InitConfig();
            this.m_CoordHelper = CoordinateFactory.CreateCoordinate(this.m_CurrentSpheroid);
            this.labela.Text = this.m_CoordHelper.a.ToString();
            this.labelb.Text = this.m_CoordHelper.b.ToString();
            SayVersion();
        }

        private void InitConfig()
        {
            List<KeyValueObject> spList = new List<KeyValueObject>();
            foreach (int key in Enum.GetValues(typeof(Spheroid)))
            {
                KeyValueObject kvo = new KeyValueObject();
                kvo.Key = key.ToString();
                kvo.Value = Enum.GetName(typeof(Spheroid), key);
                spList.Add(kvo);
            }
            this.comboBoxCoordinateSystem.DataSource = spList;
            this.comboBoxCoordinateSystem.DisplayMember = "Value";
            this.comboBoxCoordinateSystem.ValueMember = "Key";
            this.comboBoxCoordinateSystem.SelectedValueChanged += new EventHandler(comboBoxCoordinateSystem_SelectedValueChanged);

            List<KeyValueObject> lwList = new List<KeyValueObject>();
            foreach (int key in Enum.GetValues(typeof(LonWide)))
            {
                KeyValueObject kvo = new KeyValueObject();
                kvo.Key = key.ToString();
                kvo.Value = Enum.GetName(typeof(LonWide), key);
                lwList.Add(kvo);
            }
            this.comboBoxLonWide.DataSource = lwList;
            this.comboBoxLonWide.DisplayMember = "Value";
            this.comboBoxLonWide.ValueMember = "Key";

            this.textBoxDELNO.Text = "40";
            this.textBoxXn.Text = "3142561.499";
            this.textBoxYe.Text = "638661.207";

            //初始化比例尺
            List<MapScaleObject> tfhMapScaleList = TFHHelper.TFHMapScaleList;
            this.comboBoxMapScale.DataSource = tfhMapScaleList;
            this.comboBoxMapScale.DisplayMember = "Label";
            this.comboBoxMapScale.ValueMember = "Key";

            List<MapScaleObject> tfhLTRBMapScaleList = TFHHelper.TFHMapScaleList;
            this.comboBoxLTRBMapScale.DataSource = tfhLTRBMapScaleList;
            this.comboBoxLTRBMapScale.DisplayMember = "Label";
            this.comboBoxLTRBMapScale.ValueMember = "Key";
        }

        void comboBoxCoordinateSystem_SelectedValueChanged(object sender, EventArgs e)
        {
            this.m_CurrentSpheroid = (Spheroid)int.Parse(this.comboBoxCoordinateSystem.SelectedValue.ToString());
            this.m_CoordHelper = CoordinateFactory.CreateCoordinate(this.m_CurrentSpheroid);
            this.labela.Text = this.m_CoordHelper.a.ToString();
            this.labelb.Text = this.m_CoordHelper.b.ToString();
        }

        private void buttonXnYeToLB_Click(object sender, EventArgs e)
        {
            double Xn = 0;
            double.TryParse(this.textBoxXn.Text.Trim(), out Xn);
            double Ye = 0;
            double.TryParse(this.textBoxYe.Text.Trim(), out Ye);
            LonWide lw = (LonWide)int.Parse(this.comboBoxLonWide.SelectedValue.ToString());
            int delno = int.Parse(this.textBoxDELNO.Text.Trim());
            
            double L, B;
            this.m_CoordHelper.GaussPrjInvCalculate(Ye, Xn, lw, delno, out L, out B);
            this.textBoxB.Text = B.ToString();
            this.textBoxL.Text = L.ToString();
        }

        private void buttonLBToXnYe_Click(object sender, EventArgs e)
        {
            double Ye, Xn;
            double L = 0;
            double.TryParse(this.textBoxL.Text.Trim(), out L);
            double B = 0;
            double.TryParse(this.textBoxB.Text.Trim(), out B);
            LonWide lw = (LonWide)int.Parse(this.comboBoxLonWide.SelectedValue.ToString());
            int delno = int.Parse(this.textBoxDELNO.Text.Trim());
            this.m_CoordHelper.GaussPrjCalculate(L, B, lw, delno, out Ye, out Xn);
            this.textBoxYe.Text = Ye.ToString();
            this.textBoxXn.Text = Xn.ToString();
        }

        /// <summary>
        /// 图幅号经纬度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTFHToLB_Click(object sender, EventArgs e)
        {
            MapScaleObject scaleObj = null;
            double LTL, LTB, RBL, RBB;
            string tfh = this.textBoxTFH.Text.Trim();
            TFHHelper.GetLTRBPoints(tfh, out scaleObj, out LTL, out LTB, out RBL, out RBB);
            if (scaleObj != null)
            {
                MessageBox.Show(string.Format("比例尺：{0}  LTL:{1}  LTB:{2}  RBL:{3}  RBB:{4}", scaleObj.Label, LTL.ToString(), LTB.ToString(), RBL.ToString(), RBB.ToString()));
            }
            else
            {
                MessageBox.Show(string.Format("图幅号[{0}]无法解析！", tfh));
            }
        }

        /// <summary>
        /// 经纬度转图幅号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLBToTFH_Click(object sender, EventArgs e)
        {
            MapScaleObject obj = this.comboBoxMapScale.SelectedItem as MapScaleObject;
            if (obj != null)
            {
                double L = 0; double.TryParse(this.textBoxToTFHL.Text.Trim(), out L);
                double B = 0; double.TryParse(this.textBoxToTFHB.Text.Trim(), out B);
                string tfh = TFHHelper.GetTFH(L, B, obj);
                this.textBoxTFH.Text = tfh;
            }
        }

        private void buttonXnYeZToLBh_Click(object sender, EventArgs e)
        {

        }

        private void buttonLBhToXnYeZ_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 导出所有图幅号到shape文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTFHExportShp_Click(object sender, EventArgs e)
        {
            MapScaleObject scaleObj = this.comboBoxLTRBMapScale.SelectedItem as MapScaleObject;
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Shape文件(*.shp)|*.*|所有文件(*.*)|*.*";
            saveDialog.FilterIndex = 1;
            saveDialog.RestoreDirectory = true;
            saveDialog.FileName = "LB-" + scaleObj.Scale.ToString();
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = saveDialog.FileName;
                List<string> xyList = new List<string>();
                foreach (TFHObject obj in this.checkedListBoxLTRBTFHs.Items)
                {
                    string xy = string.Empty;
                    string[] xys = obj.Extent.Split(',');
                    if (xys.Length == 4)
                    {
                        double ltl = double.Parse(xys[0]);
                        double ltb = double.Parse(xys[1]);
                        double rbl = double.Parse(xys[2]);
                        double rbb = double.Parse(xys[3]);

                        xy += string.Format("{0},", obj.TFH);
                        xy += string.Format("{0} {1},", ltl, rbb);
                        xy += string.Format("{0} {1},", ltl, ltb);
                        xy += string.Format("{0} {1},", rbl, ltb);
                        xy += string.Format("{0} {1},", rbl, rbb);
                        xy += string.Format("{0} {1}", ltl, rbb);
                        xyList.Add(xy);
                    }
                }

                try
                {
                    ShapeUtility.ExportShapeFile(fileName, xyList);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void buttonExportXYTFHShape_Click(object sender, EventArgs e)
        {
            if (this.m_AllTFHS.Count > 0)
            {
                this.m_CurrentSpheroid = (Spheroid)int.Parse(this.comboBoxCoordinateSystem.SelectedValue.ToString());
                this.m_CoordHelper = CoordinateFactory.CreateCoordinate(this.m_CurrentSpheroid);
                MapScaleObject scaleObj = this.comboBoxLTRBMapScale.SelectedItem as MapScaleObject;

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Shape文件(*.shp)|*.*|所有文件(*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.RestoreDirectory = true;
                saveDialog.FileName = "XY-" + scaleObj.Scale.ToString();
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = saveDialog.FileName;
                    List<string> xyList = new List<string>();
                    //获取第一个和最后一格
                    TFHObject firstTFHObj = null;
                    TFHObject endTFHObj = null;
                    int tmpHs = 0;
                    int tmpLs = 0;
                    firstTFHObj = this.m_AllTFHS[0];
                    endTFHObj = this.m_AllTFHS[this.m_AllTFHS.Count - 1];
                    for (int i = 0; i < this.m_AllTFHS.Count; i++)
                    {
                        if (this.m_AllTFHS[i].H1 == firstTFHObj.H1)
                        {
                            tmpLs++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    tmpHs = this.m_AllTFHS.Count / tmpLs;

                    double ltYe, ltXn, rbYe, rbXn;
                    LonWide lw = (LonWide)int.Parse(this.comboBoxLonWide.SelectedValue.ToString());
                    int delno = int.Parse(this.textBoxDELNO.Text.Trim());
                    string[] firstTLXYs = firstTFHObj.Extent.Split(',');
                    string[] endRBXYs = endTFHObj.Extent.Split(',');
                    double ltl = double.Parse(firstTLXYs[0]);
                    double ltb = double.Parse(firstTLXYs[1]);
                    double rbl = double.Parse(endRBXYs[2]);
                    double rbb = double.Parse(endRBXYs[3]);
                    this.m_CoordHelper.GaussPrjCalculate(ltl, ltb, lw, delno, out ltYe, out ltXn);
                    this.m_CoordHelper.GaussPrjCalculate(rbl, rbb, lw, delno, out rbYe, out rbXn);
                    double rXn = 0, rYe = 0;
                    rXn = Math.Abs((ltXn - rbXn) / tmpHs);
                    rYe = Math.Abs((rbYe - ltYe) / tmpLs);

                    for (int i = 0; i < this.m_AllTFHS.Count; i++)
                    {
                        double tmpLTYe = 0, tmpLTXn = 0, tmpRBYe = 0, tmpRBXn = 0;
                        int tmpH = (int)(Math.Floor((double)(i / tmpLs)));
                        int tmpL = (int)(i % tmpLs);
                        string xy = string.Empty;

                        tmpLTYe = ltYe + rYe * tmpL;
                        tmpLTXn = ltXn - rXn * tmpH;
                        tmpRBYe = ltYe + rYe * (tmpL + 1);
                        tmpRBXn = ltXn - rXn * (tmpH + 1);

                        xy += string.Format("{0},", this.m_AllTFHS[i].TFH);
                        xy += string.Format("{0} {1},", tmpLTYe, tmpRBXn);
                        xy += string.Format("{0} {1},", tmpLTYe, tmpLTXn);
                        xy += string.Format("{0} {1},", tmpRBYe, tmpLTXn);
                        xy += string.Format("{0} {1},", tmpRBYe, tmpRBXn);
                        xy += string.Format("{0} {1}", tmpLTYe, tmpRBXn);
                        xyList.Add(xy);
                    }
                    try
                    {
                        ShapeUtility.ExportShapeFile(fileName, xyList);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void buttonExportTFHCoord_Click(object sender, EventArgs e)
        {
            this.m_CurrentSpheroid = (Spheroid)int.Parse(this.comboBoxCoordinateSystem.SelectedValue.ToString());
            this.m_CoordHelper = CoordinateFactory.CreateCoordinate(this.m_CurrentSpheroid);

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "文本文件(*.txt)|*.txt|所有文件(*.*)|*.*";
            saveDialog.FilterIndex = 1;
            saveDialog.RestoreDirectory = true;
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = saveDialog.FileName;
                Stream fs = saveDialog.OpenFile();
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    List<string> xyList = new List<string>();
                    foreach (TFHObject obj in this.m_AllTFHS)
                    {
                        string[] xys = obj.Extent.Split(',');
                        if (xys.Length == 4)
                        {
                            double ltl = double.Parse(xys[0]);
                            double ltb = double.Parse(xys[1]);
                            double rbl = double.Parse(xys[2]);
                            double rbb = double.Parse(xys[3]);

                            double ltYe, ltXn, rbYe, rbXn;
                            LonWide lw = (LonWide)int.Parse(this.comboBoxLonWide.SelectedValue.ToString());
                            int delno = int.Parse(this.textBoxDELNO.Text.Trim());
                            this.m_CoordHelper.GaussPrjCalculate(ltl, ltb, lw, delno, out ltYe, out ltXn);
                            this.m_CoordHelper.GaussPrjCalculate(rbl, rbb, lw, delno, out rbYe, out rbXn);
                            string lbinfo = string.Format("{0} {1} {2} {3}", ltl, ltb, rbl, rbb);
                            string xyinfo = string.Format("{0} {1} {2} {3}", ltYe, ltXn, rbYe, rbXn);
                            string tfhinfo = string.Format("{0},{1},{2}", obj.TFH, lbinfo, xyinfo);
                            sw.WriteLine(tfhinfo);
                        }
                    }
                }
            }
        }

        private void buttonLTRBPointToTFHs_Click(object sender, EventArgs e)
        {
            MapScaleObject obj = this.comboBoxLTRBMapScale.SelectedItem as MapScaleObject;
            this.m_AllTFHS = new List<TFHObject>();
            if (obj != null)
            {
                string ltTFH, rbTFH;
                ltTFH = this.textBoxLTTFH.Text.Trim();
                rbTFH = this.textBoxRBTFH.Text.Trim();
                string msg = string.Empty;
                this.m_AllTFHS = TFHHelper.GetTFHList(ltTFH, rbTFH, out msg);
                this.checkedListBoxLTRBTFHs.DataSource = null;
                this.checkedListBoxLTRBTFHs.DataSource = this.m_AllTFHS;
                this.checkedListBoxLTRBTFHs.DisplayMember = "TFH";
                this.checkedListBoxLTRBTFHs.ValueMember = "TFH";
            }
        }

        private void buttonLTBRTFH_Click(object sender, EventArgs e)
        {
            MapScaleObject obj = this.comboBoxLTRBMapScale.SelectedItem as MapScaleObject;
            if (obj != null)
            {
                double ltl, ltb, rbl, rbb;
                double.TryParse(this.textBoxLTL.Text.Trim(), out ltl);
                double.TryParse(this.textBoxLTB.Text.Trim(), out ltb);
                double.TryParse(this.textBoxRBL.Text.Trim(), out rbl);
                double.TryParse(this.textBoxRBB.Text.Trim(), out rbb);
                string ltTFH = TFHHelper.GetTFH(ltl, ltb, obj);
                string rbTFH = TFHHelper.GetTFH(rbl, rbb, obj);
                this.textBoxLTTFH.Text = ltTFH;
                this.textBoxRBTFH.Text = rbTFH;
            }
        }

        private void SayVersion()
        {
            this.labelVersion.AutoSize = true;
            int w = 490;
            this.labelVersion.MaximumSize = new Size(w, 0);
            string msg = string.Empty;
            msg += "    通用地图工具箱V1.0提供了两个基本功能：地理坐标转换和图幅号生成工具！\r\n\r\n";
            msg += "    地理坐标转换工具支持WGS84，BJ54，XA80坐标系下的经纬度和大地坐标互换。注意不是从WGS84转BJ54这样的不同坐标系转换，而是统一坐标系下面的经纬度和大地坐标系转换。\r\n\r\n";
            msg += "    图幅号生成工具是根据经纬网国际分幅法的新图幅号算法进行计算。工具支持根据某个坐标点查找该点所在的图幅号，也支持根据左上和右下两个点获取所有图幅号，如果需要，还能够生成图幅号Shape文件。\r\n\r\n";
            msg += "    注意，图幅号生成有两种方式，即大地坐标和经纬度坐标。当导出为大地坐标的时候，两点的距离最好是在同一分度带内，否则跨带误差会较大。";
            msg += "    有需要的朋友可以Q我 ^_^\r\n\r\n";
            this.labelVersion.Text = msg;
        }
    }

    public class KeyValueObject
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
