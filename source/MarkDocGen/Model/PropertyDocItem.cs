using System.Linq;
using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class PropertyDocItem : MemberDocItem, IParameterizedDocItem
   {
      public PropertyDocItem(DocItem parent, IProperty property, XElement documentation)
         : base(parent, property, documentation)
      {
         Property = property;
         Parameters = Property.Parameters.Select(p => new ParameterDocItem(this, p, documentation)).ToArray();
      }


      public IProperty Property { get; }

      public ParameterDocItem[] Parameters { get; }

      public override DocItemKind Kind => DocItemKind.Property;

      public string Name => Property.Name;

      public bool IsOverloaded => Parent is OverloadGroupDocItem;
   }
}
