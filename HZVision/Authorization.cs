using SurfaceDefectDetection.Auth;
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
    public partial class Authorization : Form
    {
        public Authorization()
        {
            InitializeComponent();
            InitUI();
        }
        private void InitUI()
        {
            lblTitle.Text = "软件授权失败或未授权";
            lblTitle.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);

            lblTip.Text = "请将下面的机器码发送给软件供应商获取授权文件。";
            lblTip.AutoSize = true;

            txtMachineCode.Text = MachineCodeHelper.GetMachineCode();
        }
        private void Authorization_Load(object sender, EventArgs e)
        {

        }

        private void btnCopy_Click_1(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMachineCode.Text))
            {
                Clipboard.SetText(txtMachineCode.Text);
                MessageBox.Show("机器码已复制到剪贴板！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
