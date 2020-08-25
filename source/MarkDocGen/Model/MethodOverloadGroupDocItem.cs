using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ICSharpCode.Decompiler.Documentation;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal abstract class OverloadGroupDocItem : DocItem
   {
      public OverloadGroupDocItem(DocItem parent, string fullName, string name, XElement documentation)
         : base(parent.Project, parent, "O:" + fullName, documentation)
      {
      }
   }

   internal abstract class MethodBaseOverloadGroupDocItem : OverloadGroupDocItem
   {
      public MethodBaseOverloadGroupDocItem(DocItem parent, string fullName, string name, XElement documentation)          
         : base(parent, fullName, name, documentation)
      {
      }

      public abstract IEnumerable<MethodBaseDocItem> Members { get; }
   }

   internal sealed class MethodOverloadGroupDocItem : MethodBaseOverloadGroupDocItem
   {
      public MethodOverloadGroupDocItem(DocItem parent, string fullName, string name, XElement documentation)
         : base(parent, fullName, name, documentation)
      {
      }

      public override DocItemKind Kind => DocItemKind.MethodOverloadGroup;

      public IEnumerable<MethodDocItem> Methods => Children.Cast<MethodDocItem>().OrderBy(m => m.Name).ThenBy(m => m.Parameters.Length);
      public override IEnumerable<MethodBaseDocItem> Members => Methods;
   }

   internal sealed class OperatorOverloadGroupDocItem : MethodBaseOverloadGroupDocItem
   {
      public OperatorOverloadGroupDocItem(DocItem parent, string fullName, string name, XElement documentation)
         : base(parent, fullName, name, documentation)
      {
      }

      public override DocItemKind Kind => DocItemKind.OperatorOverloadGroup;

      public IEnumerable<OperatorDocItem> Operators => Children.Cast<OperatorDocItem>().OrderBy(m => m.Name).ThenBy(m => m.Parameters.Length);
      public override IEnumerable<MethodBaseDocItem> Members => Operators;
   }

   internal sealed class ConstructorOverloadGroupDocItem : MethodBaseOverloadGroupDocItem
   {
      public ConstructorOverloadGroupDocItem(DocItem parent, string fullName, string name, XElement documentation)
         : base(parent, fullName, name, documentation)
      {
      }

      public override DocItemKind Kind => DocItemKind.ConstructorOverloadGroup;

      public IEnumerable<ConstructorDocItem> Constructors => Children.Cast<ConstructorDocItem>().OrderBy(c => c.Parameters.Length);
      public override IEnumerable<MethodBaseDocItem> Members => Constructors;
   }

   internal sealed class PropertyOverloadGroupDocItem : OverloadGroupDocItem
   {
      public PropertyOverloadGroupDocItem(DocItem parent, string fullName, string name, XElement documentation)
       : base(parent, fullName, name, documentation)
      {
      }

      public override DocItemKind Kind => DocItemKind.PropertyOverloadGroup;

      public IEnumerable<PropertyDocItem> Properties => Children.Cast<PropertyDocItem>().OrderBy(p => p.Name);      
   }
}
