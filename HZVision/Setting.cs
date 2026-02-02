using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HZVision
{
    public partial class Setting : Form
    {
        //public delegate void SettingsChangedHandler(object sender, SettingsEventArgs e);
        //public event SettingsChangedHandler SettingsChanged;
        private readonly int _regionthreshold;
        private readonly int _regionfilter1;
        private readonly int _regionfilter2;
        private readonly int _imgsaveNum;
        public int threshold {  get; private set; }
        public int filtersize1 { get; private set; }
        public int filtersize2 { get; private set; }
        public int saveNum { get; private set; }
        public bool IsSaved { get; private set; }=false;
        public Setting()
        {
            InitializeComponent();
            _regionthreshold = regionthreshold;
            _regionfilter1 = regionfilter1;
            _regionfilter2 = regionfilter2;
            _imgsaveNum = imgsaveNum;
            textBox_threshold.Text = _regionthreshold.ToString();
            textBox_filtersize1.Text= _regionfilter1.ToString();
            textBox_filtersize2.Text= _regionfilter2.ToString();
            textImgNum.Text= _imgsaveNum.ToString();
        }

        private void butSaveNum_Click(object sender, EventArgs e)
        {
            int.TryParse(textImgNum.Text, out threshold);
            int.TryParse(textBox_filtersize1.Text, out filtersize1);
            int.TryParse(textBox_filtersize2.Text, out filtersize2);
        }

        public class SettingsEventArgs : EventArgs
        {
            //public string Setting1 { get; set; }
            //public int Setting2 { get; set; }
            //public bool Setting3 { get; set; }
        }

        private void btnConcel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
