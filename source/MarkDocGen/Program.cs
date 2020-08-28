using DefaultDocumentation.Model;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
         //            w.Write("## Paragraph 1");
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
         //         }

         //         return;

         var element = XElement.Parse("<summary><para apa=\"1\">This is some <b><i>bold and italic</i></b> text, and some in <c>Code</c> with a see <see cref=\"T:System.Collections.Generic.IEnumerable`1\" />. That's it!</para><para>And this is a second paragraph.\r\nAnd this is a see with content: <see cref=\"T:System.String\">A string link</see></para>" +
            "<para>And a parameter reference: <paramref name=\"value\"/> and a non existing one in <paramref name=\"other\"/></para>" +
            "</summary>");



         var assemblyPath = @"D:\Git\DefaultDocumentation\source\Dummy\bin\Debug\netstandard2.0\Dummy.dll";
         //assemblyPath = @"F:\ETWork\Bin\iCore.Public.Types.dll";
         var assemblyDocPath = Path.ChangeExtension(assemblyPath, ".xml");            
         var outputDirectory = @"F:\WIP\d\docs\iCoreCodedApi";

         var logger = new LoggerConfiguration()
            .WriteTo.Console().MinimumLevel.Verbose().CreateLogger();

         Microsoft.Extensions.Logging.ILogger msLog = new SerilogLoggerProvider(logger).CreateLogger("log");
         DocProject project = new DocProject(msLog);
         project.Add(assemblyPath, assemblyDocPath);

         //var cls = project.Items.OfType<ConstructorDocItem>().First(c => c.Method.ReflectionName.Contains("SubDummy") &&c.Method.Parameters.Count > 1);
         foreach (var cls in project.Items.OfType<EntityDocItem>())
         {
            Console.WriteLine();
            IEntity t = cls.Entity;
            Console.WriteLine($"ReflectionName: {t.ReflectionName}");
            Console.WriteLine($"FullName: {t.FullName}");
            Console.WriteLine($"Name: {t.Name}");
            var amb = new CSharpAmbience();
            amb.ConversionFlags = ConversionFlags.None;
            Console.WriteLine($"AmbNone: {amb.ConvertSymbol(t)}");
            amb.ConversionFlags = ConversionFlags.StandardConversionFlags;
            Console.WriteLine($"AmbAll:  {amb.ConvertSymbol(t)}");
         }
         //return;
         // TemplateSystem templates = new TemplateSystem(@"D:\Git\DefaultDocumentation\source\MarkDocGen\Template", log: msLog);

         IFileNameStrategy fileNameStrategy = new DefaultFileNameStrategy();
         //Renderer renderer = new Renderer(templates, fileNameStrategy);

         if (Directory.Exists(outputDirectory))
         {
            Empty(new DirectoryInfo(outputDirectory));
         }
         Directory.CreateDirectory(outputDirectory);

         DefaultLinkResolver resolver = new DefaultLinkResolver(new DefaultFileNameStrategy());

         DocusaurusTemplate template = new DocusaurusTemplate(fileNameStrategy);
         DocGen generator = new DocGen(DefaultFileNameStrategy.Instance, resolver, msLog);

         generator.Generate(project, template, outputDirectory);

         SideBarTemplate sbt = new SideBarTemplate(template, "iCoreCodedApi");
         generator.Generate(project, sbt, outputDirectory);


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
