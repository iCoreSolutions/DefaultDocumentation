﻿using System.Linq;
using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class PropertyDocItem : MemberDocItem, IParameterizedDocItem
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
              | ConversionFlags.UseFullyQualifiedTypeNames
      };

      public IProperty Property { get; }

      public ParameterDocItem[] Parameters { get; }

      public PropertyDocItem(DocItem parent, IProperty property, XElement documentation)
          : base(parent, property, documentation)
      {
         Property = property;
         Parameters = Property.Parameters.Select(p => new ParameterDocItem(this, p, documentation)).ToArray();
      }

      public override DocItemKind Kind => DocItemKind.Property;

      public string Name => Property.Name;

      // TODO PP (2020-08-20): Remove commented code.
      //public override void WriteDocumentation(DocumentationWriter writer)
      //{
      //   writer.WriteHeader();
      //   writer.WritePageTitle($"{Parent.Name}.{Name}", "Property");

      //   writer.Write(this, Documentation.GetSummary());

      //   writer.WriteLine("```csharp");
      //   writer.WriteLine(CodeAmbience.ConvertSymbol(Property));
      //   writer.WriteLine("```");

      //   // attributes

      //   writer.WriteDocItems(Parameters, "#### Parameters");

      //   if (Property.ReturnType.Kind != TypeKind.Void)
      //   {
      //      writer.WriteLine("#### Property Value");
      //      writer.WriteLine(writer.GetTypeLink(Property.ReturnType) + "  ");
      //      writer.Write(this, Documentation.GetValue());
      //   }

      //   writer.WriteExceptions(this);

      //   writer.Write("### Example", Documentation.GetExample(), this);
      //   writer.Write("### Remarks", Documentation.GetRemarks(), this);
      //}
   }
}
