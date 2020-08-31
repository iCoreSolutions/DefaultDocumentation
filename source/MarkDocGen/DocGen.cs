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

   // TODO PP (2020-08-23): Rename and stuff.
   class DocGen
   {
      public DocGen(IFileNameStrategy fileNameStrategy, ILinkResolver linkResolver, ILogger log)
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

       //TODO PP (2020-08-25): change RenderNodes to call into "StartParagraph" and "EndParagraph" or something... but.. hm... how do we handle the *content* of the paragraph then? .. Uhm.. we just started one... just continue rendering as normal.

      public void RenderNodes(RenderingContext context, IEnumerable<XNode> nodes, IXmlDocWriter renderer, bool trim = true)
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
                  renderer.WriteText(context, text.Value);
                  break;

               case XElement el:
                  switch (el.Name.LocalName)
                  {
                     // TODO PP (2020-08-31): Add support for support <b>, <i>, <em>, <typeparamref>, <list>
                     case "para":
                        renderer.WriteStartParagraph();
                        RenderNodes(context, el.Nodes(), renderer);
                        renderer.WriteEndParagraph();
                        break;

                     case "see":
                        if (el.Attribute("cref") != null)
                        {
                           var link = context.ResolveCrefLink(el.Attribute("cref").Value, String.IsNullOrWhiteSpace(el.Value) ? null : el.Value);
                           renderer.WriteLink(context, link);                           
                        }
                        else if (el.Attribute("langword") != null)
                        {
                           var link = context.ResolveLangWordLink(el.GetLangWord());
                           renderer.WriteLink(context, link);
                        }
                        else
                           throw new NotImplementedException("Logwarning");
                        break;

                     case "c":
                        renderer.WriteInlineCode(context, el.Value);
                        break;

                     case "code":
                        renderer.WriteCodeBlock(context, el.Value);
                        break;

                     case "paramref":
                        var name = el.Attribute("name")?.Value;
                        if (name != null)
                        {
                           if (parent is IParameterizedDocItem pdi)
                           {
                              var parameter = pdi.Parameters.FirstOrDefault(p => p.Name == name);
                              if (parameter == null)
                              {
                                 // TODO PP (2020-08-23): Log warning (invalid paramref)
                                 Log.LogWarning("Invalid paramref to parameter \"{Name}\" in {CurrentItem}.", name, context.CurrentItem.Id);
                                 renderer.WriteText(context, name);
                              }
                              else
                              {
                                 var link = context.ResolveLink(parent, parameter.Name);
                                 if (link != null)
                                 {
                                    renderer.WriteLink(context, link);
                                 }
                                 else
                                 {
                                    Log.LogWarning("Invalid paramref to parameter \"{Name}\" in {CurrentItem}.", name, context.CurrentItem.Id);
                                    renderer.WriteText(context, parameter.Name);
                                 }
                              }
                           }
                           else
                           {
                              // TODO PP (2020-08-23): Warning error failure etc... 
                           }
                        }
                        else
                        {
                           // TODO PP (2020-08-23): Warning log?
                        }
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
   }
}
