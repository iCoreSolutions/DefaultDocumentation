using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace MarkDocGen
{
   class TemplateLoader : ITemplateLoader
   {
      public TemplateLoader(string baseDirectory)
      {
         BaseDirectory = baseDirectory;
      }

      public string BaseDirectory { get; }

      public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
      {
         return Path.Combine(BaseDirectory, templateName) + ".sbntxt";
      }

      public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
      {
         return File.ReadAllText(templatePath);
      }

      public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
      {
         return await File.ReadAllTextAsync(templatePath).ConfigureAwait(false);
      }
   }
}
