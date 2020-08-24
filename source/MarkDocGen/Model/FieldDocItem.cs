using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class FieldDocItem : DocItem
   {
      private static readonly CSharpAmbience CodeAmbience = new CSharpAmbience
      {
         ConversionFlags =
              ConversionFlags.ShowAccessibility
              | ConversionFlags.ShowBody
              | ConversionFlags.ShowDefinitionKeyword
              | ConversionFlags.ShowModifiers
      };

      public IField Field { get; }

      public FieldDocItem(TypeDocItem parent, IField field, XElement documentation)
          : base(parent, field, documentation)
      {
         Field = field;
      }

      public override DocItemKind Kind => DocItemKind.Field;

      // TODO PP (2020-08-20): Remove commented code.
      //public override void WriteDocumentation(DocumentationWriter writer)
      //{
      //    writer.WriteHeader();
      //    writer.WritePageTitle($"{Parent.Name}.{Name}", "Field");

      //    writer.Write(this, Documentation.GetSummary());

      //    writer.WriteLine("```csharp");
      //    writer.WriteLine(CodeAmbience.ConvertSymbol(Field));
      //    writer.WriteLine("```");
      //    // todo attributes

      //    writer.WriteLine("#### Field Value");
      //    writer.WriteLine($"{writer.GetTypeLink(Field.Type)}  ");

      //    writer.Write("### Example", Documentation.GetExample(), this);
      //    writer.Write("### Remarks", Documentation.GetRemarks(), this);
      //}
   }
}
