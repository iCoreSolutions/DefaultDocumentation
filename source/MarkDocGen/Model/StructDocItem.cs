using System.Xml.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class StructDocItem : TypeDocItem
   {
      public StructDocItem(DocItem parent, ITypeDefinition type, XElement documentation)
          : base(parent, type, documentation)
      { }

      public override DocItemKind Kind => DocItemKind.Struct;

   }
}
