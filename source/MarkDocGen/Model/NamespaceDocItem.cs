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


      // TODO PP (2020-08-20): Remove commented code.
      //public override void WriteDocumentation(DocumentationWriter writer)
      //{
      //    writer.WriteHeader();
      //    writer.WritePageTitle(Name, "Namespace");

      //    writer.Write(this, Documentation.GetSummary());

      //    writer.Write("### Remarks", Documentation.GetRemarks(), this);

      //    if (writer.NestedTypeVisibility == NestedTypeVisibility.Namespace
      //        || writer.NestedTypeVisibility == NestedTypeVisibility.Everywhere)
      //    {
      //        writer.WriteChildrenLink<ClassDocItem>("Classes");
      //        writer.WriteChildrenLink<StructDocItem>("Structs");
      //        writer.WriteChildrenLink<InterfaceDocItem>("Interfaces");
      //        writer.WriteChildrenLink<EnumDocItem>("Enums");
      //        writer.WriteChildrenLink<DelegateDocItem>("Delegates");
      //    }
      //    else
      //    {
      //        writer.WriteDirectChildrenLink<ClassDocItem>("Classes");
      //        writer.WriteDirectChildrenLink<StructDocItem>("Structs");
      //        writer.WriteDirectChildrenLink<InterfaceDocItem>("Interfaces");
      //        writer.WriteDirectChildrenLink<EnumDocItem>("Enums");
      //        writer.WriteDirectChildrenLink<DelegateDocItem>("Delegates");
      //    }
      //}
   }
}
