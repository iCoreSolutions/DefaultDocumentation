using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal class FieldDocItem : MemberDocItem
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

      public string Name => Field.Name;

      public FieldDocItem(TypeDocItem parent, IField field, XElement documentation)
          : base(parent, field, documentation)
      {
         Field = field;
      }

      public override DocItemKind Kind => DocItemKind.Field;

   }
   
}
