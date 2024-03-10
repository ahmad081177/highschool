using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace HighSchoolClass
{

    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            DocumentView dv = await VS.Documents.GetActiveDocumentViewAsync();

            string allTxt = dv.TextBuffer.CurrentSnapshot.GetText();

            var cd = ExtractInfo(allTxt);
            if (cd != null)
            {
                CreateConstructors(cd, dv);
                CreateGetAndSet(cd.PrivateMembers, dv);

                await VS.Commands.ExecuteAsync(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.FORMATDOCUMENT);

                dv.Document.Save();
            }
        }

        private void CreateGetAndSet(List<Tuple<string, string>> privateMembers, DocumentView dv)
        {
            string methods = "\n//====== Get & Set =========\n";
            foreach (var member in privateMembers)
            {
                string ptype = member.Item1;
                string pname = member.Item2;
                if (pname.Length == 1) pname = char.ToUpper(pname[0]).ToString();
                else pname = char.ToUpper(pname[0]) + pname.Substring(1);

                methods += $"public void Set{pname}({ptype} {member.Item2})\n";
                methods += "{\n";
                methods += $"\tthis.{member.Item2} = {member.Item2};\n";
                methods += "}\n";

                methods += $"public {ptype} Get{pname}()";
                methods += "{";
                methods += $"return this.{member.Item2};";
                methods += "}\n";
            }
            methods += "\n";
            dv.TextBuffer.Insert(dv.TextView.Selection.End.Position.Position, methods);
        }

        private void CreateConstructors(ClassData cd, DocumentView dv)
        {
            string ctor = "// === CTORs === \n";
            //default CTOR
            ctor += $"public {cd.ClassName} ()\n";
            ctor += "{\n";
            ctor += "}\n";
            //CTOR with all params
            ctor += $"public {cd.ClassName} (";
            foreach (var member in cd.PrivateMembers)
            {
                string ptype = member.Item1;
                string pname = member.Item2;

                ctor += $"{ptype} {pname},";
            }
            //remove last ,
            ctor = ctor.Remove(ctor.Length - 1);
            ctor += ")\n";
            ctor += "{\n";
            foreach (var member in cd.PrivateMembers)
            {
                string ptype = member.Item1;
                string pname = member.Item2;

                ctor += $"this.{pname} = {pname};\n";
            }
            ctor += "}\n";

            dv.TextBuffer.Insert(dv.TextView.Selection.End.Position.Position, ctor);
        }
        private ClassData ExtractInfo(string allTxt)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters()
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };
            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, allTxt);
            if (results != null && results.Errors.HasErrors)
            {
                VS.MessageBox.ShowError("Errors", "Your code has errors, fix them first!");
                return null;
            }
            var assembly = results.CompiledAssembly;
            var types = assembly.GetTypes();
            if (types.Length == 0)
            {
                return null;
            }
            ClassData cd = new ClassData();
            var tp = types[0];
            cd.ClassName = tp.Name;
            var fs = ((System.Reflection.TypeInfo)tp).DeclaredFields;
            foreach (var f in fs)
            {
                if (f.IsPrivate)
                {
                    cd.PrivateMembers.Add(new Tuple<string, string>(f.FieldType.Name, f.Name));
                }
            }
            return cd;
        }
    }
    internal class ClassData
    {
        public string ClassName { get; set; }
        public List<Tuple<string, string>> PrivateMembers = new List<Tuple<string, string>>();
    }
}
