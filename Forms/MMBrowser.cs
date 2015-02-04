using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NppPluginNET;

namespace NppModelica
{
    public partial class MMBrowser : Form
    {
        public String fullfilename = "";
        public String source = "";
        public MetaModelica.Scope scope = null;
        public Modelica.Parser.Lexer lexer = null;

        public String dataPath = "";

        protected const Int32 iPackage = 0;
        protected const Int32 iPackagePublic = 1;
        protected const Int32 iFunction = 2;
        protected const Int32 iFunctionPublic = 3;
        protected const Int32 iUniontype = 4;
        protected const Int32 iUniontypePublic = 5;
        protected const Int32 iConstant = 6;
        protected const Int32 iConstantPublic = 7;
        protected const Int32 iType = 8;
        protected const Int32 iTypePublic = 9;
        protected const Int32 iRecord = 10;
        protected const Int32 iRecordPublic = 11;
        
        public MMBrowser()
        {
            InitializeComponent();

            // Load the images in an ImageList.
            ImageList myImageList = new ImageList();
            myImageList.Images.Add(Properties.Resources.package);           //  0
            myImageList.Images.Add(Properties.Resources.package_public);    //  1
            myImageList.Images.Add(Properties.Resources.function);          //  2
            myImageList.Images.Add(Properties.Resources.function_public);   //  3
            myImageList.Images.Add(Properties.Resources.uniontype);         //  4
            myImageList.Images.Add(Properties.Resources.uniontype_public);  //  5
            myImageList.Images.Add(Properties.Resources.constant);          //  6
            myImageList.Images.Add(Properties.Resources.constant_public);   //  7
            myImageList.Images.Add(Properties.Resources.type);              //  8
            myImageList.Images.Add(Properties.Resources.type_public);       //  9
            myImageList.Images.Add(Properties.Resources.record);            // 10
            myImageList.Images.Add(Properties.Resources.record_public);     // 11

            treeView1.ImageList = myImageList;
            
            // load config
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            string configPath = sbIniFilePath.ToString();
            dataPath = System.IO.Path.Combine(configPath, Main.PluginName);
            
            if (!System.IO.Directory.Exists(configPath))
            {
                MessageBox.Show("There is no OMNotepad++ config folder for plugins. The following folder got created:\n" + configPath, "OMNotpad++ | " + Main.PluginName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.IO.Directory.CreateDirectory(configPath);
            }
            
            if (!System.IO.Directory.Exists(dataPath))
                System.IO.Directory.CreateDirectory(dataPath);
            
            StringBuilder sbWorkspace = new StringBuilder(Win32.MAX_PATH);
            Win32.GetPrivateProfileString("Graphviz", "path", "", sbWorkspace, Win32.MAX_PATH, Main.iniFilePath);
            tstbGraphvizPath.Text = sbWorkspace.ToString();

            publicOnlyToolStripMenuItem.Checked = (Win32.GetPrivateProfileInt("General", "publicOnly", 0, Main.iniFilePath) != 0);
            constantToolStripMenuItem.Checked = (Win32.GetPrivateProfileInt("General", "constant", 1, Main.iniFilePath) != 0);
            typesToolStripMenuItem.Checked = (Win32.GetPrivateProfileInt("General", "type", 1, Main.iniFilePath) != 0);
            recordToolStripMenuItem.Checked = (Win32.GetPrivateProfileInt("General", "record", 1, Main.iniFilePath) != 0);
            uniontypeToolStripMenuItem.Checked = (Win32.GetPrivateProfileInt("General", "uniontype", 1, Main.iniFilePath) != 0);
            functionToolStripMenuItem.Checked = (Win32.GetPrivateProfileInt("General", "function", 1, Main.iniFilePath) != 0);

            StringBuilder sbBuffer = new StringBuilder(Win32.MAX_PATH);
            Win32.GetPrivateProfileString("Update", "path", "http://dev.openmodelica.org/~lochel/NppModelica/", sbBuffer, Win32.MAX_PATH, Main.iniFilePath);
            tstbUpdatePath.Text = sbBuffer.ToString();
            checkForUpdatesToolStripMenuItem.Checked = (Win32.GetPrivateProfileInt("Update", "checkForUpdates", 0, Main.iniFilePath) != 0);
            checkForUpdates();

            splitContainer2.Panel1Collapsed = !searchToolStripMenuItem.Checked;
            splitContainer1.Panel2Collapsed = !consoleToolStripMenuItem.Checked;

            Main.initialized = true;

            updateOutline(true);
        }

        private void checkForUpdates()
        {
            if (checkForUpdatesToolStripMenuItem.Checked)
            {
                try
                {
                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        String currVersion = wc.DownloadString(tstbUpdatePath.Text + "/version");

                        if (Main.PluginVersionNumber != currVersion)
                        {
                            if (MessageBox.Show("This is " + Main.PluginName + " version " + Main.PluginVersion + ". The latest version is " + currVersion + ".\n\nDo you want to download the latest version?", Main.PluginName, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                                Process.Start(tstbUpdatePath.Text);
                        }
                    }
                }
                catch
                {
                    checkForUpdatesToolStripMenuItem.Checked = false;
                    Win32.WritePrivateProfileString("Update", "checkForUpdates", "0", Main.iniFilePath);

                    MessageBox.Show("It was not possible to check for updates.", Main.PluginName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        
        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer2.Panel1Collapsed = !searchToolStripMenuItem.Checked;
        }

        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !consoleToolStripMenuItem.Checked;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main.cmdAbout();
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (treeView1.SelectedNode == null)
                    return;

                if (treeView1.SelectedNode.Tag == null)
                    return;

                if (treeView1.SelectedNode.Tag.GetType().ToString() == "MetaModelica.Package")
                {
                    MetaModelica.Package pcg = (MetaModelica.Package)treeView1.SelectedNode.Tag;
                    treeView1.SelectedNode.Expand();

                    IntPtr curScintilla = PluginBase.GetCurrentScintilla();
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, pcg.startPosition.row + 1000, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_ENSUREVISIBLE, pcg.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, pcg.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);

                    richTextBox1.Text = "package " + pcg.name + ":\n" + pcg.description;
                }
                if (treeView1.SelectedNode.Tag.GetType().ToString() == "MetaModelica.Function")
                {
                    MetaModelica.Function fcn = (MetaModelica.Function)treeView1.SelectedNode.Tag;

                    IntPtr curScintilla = PluginBase.GetCurrentScintilla();
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, fcn.startPosition.row + 1000, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_ENSUREVISIBLE, fcn.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, fcn.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);

                    richTextBox1.Text = "function " + fcn.name + ":\n" + fcn.description;
                }
                else if (treeView1.SelectedNode.Tag.GetType().ToString() == "MetaModelica.Uniontype")
                {
                    MetaModelica.Uniontype uty = (MetaModelica.Uniontype)treeView1.SelectedNode.Tag;

                    IntPtr curScintilla = PluginBase.GetCurrentScintilla();
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, uty.startPosition.row + 1000, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_ENSUREVISIBLE, uty.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, uty.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);

                    richTextBox1.Text = "uniontype " + uty.name;
                }
                else if (treeView1.SelectedNode.Tag.GetType().ToString() == "MetaModelica.Record")
                {
                    MetaModelica.Record rcd = (MetaModelica.Record)treeView1.SelectedNode.Tag;

                    IntPtr curScintilla = PluginBase.GetCurrentScintilla();
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, rcd.startPosition.row + 1000, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_ENSUREVISIBLE, rcd.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, rcd.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);

                    richTextBox1.Text = "record " + rcd.name;
                }
                else if (treeView1.SelectedNode.Tag.GetType().ToString() == "MetaModelica.Constant")
                {
                    MetaModelica.Constant cst = (MetaModelica.Constant)treeView1.SelectedNode.Tag;

                    IntPtr curScintilla = PluginBase.GetCurrentScintilla();
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, cst.startPosition.row + 1000, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_ENSUREVISIBLE, cst.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, cst.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);

                    richTextBox1.Text = "constant " + cst.name;
                }
                else if (treeView1.SelectedNode.Tag.GetType().ToString() == "MetaModelica.Type")
                {
                    MetaModelica.Type typ = (MetaModelica.Type)treeView1.SelectedNode.Tag;

                    IntPtr curScintilla = PluginBase.GetCurrentScintilla();
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, typ.startPosition.row + 1000, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_ENSUREVISIBLE, typ.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, typ.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);

                    richTextBox1.Text = "constant " + typ.name;
                }
                else
                {
                    richTextBox1.Text = treeView1.SelectedNode.Tag.GetType().ToString();
                }
            }
            catch (Exception exception)
            {
                richTextBox1.Text = exception.ToString();
                richTextBox1.Text += "\n" + treeView1.SelectedNode.Tag.GetType();
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                treeView1_DoubleClick(sender, null);
        }

        protected string getText()
        {
            IntPtr curScintilla = PluginBase.GetCurrentScintilla();

            // get the length of the document in bytes
            IntPtr ptr = Win32.SendMessage(curScintilla, SciMsg.SCI_GETLENGTH, 0, 0);
            Int32 length = ptr.ToInt32();

            // get the text of the document
            StringBuilder buffer = new StringBuilder(length);
            Win32.SendMessage(curScintilla, SciMsg.SCI_GETTEXT, length + 1, buffer);

            return buffer.ToString();
        }

        protected void updateOutline(Boolean parseSource)
        {
            StringBuilder currentDirectory = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTDIRECTORY, Win32.MAX_PATH, currentDirectory);
            StringBuilder filename = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFILENAME, Win32.MAX_PATH, filename);

            treeView1.Nodes.Clear();

            fullfilename = System.IO.Path.Combine(currentDirectory.ToString(), filename.ToString());
            if (System.IO.File.Exists(fullfilename) == false)
                return;

            if (fullfilename.Substring(fullfilename.Length - 3) != ".mo")
                return;

            try
            {
                richTextBox1.Text = "";

                if (parseSource)
                {
                    source = getText();

                    scope = new MetaModelica.Scope();
                    lexer = new Modelica.Parser.Lexer(source, Modelica.Parser.Version.MetaModelica, true);

                    try
                    {
                        scope.loadSource(lexer.tokenList);
                    }
                    catch (Exception e)
                    {
                        richTextBox1.Text = e.Message;
                        throw e;
                    }

                    toolStripStatusLabel1.Text = lexer.tokenList.Count + " tokens | " + lexer.numberOfErrors + " errors";
                }

                if (constantToolStripMenuItem.Checked)
                {
                    foreach (MetaModelica.Constant cst in scope.constants.Values)
                    {
                        TreeNode cstNode = new TreeNode();
                        cstNode.Text = cst.name;
                        cstNode.Tag = cst;
                        cstNode.ImageIndex = iConstantPublic;
                        cstNode.SelectedImageIndex = iConstantPublic;

                        treeView1.Nodes.Add(cstNode);
                    }
                }

                if (typesToolStripMenuItem.Checked)
                {
                    foreach (MetaModelica.Type typ in scope.types.Values)
                    {
                        TreeNode typNode = new TreeNode();
                        typNode.Text = typ.name;
                        typNode.Tag = typ;
                        typNode.ImageIndex = iTypePublic;
                        typNode.SelectedImageIndex = iTypePublic;

                        treeView1.Nodes.Add(typNode);
                    }
                }

                if (recordToolStripMenuItem.Checked)
                {
                    foreach (MetaModelica.Record rcd in scope.records.Values)
                    {
                        TreeNode rcdNode = new TreeNode();
                        rcdNode.Text = rcd.name;
                        rcdNode.Tag = rcd;
                        rcdNode.ImageIndex = iRecordPublic;
                        rcdNode.SelectedImageIndex = iRecordPublic;

                        treeView1.Nodes.Add(rcdNode);
                    }
                }

                if (uniontypeToolStripMenuItem.Checked)
                {
                    foreach (MetaModelica.Uniontype uty in scope.uniontypes.Values)
                    {
                        TreeNode utyNode = new TreeNode();
                        utyNode.Text = uty.name;
                        utyNode.Tag = uty;
                        utyNode.ImageIndex = iUniontypePublic;
                        utyNode.SelectedImageIndex = iUniontypePublic;

                        foreach (MetaModelica.Record rcd in uty.records.Values)
                        {
                            TreeNode rcdNode = new TreeNode();
                            rcdNode.Text = rcd.name;
                            rcdNode.Tag = rcd;
                            rcdNode.ImageIndex = iRecordPublic;
                            rcdNode.SelectedImageIndex = iRecordPublic;

                            utyNode.Nodes.Add(rcdNode);
                        }

                        treeView1.Nodes.Add(utyNode);
                    }
                }

                foreach(MetaModelica.Package p in scope.packages.Values)
                {
                    TreeNode node = new TreeNode();
                    node.Name = p.name;
                    node.Text = p.name;
                    node.Tag = p;
                    node.ImageIndex = iPackagePublic;
                    node.SelectedImageIndex = iPackagePublic;

                    try
                    {
                        // call graph
                        if (callGraphToolStripMenuItem.Checked)
                        {
                            List<string> unusedFunctions;
                            String dotSource = p.getGraphvizSource(out unusedFunctions);
                            if (unusedFunctions.Count > 0)
                            {
                                if (unusedFunctions.Count > 1)
                                    richTextBox1.Text = unusedFunctions.Count + " unused protected functions found:";
                                else
                                    richTextBox1.Text = "1 unused protected function found:";

                                foreach (String fnc in unusedFunctions)
                                    richTextBox1.Text += "\n  - " + fnc;
                            }
                            else
                                richTextBox1.Text = "No unused protected functions found";

                            System.IO.File.WriteAllText(System.IO.Path.Combine(dataPath, p.name + ".dot"), dotSource);

                            Process process = new Process();
                            process.StartInfo.FileName = System.IO.Path.Combine(tstbGraphvizPath.Text, @"bin\dot");
                            process.StartInfo.WorkingDirectory = dataPath;
                            //process.StartInfo.Arguments = "-Tsvg " + packageName + ".dot -o " + packageName + ".svg";
                            process.StartInfo.Arguments = "-Tsvg " + p.name + ".dot -o temp.svg";
                            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            process.Start();
                            process.WaitForExit();

                            System.IO.File.Delete(System.IO.Path.Combine(dataPath, p.name + ".dot"));
                            //callGraphForm.updateCallGraph(System.IO.Path.Combine(dataPath, packageName + ".svg"));
                        }
                    }
                    catch (Exception e)
                    {
                        toolStripStatusLabel1.Text = e.Message;
                        MessageBox.Show(e.Message);
                    }

                    if (functionToolStripMenuItem.Checked)
                    {
                        foreach (MetaModelica.Function fcn in p.publicFunctions.Values)
                        {
                            TreeNode fcnNode = new TreeNode();
                            fcnNode.Text = fcn.name;
                            fcnNode.Tag = fcn;
                            fcnNode.ImageIndex = iFunctionPublic;
                            fcnNode.SelectedImageIndex = iFunctionPublic;

                            node.Nodes.Add(fcnNode);
                        }
                    }
                    if (functionToolStripMenuItem.Checked && !publicOnlyToolStripMenuItem.Checked)
                    {
                        foreach (MetaModelica.Function fcn in p.protectedFunctions.Values)
                        {
                            TreeNode fcnNode = new TreeNode();
                            fcnNode.Text = fcn.name;
                            fcnNode.Tag = fcn;
                            fcnNode.ImageIndex = iFunction;
                            fcnNode.SelectedImageIndex = iFunction;

                            node.Nodes.Add(fcnNode);
                        }
                    }

                    if (typesToolStripMenuItem.Checked)
                    {
                        foreach (MetaModelica.Type typ in p.publicTypes.Values)
                        {
                            TreeNode typNode = new TreeNode();
                            typNode.Text = typ.name;
                            typNode.Tag = typ;
                            typNode.ImageIndex = iTypePublic;
                            typNode.SelectedImageIndex = iTypePublic;

                            node.Nodes.Add(typNode);
                        }
                    }
                    if (functionToolStripMenuItem.Checked && !publicOnlyToolStripMenuItem.Checked)
                    {
                        foreach (MetaModelica.Type typ in p.protectedTypes.Values)
                        {
                            TreeNode typNode = new TreeNode();
                            typNode.Text = typ.name;
                            typNode.Tag = typ;
                            typNode.ImageIndex = iType;
                            typNode.SelectedImageIndex = iType;

                            node.Nodes.Add(typNode);
                        }
                    }

                    if (recordToolStripMenuItem.Checked)
                    {
                        foreach (MetaModelica.Record rcd in p.publicRecords.Values)
                        {
                            TreeNode rcdNode = new TreeNode();
                            rcdNode.Text = rcd.name;
                            rcdNode.Tag = rcd;
                            rcdNode.ImageIndex = iRecordPublic;
                            rcdNode.SelectedImageIndex = iRecordPublic;

                            node.Nodes.Add(rcdNode);
                        }
                    }
                    if (recordToolStripMenuItem.Checked && !publicOnlyToolStripMenuItem.Checked)
                    {
                        foreach (MetaModelica.Record rcd in p.protectedRecords.Values)
                        {
                            TreeNode rcdNode = new TreeNode();
                            rcdNode.Text = rcd.name;
                            rcdNode.Tag = rcd;
                            rcdNode.ImageIndex = iRecord;
                            rcdNode.SelectedImageIndex = iRecord;

                            node.Nodes.Add(rcdNode);
                        }
                    }

                    if (constantToolStripMenuItem.Checked)
                    {
                        foreach (MetaModelica.Constant cst in p.publicConstants.Values)
                        {
                            TreeNode cstNode = new TreeNode();
                            cstNode.Text = cst.name;
                            cstNode.Tag = cst;
                            cstNode.ImageIndex = iConstantPublic;
                            cstNode.SelectedImageIndex = iConstantPublic;

                            node.Nodes.Add(cstNode);
                        }
                    }
                    if (constantToolStripMenuItem.Checked && !publicOnlyToolStripMenuItem.Checked)
                    {
                        foreach (MetaModelica.Constant cst in p.protectedConstants.Values)
                        {
                            TreeNode cstNode = new TreeNode();
                            cstNode.Text = cst.name;
                            cstNode.Tag = cst;
                            cstNode.ImageIndex = iConstant;
                            cstNode.SelectedImageIndex = iConstant;

                            node.Nodes.Add(cstNode);
                        }
                    }

                    if (uniontypeToolStripMenuItem.Checked)
                    {
                        foreach (MetaModelica.Uniontype uty in p.publicUniontypes.Values)
                        {
                            TreeNode utyNode = new TreeNode();
                            utyNode.Text = uty.name;
                            utyNode.Tag = uty;
                            utyNode.ImageIndex = iUniontypePublic;
                            utyNode.SelectedImageIndex = iUniontypePublic;

                            foreach (MetaModelica.Record rcd in uty.records.Values)
                            {
                                TreeNode rcdNode = new TreeNode();
                                rcdNode.Text = rcd.name;
                                rcdNode.Tag = rcd;
                                rcdNode.ImageIndex = iRecordPublic;
                                rcdNode.SelectedImageIndex = iRecordPublic;

                                utyNode.Nodes.Add(rcdNode);
                            }

                            node.Nodes.Add(utyNode);
                        }
                    }
                    if (uniontypeToolStripMenuItem.Checked && !publicOnlyToolStripMenuItem.Checked)
                    {
                        foreach (MetaModelica.Uniontype uty in p.protectedUniontypes.Values)
                        {
                            TreeNode utyNode = new TreeNode();
                            utyNode.Text = uty.name;
                            utyNode.Tag = uty;
                            utyNode.ImageIndex = iUniontype;
                            utyNode.SelectedImageIndex = iUniontype;

                            foreach (MetaModelica.Record rcd in uty.records.Values)
                            {
                                TreeNode rcdNode = new TreeNode();
                                rcdNode.Text = rcd.name;
                                rcdNode.Tag = rcd;
                                rcdNode.ImageIndex = iRecord;
                                rcdNode.SelectedImageIndex = iRecord;

                                utyNode.Nodes.Add(rcdNode);
                            }

                            node.Nodes.Add(utyNode);
                        }
                    }

                    node.Expand();
                    treeView1.Nodes.Add(node);
                }

                treeView1.Sort();
            }
            catch (Exception e)
            {
                toolStripStatusLabel1.Text = e.Message;
            }
        }

        public void notification(SCNotification nc)
        {
            if ((nc.nmhdr.code == (uint)NppMsg.NPPN_BUFFERACTIVATED) || (nc.nmhdr.code == (uint)NppMsg.NPPN_FILESAVED))
                updateOutline(true);
        } 
        
        private void simulateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);
            StringBuilder currentDirectory = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTDIRECTORY, Win32.MAX_PATH, currentDirectory);
            StringBuilder filename = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFILENAME, Win32.MAX_PATH, filename);
            
            // simulate
            // create the ProcessStartInfo using "cmd" as the program to be run,
            // and "/c " as the parameters.
            // Incidentally, /c tells cmd that we want it to execute the command that follows,
            // and then exit.
            System.Diagnostics.ProcessStartInfo procStartInfo =
                new System.Diagnostics.ProcessStartInfo("omc", "\"" + filename.ToString() + "\"");

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Set working directory
            procStartInfo.WorkingDirectory = currentDirectory.ToString();
            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            // Get the output into a string
            string result = proc.StandardOutput.ReadToEnd();

            MessageBox.Show(result, "OpenModelica", MessageBoxButtons.OK);

            // open file
            // Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DOOPEN, 0, new StringBuilder(tmpPath + "output"));
        }

        private void tstbGraphvizPath_TextChanged(object sender, EventArgs e)
        {
            StringBuilder sbConfigPath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbConfigPath);
            string configPath = sbConfigPath.ToString();

            if (!System.IO.Directory.Exists(configPath))
                System.IO.Directory.CreateDirectory(configPath);

            Win32.WritePrivateProfileString("Graphviz", "path", tstbGraphvizPath.Text, Main.iniFilePath);
        }

        private void callGraphViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            callGraphToolStripMenuItem.Checked = true;
            String filename = System.IO.Path.Combine(dataPath, "temp.svg");
            if(System.IO.File.Exists(filename))
                System.Diagnostics.Process.Start(filename);
        }

        private void callGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateOutline(false);
        }

        private void publicOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Win32.WritePrivateProfileString("General", "publicOnly", publicOnlyToolStripMenuItem.Checked ? "1" : "0", Main.iniFilePath);
            updateOutline(false);
        }

        private void constantToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Win32.WritePrivateProfileString("General", "constant", constantToolStripMenuItem.Checked ? "1" : "0", Main.iniFilePath);
            updateOutline(false);
        }

        private void functionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Win32.WritePrivateProfileString("General", "function", functionToolStripMenuItem.Checked ? "1" : "0", Main.iniFilePath);
            updateOutline(false);
        }

        private void uniontypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Win32.WritePrivateProfileString("General", "uniontype", uniontypeToolStripMenuItem.Checked ? "1" : "0", Main.iniFilePath);
            updateOutline(false);
        }

        private void typesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Win32.WritePrivateProfileString("General", "type", typesToolStripMenuItem.Checked ? "1" : "0", Main.iniFilePath);
            updateOutline(false);
        }

        private void recordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Win32.WritePrivateProfileString("General", "record", recordToolStripMenuItem.Checked ? "1" : "0", Main.iniFilePath);
            updateOutline(false);
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Win32.WritePrivateProfileString("Update", "checkForUpdates", checkForUpdatesToolStripMenuItem.Checked ? "1" : "0", Main.iniFilePath);
            checkForUpdates();
        }

        private void tstbUpdatePath_TextChanged(object sender, EventArgs e)
        {
            Win32.WritePrivateProfileString("Update", "path", tstbUpdatePath.Text, Main.iniFilePath);
        }

        private void tstbUpdatePath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                checkForUpdatesToolStripMenuItem.Checked = true;
                Win32.WritePrivateProfileString("Update", "checkForUpdates", "1", Main.iniFilePath);
                checkForUpdates();
            }
        }
    }
}
