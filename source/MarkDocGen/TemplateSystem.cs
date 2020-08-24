using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultDocumentation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace MarkDocGen
{
   // TODO PP (2020-08-23): Remove commented code.
   //class TemplateSystem
   //{
   //   public TemplateSystem(string templateDirectory, string templateExtension = ".sbntxt", ILogger log = null)
   //   {
   //      TemplateLoader = new TemplateLoader(templateDirectory);

   //      Log = log ?? NullLogger.Instance;

   //      if (!Directory.Exists(templateDirectory))
   //         throw new DirectoryNotFoundException($"The specified template directory \"{templateDirectory}\" does not exist.");

   //      if (!templateExtension.StartsWith("."))
   //         templateExtension = "." + templateExtension;

   //      Dictionary<DocItemKind, Template> pageTemplates = new Dictionary<DocItemKind, Template>();

   //      bool hasErrors = false;
   //      foreach (DocItemKind kind in Enum.GetValues(typeof(DocItemKind)))
   //      {
   //         string filePath = Path.Combine(templateDirectory, $"page.{kind.ToString()}{templateExtension}");
   //         if (File.Exists(filePath))
   //         {
   //            Log.LogDebug("Loading page template \"{File}\".", filePath);
   //            Template template = Template.Parse(File.ReadAllText(filePath), filePath, null);

   //            if (template.HasErrors)
   //            {
   //               foreach (var message in template.Messages)
   //                  Log.LogError(message.ToString());

   //               hasErrors = true;
   //            }

   //            pageTemplates.Add(kind, template);
   //         }
   //         else
   //         {
   //            Log.LogDebug("No page template found for {Kind}", kind);
   //         }
   //      }

   //      if (hasErrors)
   //      {
   //         throw new ArgumentException($"One or more templates contained errors. Execution aborted.");
   //      }

   //      TemplateLoader = new TemplateLoader(templateDirectory);
   //      FileTemplates = pageTemplates;

   //   }

   //   private ILogger Log { get; }

   //   public IReadOnlyDictionary<DocItemKind, Template> FileTemplates { get; }

   //   public ITemplateLoader TemplateLoader { get; }

   //   public bool IsPage(DocItem item)
   //   {
   //      return FileTemplates.ContainsKey(item.Kind);
   //   }
   //}
}
