using DefaultDocumentation.Model;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MarkDocGen
{
   class Program
   {
      static void Main(string[] args)
      {
         var element = XElement.Parse("<summary><para apa=\"1\">This is some <b><i>bold and italic</i></b> text, and some in <c>Code</c> with a see <see cref=\"T:System.Collections.Generic.IEnumerable`1\" />. That's it!</para><para>And this is a second paragraph.\r\nAnd this is a see with content: <see cref=\"T:System.String\">A string link</see></para>" + 
            "<para>And a parameter reference: <paramref name=\"value\"/> and a non existing one in <paramref name=\"other\"/></para>" +
            "</summary>");



         var assemblyPath = @"D:\Git\DefaultDocumentation\source\Dummy\bin\Debug\netstandard2.0\Dummy.dll";
         var assemblyDocPath = @"D:\Git\DefaultDocumentation\source\Dummy\bin\Debug\netstandard2.0\Dummy.xml";
         var outputDirectory = "F:\\Temp\\Render";

         var logger = new LoggerConfiguration()
            .WriteTo.Console().MinimumLevel.Verbose().CreateLogger();

         Microsoft.Extensions.Logging.ILogger msLog = new SerilogLoggerProvider(logger).CreateLogger("log");
         DocProject project = new DocProject(msLog);
         project.Add(assemblyPath, assemblyDocPath);

        // TemplateSystem templates = new TemplateSystem(@"D:\Git\DefaultDocumentation\source\MarkDocGen\Template", log: msLog);

         IFileNameStrategy fileNameStrategy = new DefaultFileNameStrategy();
         //Renderer renderer = new Renderer(templates, fileNameStrategy);

         if (Directory.Exists(outputDirectory))
         {
            Empty(new DirectoryInfo(outputDirectory));            
         }
         Directory.CreateDirectory(outputDirectory);

         LinkResolver resolver = new LinkResolver(new DefaultFileNameStrategy());

         MyTemplate2 template = new MyTemplate2();
         DocGen generator = new DocGen(DefaultFileNameStrategy.Instance, resolver, msLog);

         generator.Generate(project, template, @"F:\Temp\Render");
         //var item = project.Items.OfType<MethodDocItem>().First(m => m.Method.Name == "DummyAsync");
         //Console.WriteLine(generator.RenderNodes(new RenderingContext(generator, template, item, fileNameStrategy, resolver, msLog), element.Nodes()));
      }

      public static void Empty(System.IO.DirectoryInfo directory)
      {
         foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
         foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
      }
   }

}
