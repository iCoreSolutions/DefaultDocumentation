using MarkDocGen;
using System.Xml.Linq;

namespace DefaultDocumentation.Model
{
   internal sealed class NamespaceDocItem : DocItem
   {
      public NamespaceDocItem(AssemblyDocItem parent, string name, XElement documentation)
          : base(parent.Project, parent, $"N:{name}", documentation)
      {
         Name = name;
      }

      public override DocItemKind Kind => DocItemKind.Namespace;

      public string Name { get; }
   }
}
