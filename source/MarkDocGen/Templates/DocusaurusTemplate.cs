using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DefaultDocumentation;
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

         AddRenderer(new MarkdownPageRenderer<EnumDocItem>(RenderEnum, GetFileName));
         AddRenderer(new MarkdownPageRenderer<TypeDocItem>(RenderType, GetFileName, item => item.Kind != DocItemKind.Enum));
         AddRenderer(new MarkdownPageRenderer<MethodBaseOverloadGroupDocItem>(RenderMethods, GetFileName));
         AddRenderer(new MarkdownPageRenderer<MethodBaseDocItem>(RenderMethod, GetFileName, item => !item.IsOverloaded));
         AddRenderer(new MarkdownPageRenderer<PropertyDocItem>(RenderProperty, GetFileName, item => !item.IsOverloaded));
         AddRenderer(new MarkdownPageRenderer<PropertyOverloadGroupDocItem>(RenderProperties, GetFileName));
         AddRenderer(new MarkdownPageRenderer<NamespaceDocItem>(RenderNamespace, GetFileName));
         AddRenderer(new MarkdownPageRenderer<FieldDocItem>(RenderField, GetFileName, item => item.Kind != DocItemKind.EnumField));
         AddRenderer(new MarkdownPageRenderer<HomeDocItem>(RenderHomePage, _ => "index.md"));

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


      private void RenderHomePage(RenderingContext context, HomeDocItem hdi, MarkdownWriter writer)
      {
         foreach (var ns in hdi.AllNamespaces())
         {
            writer.WriteStartBulletItem();
            RenderLink(context, context.ResolveLink(ns), writer);
            writer.WriteEndBulletItem();
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

      private void WriteHeader(RenderingContext context, DocItem docItem, MarkdownWriter writer)
      {
         if (this.GeneratesPage(docItem))
         {
            writer.WriteRaw("---");
            writer.WriteLine();
            writer.WriteRaw($"id: {docItem.AnchorId}");
            writer.WriteLine();
            writer.WriteRaw($"title: \"{GetTitle(docItem)}\"");
            writer.WriteLine();
            writer.WriteRaw($"sidebar_label: \"{GetSidebarLabel(docItem)}\"");
            writer.WriteLine();
            writer.WriteRaw($"reflection_id: \"{docItem.Id}\"");
            writer.WriteLine();
            writer.WriteRaw("---");
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


      static XElement Trim(XElement element)
      {
         static void TrimMe(XElement element)
         {
            foreach (var node in element.Nodes())
            {
               if (node is XText textNode && textNode.Value != null && textNode.Value.Length > 0)
               {
                  textNode.Value = TrimValue(textNode.Value);
               }
               else if (node is XElement elementNode && elementNode.Name.LocalName != "code")
               {
                  TrimMe(elementNode);
               }
            }
         }

         TrimMe(element);

         while (element.FirstNode is XText text && String.IsNullOrWhiteSpace(text.Value))
            text.Remove();

         if (element.FirstNode is XText text2)
            text2.Value = text2.Value.TrimStart();

         while (element.LastNode is XText text && String.IsNullOrWhiteSpace(text.Value))
            text.Remove();

         if (element.LastNode is XText lastTextNode)
            lastTextNode.Value = lastTextNode.Value.TrimEnd();

         return element;
      }

      static string TrimValue(string value)
      {
         var lines = value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

         StringBuilder result = new StringBuilder();

         for (int i = 0; i < lines.Length; i++)
         {
            var line = lines[i];
            if (line.Length == 0)
               continue;

            int pos = 0;
            if (char.IsWhiteSpace(line[pos]))
            {
               while (pos < line.Length && Char.IsWhiteSpace(line[pos]))
                  ++pos;

               result.Append(' ');
            }

            int ws = 0;
            for (; pos < line.Length; pos++)
            {
               var ch = line[pos];
               if (Char.IsWhiteSpace(ch))
               {
                  ws++;
               }
               else
               {
                  if (ws > 0)
                  {
                     result.Append(' ');
                     ws = 0;
                  }
                  result.Append(ch);
               }
            }

            if (ws > 0)
               result.Append(' ');
         }
         return result.ToString();
      }

      private void RenderXmlDoc(RenderingContext context, XElement element, MarkdownWriter writer)
      {
         if (element == null)
            return;

         Trim(element);

         if (element.IsEmpty)
            return;

         MarkdownXmlDocWriter xmlWriter = new MarkdownXmlDocWriter(writer, (r, l, w) => RenderLink(r, l, w));
         context.Generator.RenderNodes(context, Trim(element).Nodes(), xmlWriter);

      }

      private void RenderSummary(RenderingContext context, DocItem item, MarkdownWriter writer)
      {
         writer.EnsureNewParagraph();
         RenderXmlDoc(context, item.Documentation.GetSummary(), writer);
         writer.EnsureNewParagraph();
      }

      // TODO PP (2020-08-28): We have to move AnchorId to the template as well apparently. It seems Docusaurus doesn't support custom anchors (like we tried below)... kinda sucks.
      private void RenderAnchorTitle(string title, string anchor, MarkdownWriter writer)
      {
         // TODO PP (2020-08-31): Anchor?
         writer.WriteHeading2(title);
         //writer.Write("## ");
         //if (!String.IsNullOrEmpty(anchor))
         //   writer.Write($" <a name=\"{anchor}\"> </a> ");
         //writer.Write($"{Escape(title)}");
         //writer.WriteLine();
         //writer.WriteLine();
      }

      private void RenderMethods(RenderingContext context, MethodBaseOverloadGroupDocItem item, MarkdownWriter writer)
      {
         WriteHeader(context, item, writer);

         if (item is ConstructorOverloadGroupDocItem)
            WriteSectionHeading($"## {Escape(GetDisplayName(item.Parent))} Constructors", writer);
         else
            WriteSectionHeading($"## {Escape(GetDisplayName(item.Parent))}.{item.Members.First().Name} Method", writer);

         // Render summary
         RenderSummary(context, item, writer);

         RenderMemberTable(context, "Overloads", item.Members, writer);

         foreach (var method in item.Members)
         {
            RenderMethod(context, method, writer);
         }
      }

      private void RenderProperties(RenderingContext context, PropertyOverloadGroupDocItem item, MarkdownWriter writer)
      {
         WriteHeader(context, item, writer);

         // Render summary
         RenderSummary(context, item, writer);

         WriteSectionHeading("Overloads", writer);

         RenderMemberTable(context, "Overloads", item.Properties, writer);

         foreach (var property in item.Properties)
         {
            RenderProperty(context, property, writer);
         }
      }

      private void RenderExample(RenderingContext context, DocItem item, MarkdownWriter writer)
      {
         XElement exampleElement = item.Documentation.GetExample();
         if (exampleElement != null)
         {
            WriteSectionHeading("Examples", writer);
            RenderXmlDoc(context, exampleElement, writer);
         }
      }

      private void RenderMethod(RenderingContext context, MethodBaseDocItem item, MarkdownWriter writer)
      {
         WriteHeader(context, item, writer);

         if (!this.GeneratesPage(item))
            RenderAnchorTitle($"{GetDisplayName(item)} {item.Method.SymbolKind}", item.AnchorId, writer);

         RenderSummary(context, item, writer);

         writer.WriteCodeBlock(MethodCodeAmbience.ConvertSymbol(item.Method), "csharp");

         RenderParameters(context, item, writer);

         RenderReturns(context, item, writer);

         RenderExceptions(context, item, writer);

         RenderExample(context, item, writer);

         RenderRemarks(context, item, writer);

         RenderSeeAlsos(context, item, writer);
      }

      private void RenderProperty(RenderingContext context, PropertyDocItem item, MarkdownWriter writer)
      {
         WriteHeader(context, item, writer);

         RenderSummary(context, item, writer);

         RenderAnchorTitle(GetDisplayName(item), item.AnchorId, writer);

         writer.WriteCodeBlock(MethodCodeAmbience.ConvertSymbol(item.Property), "csharp");

         RenderParameters(context, item, writer);

         WriteSectionHeading("Property Value", writer);
         RenderLink(context, context.ResolveTypeLink(item.Property.ReturnType), writer);
         RenderXmlDoc(context, item.Documentation.GetValue(), writer);

         RenderExceptions(context, item, writer);

         RenderExample(context, item, writer);

         RenderRemarks(context, item, writer);

         RenderSeeAlsos(context, item, writer);
      }

      private void RenderField(RenderingContext context, FieldDocItem item, MarkdownWriter writer)
      {
         WriteHeader(context, item, writer);

         //RenderAnchorTitle($"{item.Name} {item.Field.SymbolKind}", item.AnchorId, writer);

         RenderSummary(context, item, writer);

         writer.WriteCodeBlock(MethodCodeAmbience.ConvertSymbol(item.Field), "csharp");

         WriteSectionHeading("Field Value", writer);
         RenderLink(context, context.ResolveTypeLink(item.Field.ReturnType), writer);
         RenderXmlDoc(context, item.Documentation.GetValue(), writer);

         RenderExceptions(context, item, writer);

         RenderExample(context, item, writer);

         RenderRemarks(context, item, writer);

         RenderSeeAlsos(context, item, writer);
      }

      private void RenderNamespace(RenderingContext context, NamespaceDocItem ndi, MarkdownWriter writer)
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

      private void RenderSeeAlsos(RenderingContext context, DocItem item, MarkdownWriter writer)
      {
         IEnumerable<XElement> elements = item.Documentation.GetSeeAlsos();
         if (elements != null && elements.Any())
         {
            WriteSectionHeading("See also", writer);

            foreach (var element in elements)
            {
               writer.WriteStartBulletItem();
               RenderLink(context, context.ResolveCrefLink(element.GetReferenceName(), String.IsNullOrWhiteSpace(element.Value) ? null : element.Value), writer);
               writer.WriteEndBulletItem();
            }
         }
      }

      private void RenderReturns(RenderingContext context, MethodBaseDocItem item, MarkdownWriter writer)
      {
         if (item.Entity is IMember member && member.ReturnType != null && member.ReturnType.Kind != TypeKind.Void)
         {
            WriteSectionHeading("Returns", writer);
            RenderLink(context, context.ResolveTypeLink(member.ReturnType), writer);
            writer.WriteLine("  "); // line break
            RenderXmlDoc(context, item.Documentation?.GetReturns(), writer);
         }
      }

      private void WriteSectionHeading(string heading, MarkdownWriter writer)
      {
         writer.WriteHeading3(heading);
      }

      private void RenderParameters(RenderingContext context, IParameterizedDocItem item, MarkdownWriter writer)
      {
         if (item != null && item.Parameters.Any())
         {
            writer.WriteHeading3("Parameters");

            // TODO PP (2020-08-24): We should probably warn here if a parameter is missing. So should loop on the actual parameters of the method instead perhaps?
            foreach (var parameter in item.Parameters)
            {
               writer.WriteInlineCode(parameter.Name);
               writer.Write(" ");
               RenderLink(context, context.ResolveTypeLink(parameter.Parameter.Type), writer);
               XElement parameterElement = parameter.Documentation;
               if (parameterElement != null)
               {
                  writer.EnsureNewLine();
                  RenderXmlDoc(context, parameterElement, writer);
               }
               else
               {
                  // TODO PP (2020-08-24): Log warning.
               }
            }
         }
      }

      private void RenderRemarks(RenderingContext context, DocItem item, MarkdownWriter writer)
      {
         var remarksElement = item.Documentation?.GetRemarks();
         if (remarksElement != null)
         {
            WriteSectionHeading("Remarks", writer);
            RenderXmlDoc(context, remarksElement, writer);
         }
      }

      private void RenderExceptions(RenderingContext context, DocItem item, MarkdownWriter writer)
      {
         var exceptionsElements = item.Documentation?.GetExceptions();
         if (exceptionsElements != null && exceptionsElements.Any())
         {
            WriteSectionHeading("Exceptions", writer);

            foreach (var exception in exceptionsElements)
            {
               // TODO PP (2020-08-24): Warn whenever we enounter an unresolved cref?
               RenderLink(context, context.ResolveCrefLink(exception.GetReferenceName()), writer);
               writer.WriteLine("  "); // line break               
               RenderXmlDoc(context, exception, writer);
               writer.WriteLine();
            }
         }
      }

      private void RenderEnum(RenderingContext context, EnumDocItem item, MarkdownWriter writer)
      {
         WriteHeader(context, item, writer);

         RenderAssemblyAndNamespaceInfo(context, item, writer);

         RenderSummary(context, item, writer);

         writer.WriteStartCodeBlock("csharp");
         writer.Write(CodeAmbience.ConvertSymbol(item.Type));
         IType enumType = item.Type.GetEnumUnderlyingType();
         writer.WriteLine(enumType.IsKnownType(KnownTypeCode.Int32) ? string.Empty : $" : {enumType.FullName}");
         writer.WriteEndCodeBlock();

         RenderInheritance(context, item, writer);

         WriteSectionHeading("Fields", writer);

         writer.WriteStartTable(3);
         writer.WriteStartTableRow();
         writer.WriteTableCell("Name");
         writer.WriteTableCell("Value");
         writer.WriteTableCell("Description");
         writer.WriteEndTableRow();
         writer.WriteTableHeaderSeparator();

         foreach (var field in item.Fields)
         {
            writer.WriteStartTableRow();
            writer.WriteTableCell(field.Name);
            writer.WriteTableCell(field.Field.GetConstantValue()?.ToString());
            writer.WriteStartTableCell();
            RenderXmlDoc(context, field.Documentation?.GetSummary(), writer);            
            writer.WriteEndTableCell();
            writer.WriteEndTableRow();
         }

         RenderExample(context, item, writer);

         RenderRemarks(context, item, writer);

         RenderSeeAlsos(context, item, writer);
      }

      private void RenderAssemblyAndNamespaceInfo(RenderingContext context, DocItem item, MarkdownWriter writer)
      {
         var namespaceItem = item.ContainingNamespace();
         var assemblyItem = item.ContainingAssembly();

         if (namespaceItem == null && assemblyItem == null)
            return;

         writer.WriteStartParagraph();
         if (namespaceItem != null)
         {
            writer.Write("Namespace: ");
            RenderLink(context, context.ResolveLink(namespaceItem), writer);
            if (assemblyItem != null)
               writer.WriteLine("  "); // line break            
         }

         if (assemblyItem != null)
         {
            writer.Write("Assembly: ");
            RenderLink(context, context.ResolveLink(assemblyItem), writer);
         }

         writer.WriteEndParagraph();
      }

      private void RenderInheritance(RenderingContext context, TypeDocItem item, MarkdownWriter writer)
      {
         if (item.Type.Kind == TypeKind.Class)
         {
            writer.Write("Inheritance ");

            foreach (var type in item.Type.GetNonInterfaceBaseTypes().Where(t => t != item.Type).AsSmart())
            {
               if (!type.IsFirst)
                  writer.WriteRaw(" &#129106; ");

               RenderLink(context, context.ResolveTypeLink(type.Value), writer);
            }

            writer.WriteRaw(" &#129106; ");
            writer.Write(item.Name);
            writer.WriteLine("  ");
         }

      }
      private void RenderType(RenderingContext context, TypeDocItem item, MarkdownWriter writer)
      {
         WriteHeader(context, item, writer);

         if (!this.GeneratesPage(item))
            RenderAnchorTitle($"{item.Name} {item.Type.Kind}", item.AnchorId, writer);

         RenderAssemblyAndNamespaceInfo(context, item, writer);

         RenderSummary(context, item, writer);

         List<IType> interfaces = item.Type.DirectBaseTypes.Where(t => t.Kind == TypeKind.Interface && t.GetDefinition().Accessibility == Accessibility.Public).ToList();

         writer.WriteStartCodeBlock();
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
         writer.WriteEndCodeBlock();

         RenderInheritance(context, item, writer);

         List<TypeDocItem> derived = context.CurrentItem.Project.Items.OfType<TypeDocItem>().Where(i => i.Type.DirectBaseTypes.Select(t => t is ParameterizedType g ? g.GetDefinition() : t).Contains(item.Type)).OrderBy(i => i.Type.FullName).ToList();
         if (derived.Count > 0)
         {
            writer.EnsureNewLine();

            writer.WriteLine("Derived  ");
            writer.WriteRaw("&#8627;");
            foreach (var type in derived.AsSmart())
            {
               if (!type.IsFirst)
               {
                  writer.WriteLine("  ");
                  writer.WriteRaw("&#8627; ");
               }

               RenderLink(context, context.ResolveLink(type.Value), writer);
            }
            writer.WriteLine("  ");
         }

         // attribute

         if (interfaces.Count > 0)
         {
            writer.EnsureNewLine();

            writer.Write("Implements ");
            foreach (var ifc in interfaces.AsSmart())
            {
               if (!ifc.IsFirst)
               {
                  writer.Write(", ");
               }

               RenderLink(context, context.ResolveTypeLink(ifc.Value), writer);
            }
            writer.WriteLine("  ");
         }

         if (item.AllConstructors().Any())
         {
            writer.WriteLine();
            RenderMemberTable(context, "Constructors", item.AllConstructors().OrderBy(method => method.Parameters.Count()), writer);
         }

         RenderMemberTable(context, "Fields", item.Fields, writer);

         RenderMemberTable(context, "Properties", item.AllProperties(), writer);

         RenderMemberTable(context, "Methods", item.AllMethods(), writer);

         RenderMemberTable(context, "Operators", item.AllOperators(), writer);

         RenderExample(context, item, writer);

         RenderRemarks(context, item, writer);

         RenderSeeAlsos(context, item, writer);
      }

      private void RenderMemberTable(RenderingContext context, string title, IEnumerable<DocItem> items, MarkdownWriter writer)
      {
         if (items.Any())
         {
            WriteSectionHeading(title, writer);
            writer.WriteStartTable(2);
            writer.WriteStartTableRow();
            writer.WriteTableCell("Name");
            writer.WriteTableCell("Description");
            writer.WriteEndTableRow();
            writer.WriteTableHeaderSeparator();

            foreach (var item in items)
            {
               writer.WriteStartTableRow();
               writer.WriteStartTableCell();
               RenderLink(context, context.ResolveLink(item), writer);
               writer.WriteEndTableCell();
               writer.WriteStartTableCell();
               RenderXmlDoc(context, item.Documentation?.GetSummary(), writer);
               writer.WriteEndTableCell();
               writer.WriteEndTableRow();
            }
            writer.WriteEndTable();
         }
      }

      public void RenderLink(RenderingContext context, ILinkModel link, MarkdownWriter writer)
      {
         switch (link)
         {
            case InternalLinkModel internalLink:
               RenderInternalLink(internalLink, writer);
               break;
            case ExternalLinkModel externalLink:
               RenderExternalLink(externalLink, writer);
               break;
            case TypeLinkModel typeLink:
               RenderTypeLink(context, typeLink, writer);
               break;
            case NoLinkModel noLink:
               writer.Write(noLink.Text);
               break;
            default:
               throw new NotSupportedException($"Unsupported link type {link.GetType().FullName}.");
         }
      }

      private void RenderInternalLink(InternalLinkModel link, MarkdownWriter writer)
      {
         writer.WriteLink(link.Text, $"{link.FileName}{(link.HasAnchor ? $"#{link.Anchor}" : "")}");
      }

      private void RenderExternalLink(ExternalLinkModel link, MarkdownWriter writer)
      {
         writer.WriteLink(link.Text, link.Url);
      }

      private string Escape(string text)
      {
         return text
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("*", "\\*")
            .Replace("_", "\\_");
      }

      private string RenderTypeLink(RenderingContext context, TypeLinkModel link, MarkdownWriter writer)
      {
         StringBuilder builder = new StringBuilder();
         RenderLink(context, link.TypeLink, writer);
         if (link.TypeArguments.Any())
         {
            writer.Write('<');
            foreach (var typeArgument in link.TypeArguments.AsSmart())
            {
               if (!typeArgument.IsFirst)
                  writer.Write(", ");

               RenderLink(context, typeArgument.Value, writer);
            }
            writer.Write('>');
         }

         if (link.Suffix != null)
         {
            RenderLink(context, link.Suffix, writer);
         }

         return builder.ToString();
      }

   }

   class MarkdownXmlDocWriter : IXmlDocWriter
   {
      private readonly MarkdownWriter m_writer;
      private readonly Action<RenderingContext, ILinkModel, MarkdownWriter> m_renderLink;

      public MarkdownXmlDocWriter(MarkdownWriter writer, Action<RenderingContext, ILinkModel, MarkdownWriter> renderLink)
      {
         m_writer = writer;
         this.m_renderLink = renderLink;
      }

      public void WriteCodeBlock(RenderingContext context, string value, string language)
      {
         m_writer.WriteCodeBlock(value, language);
      }

      public void WriteEndParagraph()
      {
         m_writer.WriteEndParagraph();
      }

      public void WriteInlineCode(RenderingContext context, string content)
      {
         m_writer.WriteInlineCode(content);
      }

      public void WriteLink(RenderingContext context, ILinkModel link)
      {
         m_renderLink(context, link, m_writer);
      }

      public void WriteStartCodeBlock(string language)
      {
         m_writer.WriteStartCodeBlock(language);
      }

      public void WriteStartParagraph()
      {
         m_writer.WriteStartParagraph();
      }

      public void WriteText(RenderingContext context, string text)
      {
         m_writer.Write(text);
      }
   }
}
