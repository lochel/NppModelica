using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Susan
{
    public class Template
    {
        public String name;
        public String description;
        public Hashtable templateCalls;
        public Parser.Position startPosition;

        public Template(List<Parser.Token> tokenList, Int32 i, out Int32 length)
        //string name, Boolean isPublic, string description, string parent, Parser.Position startPosition)
        {
            Int32 startIndex = i;
            this.startPosition = tokenList[i].startPosition;
            this.templateCalls = new Hashtable();

            if (i < tokenList.Count && tokenList[i].isIDENT("template"))
            {
                i++;
            }
            else
            {
                throw new Exception("IDENT(\"function\") expected: got " + tokenList[i].ToString());
            }

            if (i < tokenList.Count && tokenList[i].isIDENT())
            {
                this.name = tokenList[i].value;
                i++;
            }
            else
            {
                throw new Exception("IDENT expected: got " + tokenList[i].ToString());
            }

            // HACK to parse "function abc = xyz;"
            if (i < tokenList.Count && tokenList[i].isSYMBOL("="))
            {
                i++;
                while (i < tokenList.Count && !tokenList[i].isSYMBOL(";"))
                {
                    i++;
                }
                i += 1;
            }
            else
            {
                while (i + 2 < tokenList.Count && !(tokenList[i].isIDENT("end") && tokenList[i + 1].isIDENT(name) && tokenList[i + 2].isSYMBOL(";")))
                {
                    #region function call
                    if (i < tokenList.Count - 2 &&
                                tokenList[i].isIDENT() &&
                                tokenList[i + 1].isSYMBOL(".") &&
                                tokenList[i + 2].isIDENT())
                    {
                        Boolean exist = templateCalls.Contains(tokenList[i].value + "." + tokenList[i + 2].value);

                        if (exist == false)
                            templateCalls.Add(tokenList[i].value + "." + tokenList[i + 2].value, null);

                        i += 3;
                    }
                    else if (i < tokenList.Count &&
                        tokenList[i].isIDENT())
                    {
                        Boolean exist = templateCalls.Contains(tokenList[i].value);

                        if (exist == false)
                            templateCalls.Add(tokenList[i].value, null);

                        i++;
                    }
                    #endregion
                    else
                        i++;
                }
                i += 3;
            }

            length = i - startIndex;
        }
    }

    public class Package
    {
        public String name;
        public String description;
        public Parser.Position startPosition;

        public Hashtable templates;

        public Package(List<Parser.Token> tokenList, Int32 i, out Int32 length)
        {
            Int32 startIndex = i;
            this.startPosition = tokenList[i].startPosition;

            templates = new Hashtable();

            if (i < tokenList.Count && tokenList[i].isIDENT("package"))
                i++;
            else
                throw new Exception("expected: IDENT(\"package\"), got: " + tokenList[i].ToString());

            if (i < tokenList.Count && tokenList[i].isIDENT())
            {
                name = tokenList[i].value;
                i++;
            }
            else
                throw new Exception("expected: IDENT, got: " + tokenList[i].ToString());

            if (i < tokenList.Count && tokenList[i].isSTRING())
            {
                description = tokenList[i].value;
                i++;
            }
            else
                description = "";

            while (i + 2 < tokenList.Count && !(tokenList[i].isIDENT("end") && tokenList[i + 1].isIDENT(name) && tokenList[i + 2].isSYMBOL(";")))
            {
                if (i < tokenList.Count && tokenList[i].isIDENT("template"))
                {
                    try
                    {
                        Template tpl = new Template(tokenList, i, out length);
                        templates.Add(tpl.name, tpl);
                        i += length;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Package: Error in template: " + e.Message);
                    }
                }
                else
                {
                    //throw new Exception("Package: Unexpected token: " + tokenList[i].ToString());
                    i++;
                }
            }

            if (i < tokenList.Count && tokenList[i].isIDENT("end"))
                i++;
            else
                throw new Exception("expected: IDENT(\"end\"), got: " + tokenList[i].ToString());

            if (i < tokenList.Count && tokenList[i].isIDENT(name))
                i++;
            else
                throw new Exception("expected: IDENT(\"" + name + "\"), got: " + tokenList[i].ToString());

            if (i < tokenList.Count && tokenList[i].isSYMBOL(";"))
                i++;
            else
                throw new Exception("expected: SYMBOL(\";\"), got: " + tokenList[i].ToString());

            length = i - startIndex;
        }

        public string getGraphvizSource(out List<string> unusedProtectedFunctions)
        {
            Dictionary<string, int> functionStats = new Dictionary<string, int>();

            string dotSource = "digraph G\n";
            dotSource += "{\n";

            dotSource += "labelloc = \"t\"\n";
            dotSource += "label = \"Internal call graph of package " + name + " \\nGenerated with NppModelica (c) 2013-2015, Lennart Ochel.\\n \"\n";


            dotSource += "subgraph cluster_" + name + "\n";
            dotSource += "{\n";
            dotSource += "  label = \"" + name + ".tpl\"\n";
            dotSource += "  style=filled\n";
            dotSource += "  color=lightgray\n";
            dotSource += "  node [style=filled, fillcolor=white, shape=box]\n\n";

            foreach (Template tpl in templates.Values)
            {
                dotSource += "  " + name + "_" + tpl.name + " [label = \"" + tpl.name + "\", color=red]\n";
                functionStats.Add(tpl.name, 0);
            }
            dotSource += "}\n";

            #region Kanten
            foreach (Susan.Template f in templates.Values)
            {
                foreach (String fc in f.templateCalls.Keys)
                {
                    if (!fc.Contains("."))
                    {
                        dotSource += "  " + name + "_" + f.name + " -> " + name + "_" + fc + "\n";
                        if (functionStats.ContainsKey(fc) && fc != f.name)
                        {
                            functionStats[fc] += 1;
                        }
                    }
                }
            }
            #endregion

            dotSource += "}\n";

            unusedProtectedFunctions = new List<string>();
            unusedProtectedFunctions.Clear();
            foreach (String s in functionStats.Keys)
            {
                if (functionStats[s] == 0)
                    unusedProtectedFunctions.Add(s);
            }

            return dotSource;
        }
    }

    public class Scope
    {
        public Hashtable packages;

        protected const Int32 iPackagePublic = 1;
        protected const Int32 iTemplatePublic = 9;

        public Scope()
        {
            packages = new Hashtable();
        }

        public void loadSource(List<Parser.Token> tokenList)
        {
            int i = 0;
            int count = 0;

            while (i < tokenList.Count)
            {
                if ((i < tokenList.Count && tokenList[i].isIDENT("package")) ||
                         (i + 1 < tokenList.Count && tokenList[i].isIDENT("encapsulated") && tokenList[i + 1].isIDENT("package")))
                {
                    try
                    {
                        Package pcg = new Package(tokenList, i, out count);
                        packages.Add(pcg.name, pcg);
                        i += count;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Scope: Error in package: " + e.Message);
                    }
                }
                else
                {
                    throw new Exception("Scope: Unexpected token: " + tokenList[i].ToString());
                }
            }

            #region remove invalid function calls
            foreach (Package p in packages.Values)
            {
                foreach (Template tpl in p.templates.Values)
                {
                    bool b;
                    do
                    {
                        b = false;
                        foreach (String fc in tpl.templateCalls.Keys)
                        {
                            if (!fc.Contains(".") && !p.templates.Contains(fc))
                            {
                                tpl.templateCalls.Remove(fc);
                                b = true;
                                break;
                            }
                        }
                    }
                    while (b);
                }
            }
            #endregion
        }

        public TreeNode[] getTreeNodes(string filter)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            filter = filter.ToLower();

            foreach (Package p in packages.Values)
            {
                TreeNode node = new TreeNode();
                node.Name = p.name;
                node.Text = p.name;
                node.Tag = p;
                node.ImageIndex = iPackagePublic;
                node.SelectedImageIndex = iPackagePublic;

                foreach (Template tpl in p.templates.Values)
                {
                    if (tpl.name.ToLower().Contains(filter))
                    {
                        TreeNode tplNode = new TreeNode();
                        tplNode.Text = tpl.name;
                        tplNode.Tag = tpl;
                        tplNode.ImageIndex = iTemplatePublic;
                        tplNode.SelectedImageIndex = iTemplatePublic;

                        node.Nodes.Add(tplNode);
                    }
                }

                node.Expand();
                nodes.Add(node);
            }

            return nodes.ToArray();
        }
    }
}
