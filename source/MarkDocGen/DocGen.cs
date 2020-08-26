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

      public void Generate(DocProject project, MyTemplate2 template, string outputDirectory)
      {
         foreach (var item in project.Items)
         {
            if (template.GeneratesPage(item))
            {
               string filePath = Path.Combine(outputDirectory, FileNameStrategy.GetFileName(item));

               using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
               using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
               {
                  RenderingContext renderingContext = new RenderingContext(this, template, item, FileNameStrategy, LinkResolver, Log);
                  template.RenderPage(renderingContext, writer);
               }
            }
         }
      }

       //TODO PP (2020-08-25): change RenderNodes to call into "StartParagraph" and "EndParagraph" or something... but.. hm... how do we handle the *content* of the paragraph then? .. Uhm.. we just started one... just continue rendering as normal.

      //public string RenderNodes(RenderingContext context, IEnumerable<XNode> nodes)
      //{
      //   if (nodes == null || !nodes.Any())
      //      return null;

      //   StringBuilder builder = new StringBuilder();
      //   var parent = context.CurrentItem;

      //   foreach (var node in nodes)
      //   {
      //      MyTemplate2 template = context.Template;
      //      switch (node)
      //      {
      //         case XText text:
      //            builder.Append(template.RenderText(context, text.Value));
      //            break;

      //         case XElement el:
      //            switch (el.Name.LocalName)
      //            {
      //               case "para":
      //                  builder.Append(template.RenderPara(context, RenderNodes(context, el.Nodes())));
      //                  break;

      //               case "see":
      //                  if (el.Attribute("cref") != null)
      //                  {
      //                     var link = context.ResolveCrefLink(el.Attribute("cref").Value, RenderNodes(context, el.Nodes()));
      //                     builder.Append(template.RenderLink(context, link));
      //                  }
      //                  else if (el.Attribute("langword") != null)
      //                  {
      //                     var link = context.ResolveLangWordLink(el.GetLangWord());
      //                     builder.Append(template.RenderLink(context, link));
      //                  }
      //                  else
      //                     throw new NotImplementedException("Logwarning");
      //                  break;

      //               case "c":
      //                  builder.Append(template.RenderC(context, RenderNodes(context, el.Nodes())));
      //                  break;

      //               case "code":
      //                  builder.Append(template.RenderCode(context, el.Value));
      //                  break;

      //               case "paramref":
      //                  var name = el.Attribute("name")?.Value;
      //                  if (name != null)
      //                  {
      //                     if (parent is IParameterizedDocItem pdi)
      //                     {
      //                        var parameter = pdi.Parameters.FirstOrDefault(p => p.Name == name);
      //                        if (parameter == null)
      //                        {
      //                           // TODO PP (2020-08-23): Log warning (invalid paramref)
      //                           builder.Append(template.RenderText(context, name));
      //                        }
      //                        else
      //                        {
      //                           var link = context.ResolveLink(parent, parameter.Name);
      //                           if (link != null)
      //                           {
      //                              builder.Append(template.RenderParamRef(context, link));
      //                           }
      //                           else
      //                           {
      //                              // TODO PP (2020-08-23): Add warning, non-existing link!?
      //                              builder.Append(template.RenderText(context, parameter.Name));
      //                           }
      //                        }
      //                     }
      //                     else
      //                     {
      //                        // TODO PP (2020-08-23): Warning error failure etc... 
      //                     }
      //                  }
      //                  else
      //                  {
      //                     // TODO PP (2020-08-23): Warning log?
      //                  }
      //                  break;
      //               default:
      //                  using (StringWriter writer = new StringWriter())
      //                  {
      //                     writer.Write("<");
      //                     writer.Write(el.Name.LocalName);
      //                     if (el.HasAttributes)
      //                     {
      //                        foreach (var attr in el.Attributes())
      //                        {
      //                           writer.Write(' ');
      //                           writer.Write(attr.Name.LocalName);
      //                           writer.Write("=\"");
      //                           writer.Write(attr.Value);
      //                           writer.Write('\"');
      //                        }
      //                     }
      //                     if (el.IsEmpty)
      //                     {
      //                        writer.Write("/>");
      //                     }
      //                     else
      //                     {
      //                        writer.Write('>');
      //                        writer.Write(RenderNodes(context, el.Nodes()));
      //                        writer.Write($"</{el.Name.LocalName}>");
      //                     }

      //                     // TODO PP (2020-08-23): wtf? builder + writer?
      //                     builder.Append(writer.ToString());
      //                  }

      //                  break;
      //            }
      //            break;

      //         default:
      //            throw new NotImplementedException();
      //      }

      //   }

      //   return builder.ToString();
      //}

      public void RenderNodes2(RenderingContext context, IEnumerable<XNode> nodes, TO writer)
      {
         if (nodes == null || !nodes.Any())
            return;

         var parent = context.CurrentItem;

         foreach (var node in nodes)
         {
            MyTemplate2 template = context.Template;
            switch (node)
            {
               case XText text:                  
                  writer.WriteText(context, text.Value);
                  break;

               case XElement el:
                  switch (el.Name.LocalName)
                  {
                     case "para":
                        writer.StartParagraph(context);
                        RenderNodes2(context, el.Nodes(), writer);
                        writer.EndParagraph(context);
                        break;

                     case "see":
                        if (el.Attribute("cref") != null)
                        {
                           var link = context.ResolveCrefLink(el.Attribute("cref").Value, String.IsNullOrEmpty(el.Value) ? null : el.Value);
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
                        writer.WriteCodeBlock(context, el.Value, el.Attribute("lang")?.Value);                        
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
                                 writer.WriteText(context, name);
                              }
                              else
                              {
                                 var link = context.ResolveLink(parent, parameter.Name);
                                 if (link != null)
                                 {
                                    writer.WriteLink(context, link);
                                 }
                                 else
                                 {
                                    // TODO PP (2020-08-23): Add warning, non-existing link!?
                                    writer.WriteText(context, parameter.Name);
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
                        using (StringWriter swriter = new StringWriter())
                        {
                           swriter.Write("<");
                           swriter.Write(el.Name.LocalName);
                           if (el.HasAttributes)
                           {
                              foreach (var attr in el.Attributes())
                              {
                                 swriter.Write(' ');
                                 swriter.Write(attr.Name.LocalName);
                                 swriter.Write("=\"");
                                 swriter.Write(attr.Value);
                                 swriter.Write('\"');
                              }
                           }
                           if (el.IsEmpty)
                           {
                              swriter.Write("/>");
                              writer.WriteXml(swriter.ToString());                              
                           }
                           else
                           {
                              swriter.Write('>');
                              writer.WriteXml(swriter.ToString());
                              RenderNodes2(context, el.Nodes(), writer);
                              writer.WriteXml($"</{el.Name.LocalName}>");
                           }
                        }

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
