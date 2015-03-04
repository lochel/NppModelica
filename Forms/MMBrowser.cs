﻿using System;
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
        private String fullfilename = "";
        private String source = "";
        private MetaModelica.Scope mmScope = null;
        private Susan.Scope tplScope = null;
        private MetaModelica.Parser.Lexer mmLexer = null;
        private Susan.Parser.Lexer tplLexer = null;

        private String dataPath = "";
        
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

            splitContainer2.Panel1Collapsed = !searchToolStripMenuItem.Checked;
            splitContainer1.Panel2Collapsed = !consoleToolStripMenuItem.Checked;

            Main.initialized = true;

            updateOutline(true);
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
                else if (treeView1.SelectedNode.Tag.GetType().ToString() == "MetaModelica.Function")
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
                else if (treeView1.SelectedNode.Tag.GetType().ToString() == "Susan.Template")
                {
                    Susan.Template tpl = (Susan.Template)treeView1.SelectedNode.Tag;

                    IntPtr curScintilla = PluginBase.GetCurrentScintilla();
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, tpl.startPosition.row + 1000, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_ENSUREVISIBLE, tpl.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, tpl.startPosition.row - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);

                    richTextBox1.Text = "template " + tpl.name;
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

        private string getText()
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

        private void updateOutline(Boolean parseSource)
        {
            StringBuilder currentDirectory = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTDIRECTORY, Win32.MAX_PATH, currentDirectory);
            StringBuilder filename = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFILENAME, Win32.MAX_PATH, filename);

            treeView1.Nodes.Clear();

            richTextBox1.Text = "";
            toolStripStatusLabel1.Text = "";

            fullfilename = System.IO.Path.Combine(currentDirectory.ToString(), filename.ToString());
            if (System.IO.File.Exists(fullfilename) == false)
                return;

            if (fullfilename.Substring(fullfilename.Length - 3) != ".mo" && fullfilename.Substring(fullfilename.Length - 4) != ".tpl")
                return;

            try
            {
                if (parseSource)
                {
                    source = getText();

                    if (fullfilename.Substring(fullfilename.Length - 3) == ".mo")
                    {
                        mmScope = new MetaModelica.Scope();
                        mmLexer = new MetaModelica.Parser.Lexer(source, MetaModelica.Parser.Version.MetaModelica, true);
                    }

                    if (fullfilename.Substring(fullfilename.Length - 4) == ".tpl")
                    {
                        tplScope = new Susan.Scope();
                        tplLexer = new Susan.Parser.Lexer(source, true);
                    }

                    try
                    {
                        if (fullfilename.Substring(fullfilename.Length - 3) == ".mo")
                            mmScope.loadSource(mmLexer.tokenList);
                        if (fullfilename.Substring(fullfilename.Length - 4) == ".tpl")
                            tplScope.loadSource(tplLexer.tokenList);
                    }
                    catch (Exception e)
                    {
                        richTextBox1.Text += e.Message;
                        throw e;
                    }

                    if (fullfilename.Substring(fullfilename.Length - 3) == ".mo")
                        toolStripStatusLabel1.Text = mmLexer.tokenList.Count + " tokens | " + mmLexer.numberOfErrors + " errors";
                    if (fullfilename.Substring(fullfilename.Length - 4) == ".tpl")
                        toolStripStatusLabel1.Text = tplLexer.tokenList.Count + " tokens | " + tplLexer.numberOfErrors + " errors";
                }
            }
            catch (Exception e)
            {
                richTextBox1.Text += e.Message;
                toolStripStatusLabel1.Text = e.Message;
                return;
            }

            treeView1.BeginUpdate();
            if (fullfilename.Substring(fullfilename.Length - 3) == ".mo")
                treeView1.Nodes.AddRange(mmScope.getTreeNodes(constantToolStripMenuItem.Checked, typesToolStripMenuItem.Checked, recordToolStripMenuItem.Checked, uniontypeToolStripMenuItem.Checked, functionToolStripMenuItem.Checked, publicOnlyToolStripMenuItem.Checked, textBox1.Text));
            if (fullfilename.Substring(fullfilename.Length - 4) == ".tpl")
                treeView1.Nodes.AddRange(tplScope.getTreeNodes(textBox1.Text));
            treeView1.Sort();
            treeView1.EndUpdate();

            // call graph
            if (callGraphToolStripMenuItem.Checked)
            {
                if (fullfilename.Substring(fullfilename.Length - 3) == ".mo")
                {
                    foreach (MetaModelica.Package p in mmScope.packages.Values)
                    {
                        try
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
                            process.StartInfo.Arguments = "-Tsvg " + p.name + ".dot -o temp.svg";
                            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            process.Start();
                            process.WaitForExit();

                            System.IO.File.Delete(System.IO.Path.Combine(dataPath, p.name + ".dot"));
                        }
                        catch (Exception e)
                        {
                            toolStripStatusLabel1.Text = e.Message;
                            MessageBox.Show(e.Message);
                        }
                    }
                }

                if (fullfilename.Substring(fullfilename.Length - 4) == ".tpl")
                {
                    foreach (Susan.Package p in tplScope.packages.Values)
                    {
                        try
                        {
                            List<string> unusedFunctions;
                            String dotSource = p.getGraphvizSource(out unusedFunctions);
                            if (unusedFunctions.Count > 0)
                            {
                                if (unusedFunctions.Count > 1)
                                    richTextBox1.Text = unusedFunctions.Count + " top-level templates found:";
                                else
                                    richTextBox1.Text = "1 top-level template found:";

                                foreach (String fnc in unusedFunctions)
                                    richTextBox1.Text += "\n  - " + fnc;
                            }
                            else
                                richTextBox1.Text = "No top-level templates found";

                            System.IO.File.WriteAllText(System.IO.Path.Combine(dataPath, p.name + ".dot"), dotSource);

                            Process process = new Process();
                            process.StartInfo.FileName = System.IO.Path.Combine(tstbGraphvizPath.Text, @"bin\dot");
                            process.StartInfo.WorkingDirectory = dataPath;
                            process.StartInfo.Arguments = "-Tsvg " + p.name + ".dot -o temp.svg";
                            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            process.Start();
                            process.WaitForExit();

                            System.IO.File.Delete(System.IO.Path.Combine(dataPath, p.name + ".dot"));
                        }
                        catch (Exception e)
                        {
                            toolStripStatusLabel1.Text = e.Message;
                            MessageBox.Show(e.Message);
                        }
                    }
                }
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
            updateOutline(false);
            String filename = System.IO.Path.Combine(dataPath, "temp.svg");
            if(System.IO.File.Exists(filename))
                System.Diagnostics.Process.Start(filename);
        }

        private void callGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(callGraphToolStripMenuItem.Checked)
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            updateOutline(false);
        }

        private void latestReleaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/lochel/NppModelica/releases/latest");
        }
    }
}
