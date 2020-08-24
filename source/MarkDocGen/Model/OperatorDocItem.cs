using System.Linq;
using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class OperatorDocItem : MethodBaseDocItem, IParameterizedDocItem
   {
      private static readonly CSharpAmbience CodeAmbience = new CSharpAmbience
      {
         ConversionFlags =
              ConversionFlags.ShowAccessibility
              | ConversionFlags.ShowBody
              | ConversionFlags.ShowModifiers
              | ConversionFlags.ShowParameterDefaultValues
              | ConversionFlags.ShowParameterList
              | ConversionFlags.ShowParameterModifiers
              | ConversionFlags.ShowParameterNames
              | ConversionFlags.ShowReturnType
              | ConversionFlags.ShowTypeParameterList
              | ConversionFlags.ShowTypeParameterVarianceModifier
              | ConversionFlags.UseFullyQualifiedTypeNames
      };

      public override DocItemKind Kind => DocItemKind.Operator;

      public OperatorDocItem(DocItem parent, IMethod method, XElement documentation)
          : base(parent, method, documentation)
      {
      }

      //public override void WriteDocumentation(DocumentationWriter writer)
      //{
      //   writer.WriteHeader();
      //   writer.WritePageTitle($"{Parent.Name}.{Name}", "Operator");

      //   writer.Write(this, Documentation.GetSummary());

      //   writer.WriteLine("```csharp");
      //   writer.WriteLine(CodeAmbience.ConvertSymbol(Method));
      //   writer.WriteLine("```");

      //   // attributes

      //   writer.WriteDocItems(Parameters, "#### Parameters");

      //   if (Method.ReturnType.Kind != TypeKind.Void)
      //   {
      //      writer.WriteLine("#### Returns");
      //      writer.WriteLine(writer.GetTypeLink(Method.ReturnType) + "  ");
      //      writer.Write(this, Documentation.GetReturns());
      //   }

      //   writer.WriteExceptions(this);

      //   writer.Write("### Example", Documentation.GetExample(), this);
      //   writer.Write("### Remarks", Documentation.GetRemarks(), this);
      //}
   }
}
