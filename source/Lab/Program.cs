using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab
{
   class Program
   {
      static void Main(string[] args)
      {
         var decompiler = new CSharpDecompiler(@"D:\Git\DefaultDocumentation\source\Dummy\bin\Debug\netstandard2.0\Dummy.dll", new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
         var typeSys = decompiler.TypeSystem.MainModule.TypeDefinitions.OfType<ITypeDefinition>().Skip(1).First();         
         Console.WriteLine(typeSys.Name);
      }
   }
}
