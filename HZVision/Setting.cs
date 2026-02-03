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
        private readonly string _ccdName;
        private readonly int _port;
        public int threshold {  get; private set; }
        public int filtersize1 { get; private set; }
        public int filtersize2 { get; private set; }
        public int saveNum { get; private set; }
        public int port { get; private set; }
        public string ccdName { get; private set; }
        public bool IsSaved { get; private set; }
        public Setting(string currentccdName,int currentPort,int currentthreshold, int currentfilter1, int currentfilter2, int currentsaveNum,bool currentAutoSave)
        {
            InitializeComponent();
            _regionthreshold = currentthreshold;
            _regionfilter1 = currentfilter1;
            _regionfilter2 = currentfilter2;
            _imgsaveNum = currentsaveNum;
            _ccdName= currentccdName;
            _port= currentPort;

            textBox_threshold.Text = _regionthreshold.ToString();
            textBox_filtersize1.Text= _regionfilter1.ToString();
            textBox_filtersize2.Text= _regionfilter2.ToString();
            textImgNum.Text= _imgsaveNum.ToString();
            textBox_CCDName.Text= _ccdName;
            textBox_Port.Text= _port.ToString();
            checkAutoSave.Checked = currentAutoSave;
        }

        private void butSaveNum_Click(object sender, EventArgs e)
        {
            int temp;
            threshold = int.TryParse(textBox_threshold.Text.Trim(), out temp) ? temp : 120;
            filtersize1 = int.TryParse(textBox_filtersize1.Text.Trim(), out temp) ? temp : 9;
            filtersize2 = int.TryParse(textBox_filtersize2.Text.Trim(), out temp) ? temp : 251;
            saveNum = int.TryParse(textImgNum.Text.Trim(), out temp) ? temp : 0;
            ccdName = textBox_CCDName.Text;
            port= int.TryParse(textBox_Port.Text.Trim(), out temp) ? temp : 6000;
            IsSaved = checkAutoSave.Checked;
            this.DialogResult= DialogResult.OK;
            //this.Close();
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
