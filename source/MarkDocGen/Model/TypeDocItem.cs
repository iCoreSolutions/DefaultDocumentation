using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal abstract class TypeDocItem : EntityDocItem, ITypeParameterizedDocItem
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

      private static readonly CSharpAmbience BaseTypeAmbience = new CSharpAmbience
      {
         ConversionFlags = ConversionFlags.ShowTypeParameterList
      };

      protected TypeDocItem(DocItem parent, ITypeDefinition type, XElement documentation)
         : base(parent, type, documentation)
      {
         Type = type;
         TypeParameters = Type.TypeParameters.Select(p => new TypeParameterDocItem(this, p, documentation)).ToArray();
      }

      public ITypeDefinition Type { get; }

      public string Name => Type.Name;
      
      // TODO PP (2020-08-20): Make IReadOnlyList
      public TypeParameterDocItem[] TypeParameters { get; }

      public IEnumerable<PropertyDocItem> Properties => Children.OfType<PropertyDocItem>().OrderBy(property => property.Name);

      public virtual IEnumerable<FieldDocItem> Fields => Children.OfType<FieldDocItem>().OrderBy(item => item.Name);


   }
}
