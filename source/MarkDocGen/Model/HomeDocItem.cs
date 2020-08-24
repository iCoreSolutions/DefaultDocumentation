using System.Xml.Linq;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class HomeDocItem : DocItem
   {
      private readonly bool _explicitGenerate;

      public bool HasMultipleNamespaces { get; set; }

      public HomeDocItem(DocProject project, string pageName, string name, XElement documentation)
          : base(project, null, string.Empty, pageName ?? "index", name, documentation)
      {
         _explicitGenerate = !string.IsNullOrEmpty(pageName) || documentation != null;
      }

      public override DocItemKind Kind => DocItemKind.Home;


      // TODO PP (2020-08-20): Remove commented code.
      //public override bool GeneratePage => _explicitGenerate || HasMultipleNamespaces;

      //public override void WriteDocumentation(DocumentationWriter writer)
      //{
      //    writer.WriteHeader();

      //    writer.Write(this, Documentation.GetSummary());

      //    writer.Write("### Remarks", Documentation.GetRemarks(), this);

      //    writer.WriteChildrenLink<NamespaceDocItem>("Namespaces");
      //}

      //public override string GetLink(FileNameMode fileNameMode) => FullName.Clean();
   }
}
