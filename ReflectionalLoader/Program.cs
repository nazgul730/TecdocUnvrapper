using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReflectionalLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            CompilerParameters cp = new CompilerParameters();
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            FileInfo file = new FileInfo("C:\\Users\\Vitia\\source\\repos\\TecDocProcessor\\TecDocDataStructureAnalizer\\Models\\TableFieldsDescriber.cs");

            cp.GenerateExecutable = false;
            cp.GenerateInMemory = false;
            cp.TreatWarningsAsErrors = false;
            cp.OutputAssembly = "TecDocStructureFileAnilizer.dll";
            cp.ReferencedAssemblies.AddRange(new string[] { "System.dll", "System.Core.dll" });

            CompilerResults cr = provider.CompileAssemblyFromFile(cp,
                    file.FullName);
            int c = 0;

            Assembly asm = Assembly.LoadFrom("TecDocStructureFileAnilizer.dll");

            Type[] types = asm.GetTypes();
            foreach (Type t in types)
            {
                Console.WriteLine(t.Name);
            }
            Console.ReadLine();
        }
    }
}
