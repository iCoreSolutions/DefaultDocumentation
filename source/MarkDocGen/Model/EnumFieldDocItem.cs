using System.Xml.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class EnumFieldDocItem : DocItem
   {
      public IField Field { get; }

      public EnumFieldDocItem(EnumDocItem parent, IField field, XElement documentation)
          : base(parent, field, documentation)
      {
         Field = field;         
      }

      public override DocItemKind Kind => DocItemKind.EnumField;

      // TODO PP (2020-08-20): Remove commented code.
      //public override bool GeneratePage => false;

      // TODO PP (2020-08-20): Remove commented code.
      //public override void WriteDocumentation(DocumentationWriter writer)
      //{
      //    writer.WriteLinkTarget(this);
      //  writer.WriteLine($"`{Name}` {Field.GetConstantValue()}  ");

      //    writer.Write(this, Documentation.GetSummary());

      //    writer.Write("### Example", Documentation.GetExample(), this);
      //    writer.Write("### Remarks", Documentation.GetRemarks(), this);
      //}
   }
}
