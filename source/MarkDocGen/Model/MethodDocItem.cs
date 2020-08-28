using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DefaultDocumentation.Helper;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Documentation;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class MethodDocItem : MethodBaseDocItem, ITypeParameterizedDocItem, IParameterizedDocItem
   {
      public MethodDocItem(DocItem parent, IMethod method, XElement documentation)
          : base(parent, method, documentation)
      {         
         TypeParameters = method.TypeParameters.Select(p => new TypeParameterDocItem(this, p, documentation)).ToArray();
      }

      public override DocItemKind Kind => DocItemKind.Method;

      public TypeParameterDocItem[] TypeParameters { get; }

   }
}
