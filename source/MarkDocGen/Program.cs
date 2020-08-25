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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MarkDocGen
{
   [Flags]
   public enum MarkdownEscapeMode
   {
      None = 0,
      Entities = 0x01,
      Normal = 0x02 | Entities,
      LinkContent = 0x04 | Normal,
      Code = 0x08,
      TableCell = 0x10 | Normal | NewLines,
      NewLines = 0x20
   }

   class MarkdownWriter
   {
      private readonly TextWriter m_writer;
      private bool m_lastLineEmpty = true;
      private bool m_digitsOnly = false;
      private bool m_hasWrittenNonWhitespace = false;
      private bool m_hasPendingCr = false;

      private readonly static char[] s_escapeDefault = "`[]\\".ToCharArray();
      private readonly static char[] s_escapeDefaultLineStart = "|#*-+".ToCharArray();
      private readonly static char[] s_escapeLinkText = s_escapeDefault.Concat("()").ToArray();

      public MarkdownWriter(TextWriter writer)
      {
         m_writer = writer;
      }

      public int Line { get; private set; }

      public int Column { get; private set; }

      public void Write(char c)
      {
         Write(c, MarkdownEscapeMode.Normal);
      }

      public void Write(string s)
      {
         if (s == null)
            return;

         foreach (var c in s)
            Write(c);
      }

      public void Write(char c, MarkdownEscapeMode mode)
      {
         if (c == '\r' && !m_hasPendingCr)
         {
            m_hasPendingCr = true;
            return;
         }

         if (c == '\n')
         {            
            if (mode.HasFlag(MarkdownEscapeMode.NewLines))
            {
               WriteRaw("<br/>");
               return;
            }

            if (m_hasPendingCr)
               m_writer.Write('\r');
            
            Line++;
            Column = 0;

            if (!m_hasWrittenNonWhitespace)
               m_lastLineEmpty = true;
            else
               m_lastLineEmpty = false;

            m_hasWrittenNonWhitespace = false;
            m_digitsOnly = false;
         }
         else
         {
            Column++;
         }

         m_hasPendingCr = false;

         if (Char.IsDigit(c))
         {
            if (!m_hasWrittenNonWhitespace)
               m_digitsOnly = true;
         }
         else if (c != '.')
         {
            m_digitsOnly = false;
         }

         bool hadWrittenNonWhitespace = m_hasWrittenNonWhitespace;
         if (!Char.IsWhiteSpace(c))
         {
            m_hasWrittenNonWhitespace = true;
         }

         if (mode.HasFlag(MarkdownEscapeMode.Entities))
         {
            switch (c)
            {
               case '<':
                  WriteRaw("&lt;");
                  return;

               case '>':
                  WriteRaw("&gt;");
                  return;

               case '&':
                  WriteRaw("&amp;");
                  return;
            }
         }

         if (mode.HasFlag(MarkdownEscapeMode.Normal))
         {
            if (c == '.' && m_digitsOnly)
            {
               m_digitsOnly = false;
               WriteRaw("\\.");
               return;
            }

            if (s_escapeDefaultLineStart.Contains(c) && !hadWrittenNonWhitespace || s_escapeDefault.Contains(c))
            {
               WriteRaw($"\\{c}");
               return;
            }
         }

         if (mode.HasFlag(MarkdownEscapeMode.LinkContent))
         {
            if (s_escapeLinkText.Contains(c))
            {
               WriteRaw($"\\{c}");
               return;
            }
         }
         
         if (mode.HasFlag(MarkdownEscapeMode.Code))
         {
            if (c == '`')
            {
               WriteRaw("``");
               return;
            }
         }

         if (mode.HasFlag(MarkdownEscapeMode.TableCell))
         {
            if (c == '|')
            {
               WriteRaw($"&#124;");
               return;
            }            
         }

         if (!Char.IsWhiteSpace(c) && !Char.IsDigit(c))
            m_digitsOnly = false;

         m_writer.Write(c);
      }

      public void Write(string s, MarkdownEscapeMode mode)
      {
         foreach (var c in s)
            Write(c, mode);
      }

      public void WriteRaw(char c)
      {
         Write(c, MarkdownEscapeMode.None);
      }

      public void WriteRaw(string s)
      {
         foreach (var c in s)
            Write(c, MarkdownEscapeMode.None);
      }

      public void WriteLine()
      {
         Write(Environment.NewLine);
      }

      public void WriteLine(string s)
      {
         Write(s);
         WriteLine();
      }

      public void Flush() => m_writer.Flush();

      public void Write(ReadOnlySpan<char> buffer)
      {
         foreach (var c in buffer)
            Write(c);
      }

      public void Write(object o)
      {
         if (o == null)
            return;

         Write(o.ToString());
      }

      public void WriteLine(object o)
      {
         if (o == null)
            return;

         WriteLine(o.ToString());
      }

      public void EnsureNewParagraph()
      {
         EnsureNewLine();

         if (!m_lastLineEmpty)
            WriteLine();
      }

      public void EnsureNewLine()
      {
         if (Column > 0)
            WriteLine();
      }

      public void WriteLink(string displayText, string linkTarget)
      {
         if (displayText == null)
            displayText = linkTarget;

         WriteRaw("[");
         Write(displayText, MarkdownEscapeMode.LinkContent);
         WriteRaw("](");
         Write(linkTarget, MarkdownEscapeMode.LinkContent);
         WriteRaw(")");
      }

      public void WriteInlineCode(string text)
      {
         if (text.Contains("`"))
         {
            WriteRaw($"``{text}``");
         }
         else
         {
            WriteRaw($"`{text}`");
            WriteRaw('`');
         }
      }

      public void WriteCodeBlock(string content, string language = null)
      {
         if (Column != 0)
            WriteLine();

         WriteRaw($"```{language}");
         WriteLine();
         WriteRaw(content);

         if (Column != 0)
            WriteLine();
         WriteRaw("```");
         WriteLine();
      }

      public void WriteTable(TableDefinition tableDefinition)
      {
         var headers = tableDefinition.Headers;
         int columnCount = tableDefinition.Headers.Count;
         
         int[] maxLengths = headers.Select(s => s?.Length ?? 0).ToArray();
         foreach (var row in tableDefinition.Rows)
         {
            for (int i = 0; i < columnCount && i < row.Count; i++)
            {
               maxLengths[i] = Math.Max(maxLengths[i], row[i]?.Length ?? 0);
            }
         }

         WriteRow(headers);

         for (int i = 0; i < columnCount; i++)
         {
            if (i == 0)
               WriteRaw('|');

            WriteRaw(new string('-', maxLengths[i] + 2));

            WriteRaw('|');
         }

         WriteLine();

         foreach (var row in tableDefinition.Rows)
         {
            WriteRow(row);
         }

         void WriteRow(IReadOnlyList<string> row)
         {
            for (int i = 0; i < columnCount; i++)
            {
               if (i == 0)
                  WriteRaw("| ");
               else
                  WriteRaw(" ");
               
               WriteRaw(row[i]);
               if (row[i].Length < maxLengths[i])
                  Write(new string(' ', maxLengths[i] - row[i].Length));

               WriteRaw(" |");
            }

            WriteLine();
         }
      }

      public void WriteHeading(int level, string text)
      {
         if (level < 1 || level > 6)
            throw new ArgumentOutOfRangeException(nameof(level), $"Level must be between 1-6");

         EnsureNewLine();
         WriteRaw(new string('#', level));
         Write(' ');
         Write(text, MarkdownEscapeMode.Normal | MarkdownEscapeMode.NewLines);
         EnsureNewLine();
      }

      public void WriteBold(string text)
      {
         WriteRaw('**');
         Write(text);
         WriteRaw('**');
      }

      public class TableDefinition
      {
         private readonly IReadOnlyList<string> headers;
         private readonly List<string[]> rows = new List<string[]>();

         public TableDefinition(IEnumerable<string> columns)
            : this(columns, null)
         {
         }

         public IReadOnlyList<string> Headers => headers;
         public IReadOnlyList<IReadOnlyList<string>> Rows => rows;

         public TableDefinition(IEnumerable<string> columns, IEnumerable<IEnumerable<string>> rows)
         {
            headers = columns.Select(c => Transform(c)).ToList();
            if (rows != null)
            {
               foreach (var row in rows)
                  AddRow(row);
            }
         }

         public void AddRow(IEnumerable<string> row)
         {
            var targetRow = new string[headers.Count];
            using (IEnumerator<string> enumerator = row.GetEnumerator())
            {
               for (int i = 0; i < headers.Count; i++)
               {
                  if (enumerator.MoveNext())
                     targetRow[i] = Transform(enumerator.Current);
                  else
                     targetRow[i] = Transform(null);
               }
            }

            rows.Add(targetRow);
         }

         private string Transform(string input)
         {
            if (String.IsNullOrWhiteSpace(input))
               return "<!-- -->";

            using (StringWriter swriter = new StringWriter())
            {
               MarkdownWriter writer = new MarkdownWriter(swriter);
               writer.Write(input, MarkdownEscapeMode.TableCell);
               return swriter.ToString();
            }
         }
      }
   }

   class Program
   {
      static void Main(string[] args)
      {
         using (StreamWriter sw = new StreamWriter("F:\\test_md.md"))
         {
            MarkdownWriter w = new MarkdownWriter(sw);

            w.WriteLine("Hello World");
            w.EnsureNewParagraph();
            w.Write("## Paragraph 1");
            w.EnsureNewParagraph();
            w.EnsureNewParagraph();
            w.EnsureNewParagraph();
            w.Write("Paragraph 2");
            w.EnsureNewParagraph();
            w.WriteLine("  1. Pelle");
            w.WriteLine(" [Hello () World]");
            w.WriteLine(" And this is a sentence. The previous #dot should not be escaped.");
            w.WriteLink("[(My display Text)", "Pelle.md#Section1");
            w.EnsureNewParagraph();
            w.WriteInlineCode("Command`1");
            w.WriteCodeBlock(@"public void WriteCodeBlock(string content, string language = null)
      {
         if (Column != 0)
            WriteLine();

         WriteRaw($""```{ language}"");
         WriteLine();
            WriteRaw(content);

            if (Column != 0)
               WriteLine();
            WriteRaw(""```"");
            WriteLine();
", "csharp");

            w.EnsureNewParagraph();

            MarkdownWriter.TableDefinition td = new MarkdownWriter.TableDefinition(new[] { "Apa", "Pelle", "Peter är här lite längre\nOch detta är en ny rad.", "Cepa" });
            td.AddRow(new[] { "Pelle", "Gustav", "d|d", "A longer line this is... yes!" });
            td.AddRow(new[] { "Gurra" });
            w.WriteTable(td);
            w.WriteHeading(1, "This is a heading");
            w.WriteHeading(6, "This is a heading\r\nwith a newline.");
         }
               
         return;

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
