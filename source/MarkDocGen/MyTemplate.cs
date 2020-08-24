using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DefaultDocumentation.Helper;
using DefaultDocumentation.Model;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;

namespace MarkDocGen
{



   /* Having a single class per file might be nice... also to have partials that are files... 
    */
   // TODO PP (2020-08-23): Rename to something sensible... or ... yeah..
   // TODO PP (2020-08-23): Skip this for now.
   interface ITemplate
   {
      bool GeneratesPage(DocItem item);
   }

   /// <summary>
   /// <code language="csharp">This is some code </code>
   /// </summary>
   class MyTemplate2 : ITemplate
   {
      // TODO PP (2020-08-23): Allow to return extension as well...
      public virtual bool GeneratesPage(DocItem item)
      {
         return item is TypeDocItem || item is MethodOverloadGroupDocItem;
      }

      public virtual void RenderPage(RenderingContext context, TextWriter writer)
      {
         // TODO PP (2020-08-24): Add namespace (top level item)
         switch (context.CurrentItem)
         {
            case TypeDocItem classDocItem:
               RenderType(context, classDocItem, writer);
               break;

            case MethodOverloadGroupDocItem mogd:
               RenderMethods(context, mogd, writer);
               break;
         }
      }

      private void WriteHeader(RenderingContext context, DocItem docItem, TextWriter writer)
      {
      }

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

      private static readonly CSharpAmbience MethodCodeAmbience = new CSharpAmbience
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

      private static readonly CSharpAmbience BaseTypeAmbience = new CSharpAmbience
      {
         ConversionFlags = ConversionFlags.ShowTypeParameterList
      };

      private string RenderXmlDoc(RenderingContext context, XElement element)
      {
         if (element == null || element.IsEmpty)
            return null;

         var text = String.Join('\n', context.Generator.RenderNodes(context, element.Nodes()).Split('\n').Select(line => line.Trim()));
         return text;

      }

      private void RenderMethods(RenderingContext context, MethodOverloadGroupDocItem item, TextWriter writer)
      {
         if (item.Methods.OfType<MethodDocItem>().Count() == 1)
         {
            RenderMethod(context, item.Methods.OfType<MethodDocItem>().First(), writer);
         }
         else if (item.Methods.OfType<MethodDocItem>().Count() > 1)
         {
            // Render summary
            writer.Write(RenderXmlDoc(context, item.Documentation.GetSummary()));
            
            writer.WriteLine();            
            writer.WriteLine("### Overloads");
            RenderTable(context, item.Methods.OfType<MethodDocItem>().Select(m => ((ILinkModel)context.ResolveLink(m), RenderXmlDoc(context, m.Documentation.GetSummary()))), writer);
            
            foreach (var method in item.Methods.OfType<MethodDocItem>())
            {
               RenderMethod(context, method, writer);
            }
         }
      }

      private void RenderExample(RenderingContext context, DocItem item, TextWriter writer)
      {
         XElement exampleElement = item.Documentation.GetExample();
         if (exampleElement != null)
         {
            writer.WriteLine();
            writer.WriteLine("### Examples");
            writer.WriteLine();
            writer.WriteLine(RenderXmlDoc(context, exampleElement));
         }
      } 

      private void RenderMethod(RenderingContext context, MethodDocItem item, TextWriter writer)
      {
         WriteHeader(context, item, writer);

         // TODO PP (2020-08-24): Add anchor... perhaps WriteTitle method?
         writer.WriteLine($"## {item.Name} {item.Method.SymbolKind}");

         writer.WriteLine();
         writer.Write(RenderXmlDoc(context, item.Documentation.GetSummary()));
         writer.WriteLine();

         writer.WriteLine("```csharp");
         writer.Write(MethodCodeAmbience.ConvertSymbol(item.Method));
         writer.WriteLine();
         writer.WriteLine("```");

         RenderParameters(context, item, writer);

         RenderReturns(context, item, writer);

         RenderExceptions(context, item, writer);

         RenderExample(context, item, writer);

         RenderRemarks(context, item, writer);

         RenderSeeAlsos(context, item, writer);
      }

      private void RenderSeeAlsos(RenderingContext context, MethodDocItem item, TextWriter writer)
      {
         IEnumerable<XElement> elements = item.Documentation.GetSeeAlsos();
         if (elements != null && elements.Any() )
         {
            writer.WriteLine($"### See also");
            foreach (var element in elements)
            {
               writer.WriteLine($"* {RenderLink(context, context.ResolveCrefLink(element.GetReferenceName(), RenderXmlDoc(context, element)))}");
            }
         }
      }

      private void RenderReturns(RenderingContext context, MethodBaseDocItem item, TextWriter writer)
      {
         // TODO PP (2020-08-24): The item should expose this!
         if (item.Entity is IMember member && member.ReturnType != null && member.ReturnType.Kind != TypeKind.Void)
         {
            writer.WriteLine("### Returns");
            writer.WriteLine();
            writer.WriteLine($"{RenderLink(context, context.ResolveTypeLink(member.ReturnType))}<br/>");
            writer.WriteLine(RenderXmlDoc(context, item.Documentation?.GetReturns()));
         }
      }

      private void RenderParameters(RenderingContext context, IParameterizedDocItem item, TextWriter writer)
      {
         if (item != null && item.Parameters.Any())
         {
            writer.WriteLine("### Parameters");

            // TODO PP (2020-08-24): We should probably warn here if a parameter is missing. So should loop on the actual parameters of the method instead perhaps?
            foreach (var parameter in item.Parameters)
            {
               writer.WriteLine($"`{parameter.Name}`  {RenderLink(context, context.ResolveTypeLink(parameter.Parameter.Type))}");
               XElement parameterElement = parameter.Documentation;
               if (parameterElement != null)
               {
                  writer.Write("<br/>");
                  writer.WriteLine(RenderXmlDoc(context, parameterElement));
               }
               else
               {
                  // TODO PP (2020-08-24): Log warning.
               }
            }
         }
      }

      private void RenderRemarks(RenderingContext context, DocItem item, TextWriter writer)
      {
         var remarksElement = item.Documentation?.GetRemarks();
         if (remarksElement != null)
         {
            writer.WriteLine("### Remarks");
            writer.WriteLine();
            writer.WriteLine(RenderXmlDoc(context, remarksElement));
         }
      }

      private void RenderExceptions(RenderingContext context, DocItem item, TextWriter writer)
      {
         var exceptionsElements = item.Documentation?.GetExceptions();
         if (exceptionsElements != null && exceptionsElements.Any())
         {
            writer.WriteLine("### Exceptions");
            writer.WriteLine();
            foreach (var exception in exceptionsElements)
            {
               // TODO PP (2020-08-24): Warn whenever we enounter an unresolved cref?
               writer.Write(RenderLink(context, context.ResolveCrefLink(exception.GetReferenceName())));
               writer.WriteLine("<br/>");
               writer.WriteLine(RenderXmlDoc(context, exception));
               writer.WriteLine();
            }
         }
      }

      private void RenderType(RenderingContext context, TypeDocItem item, TextWriter writer)
      {
         WriteHeader(context, item, writer);

         writer.WriteLine($"## {item.Name} {item.Type.Kind}");
         writer.WriteLine();
         
         // TODO PP (2020-08-24): Add namespace and assembly if multiple assemblies in project!

         writer.Write(RenderXmlDoc(context, item.Documentation.GetSummary()));
         writer.WriteLine();

         List<IType> interfaces = item.Type.DirectBaseTypes.Where(t => t.Kind == TypeKind.Interface && t.GetDefinition().Accessibility == Accessibility.Public).ToList();

         writer.WriteLine("```csharp");
         writer.Write(CodeAmbience.ConvertSymbol(item.Type));
         IType baseType = item.Type.DirectBaseTypes.FirstOrDefault(t => t.Kind == TypeKind.Class && !t.IsKnownType(KnownTypeCode.Object) && !t.IsKnownType(KnownTypeCode.ValueType));
         if (baseType != null)
         {
            writer.Write(" : ");
            writer.Write(BaseTypeAmbience.ConvertType(baseType));
         }

         foreach (IType @interface in interfaces)
         {
            writer.WriteLine(baseType is null ? " :" : ",");
            baseType = item.Type;
            writer.Write(BaseTypeAmbience.ConvertType(@interface));
         }
         writer.WriteLine();
         writer.WriteLine("```");

         bool needBreak = false;

         if (item.Type.Kind == TypeKind.Class)
         {
            writer.Write("Inheritance ");
            writer.Write(string.Join(" &#129106; ", item.Type.GetNonInterfaceBaseTypes().Where(t => t != item.Type).Select(t => RenderLink(context, context.ResolveTypeLink(t, null)))));
            writer.Write(" &#129106; ");
            writer.Write(item.Name);
            writer.WriteLine("  ");
            needBreak = true;
         }

         List<TypeDocItem> derived = context.CurrentItem.Project.Items.OfType<TypeDocItem>().Where(i => i.Type.DirectBaseTypes.Select(t => t is ParameterizedType g ? g.GetDefinition() : t).Contains(item.Type)).OrderBy(i => i.FullName).ToList();
         if (derived.Count > 0)
         {
            if (needBreak)
            {
               writer.WriteLine();
            }

            writer.Write("Derived  " + Environment.NewLine + "&#8627; ");
            writer.Write(string.Join("  " + Environment.NewLine + "&#8627; ", derived.Select(t => RenderInternalLink(context.ResolveLink(t, null)))));
            writer.WriteLine("  ");
            needBreak = true;
         }

         // attribute

         if (interfaces.Count > 0)
         {
            if (needBreak)
            {
               writer.WriteLine();
            }

            writer.Write("Implements ");
            writer.Write(string.Join(", ", interfaces.Select(t => RenderLink(context, context.ResolveTypeLink(t, null)))));
            writer.WriteLine("  ");
         }

         // TODO PP (2020-08-24): Fix properties/methods/fields/operators/etc... Similar beasts... perhaps we can do something here?
         if (item.Children.OfType<PropertyDocItem>().Any())
         {
            writer.WriteLine();
            writer.WriteLine("## Properties");

            RenderTable(context, item.Children.OfType<PropertyDocItem>().Select(prop => ((ILinkModel)context.ResolveLink(prop), context.Generator.RenderNodes(context, prop.Documentation.GetSummary().Nodes()))), writer);

            writer.WriteLine();
            writer.WriteLine("## Methods");
            RenderTable(context, item.Children.OfType<MethodOverloadGroupDocItem>().SelectMany(g => g.Children.OfType<MethodDocItem>()).Select(prop => ((ILinkModel)context.ResolveLink(prop), context.Generator.RenderNodes(context, prop.Documentation.GetSummary()?.Nodes()))), writer);
         }
      }

      private void RenderTable(RenderingContext context, IEnumerable<(ILinkModel Link, string Summary)> items, TextWriter writer)
      {
         if (items != null && items.Any())
         {
            // TODO PP (2020-08-24): Format tables nicer.
            writer.WriteLine("|Name|Description|");
            writer.WriteLine("|---|---|");
            foreach (var item in items)
            {
               writer.Write('|');
               writer.Write(RenderLink(context, item.Link));
               writer.Write('|');
               writer.Write(item.Summary.Trim().Replace("\r\n", " ").Replace("\n", " "));
               writer.WriteLine("|");
            }

         }
      }

      public string RenderLink(RenderingContext context, ILinkModel link)
      {
         return link switch
         {
            InternalLinkModel internalLink => RenderInternalLink(internalLink),
            ExternalLinkModel externalLink => RenderExternalLink(externalLink),
            TypeLinkModel typeLink => RenderTypeLink(context, typeLink),
            NoLinkModel noLink => RenderText(context, noLink.Text),
            _ => throw new NotSupportedException($"Unsupported link type {link.GetType().FullName}.")
         };
      }

      private string RenderInternalLink(InternalLinkModel link)
      {
         return $"[{Escape(link.Text)}]({link.FileName}{(link.HasAnchor ? $"#{link.Anchor}" : "")})";
      }

      private string RenderExternalLink(ExternalLinkModel link)
      {
         return $"[{Escape(link.Text)}]({link.Url})";
      }

      private string Escape(string text)
      {
         return text.Replace("<", "&lt;").Replace(">", "&gt;");
      }

      private string RenderTypeLink(RenderingContext context, TypeLinkModel link)
      {
         StringBuilder builder = new StringBuilder();
         builder.Append(RenderLink(context, link.TypeLink));
         if (link.TypeArguments.Any())
         {
            builder.Append("&lt;");
            builder.Append(String.Join(", ", link.TypeArguments.Select(arg => RenderLink(context, arg))));
            builder.Append("&gt;");
         }

         if (link.Suffix != null)
         {
            builder.Append(RenderLink(context, link.Suffix));
         }

         return builder.ToString();
      }

      public string RenderText(RenderingContext context, string text)
      {
         // TODO PP (2020-08-21): Escape stuff perhaps.
         return text;
      }

      public string RenderPara(RenderingContext context, string content)
      {
         if (!content.EndsWith("\n"))
            content += Environment.NewLine;

         content += Environment.NewLine;
         return content;
      }

      public string RenderC(RenderingContext context, string content)
      {
         return $"`{content}`";
      }

      public string RenderCode(RenderingContext context, string code)
      {
         return $"```csharp\r\n{code.Trim()}\r\n```";
      }

      // TODO PP (2020-08-23): We should allow for invalid links as well here, add to ILinkModel.
      public string RenderParamRef(RenderingContext context, InternalLinkModel link)
      {
         return RenderInternalLink(link);
      }
   }
}
