using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MetaModelica
{
    public class Import
    {
        public String name;
        public Parser.Position startPosition;

        public Import(List<Parser.Token> tokenList, Int32 i, out Int32 length)
        {
            Int32 startIndex = i;
            this.startPosition = tokenList[i].startPosition;

            if (i < tokenList.Count && tokenList[i].isIDENT("import"))
            {
                i++;
            }
            else
            {
                throw new Exception("IDENT<\"import\"> expected: got " + tokenList[i].ToString());
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

            // HACK for List.mo
            while (i < tokenList.Count && !tokenList[i].isSYMBOL(";"))
            {
                this.name = tokenList[i].value;
                i++;
            }

            if (i < tokenList.Count && tokenList[i].isSYMBOL(";"))
            {
                i++;
            }
            else
            {
                throw new Exception("SYMBOL(\";\") expected: got " + tokenList[i].ToString());
            }

            length = i - startIndex;
        }
    }

    public class Constant
    {
        public String name;
        public Parser.Position startPosition;

        protected const Int32 iConstant = 6;
        protected const Int32 iConstantPublic = 7;

        public Constant(List<Parser.Token> tokenList, Int32 i, out Int32 length)
        {
            Int32 startIndex = i;
            this.startPosition = tokenList[i].startPosition;

            // skip type
            while (i < tokenList.Count && !tokenList[i].isSYMBOL("=") && !tokenList[i].isSYMBOL(";"))
                i++;
            i--;

            if (i < tokenList.Count && tokenList[i].isIDENT())
            {
                this.name = tokenList[i].value;
                i++;
            }
            else
            {
                throw new Exception("IDENT expected: got " + tokenList[i].ToString());
            }

            if (i < tokenList.Count && tokenList[i].isSYMBOL("="))
            {
                i++;

                // skip value
                while (i < tokenList.Count && !tokenList[i].isSYMBOL(";"))
                    i++;
            }

            if (i < tokenList.Count && tokenList[i].isSYMBOL(";"))
            {
                i++;
            }
            else
            {
                if (i < tokenList.Count)
                    throw new Exception("SYMBOL(\";\") expected: got " + tokenList[i].ToString());
                else
                    throw new Exception("SYMBOL(\";\") expected: got EOF");
            }

            length = i - startIndex;
        }

        public TreeNode[] getTreeNodes(Boolean isPublic, string filter)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            filter = filter.ToLower();

            if (name.ToLower().Contains(filter))
            {
                TreeNode cstNode = new TreeNode();
                cstNode.Text = name;
                cstNode.Tag = this;
                cstNode.ImageIndex = isPublic ? iConstantPublic : iConstant;
                cstNode.SelectedImageIndex = isPublic ? iConstantPublic : iConstant;

                nodes.Add(cstNode);
            }

            return nodes.ToArray();
        }
    }

    public class Type
    {
        public String name;
        public Parser.Position startPosition;

        protected const Int32 iType = 8;
        protected const Int32 iTypePublic = 9;

        public Type(List<Parser.Token> tokenList, Int32 i, out Int32 length)
        {
            Int32 startIndex = i;
            this.startPosition = tokenList[i].startPosition;

            if (i < tokenList.Count && tokenList[i].isIDENT("type"))
            {
                i++;
            }
            else
            {
                throw new Exception("IDENT(\"type\") expected: got " + tokenList[i].ToString());
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

            // skip type
            while (i < tokenList.Count && !tokenList[i].isSYMBOL(";"))
                i++;

            if (i < tokenList.Count && tokenList[i].isSYMBOL(";"))
            {
                i++;
            }
            else
            {
                if (i < tokenList.Count)
                    throw new Exception("SYMBOL(\";\") expected: got " + tokenList[i].ToString());
                else
                    throw new Exception("SYMBOL(\";\") expected: got EOF");
            }

            length = i - startIndex;
        }

        public TreeNode[] getTreeNodes(Boolean isPublic, string filter)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            filter = filter.ToLower();

            if (name.ToLower().Contains(filter))
            {
                TreeNode typNode = new TreeNode();
                typNode.Text = name;
                typNode.Tag = this;
                typNode.ImageIndex = isPublic ? iTypePublic : iType;
                typNode.SelectedImageIndex = isPublic ? iTypePublic : iType;

                nodes.Add(typNode);
            }

            return nodes.ToArray();
        }
    }

    public class Record
    {
        public String name;
        public Parser.Position startPosition;
        public String description;

        protected const Int32 iRecord = 10;
        protected const Int32 iRecordPublic = 11;

        public Record(List<Parser.Token> tokenList, Int32 i, out Int32 length)
        {
            Int32 startIndex = i;
            this.startPosition = tokenList[i].startPosition;

            if (i < tokenList.Count && tokenList[i].isIDENT("record"))
            {
                i++;
            }
            else
            {
                throw new Exception("IDENT(\"record\") expected: got " + tokenList[i].ToString());
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

            if (i < tokenList.Count && tokenList[i].isSTRING())
            {
                this.description = tokenList[i].value;
                i++;
            }
            else
                this.description = "";

            // skip type
            while (i + 2 < tokenList.Count && !(tokenList[i].isIDENT("end") && tokenList[i + 1].isIDENT(name) && tokenList[i + 2].isSYMBOL(";")))
                i++;

            if (i + 2 < tokenList.Count && tokenList[i].isIDENT("end") && tokenList[i + 1].isIDENT(name) && tokenList[i + 2].isSYMBOL(";"))
            {
                i += 3;
            }
            else
            {
                throw new Exception("Error in record");
            }

            length = i - startIndex;
        }

        public TreeNode[] getTreeNodes(Boolean isPublic, string filter)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            filter = filter.ToLower();

            if (name.ToLower().Contains(filter))
            {
                TreeNode rcdNode = new TreeNode();
                rcdNode.Text = name;
                rcdNode.Tag = this;
                rcdNode.ImageIndex = isPublic ? iRecordPublic : iRecord;
                rcdNode.SelectedImageIndex = isPublic ? iRecordPublic : iRecord;

                nodes.Add(rcdNode);
            }

            return nodes.ToArray();
        }
    }

    public class Uniontype
    {
        public string name;
        public Parser.Position startPosition;
        public Hashtable records;
        public String description;

        protected const Int32 iUniontype = 4;
        protected const Int32 iUniontypePublic = 5;

        public Uniontype(List<Parser.Token> tokenList, Int32 i, out Int32 length)
        {
            Int32 startIndex = i;
            this.startPosition = tokenList[i].startPosition;
            this.records = new Hashtable();

            if (i < tokenList.Count && tokenList[i].isIDENT("uniontype"))
            {
                i++;
            }
            else
            {
                throw new Exception("IDENT(\"uniontype\") expected: got " + tokenList[i].ToString());
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

            if (i < tokenList.Count && tokenList[i].isSTRING())
            {
                this.description = tokenList[i].value;
                i++;
            }
            else
                this.description = "";

            // skip type
            while (i + 2 < tokenList.Count && !(tokenList[i].isIDENT("end") && tokenList[i + 1].isIDENT(name) && tokenList[i + 2].isSYMBOL(";")))
            {
                try
                {
                    Record rcd = new Record(tokenList, i, out length);
                    records.Add(rcd.name, rcd);
                    i += length;
                }
                catch (Exception e)
                {
                    throw new Exception("Error in uniontype: " + e.Message);
                }
            }

            if (i + 2 < tokenList.Count && tokenList[i].isIDENT("end") && tokenList[i + 1].isIDENT(name) && tokenList[i + 2].isSYMBOL(";"))
            {
                i += 3;
            }
            else
            {
                throw new Exception("Error in uniontype");
            }

            length = i - startIndex;
        }

        public TreeNode[] getTreeNodes(Boolean isPublic, string filter)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            filter = filter.ToLower();

            TreeNode utyNode = new TreeNode();
            utyNode.Text = name;
            utyNode.Tag = this;
            utyNode.ImageIndex = isPublic ? iUniontypePublic : iUniontype;
            utyNode.SelectedImageIndex = isPublic ? iUniontypePublic : iUniontype;

            bool emptyUty = true;
            foreach (MetaModelica.Record rcd in records.Values)
            {
                if (name.ToLower().Contains(filter) || rcd.name.ToLower().Contains(filter))
                {
                    utyNode.Nodes.AddRange(rcd.getTreeNodes(isPublic, ""));
                    emptyUty = false;
                }
            }

            if (name.ToLower().Contains(filter) || !emptyUty)
            {
                nodes.Add(utyNode);
            }

            return nodes.ToArray();
        }
    }

    public class Function
    {
        public String name;
        public String description;
        public Hashtable functionCalls;
        public Parser.Position startPosition;

        protected const Int32 iFunction = 2;
        protected const Int32 iFunctionPublic = 3;

        public Function(List<Parser.Token> tokenList, Int32 i, out Int32 length)
        //string name, Boolean isPublic, string description, string parent, Parser.Position startPosition)
        {
            Int32 startIndex = i;
            this.startPosition = tokenList[i].startPosition;
            this.functionCalls = new Hashtable();

            if (i < tokenList.Count && tokenList[i].isIDENT("function"))
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
                        Boolean exist = functionCalls.Contains(tokenList[i].value + "." + tokenList[i + 2].value);

                        if (exist == false)
                            functionCalls.Add(tokenList[i].value + "." + tokenList[i + 2].value, null);

                        i += 3;
                    }
                    else if (i < tokenList.Count &&
                        tokenList[i].isIDENT())
                    {
                        Boolean exist = functionCalls.Contains(tokenList[i].value);

                        if (exist == false)
                            functionCalls.Add(tokenList[i].value, null);

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

        public TreeNode[] getTreeNodes(Boolean isPublic, string filter)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            filter = filter.ToLower();

            if (name.ToLower().Contains(filter))
            {
                TreeNode fcnNode = new TreeNode();
                fcnNode.Text = name;
                fcnNode.Tag = this;
                fcnNode.ImageIndex = isPublic ? iFunctionPublic : iFunction;
                fcnNode.SelectedImageIndex = isPublic ? iFunctionPublic : iFunction;

                nodes.Add(fcnNode);
            }

            return nodes.ToArray();
        }
    }

    public class Package
    {
        public String name;
        public Boolean isEncapsulated;
        public Boolean isInterface;
        public String description;
        public Parser.Position startPosition;

        public Hashtable publicImports;
        public Hashtable publicConstants;
        public Hashtable publicTypes;
        public Hashtable publicRecords;
        public Hashtable publicUniontypes;
        public Hashtable publicFunctions;
        public Hashtable publicPackages;

        public Hashtable protectedImports;
        public Hashtable protectedConstants;
        public Hashtable protectedTypes;
        public Hashtable protectedRecords;
        public Hashtable protectedUniontypes;
        public Hashtable protectedFunctions;
        public Hashtable protectedPackages;

        protected const Int32 iPackage = 0;
        protected const Int32 iPackagePublic = 1;

        public Package(List<Parser.Token> tokenList, Int32 i, out Int32 length)
        {
            Int32 startIndex = i;
            this.startPosition = tokenList[i].startPosition;
            Boolean isPublic = true;

            publicImports = new Hashtable();
            publicConstants = new Hashtable();
            publicTypes = new Hashtable();
            publicRecords = new Hashtable();
            publicUniontypes = new Hashtable();
            publicFunctions = new Hashtable();
            publicPackages = new Hashtable();

            protectedImports = new Hashtable();
            protectedConstants = new Hashtable();
            protectedTypes = new Hashtable();
            protectedRecords = new Hashtable();
            protectedUniontypes = new Hashtable();
            protectedFunctions = new Hashtable();
            protectedPackages = new Hashtable();

            if (i < tokenList.Count && tokenList[i].isIDENT("encapsulated"))
            {
                isEncapsulated = true;
                isInterface = false;
                i++;
            }
            else if (i < tokenList.Count && tokenList[i].isIDENT("interface"))
            {
                isEncapsulated = false;
                isInterface = true;
                i++;
            }
            else
            {
                isEncapsulated = false;
                isInterface = false;
            }

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
                if (i < tokenList.Count && tokenList[i].isIDENT("public"))
                {
                    isPublic = true;
                    i++;
                }
                if (i < tokenList.Count && tokenList[i].isIDENT("protected"))
                {
                    isPublic = false;
                    i++;
                }
                else if (i < tokenList.Count && tokenList[i].isIDENT("import"))
                {
                    try
                    {
                        Import imp = new Import(tokenList, i, out length);
                        if (isPublic)
                            publicImports.Add(imp.name, imp);
                        else
                            protectedImports.Add(imp.name, imp);
                        i += length;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Package: Error in constant {" + tokenList[i].startPosition + "}: " + e.Message);
                    }
                }
                else if (i < tokenList.Count && tokenList[i].isIDENT("constant"))
                {
                    try
                    {
                        Constant cst = new Constant(tokenList, i, out length);
                        if (isPublic)
                            publicConstants.Add(cst.name, cst);
                        else
                            protectedConstants.Add(cst.name, cst);
                        i += length;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Package: Error in constant {" + tokenList[i].startPosition + "}: " + e.Message);
                    }
                }
                else if (i < tokenList.Count && tokenList[i].isIDENT("type"))
                {
                    try
                    {
                        Type typ = new Type(tokenList, i, out length);
                        if (isPublic)
                            publicTypes.Add(typ.name, typ);
                        else
                            protectedTypes.Add(typ.name, typ);
                        i += length;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Package: Error in type: " + e.Message);
                    }
                }
                else if (i < tokenList.Count && tokenList[i].isIDENT("record"))
                {
                    try
                    {
                        Record rcd = new Record(tokenList, i, out length);
                        if (isPublic)
                            publicRecords.Add(rcd.name, rcd);
                        else
                            protectedRecords.Add(rcd.name, rcd);
                        i += length;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Scope: Error in record: " + e.Message);
                    }
                }
                else if (i < tokenList.Count && tokenList[i].isIDENT("uniontype"))
                {
                    try
                    {
                        Uniontype uty = new Uniontype(tokenList, i, out length);
                        if (isPublic)
                            publicUniontypes.Add(uty.name, uty);
                        else
                            protectedUniontypes.Add(uty.name, uty);
                        i += length;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Scope: Error in uniontype: " + e.Message);
                    }
                }
                else if (i < tokenList.Count && tokenList[i].isIDENT("function"))
                {
                    try
                    {
                        Function fcn = new Function(tokenList, i, out length);
                        if (isPublic)
                            publicFunctions.Add(fcn.name, fcn);
                        else
                            protectedFunctions.Add(fcn.name, fcn);
                        i += length;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Package: Error in function: " + e.Message);
                    }
                }
                else if ((i < tokenList.Count && tokenList[i].isIDENT("package")) ||
                         (i + 1 < tokenList.Count && tokenList[i].isIDENT("encapsulated") && tokenList[i + 1].isIDENT("package")) ||
                         (i + 1 < tokenList.Count && tokenList[i].isIDENT("interface") && tokenList[i + 1].isIDENT("package")))
                {
                    try
                    {
                        Package pcg = new Package(tokenList, i, out length);
                        if (isPublic)
                            publicPackages.Add(pcg.name, pcg);
                        else
                            protectedPackages.Add(pcg.name, pcg);
                        i += length;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Package: Error in package: " + e.Message);
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
            //dotSource += "  graph [splines=ortho]\n";

            dotSource += "labelloc = \"t\"\n";
            dotSource += "label = \"Internal call graph of package " + name + " \\nGenerated with NppModelica (c) 2013-2015, Lennart Ochel.\\n \"\n";


            dotSource += "subgraph cluster_" + name + "\n";
            dotSource += "{\n";
            dotSource += "  label = \"" + name + ".mo\"\n";
            dotSource += "  style=filled\n";
            dotSource += "  color=lightgray\n";
            dotSource += "  node [style=filled, fillcolor=white, shape=box]\n\n";

            foreach (MetaModelica.Function f in publicFunctions.Values)
            {
                dotSource += "  " + name + "_" + f.name + " [label = \"" + f.name + "\", color=red]\n";
            }
            foreach (MetaModelica.Function f in protectedFunctions.Values)
            {
                dotSource += "  " + name + "_" + f.name + " [label = \"" + f.name + "\", color=black]\n";
                functionStats.Add(f.name, 0);
            }
            dotSource += "}\n";

            #region Kanten
            foreach (MetaModelica.Function f in publicFunctions.Values)
            {
                foreach (String fc in f.functionCalls.Keys)
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
            foreach (MetaModelica.Function f in protectedFunctions.Values)
            {
                foreach (String fc in f.functionCalls.Keys)
                {
                    if (fc.Contains("."))
                    {
                        string fc_package = fc.Substring(0, fc.IndexOf('.'));
                        string fc_function = fc.Substring(fc.IndexOf('.') + 1);

                        //dotSource += "  " + name + "_" + f.name + " -> " + fc_package + "_" + fc_function + "\n";
                    }
                    else
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

        public TreeNode[] getTreeNodes(Boolean isPublic, Boolean showConstants, Boolean showTypes, Boolean showRecords, Boolean showUniontypes, Boolean showFunctions, Boolean showPublicOnly, string filter)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            filter = filter.ToLower();

            TreeNode node = new TreeNode();
            node.Name = name;
            node.Text = name;
            node.Tag = this;
            node.ImageIndex = isPublic ? iPackagePublic : iPackage;
            node.SelectedImageIndex = isPublic ? iPackagePublic : iPackage;

            foreach (MetaModelica.Package p in publicPackages.Values)
                node.Nodes.AddRange(p.getTreeNodes(true, showConstants, showTypes, showRecords, showUniontypes, showFunctions, showPublicOnly, filter));

            foreach (MetaModelica.Package p in protectedPackages.Values)
                node.Nodes.AddRange(p.getTreeNodes(false, showConstants, showTypes, showRecords, showUniontypes, showFunctions, showPublicOnly, filter));

            if (showFunctions)
            {
                foreach (MetaModelica.Function fcn in publicFunctions.Values)
                    node.Nodes.AddRange(fcn.getTreeNodes(true, filter));
            }
            if (showFunctions && !showPublicOnly)
            {
                foreach (MetaModelica.Function fcn in protectedFunctions.Values)
                    node.Nodes.AddRange(fcn.getTreeNodes(false, filter));
            }

            if (showTypes)
            {
                foreach (MetaModelica.Type typ in publicTypes.Values)
                    node.Nodes.AddRange(typ.getTreeNodes(true, filter));
            }
            if (showFunctions && !showPublicOnly)
            {
                foreach (MetaModelica.Type typ in protectedTypes.Values)
                    node.Nodes.AddRange(typ.getTreeNodes(false, filter));
            }

            if (showRecords)
            {
                foreach (MetaModelica.Record rcd in publicRecords.Values)
                    node.Nodes.AddRange(rcd.getTreeNodes(true, filter));
            }
            if (showRecords && !showPublicOnly)
            {
                foreach (MetaModelica.Record rcd in protectedRecords.Values)
                    node.Nodes.AddRange(rcd.getTreeNodes(false, filter));
            }

            if (showConstants)
            {
                foreach (MetaModelica.Constant cst in publicConstants.Values)
                    node.Nodes.AddRange(cst.getTreeNodes(true, filter));
            }
            if (showConstants && !showPublicOnly)
            {
                foreach (MetaModelica.Constant cst in protectedConstants.Values)
                    node.Nodes.AddRange(cst.getTreeNodes(false, filter));
            }

            if (showUniontypes)
            {
                foreach (MetaModelica.Uniontype uty in publicUniontypes.Values)
                    node.Nodes.AddRange(uty.getTreeNodes(true, filter));
            }
            if (showUniontypes && !showPublicOnly)
            {
                foreach (MetaModelica.Uniontype uty in protectedUniontypes.Values)
                    node.Nodes.AddRange(uty.getTreeNodes(false, filter));
            }

            node.Expand();
            nodes.Add(node);

            return nodes.ToArray();
        }
    }

    public class Scope
    {
        public Hashtable constants;
        public Hashtable types;
        public Hashtable records;
        public Hashtable uniontypes;
        public Hashtable functions;
        public Hashtable packages;

        public Scope()
        {
            constants = new Hashtable();
            types = new Hashtable();
            records = new Hashtable();
            uniontypes = new Hashtable();
            functions = new Hashtable();
            packages = new Hashtable();
        }

        public void loadSource(List<Parser.Token> tokenList)
        {
            int i = 0;
            int count = 0;

            while (i < tokenList.Count)
            {
                if (i < tokenList.Count && tokenList[i].isIDENT("constant"))
                {
                    try
                    {
                        Constant cst = new Constant(tokenList, i, out count);
                        constants.Add(cst.name, cst);
                        i += count;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Scope: Error in constant: " + e.Message);
                    }
                }
                else if (i < tokenList.Count && tokenList[i].isIDENT("type"))
                {
                    try
                    {
                        Type typ = new Type(tokenList, i, out count);
                        types.Add(typ.name, typ);
                        i += count;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Scope: Error in type: " + e.Message);
                    }
                }
                else if (i < tokenList.Count && tokenList[i].isIDENT("record"))
                {
                    try
                    {
                        Record rcd = new Record(tokenList, i, out count);
                        records.Add(rcd.name, rcd);
                        i += count;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Scope: Error in record: " + e.Message);
                    }
                }
                else if (i < tokenList.Count && tokenList[i].isIDENT("uniontype"))
                {
                    try
                    {
                        Uniontype uty = new Uniontype(tokenList, i, out count);
                        uniontypes.Add(uty.name, uty);
                        i += count;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Scope: Error in uniontype: " + e.Message);
                    }
                }
                else if (i < tokenList.Count && tokenList[i].isIDENT("function"))
                {
                    throw new Exception("function");
                }
                else if ((i < tokenList.Count && tokenList[i].isIDENT("package")) ||
                         (i + 1 < tokenList.Count && tokenList[i].isIDENT("encapsulated") && tokenList[i + 1].isIDENT("package")) ||
                         (i + 1 < tokenList.Count && tokenList[i].isIDENT("interface") && tokenList[i + 1].isIDENT("package")))
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
                foreach (Function f in p.publicFunctions.Values)
                {
                    bool b;
                    do
                    {
                        b = false;
                        foreach (String fc in f.functionCalls.Keys)
                        {
                            if (!fc.Contains(".") && !(p.publicFunctions.Contains(fc) || p.protectedFunctions.Contains(fc)))
                            {
                                f.functionCalls.Remove(fc);
                                b = true;
                                break;
                            }
                        }
                    }
                    while (b);
                }

                foreach (Function f in p.protectedFunctions.Values)
                {
                    bool b;
                    do
                    {
                        b = false;
                        foreach (String fc in f.functionCalls.Keys)
                        {
                            if (!fc.Contains(".") && !(p.publicFunctions.Contains(fc) || p.protectedFunctions.Contains(fc)))
                            {
                                f.functionCalls.Remove(fc);
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

        public TreeNode[] getTreeNodes(Boolean showConstants, Boolean showTypes, Boolean showRecords, Boolean showUniontypes, Boolean showFunctions, Boolean showPublicOnly, string filter)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            filter = filter.ToLower();

            if (showConstants)
            {
                foreach (MetaModelica.Constant cst in constants.Values)
                    nodes.AddRange(cst.getTreeNodes(true, filter));
            }

            if (showTypes)
            {
                foreach (MetaModelica.Type typ in types.Values)
                    nodes.AddRange(typ.getTreeNodes(true, filter));
            }

            if (showRecords)
            {
                foreach (MetaModelica.Record rcd in records.Values)
                    nodes.AddRange(rcd.getTreeNodes(true, filter));
            }

            if (showUniontypes)
            {
                foreach (MetaModelica.Uniontype uty in uniontypes.Values)
                    nodes.AddRange(uty.getTreeNodes(true, filter));
            }

            foreach (MetaModelica.Package p in packages.Values)
                nodes.AddRange(p.getTreeNodes(true, showConstants, showTypes, showRecords, showUniontypes, showFunctions, showPublicOnly, filter));

            return nodes.ToArray();
        }
    }
}
