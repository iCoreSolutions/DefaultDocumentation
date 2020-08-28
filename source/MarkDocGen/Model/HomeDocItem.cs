using System.Xml.Linq;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class HomeDocItem : DocItem
   {
      private readonly bool _explicitGenerate;

      public bool HasMultipleNamespaces { get; set; }

      public HomeDocItem(DocProject project, string pageName, string name, XElement documentation)
          : base(project, null, "Home", documentation)
      {
         _explicitGenerate = !string.IsNullOrEmpty(pageName) || documentation != null;
      }

      public override DocItemKind Kind => DocItemKind.Home;
   }
}
