using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JmsVstHost
{
    public partial class Dlg : Form
    {
        /// <summary>
        /// Gets or sets the Plugin Command Stub.
        /// </summary>
        public Jacobi.Vst.Core.Host.IVstPluginCommandStub PluginCommandStub { get; set; }

        public Dlg()
        {
            InitializeComponent();
        } 
        /// <summary>
          /// Shows the custom plugin editor UI.
          /// </summary>
          /// <param name="owner"></param>
          /// <returns></returns>
        public new void ShowDialog()
        {
            Rectangle wndRect = new Rectangle();

            this.Text = PluginCommandStub.GetEffectName();

            if (PluginCommandStub.EditorGetRect(out wndRect))
            {
                this.Size = this.SizeFromClientSize(new Size(wndRect.Width, wndRect.Height));
                PluginCommandStub.EditorOpen(this.Handle);
            }
            base.ShowDialog();
        }
    }
}
