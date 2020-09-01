using DefaultDocumentation.Helper;
using DefaultDocumentation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MarkDocGen
{
   class DocumentationGenerator
   {
      public DocumentationGenerator(IFileNameStrategy fileNameStrategy, ILinkResolver linkResolver, ILogger log)
      {
         FileNameStrategy = fileNameStrategy;
         LinkResolver = linkResolver;
         Log = log ?? NullLogger.Instance;
      }

      public ILogger Log { get; }
      public IFileNameStrategy FileNameStrategy { get; }
      public ILinkResolver LinkResolver { get; }

      public void Generate(DocProject project, ITemplate template, string outputDirectory)
      {
         foreach (var item in project.Items)
         {
            var renderers = template.PageRenderers;
            foreach (var renderer in renderers.Where(r => r.Supports(item)))
            {
               string filePath = Path.Combine(outputDirectory, renderer.GetFileName(item));

               Log.LogDebug("Generating {File}", filePath);
               if (File.Exists(filePath))
                  throw new InvalidOperationException($"Internal error; The file {filePath} already exists. Duplicate file names generated?");

               using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
               using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
               {
                  RenderingContext renderingContext = new RenderingContext(this, template, item, FileNameStrategy, LinkResolver, Log);
                  renderer.RenderPage(renderingContext, item, writer);                  
               }
            }
         }
      }

      public void RenderNodes(RenderingContext context, IEnumerable<XNode> nodes, IXmlDocWriter writer)
      {
         if (nodes == null || !nodes.Any())
            return;

         var parent = context.CurrentItem;
         
         foreach (var node in nodes)
         {
            ITemplate template = context.Template;
            switch (node)
            {
               case XText text:
                  writer.WriteText(context, text.Value);
                  break;

               case XElement el:
                  switch (el.Name.LocalName)
                  {
                     case "b":
                        writer.WriteStartBold();
                        RenderNodes(context, el.Nodes(), writer);
                        writer.WriteEndBold();
                        break;

                     case "i":
                     case "em":
                        writer.WriteStartItalic();
                        RenderNodes(context, el.Nodes(), writer);
                        writer.WriteEndItalic();
                        break;

                     case "para":
                        writer.WriteStartParagraph();
                        RenderNodes(context, el.Nodes(), writer);
                        writer.WriteEndParagraph();
                        break;

                     case "see":
                        if (el.Attribute("cref") != null)
                        {
                           var link = context.ResolveCrefLink(el.Attribute("cref").Value, String.IsNullOrWhiteSpace(el.Value) ? null : el.Value);
                           writer.WriteLink(context, link);                           
                        }
                        else if (el.Attribute("langword") != null)
                        {
                           var link = context.ResolveLangWordLink(el.GetLangWord());
                           writer.WriteLink(context, link);
                        }
                        else
                           throw new NotImplementedException("Logwarning");
                        break;

                     case "c":
                        writer.WriteInlineCode(context, el.Value);
                        break;

                     case "code":
                        writer.WriteCodeBlock(context, el.Value);
                        break;

                     case "list":
                        RenderList(context, el, writer);
                        break;

                     case "typeparamref":
                        writer.WriteTypeParamRef(context, el.Attribute("name")?.Value);
                        break;

                     case "paramref":
                        var name = el.Attribute("name")?.Value;
                        if (name != null)
                           writer.WriteParamRef(context, name);                        
                        break;
                     default:
                        Log.LogWarning("Unsupported XML element <{ElementName}> in {CurrentItem}.", el.Name.LocalName, context.CurrentItem.Id);
                        //using (StringWriter writer = new StringWriter())
                        //{
                        //   writer.Write("<");
                        //   writer.Write(el.Name.LocalName);
                        //   if (el.HasAttributes)
                        //   {
                        //      foreach (var attr in el.Attributes())
                        //      {
                        //         writer.Write(' ');
                        //         writer.Write(attr.Name.LocalName);
                        //         writer.Write("=\"");
                        //         writer.Write(attr.Value);
                        //         writer.Write('\"');
                        //      }
                        //   }
                        //   if (el.IsEmpty)
                        //   {
                        //      writer.Write("/>");
                        //   }
                        //   else
                        //   {
                        //      writer.Write('>');
                        //      writer.Write(RenderNodes(context, el.Nodes()));
                        //      writer.Write($"</{el.Name.LocalName}>");
                        //   }

                        //   builder.Append(writer.ToString());
                        //}

                        break;
                  }
                  break;

               default:
                  throw new NotImplementedException();
            }

         }
      }

      //private T FindParent<T>(DocItem item) 
      //{
      //   var current = item;

      //   while (current != null)
      //   {
      //      if (current is T typedItem)
      //         return typedItem;

      //      current = current.Parent;
      //   }

      //   return null;
      //}

      private void RenderList(RenderingContext context, XElement el, IXmlDocWriter writer)
      {
         const string TYPE_BULLET = "bullet";
         const string TYPE_NUMBER = "number";
         const string TYPE_TABLE = "table";

         var type = el.Attribute("type")?.Value ?? "bullet";

         var listHeaderEl = el.Element("listheader");

         if (listHeaderEl != null && type != TYPE_TABLE)
            Log.LogWarning("<listheader> is not supporteed in a list type other than \"{Table}\" at {Item}", TYPE_TABLE, context.CurrentItem.Id);

         var itemEls = el.Elements("item");

         switch (type)
         {
            case TYPE_BULLET:
               RenderList(context, itemEls, ListType.Bullet, writer);
               break;
            case TYPE_NUMBER:
               RenderList(context, itemEls, ListType.Number, writer);
               break;
            case TYPE_TABLE:
               RenderTable(context, listHeaderEl, itemEls, writer);
               break;
            default:
               Log.LogWarning("Unsupported list type {Type} at {Item}", type, context.CurrentItem.Id);
               break;
         }
      }

      private void RenderTable(RenderingContext context, XElement listHeaderEl, IEnumerable<XElement> itemEls, IXmlDocWriter writer)
      {
         if (listHeaderEl == null)
         {            
            Log.LogWarning("<list> of type \"table\" without a <listheader> is not supported at {Item}", context.CurrentItem.Id);          
         }

         int columnCount = Math.Max(listHeaderEl.Elements("term").Count(), itemEls.Max(e => e.Elements("term").Count()));

         writer.WriteStartTable(columnCount);
         if (listHeaderEl != null)
         {
            writer.WriteStartTableHeader();
            foreach (var header in listHeaderEl.Elements("term"))
            {
               writer.WriteStartTableCell();
               writer.WriteText(context, header.Value);
               writer.WriteEndTableCell();
            }
            writer.WriteEndTableHeader();
         }

         foreach (var row in itemEls)
         {
            writer.WriteStartTableRow();
            foreach (var column in row.Elements("term"))
            {
               writer.WriteStartTableCell();
               RenderNodes(context, column.Nodes(), writer);
               writer.WriteEndTableCell();
            }
            writer.WriteEndTableRow();
         }

         writer.WriteEndTable();
      }

      private void RenderList(RenderingContext context, IEnumerable<XElement> itemEls, ListType listType, IXmlDocWriter writer)
      {
         writer.WriteStartList(listType);
         int itemNumber = 1;
         foreach (var item in itemEls)
         {
            var term = item.Element("term");
            var description = item.Element("description");
            if (description == null)
            {
               Log.LogWarning("Missing <description> in list defined at {Item}", context.CurrentItem.Id);
            }

            writer.WriteStartListItem(itemNumber++, listType);
            if (term != null && !term.IsEmpty)
            {
               writer.WriteListItemTerm(term.Value);
               if (description != null)
                  writer.WriteText(context, " - ");
            }

            if (description != null)
            {
               RenderNodes(context, description.Nodes(), writer);
            }
            writer.WriteEndListItem(listType);
         }

         writer.WriteEndList(listType);
      }
   }
}
