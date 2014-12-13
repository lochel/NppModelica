using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NppModelica
{
    public partial class CallGraph : Form
    {
        MMBrowser mmBrowser = null;

        public CallGraph(MMBrowser parent)
        {
            InitializeComponent();
            mmBrowser = parent;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            mmBrowser.callGraphForm = null;
        }

        public void updateCallGraph(String filename, String package)
        {
            this.Text = "Internal call graph of package " + package;
            webBrowser1.Url = new System.Uri(filename, System.UriKind.Absolute);
        }
    }
}
