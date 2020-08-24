using System.Xml.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class InterfaceDocItem : TypeDocItem
   {
      public InterfaceDocItem(DocItem parent, ITypeDefinition type, XElement documentation)
          : base(parent, type, documentation)
      { }

      public override DocItemKind Kind => DocItemKind.Interface;

   }
}
