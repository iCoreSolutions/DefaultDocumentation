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
using Pluralize.NET;

namespace MarkDocGen
{
   class DocusaurusTemplate : TemplateBase
   {
      private readonly IFileNameStrategy m_fileNameStrategy;
      private readonly IPluralize pluralizer = new Pluralizer();

      public DocusaurusTemplate(IFileNameStrategy fns)
      {
         m_fileNameStrategy = fns ?? throw new ArgumentNullException(nameof(fns), $"{nameof(fns)} is null.");

         AddRenderer(new PageRenderer<TypeDocItem>(RenderType, GetFileName));
         AddRenderer(new PageRenderer<MethodBaseOverloadGroupDocItem>(RenderMethods, GetFileName));
         AddRenderer(new PageRenderer<MethodBaseDocItem>(RenderMethod, GetFileName, item => !item.IsOverloaded));
         AddRenderer(new PageRenderer<PropertyDocItem>(RenderProperty, GetFileName, item => !item.IsOverloaded));
         AddRenderer(new PageRenderer<PropertyOverloadGroupDocItem>(RenderProperties, GetFileName));
         AddRenderer(new PageRenderer<NamespaceDocItem>(RenderNamespace, GetFileName));
         AddRenderer(new PageRenderer<FieldDocItem>(RenderField, GetFileName));
         AddRenderer(new PageRenderer<HomeDocItem>(RenderHomePage, _ => "index.md"));
         
      }

      private string GetFileName(DocItem item)
      {
         return m_fileNameStrategy.GetFileName(item, ".md");
      }


      private string GetTitle(DocItem item)
      {

         return item switch
         {
            TypeDocItem typeItem => $"{GetDisplayName(item)} {typeItem.Type.Kind}",
            ConstructorOverloadGroupDocItem ctorItem => $"{GetDisplayName(ctorItem.Parent)} Constructors",
            // TODO PP (2020-08-28): Fix display of indexer properties.
            PropertyOverloadGroupDocItem propItem => $"{GetDisplayName(propItem.Parent)}.{propItem.Name} Property",
            MethodOverloadGroupDocItem methodItem => $"{GetDisplayName(methodItem.Parent)}.{methodItem.Name} Method",
            OperatorOverloadGroupDocItem oogdi => $"{GetDisplayName(oogdi)} Operator",
            OverloadGroupDocItem og => $"{og.Name} {og.SymbolKind}",
            MemberDocItem memberItem => $"{GetDisplayName(item)} {memberItem.Member.SymbolKind}",
            NamespaceDocItem namespaceItem => $"{GetDisplayName(namespaceItem)} Namespace",
            EntityDocItem entityItem => $"{GetDisplayName(item)} {entityItem.Entity.SymbolKind}",
            _ => GetDisplayName(item)
         };         
      }

      
      private void RenderHomePage(RenderingContext context, HomeDocItem hdi, TextWriter writer)
      {
         foreach (var ns in hdi.AllNamespaces())
         {
            writer.WriteLine($"* {RenderLink(context, context.ResolveLink(ns))}");
         }
      }


      private string GetSidebarLabel(DocItem item)
      {
         return item switch
         {
            ConstructorDocItem _ => "Constructor",
            ConstructorOverloadGroupDocItem _ => "Constructors",
            NamespaceDocItem nsItem => nsItem.Name,
            _ => GetDisplayName(item)
         };
      }

      private void WriteHeader(RenderingContext context, DocItem docItem, TextWriter writer)
      {
         if (this.GeneratesPage(docItem))
         {
            writer.WriteLine("---");
            writer.WriteLine($"id: {docItem.AnchorId}");
            writer.WriteLine($"title: \"{GetTitle(docItem)}\"");
            writer.WriteLine($"sidebar_label: \"{GetSidebarLabel(docItem)}\"");
            writer.WriteLine("---");
            writer.WriteLine();
         }
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

      // TODO PP (2020-08-28): wtf is a DisplayName anyway?
      public override string GetDisplayName(DocItem item)
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
            case AssemblyDocItem asm:
               return asm.Module.AssemblyName + ".dll";               
            
            case OverloadGroupDocItem ogd:
               // TODO PP (2020-08-28): Incorrect... this will contain the parameter for methods. Need just the simple name here. Introduce name property on items again?
               return GetDisplayName(ogd.Children.First());

            case HomeDocItem hdi:
               return "Home";
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

      // TODO PP (2020-08-28): We have to move AnchorId to the template as well apparently. It seems Docusaurus doesn't support custom anchors (like we tried below)... kinda sucks.
      private void RenderAnchorTitle(string title, string anchor, TextWriter writer)
      {
         writer.Write("## ");
         if (!String.IsNullOrEmpty(anchor))
            writer.Write($" <a name=\"{anchor}\"> </a> ");
         writer.Write($"{Escape(title)}");
         writer.WriteLine();
         writer.WriteLine();
      }

      private void RenderMethods(RenderingContext context, MethodBaseOverloadGroupDocItem item, TextWriter writer)
      {
         WriteHeader(context, item, writer);

         if (item is ConstructorOverloadGroupDocItem)
            writer.WriteLine($"## {Escape(GetDisplayName(item.Parent))} Constructors");
         else
            writer.WriteLine($"## {Escape(GetDisplayName(item.Parent))}.{item.Members.First().Name} Method");

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
         WriteHeader(context, item, writer);

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
         if (this.GeneratesPage(item))
            WriteHeader(context, item, writer);

         if (!this.GeneratesPage(item))
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
         //RenderAnchorTitle($"{item.Name} {item.Property.SymbolKind}", item.AnchorId, writer);

         RenderSummary(context, item, writer);
         writer.WriteLine();

         RenderAnchorTitle(GetDisplayName(item), item.AnchorId, writer);
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
      
      private void RenderField(RenderingContext context, FieldDocItem item, TextWriter writer)
      {
         WriteHeader(context, item, writer);
         
         //RenderAnchorTitle($"{item.Name} {item.Field.SymbolKind}", item.AnchorId, writer);

         RenderSummary(context, item, writer);
         writer.WriteLine();

         writer.WriteLine("```csharp");
         writer.Write(MethodCodeAmbience.ConvertSymbol(item.Field));
         writer.WriteLine();
         writer.WriteLine("```");

         writer.WriteLine("## Field Value");
         writer.WriteLine();
         writer.WriteLine(RenderLink(context, context.ResolveTypeLink(item.Field.ReturnType)));
         writer.WriteLine(RenderXmlDoc(context, item.Documentation.GetValue()));

         RenderExceptions(context, item, writer);

         RenderExample(context, item, writer);

         RenderRemarks(context, item, writer);

         RenderSeeAlsos(context, item, writer);
      }

      private void WritePageTitle(string title, TextWriter writer)
      {
         writer.WriteLine();
         writer.WriteLine($"## {title}");
         writer.WriteLine();
      }

      private void RenderNamespace(RenderingContext context, NamespaceDocItem ndi, TextWriter writer)
      {
         WriteHeader(context, ndi, writer);

         RenderSummary(context, ndi, writer);
         RenderMemberTable(context, "Classes", ndi.Classes(), writer);
         RenderMemberTable(context, "Structs", ndi.Structs(), writer);
         RenderMemberTable(context, "Interfaces", ndi.Interfaces(), writer);
         RenderMemberTable(context, "Enums", ndi.Enums(), writer);
         RenderMemberTable(context, "Delegates", ndi.Delegates(), writer);
         RenderRemarks(context, ndi, writer);
      }

      private void RenderSeeAlsos(RenderingContext context, DocItem item, TextWriter writer)
      {
         IEnumerable<XElement> elements = item.Documentation.GetSeeAlsos();
         if (elements != null && elements.Any())
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

      private void RenderEnum(RenderingContext context, EnumDocItem item, TextWriter writer)
      {
         WriteHeader(context, item, writer);
         writer.WriteLine($"## {GetDisplayName(item)} Enum");

         RenderSummary(context, item, writer);

         writer.WriteLine("```csharp");
         writer.Write(CodeAmbience.ConvertSymbol(item.Type));
         IType enumType = item.Type.GetEnumUnderlyingType();
         writer.WriteLine(enumType.IsKnownType(KnownTypeCode.Int32) ? string.Empty : $" : {enumType.FullName}");
         writer.WriteLine("```");

         // attribute
         foreach (var field in item.Fields)
         {
            writer.WriteLine($"* ***TODO - NOT YET IMPLEMENTED***  {field.FieldValue}");
            // TODO PP (2020-08-26): Implement rendering of table.
         }

         RenderExample(context, item, writer);

         RenderRemarks(context, item, writer);

         RenderSeeAlsos(context, item, writer);
      }

      private void RenderAssemblyAndNamespaceInfo(RenderingContext context, DocItem item, TextWriter writer)
      {
         var namespaceItem = item.ContainingNamespace();
         var assemblyItem = item.ContainingAssembly();

         if (namespaceItem != null)
         {
            writer.WriteLine($"Namespace: {RenderLink(context, context.ResolveLink(namespaceItem))}{(assemblyItem == null ? "" : "<br/>")}");
         }

         if (assemblyItem != null)
         {
            writer.WriteLine($"Assembly: {RenderLink(context, context.ResolveLink(assemblyItem))}");
         }
      }

      private void RenderType(RenderingContext context, TypeDocItem item, TextWriter writer)
      {
         WriteHeader(context, item, writer);
         
         if (!this.GeneratesPage(item))
            RenderAnchorTitle($"{item.Name} {item.Type.Kind}", item.AnchorId, writer);

         RenderAssemblyAndNamespaceInfo(context, item, writer);

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
            writer.Write(Escape(item.Name));
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
            writer.Write(string.Join("  " + Environment.NewLine + "&#8627; ", derived.Select(t => RenderLink(context, context.ResolveLink(t, null)))));
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

         // TODO PP (2020-08-26): Remarks here maybe?

         RenderMemberTable(context, "Fields", item.Fields, writer);

         RenderMemberTable(context, "Properties", item.AllProperties(), writer);

         RenderMemberTable(context, "Methods", item.AllMethods(), writer);

         RenderMemberTable(context, "Operators", item.AllOperators(), writer);

         RenderSeeAlsos(context, item, writer);
      }

      private void RenderMemberTable(RenderingContext context, string title, IEnumerable<DocItem> items, TextWriter writer)
      {
         if (items.Any())
         {
            writer.WriteLine();
            writer.WriteLine($"## {title}");
            RenderTable(context, items.Select(m => ((ILinkModel)context.ResolveLink(m), RenderXmlDoc(context, m.Documentation.GetSummary()))), writer);
         }
      }

      private void RenderTable(RenderingContext context, IEnumerable<(ILinkModel Link, string Summary)> items, TextWriter writer)
      {
         if (items != null && items.Any())
         {
            writer.WriteLine("|Name|Description|");
            writer.WriteLine("|---|---|");
            foreach (var item in items)
            {
               writer.Write('|');
               writer.Write(RenderLink(context, item.Link));
               writer.Write('|');
               writer.Write(item.Summary?.Trim().Replace("\r\n", "<br/>").Replace("\n", "<br/>"));
               writer.WriteLine("|");
            }

         }
      }

      public override string RenderLink(RenderingContext context, ILinkModel link)
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
         return text
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("*", "\\*")
            .Replace("_", "\\_");
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

      public override string RenderText(RenderingContext context, string text)
      {
         return Escape(text);
      }

      public override string RenderParagraph(RenderingContext context, string content)
      {
         if (!content.EndsWith("\n"))
            content += Environment.NewLine;

         content += Environment.NewLine;
         return content;
      }

      public override string RenderInlineCode(RenderingContext context, string content)
      {
         return $"`{content}`";
      }

      public override string RenderCodeBlock(RenderingContext context, string code)
      {
         return $"```csharp\r\n{code.Trim()}\r\n```";
      }

      // TODO PP (2020-08-23): We should allow for invalid links as well here, add to ILinkModel.
      public override string RenderParamRef(RenderingContext context, ILinkModel link)
      {
         return RenderLink(context, link);
      }
   }
}
