using System.Linq;
using System.Xml.Linq;
using DefaultDocumentation.Helper;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class ParameterDocItem : SymbolDocItem
   {
      public IParameter Parameter { get; }

      public ParameterDocItem(DocItem parent, IParameter entity, XElement documentation)
          : base(parent, entity, parent.Id + "." + entity.Name, documentation.GetParameters()?.FirstOrDefault(d => d.GetName() == entity.Name))
      {
         Parameter = entity;
      }

      public override DocItemKind Kind => DocItemKind.Parameter;

      public string Name => Parameter.Name;
   }
}
