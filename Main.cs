using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NppPluginNET;

namespace NppModelica
{
    class Main
    {
        internal const string PluginName = "NppModelica";
        internal const string PluginVersionNumber = "1.0.5";
#if DEBUG
        internal const string DebugSuffix = " (DEBUG)";
#else
        internal const string DebugSuffix = "";
#endif
        internal const string PluginVersion = PluginVersionNumber + DebugSuffix;
        static public string iniFilePath = null;
        public static MMBrowser frmMMBrowser = null;
        public static Boolean initialized = false;
        static int idMMBrowser = -1;
        static Bitmap tbBmp = Properties.Resources.icon;
        static Bitmap tbBmp_tbTab = Properties.Resources.icon_bmp;
        static Icon tbIcon = null;

        #region StartUp/CleanUp
        internal static void CommandMenuInit()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();

            if (!Directory.Exists(iniFilePath))
                Directory.CreateDirectory(iniFilePath);

            iniFilePath = Path.Combine(iniFilePath, PluginName + ".ini");
            Boolean visible = (Win32.GetPrivateProfileInt("General", "visible", 1, iniFilePath) != 0);

            PluginBase.SetCommand(0, "About", cmdAbout, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(1, "MetaModelica Browser", cmdMMBrowser); idMMBrowser = 1;

            if (visible)
                cmdMMBrowser();
        }
        internal static void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = tbBmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idMMBrowser]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }
        internal static void PluginCleanUp()
        {
            Boolean visible = false;
            if (frmMMBrowser != null)
                visible = frmMMBrowser.Visible;
            Win32.WritePrivateProfileString("General", "visible", visible ? "1" : "0", iniFilePath);
        }
        #endregion

        internal static void cmdAbout()
        {
            MessageBox.Show(PluginName + " Plugin for Notepad++\nVersion " + PluginVersion + "\n\n(c) 2013-2015, Lennart A. Ochel", PluginName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        internal static void cmdMMBrowser()
        {
            if (frmMMBrowser == null)
            {
                frmMMBrowser = new MMBrowser();

                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(tbBmp_tbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = frmMMBrowser.Handle;
                _nppTbData.pszName = "MetaModelica Browser";
                _nppTbData.dlgID = idMMBrowser;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            }
            else
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, frmMMBrowser.Handle);
            }
        }
    }
}