using DefaultDocumentation.Model;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MarkDocGen
{
   class Program
   {
      static void Main(string[] args)
      {
//         using (StreamWriter sw = new StreamWriter("F:\\test_md.md"))
//         {
//            MarkdownWriter w = new MarkdownWriter(sw);

//            w.WriteLine("Hello World");
//            w.EnsureNewParagraph();
//            w.Write("## Paragrap44h 1");
//            w.EnsureNewParagraph();
//            w.EnsureNewParagraph();
//            w.EnsureNewParagraph();
//            w.Write("Paragraph 2");
//            w.EnsureNewParagraph();
//            w.WriteLine("  1. Pelle");
//            w.WriteLine(" [Hello () World]");
//            w.WriteLine(" And this is a sentence. The previous #dot should not be escaped.");
//            w.WriteLink("[(My display Text)", "Pelle.md#Section1");
//            w.EnsureNewParagraph();
//            w.WriteInlineCode("Command`1");
//            w.WriteCodeBlock(@"public void WriteCodeBlock(string content, string language = null)
//      {
//         if (Column != 0)
//            WriteLine();

//         WriteRaw($""```{ language}"");
//         WriteLine();
//            WriteRaw(content);

//            if (Column != 0)
//               WriteLine();
//            WriteRaw(""```"");
//            WriteLine();
//", "csharp");

//            w.EnsureNewParagraph();

//            MarkdownWriter.TableDefinition td = new MarkdownWriter.TableDefinition(new[] { "Apa", "Pelle", "Peter är här lite längre\nOch detta är en ny rad.", "Cepa" });
//            td.AddRow(new[] { "Pelle", "Gustav", "d|d", "A longer line this is... yes!" });
//            td.AddRow(new[] { "Gurra" });
//            w.WriteTable(td);
//            w.WriteHeading(1, "This is a heading");
//            w.WriteHeading(6, "This is a heading\r\nwith a newline.");
//            w.EnsureNewParagraph();

//            w.WriteBlockQuote("This is my block quote.\nAnd after a CR\nAnd after an LF\r\nAnd after a CRLF.\r\n\r\n");
//         }

//         return;

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

         DefaultLinkResolver resolver = new DefaultLinkResolver(new DefaultFileNameStrategy());

   MyTemplate2 template = new MyTemplate2();
   DocGen generator = new DocGen(DefaultFileNameStrategy.Instance, resolver, msLog);

   generator.Generate(project, template, @"F:\Temp\Render");

         using (var wr = new StreamWriter("F:\\list.txt"))
         {
            foreach (var item in project.Items)
            {
               wr.WriteLine($"{item.Kind}     Id={item.Id}");
            }
         }
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
