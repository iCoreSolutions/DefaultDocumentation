using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class EnumDocItem : TypeDocItem
   {
      private static readonly CSharpAmbience CodeAmbience = new CSharpAmbience
      {
         ConversionFlags =
              ConversionFlags.ShowAccessibility
              | ConversionFlags.ShowDeclaringType
              | ConversionFlags.ShowDefinitionKeyword
              | ConversionFlags.ShowModifiers
              | ConversionFlags.ShowTypeParameterList
              | ConversionFlags.ShowTypeParameterVarianceModifier
      };

      public EnumDocItem(DocItem parent, ITypeDefinition type, XElement documentation)
          : base(parent, type, documentation)
      { }

      public override DocItemKind Kind => DocItemKind.Enum;

      public new IEnumerable<EnumFieldDocItem> Fields => base.Fields.Cast<EnumFieldDocItem>();
   }
}
