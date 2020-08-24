using System.Xml.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class ClassDocItem : TypeDocItem
   {
      public ClassDocItem(DocItem parent, ITypeDefinition type, XElement documentation)
          : base(parent, type, documentation)
      { }

      public override DocItemKind Kind => DocItemKind.Class;

   }
}
