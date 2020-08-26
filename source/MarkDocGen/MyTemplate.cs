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
         using (MarkdownWriter mdWriter = new MarkdownWriter(writer))
         {


            // TODO PP (2020-08-24): Add namespace (top level item)
            switch (context.CurrentItem)
            {
               case TypeDocItem classDocItem:
                  RenderType(context, classDocItem, mdWriter);
                  break;

               case MethodOverloadGroupDocItem mogd:
                  RenderMethods(context, mogd, mdWriter);
                  break;

               case PropertyOverloadGroupDocItem pgds:
                  RenderProperties(context, pgds, mdWriter);
                  break;

               case MethodDocItem mdi:
                  RenderMethod(context, mdi, mdWriter);
                  break;

               case PropertyDocItem pdi:
                  RenderProperty(context, pdi, mdWriter);
                  break;

               case ConstructorDocItem cdi:
                  RenderMethod(context, cdi, mdWriter);
                  break;

               case ConstructorOverloadGroupDocItem cogdi:
                  RenderMethods(context, cogdi, mdWriter);
                  break;

               case OperatorDocItem odi:
                  RenderMethod(context, odi, mdWriter);
                  break;

               case OperatorOverloadGroupDocItem oogdi:
                  RenderMethods(context, oogdi, mdWriter);
                  break;

               case NamespaceDocItem ndi:
                  RenderNamespace(context, ndi, writer);
                  break;

               default:
                  throw new NotImplementedException($"Unsupported page {context.CurrentItem.GetType()}");
            }
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

      private void RenderXmlDoc(RenderingContext context, XElement element, MarkdownWriter writer)
      {
         if (element == null || element.IsEmpty)
            return;

         context.Generator.RenderNodes2(context, element.Nodes(), new TO(writer, this));

      }

       private void RenderSummary(RenderingContext context, DocItem item, MarkdownWriter writer)
      {
         RenderXmlDoc(context, item.Documentation?.GetSummary(), writer);
      }

      private void RenderTitle(int level, string title, string anchor, MarkdownWriter writer)
      {
         writer.EnsureNewParagraph();
         writer.WriteHeading(level, title, anchor);
         writer.EnsureNewParagraph();
      }

      private void RenderMethods(RenderingContext context, MethodBaseOverloadGroupDocItem item, MarkdownWriter writer)
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

         // TODO PP (2020-08-26): Fix table
         //RenderTable(context, item.Members.Select(m => ((ILinkModel)context.ResolveLink(m), RenderXmlDoc(context, m.Documentation.GetSummary()))), writer);

         foreach (var method in item.Members)
         {
            RenderMethod(context, method, writer);
         }
      }

      private void RenderProperties(RenderingContext context, PropertyOverloadGroupDocItem item, MarkdownWriter writer)
      {
         // Render summary
         RenderSummary(context, item, writer);

         writer.WriteLine();
         writer.WriteLine("### Overloads");

         // TODO PP (2020-08-26): Fix table
         //RenderTable(context, item.Properties.Select(m => ((ILinkModel)context.ResolveLink(m), RenderXmlDoc(context, m.Documentation.GetSummary()))), writer);

         foreach (var property in item.Properties)
         {
            RenderProperty(context.WithItem(property), property, writer);
         }
      }

      private void RenderExample(RenderingContext context, DocItem item, MarkdownWriter writer)
      {
         XElement exampleElement = item.Documentation.GetExample();
         if (exampleElement != null)
         {
            writer.WriteHeading(2, "Examples");
            RenderXmlDoc(context, exampleElement, writer);
         }
      }

      private void RenderMethod(RenderingContext context, MethodBaseDocItem item, MarkdownWriter writer)
      {
         // TODO PP (2020-08-25): Write and header only if page.
         WriteHeader(context, item, writer);

         RenderTitle(1, $"{GetDisplayName(item)} {item.Method.SymbolKind}", item.AnchorId, writer);

         RenderXmlDoc(context, item.Documentation.GetSummary(), writer);

         writer.EnsureNewParagraph();

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

         // TODO PP (2020-08-24): Add anchor... perhaps WriteTitle method?
         RenderTitle(1, $"{item.Name} {item.Property.SymbolKind}", item.AnchorId, writer);

         RenderSummary(context, item, writer);

         writer.EnsureNewParagraph();

         writer.WriteCodeBlock(MethodCodeAmbience.ConvertSymbol(item.Property), "csharp");

         RenderParameters(context, item, writer);

         writer.WriteHeading(2, "Property Value");
         RenderLink(context, context.ResolveTypeLink(item.Property.ReturnType), writer);
         RenderXmlDoc(context, item.Documentation.GetValue(), writer);

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

      private void RenderSeeAlsos(RenderingContext context, DocItem item, MarkdownWriter writer)
      {
         IEnumerable<XElement> elements = item.Documentation.GetSeeAlsos();
         if (elements != null && elements.Any())
         {
            writer.WriteHeading(2, "See also");

            foreach (var element in elements)
            {
               // TODO PP (2020-08-26): Fix bullet list!
               
               //writer.WriteLine($"* {RenderLink(context, context.ResolveCrefLink(element.GetReferenceName(), RenderXmlDoc(context, element)))}");
            }
         }
      }

      private void RenderReturns(RenderingContext context, MethodBaseDocItem item, MarkdownWriter writer)
      {
         // TODO PP (2020-08-24): The item should expose this!
         if (item.Entity is IMember member && member.ReturnType != null && member.ReturnType.Kind != TypeKind.Void)
         {
            writer.WriteHeading(2, "Returns");

            RenderLink(context, context.ResolveTypeLink(member.ReturnType), writer);
            writer.EnsureNewParagraph();
            RenderXmlDoc(context, item.Documentation?.GetReturns(), writer);
         }
      }

      private void RenderParameters(RenderingContext context, IParameterizedDocItem item, MarkdownWriter writer)
      {
         if (item != null && item.Parameters.Any())
         {
            writer.WriteHeading(2, "Parameters");

            // TODO PP (2020-08-24): We should probably warn here if a parameter is missing. So should loop on the actual parameters of the method instead perhaps?
            foreach (var parameter in item.Parameters)
            {
               writer.WriteInlineCode(parameter.Name);
               writer.Write(" ");
               RenderLink(context, context.ResolveTypeLink(parameter.Parameter.Type), writer);

               XElement parameterElement = parameter.Documentation;
               if (parameterElement != null)
               {
                  writer.WriteLine();
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
            writer.WriteHeading(2, "Remarks");
            RenderXmlDoc(context, remarksElement, writer);
         }
      }

      private void RenderExceptions(RenderingContext context, DocItem item, MarkdownWriter writer)
      {
         var exceptionsElements = item.Documentation?.GetExceptions();
         if (exceptionsElements != null && exceptionsElements.Any())
         {
            writer.WriteHeading(1, "Exceptions");

            foreach (var exception in exceptionsElements)
            {
               // TODO PP (2020-08-24): Warn whenever we enounter an unresolved cref?
               RenderLink(context, context.ResolveCrefLink(exception.GetReferenceName()), writer);
               writer.WriteRaw("<br/>");
               RenderXmlDoc(context, exception, writer);
               writer.WriteLine();
            }
         }
      }

      private void RenderType(RenderingContext context, TypeDocItem item, MarkdownWriter writer)
      {
         WriteHeader(context, item, writer);

         RenderTitle(1, $"{item.Name} {item.Type.Kind}", item.AnchorId, writer);

         // TODO PP (2020-08-24): Add namespace and assembly if multiple assemblies in project!

         RenderXmlDoc(context, item.Documentation.GetSummary(), writer);

         // TODO PP (2020-08-26): Fix accssibility check here.
         List<IType> interfaces = item.Type.DirectBaseTypes.Where(t => t.Kind == TypeKind.Interface && t.GetDefinition().Accessibility == Accessibility.Public).ToList();

         // TODO PP (2020-08-26): Fix this
         writer.WriteCodeBlock(codeBlockWriter =>
         {
            codeBlockWriter.Write(CodeAmbience.ConvertSymbol(item.Type));
            IType baseType = item.Type.DirectBaseTypes.FirstOrDefault(t => t.Kind == TypeKind.Class && !t.IsKnownType(KnownTypeCode.Object) && !t.IsKnownType(KnownTypeCode.ValueType));
            if (baseType != null)
            {
               codeBlockWriter.Write(" : ");
               codeBlockWriter.Write(BaseTypeAmbience.ConvertType(baseType));
            }

            foreach (IType @interface in interfaces)
            {
               codeBlockWriter.WriteLine(baseType is null ? " :" : ",");
               baseType = item.Type;
               codeBlockWriter.Write(BaseTypeAmbience.ConvertType(@interface));
            }


         }, "csharp");

         if (item.Type.Kind == TypeKind.Class)
         {
            writer.Write("Inheritance ");
            foreach (var t in item.Type.GetNonInterfaceBaseTypes().Where(t => t != item.Type).AsSmart())
            {
               if (!t.IsFirst)
                  writer.WriteRaw(" &#129106; ");

               RenderLink(context, context.ResolveTypeLink(t.Value), writer);
            }

            writer.WriteRaw(" &#129106; ");
            writer.Write(item.Name);
         }

         List<TypeDocItem> derived = context.Project.Items.OfType<TypeDocItem>().Where(i => i.Type.DirectBaseTypes.Select(t => t is ParameterizedType g ? g.GetDefinition() : t).Contains(item.Type)).OrderBy(i => i.Type.FullName).ToList();
         if (derived.Count > 0)
         {
            writer.EnsureNewLine();
            writer.WriteLine("Derived");
            writer.WriteRaw("&#8627;");

            foreach (var d in derived.AsSmart())
            {
               if (!d.IsFirst)
               {
                  writer.EnsureNewLine();
                  writer.Write("&#8627;");
               }

               RenderLink(context, context.ResolveLink(d.Value), writer);
            }
         }

         if (interfaces.Count > 0)
         {
            writer.EnsureNewParagraph();
            writer.Write("Implements ");

            foreach (var ifc in interfaces.AsSmart())
            {
               if (!ifc.IsFirst)
                  writer.Write(", ");

               RenderLink(context, context.ResolveTypeLink(ifc.Value), writer);
            }
         }

         
         RenderMemberTable(context, "Constructors", item.AllConstructors().OrderBy(method => method.Parameters.Count()), writer);
         RenderMemberTable(context, "Fields", item.Fields, writer);
         RenderMemberTable(context, "Properties", item.AllProperties(), writer);
         RenderMemberTable(context, "Methods", item.AllMethods(), writer);
         RenderMemberTable(context, "Operators", item.AllOperators(), writer);

         RenderSeeAlsos(context, item, writer);
      }

      private void RenderMemberTable(RenderingContext context, string title, IEnumerable<DocItem> items, MarkdownWriter writer)
      {
         if (!items.Any())
            return;

         RenderTitle(2, title, null, writer);
         RenderTable(context, items.Select(i => ((ILinkModel)context.ResolveLink(i), i.Documentation.GetSummary())), writer);
      }

      private void RenderTable(RenderingContext context, IEnumerable<(ILinkModel Link, XElement docElement)> items, MarkdownWriter writer)
      {         
         if (items != null && items.Any())
         {
            var td = new MarkdownWriter.TableDefinition("Name", "Description");
            foreach (var item in items)
            {
               td.AddRow(new Action<MarkdownWriter>[]
               {
                  w => RenderLink(context, item.Link, w),
                  w => RenderXmlDoc(context, item.docElement, w)
               });

            }

            writer.WriteTable(td);          
         }
      }

      public void RenderLink(RenderingContext context, ILinkModel link, MarkdownWriter writer)
      {
         switch (link)
         {
            case InternalLinkModel internalLink:
               writer.WriteLink(internalLink.Text, $"{internalLink.FileName}{(internalLink.HasAnchor ? "#" + internalLink.Anchor : "")}");
               break;

            case ExternalLinkModel externalLink:
               writer.WriteLink(externalLink.Text, externalLink.Url);
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

      private void RenderTypeLink(RenderingContext context, TypeLinkModel link, MarkdownWriter writer)
      {
         RenderLink(context, link.TypeLink, writer);
         if (link.TypeArguments.Any())
         {
            writer.Write('<');
            bool isFirst = true;
            foreach (var arg in link.TypeArguments)
            {
               if (!isFirst)
                  writer.Write(", ");
               else
                  isFirst = false;

               RenderLink(context, arg, writer);
            }

            writer.Write('>');
         }

         if (link.Suffix != null)
         {
            RenderLink(context, link.Suffix, writer);
         }
      }
     
   }

   class TO : ITemplateOutputWriter
   {
      private readonly MarkdownWriter m_writer;
      private readonly MyTemplate2 m_template;

      public TO(MarkdownWriter writer, MyTemplate2 template)
      {
         m_writer = writer;
         m_template = template;
      }

      public void EndParagraph(RenderingContext context)
      {
         m_writer.EnsureNewParagraph();
      }

      public void StartParagraph(RenderingContext context)
      {
         m_writer.EnsureNewParagraph();
      }

      public void WriteText(RenderingContext context, string text)
      {
         if (m_writer.Column == 0)
            text = text.TrimStart();

         m_writer.Write(text);
      }

      public void WriteInlineCode(RenderingContext context, string text)
      {
         m_writer.WriteInlineCode(text);
      }

      public void WriteLink(RenderingContext context, ILinkModel link)
      {
         m_template.RenderLink(context, link, m_writer);
      }

      internal void WriteCodeBlock(RenderingContext context, string content, string language = null)
      {
         m_writer.WriteCodeBlock(content, language);
      }

      internal void WriteXml(string v)
      {
         m_writer.WriteRaw(v);
      }
   }

   interface ITemplateOutputWriter
   {
      void StartParagraph(RenderingContext context);
      void EndParagraph(RenderingContext context);
      void WriteText(RenderingContext context, string text);


   }


}
