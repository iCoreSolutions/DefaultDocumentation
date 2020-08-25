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
   interface ITemplate
   {
      bool GeneratesPage(DocItem item);
   }

   class MyTemplate2 : ITemplate
   {
      // TODO PP (2020-08-23): Allow to return extension as well...
      public virtual bool GeneratesPage(DocItem item)
      {
         return item is TypeDocItem ||
                item is MethodOverloadGroupDocItem ||
                item is MethodDocItem mdi && !(mdi.Parent is OverloadGroupDocItem) ||
                item is PropertyDocItem pdi && !(pdi.Parent is OverloadGroupDocItem) ||
                item is PropertyOverloadGroupDocItem ||
                item is ConstructorDocItem cdi && !(cdi.Parent is OverloadGroupDocItem) ||
                item is ConstructorOverloadGroupDocItem ||
                item is NamespaceDocItem;
                ;                
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

            case PropertyOverloadGroupDocItem pgds:
               RenderProperties(context, pgds, writer);
               break;

            case MethodDocItem mdi:
               RenderMethod(context, mdi, writer);
               break;

            case PropertyDocItem pdi:
               RenderProperty(context, pdi, writer);
               break;

            case ConstructorDocItem cdi:
               RenderMethod(context, cdi, writer);
               break;

            case ConstructorOverloadGroupDocItem cogdi:
               RenderMethods(context, cogdi, writer);
               break;

            case OperatorDocItem odi:
               RenderMethod(context, odi, writer);
               break;

            case OperatorOverloadGroupDocItem oogdi:
               RenderMethods(context, oogdi, writer);
               break;

            case NamespaceDocItem ndi:
               RenderNamespace(context, ndi, writer);
               break;

            default:
               throw new NotImplementedException($"Unsupported page {context.CurrentItem.GetType()}");
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

      private static readonly CSharpAmbience TypeNameAmbience = new CSharpAmbience
      {
         ConversionFlags =
        ConversionFlags.ShowParameterList
        | ConversionFlags.ShowTypeParameterList
        | ConversionFlags.ShowDeclaringType
        | ConversionFlags.UseFullyQualifiedTypeNames
      };

      private static readonly CSharpAmbience EntityNameAmbience = new CSharpAmbience
      {
         ConversionFlags =
        ConversionFlags.ShowParameterList
        | ConversionFlags.ShowTypeParameterList
        | ConversionFlags.UseFullyQualifiedTypeNames
      };

      private static readonly CSharpAmbience BaseTypeAmbience = new CSharpAmbience
      {
         ConversionFlags = ConversionFlags.ShowTypeParameterList
      };

      public string GetDisplayName(DocItem item)
      {
         switch (item)
         {
            case OperatorDocItem oper:
               if (oper.Name == "op_Explicit")
                  return $"Explicit({oper.Method.ReturnType.Name} to {oper.Method.Parameters[0].Type.Name})";
               else if (oper.Name == "op_Implicit")
                  return $"Implicit({oper.Method.ReturnType.Name} to {oper.Method.Parameters[0].Type.Name})";
               else
                  return $"{oper.Name.Substring(3)}({String.Join(", ", oper.Parameters.Select(p => TypeNameAmbience.ConvertType(p.Parameter.Type)))})";

            case TypeDocItem type:
               return TypeNameAmbience.ConvertType(type.Type);
            case SymbolDocItem symbol:
               return EntityNameAmbience.ConvertSymbol(symbol.Symbol);
            case NamespaceDocItem ns:
               return ns.Name;
            default:
               throw new NotSupportedException($"Can't get display name for doc item of type {item}");
         }         

      }

      private string RenderXmlDoc(RenderingContext context, XElement element)
      {
         if (element == null || element.IsEmpty)
            return null;

         string summary = context.Generator.RenderNodes(context, element.Nodes());
         if (summary == null)
            return null;

         var text = string.Join('\n', summary.Split('\n').Select(line => line.Trim()));
         return text;

      }

      private void RenderSummary(RenderingContext context, DocItem item, TextWriter writer)
      {
         writer.Write(RenderXmlDoc(context, item.Documentation.GetSummary()));
      }

      private void RenderAnchorTitle(string title, string anchor, TextWriter writer)
      {
         writer.Write($"## {Escape(title)}");
         if (!String.IsNullOrEmpty(anchor))
            writer.Write($"<a name=\"{anchor}\" />");
         writer.WriteLine();
      }

      private void RenderMethods(RenderingContext context, MethodBaseOverloadGroupDocItem item, TextWriter writer)
      {
         if (item is ConstructorOverloadGroupDocItem)
            writer.WriteLine($"## {GetDisplayName(item.Parent)} Constructors");
         else            
            writer.WriteLine($"## {GetDisplayName(item.Parent)}.{item.Members.First().Name} Method");

         WriteHeader(context, item, writer);

         // Render summary
         RenderSummary(context, item, writer);
            
         writer.WriteLine();            
         writer.WriteLine("### Overloads");

         RenderTable(context, item.Members.Select(m => ((ILinkModel)context.ResolveLink(m), RenderXmlDoc(context, m.Documentation.GetSummary()))), writer);
            
         foreach (var method in item.Members)
         {
            RenderMethod(context, method, writer);
         }
      }

      private void RenderProperties(RenderingContext context, PropertyOverloadGroupDocItem item, TextWriter writer)
      {
         // Render summary
         RenderSummary(context, item, writer);

         writer.WriteLine();
         writer.WriteLine("### Overloads");

         RenderTable(context, item.Properties.Select(m => ((ILinkModel)context.ResolveLink(m), RenderXmlDoc(context, m.Documentation.GetSummary()))), writer);

         foreach (var property in item.Properties)
         {
            RenderProperty(context, property, writer);
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

      private void RenderMethod(RenderingContext context, MethodBaseDocItem item, TextWriter writer)
      {
         // TODO PP (2020-08-25): Write and header only if page.
         WriteHeader(context, item, writer);

         RenderAnchorTitle($"{GetDisplayName(item)} {item.Method.SymbolKind}", item.AnchorId, writer);         

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

      private void RenderProperty(RenderingContext context, PropertyDocItem item, TextWriter writer)
      {
         WriteHeader(context, item, writer);

         // TODO PP (2020-08-24): Add anchor... perhaps WriteTitle method?
         RenderAnchorTitle($"{item.Name} {item.Property.SymbolKind}", item.AnchorId, writer);

         RenderSummary(context, item, writer);
         writer.WriteLine();

         writer.WriteLine("```csharp");
         writer.Write(MethodCodeAmbience.ConvertSymbol(item.Property));
         writer.WriteLine();
         writer.WriteLine("```");

         RenderParameters(context, item, writer);

         writer.WriteLine("## Property Value");
         writer.WriteLine(RenderLink(context, context.ResolveTypeLink(item.Property.ReturnType)));
         writer.WriteLine(RenderXmlDoc(context, item.Documentation.GetValue()));
         
         RenderExceptions(context, item, writer);

         RenderExample(context, item, writer);

         RenderRemarks(context, item, writer);

         RenderSeeAlsos(context, item, writer);
      }

      // TODO PP (2020-08-25): Handle compiler generated members, and accessibility configuration (i.e. only export public members etc)

      private void RenderNamespace(RenderingContext context, NamespaceDocItem ndi, TextWriter writer)
      {
         WriteHeader(context, ndi, writer);

         writer.WriteLine($"## {GetDisplayName(ndi)} Namespace");
      }

      private void RenderSeeAlsos(RenderingContext context, DocItem item, TextWriter writer)
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

         RenderAnchorTitle($"{item.Name} {item.Type.Kind}", item.AnchorId, writer);

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

         List<TypeDocItem> derived = context.CurrentItem.Project.Items.OfType<TypeDocItem>().Where(i => i.Type.DirectBaseTypes.Select(t => t is ParameterizedType g ? g.GetDefinition() : t).Contains(item.Type)).OrderBy(i => i.Type.FullName).ToList();
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

         if (item.AllConstructors().Any())
         {
            writer.WriteLine();
            writer.WriteLine("## Constructors");
            RenderTable(context, item.AllConstructors().OrderBy(method => method.Parameters.Count()).Select(method => ((ILinkModel)context.ResolveLink(method), RenderXmlDoc(context, method.Documentation.GetSummary()))), writer);

         }

         // TODO PP (2020-08-24): Fix properties/methods/fields/operators/etc... Similar beasts... perhaps we can do something here?
         if (item.Fields.Any())
         {
            writer.WriteLine();
            writer.WriteLine("## Fields");

            RenderTable(context, item.Fields.Select(field => ((ILinkModel)context.ResolveLink(field), RenderXmlDoc(context, field.Documentation.GetSummary()))), writer);
         }

         if (item.AllProperties().Any())
         {
            writer.WriteLine();
            writer.WriteLine("## Properties");

            RenderTable(context, item.AllProperties().Select(prop => ((ILinkModel)context.ResolveLink(prop), RenderXmlDoc(context, prop.Documentation.GetSummary()))), writer);

         }

         if (item.AllMethods().Any())
         {
            writer.WriteLine();
            writer.WriteLine("## Methods");
            RenderTable(context, item.AllMethods().Select(prop => ((ILinkModel)context.ResolveLink(prop), RenderXmlDoc(context, prop.Documentation.GetSummary()))), writer);
         }

         if (item.AllOperators().Any())
         {
            writer.WriteLine();
            writer.WriteLine("## Operators");
            RenderTable(context, item.AllOperators().Select(prop => ((ILinkModel)context.ResolveLink(prop), RenderXmlDoc(context, prop.Documentation.GetSummary()))), writer);
         }

         RenderSeeAlsos(context, item, writer);
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
               writer.Write(item.Summary?.Trim().Replace("\r\n", " ").Replace("\n", " "));
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
         return Escape(text);
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
