using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ICSharpCode.Decompiler.Documentation;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class MethodOverloadGroupDocItem : DocItem
   {
      public MethodOverloadGroupDocItem(DocItem parent, string fullName, string name, XElement documentation)
         : base(parent.Project, parent, "O:" + fullName, name, fullName, documentation)
      {
      }

      public override DocItemKind Kind => DocItemKind.MethodOverloadGroup;

      public IEnumerable<MethodBaseDocItem> Methods => Project.GetChildren(this).OfType<MethodBaseDocItem>();
   }
}
