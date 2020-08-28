using System.Xml.Linq;
using ICSharpCode.Decompiler.TypeSystem;

namespace DefaultDocumentation.Model
{
   internal abstract class SymbolDocItem : DocItem
   {
      protected SymbolDocItem(DocItem parent, ISymbol symbol, string id, XElement documentation)
      : base(parent.Project, parent, id, documentation)
      {
         Symbol = symbol;
      }

      public ISymbol Symbol { get; }

   }
}
