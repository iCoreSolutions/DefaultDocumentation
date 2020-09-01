﻿using System.Linq;
using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class DelegateDocItem : TypeDocItem, IParameterizedDocItem, IReturnTypeDocItem
   {
      public IMethod InvokeMethod { get; }

      public IType ReturnType => InvokeMethod?.ReturnType;

      public override DocItemKind Kind => DocItemKind.Delegate;

      public ParameterDocItem[] Parameters { get; }

      public DelegateDocItem(DocItem parent, ITypeDefinition type, XElement documentation)
          : base(parent, type, documentation)
      {
         InvokeMethod = type.GetDelegateInvokeMethod();
         Parameters = InvokeMethod.Parameters.Select(p => new ParameterDocItem(this, p, documentation)).ToArray();
      }

      //public override void WriteDocumentation(DocumentationWriter writer)
      //{
      //    writer.WriteHeader();
      //    writer.WritePageTitle(Name, Type.Kind.ToString());

      //    writer.Write(this, Documentation.GetSummary());

      //    writer.WriteLine("```csharp");
      //    writer.WriteLine(CodeAmbience.ConvertSymbol(Type));
      //    writer.WriteLine("```");

      //    // attribute

      //    writer.WriteDocItems(TypeParameters, "#### Type parameters");
      //    writer.WriteDocItems(Parameters, "#### Parameters");

      //    if (InvokeMethod.ReturnType.Kind != TypeKind.Void)
      //    {
      //        writer.WriteLine("#### Returns");
      //        writer.WriteLine(writer.GetTypeLink(InvokeMethod.ReturnType) + "  ");
      //        writer.Write(this, Documentation.GetReturns());
      //    }

      //    writer.Write("### Example", Documentation.GetExample(), this);
      //    writer.Write("### Remarks", Documentation.GetRemarks(), this);
      //}
   }
}
