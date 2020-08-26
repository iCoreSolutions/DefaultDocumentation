using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MarkDocGen
{
   class MarkdownWriter : TextWriter
   {
      [Flags]
      private enum MarkdownEscapeMode
      {
         None = 0,
         Entities = 0x01,
         Normal = 0x02 | Entities,
         LinkContent = 0x04 | Normal,
         Code = 0x08,
         TableCell = 0x10 | Normal | NewLines,
         NewLines = 0x20,
         BlockQuote = 0x40 | Normal
      }

      private readonly TextWriter m_writer;
      private bool m_lastLineEmpty = true;
      private bool m_digitsOnly = false;
      private bool m_hasWrittenNonWhitespace = false;
      private bool m_hasPendingCr = false;
      private int m_blockQuotes = 0;
      private bool m_inCodeBlock;
      private bool m_inTable;

      private readonly static char[] s_escapeDefault = "*_`[]\\".ToCharArray();
      private readonly static char[] s_escapeDefaultLineStart = "|#*-+".ToCharArray();
      private readonly static char[] s_escapeLinkText = s_escapeDefault.Concat("()").ToArray();

      public MarkdownWriter(TextWriter writer)
      {
         m_writer = writer;
      }

      public override Encoding Encoding => m_writer.Encoding;

      public int Line { get; private set; }

      public int Column { get; private set; }

      public override void Write(char c)
      {
         Write(c, MarkdownEscapeMode.Normal);
      }

      public override void Write(string s)
      {
         if (s == null)
            return;

         foreach (var c in s)
            Write(c);
      }

      private void Write(char c, MarkdownEscapeMode mode)
      {
         if (m_inCodeBlock)
            mode = MarkdownEscapeMode.None;

         if (c == '\r' && !m_hasPendingCr)
         {
            m_hasPendingCr = true;
            return;
         }

         if (c == '\n')
         {            
            if (mode.HasFlag(MarkdownEscapeMode.NewLines) || m_inTable)
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
            m_writer.Write('\n');
            return;
         }

         m_hasPendingCr = false;

         if (Column == 0 && m_blockQuotes > 0 && mode != MarkdownEscapeMode.None)
         {
            for (int i = 0; i < m_blockQuotes; i++)
            {
               WriteRaw('>');
            }

            WriteRaw(' ');
         }

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
         Column++;
      }

      private void Write(string s, MarkdownEscapeMode mode)
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
         if (s == null)
            return;

         foreach (var c in s)
            Write(c, MarkdownEscapeMode.None);
      }

      public override void WriteLine()
      {
         Write(Environment.NewLine);
      }

      public override void WriteLine(string s)
      {
         Write(s);
         WriteLine();
      }

      public override void Flush() => m_writer.Flush();

      public override void Write(ReadOnlySpan<char> buffer)
      {
         foreach (var c in buffer)
            Write(c);
      }

      //public override void Write(object o)
      //{
      //   if (o == null)
      //      return;

      //   Write(o.ToString());
      //}

      //public void WriteLine(object o)
      //{
      //   if (o == null)
      //      return;

      //   WriteLine(o.ToString());
      //}

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
         }
      }

      public void WriteCodeBlock(string content, string language = null)
      {
         WriteCodeBlock(writer => writer.Write(content), language);
      }

      public void WriteCodeBlock(Action<TextWriter> writer, string language = null)
      {
         if (Column != 0)
            WriteLine();

         WriteRaw($"```{language}");
         WriteLine();
         m_inCodeBlock = true;
         writer(this);
         m_inCodeBlock = false;
         if (Column != 0)
            WriteLine();
         WriteRaw("```");
         WriteLine();
      }

      public void WriteTable(TableDefinition tableDefinition)
      {
         var headers = tableDefinition.Headers;
         int columnCount = tableDefinition.Headers.Count;

         List<string[]> rows = new List<string[]>();

         int[] maxLengths = headers.Select(s => s?.Length ?? 0).ToArray();
         foreach (var row in tableDefinition.Rows)
         {
            string[] targetRow = new string[columnCount];
            for (int i = 0; i < columnCount && i < row.Count; i++)
            {
               if (row[i] != null)
               {
                  using (StringWriter w = new StringWriter())
                  using (MarkdownWriter md = new MarkdownWriter(w))
                  {
                     md.m_inTable = true;
                     row[i](md);
                     targetRow[i] = w.ToString();
                  }
               }
               else
               {
                  targetRow[i] = "<!-- -->";
               }

               maxLengths[i] = Math.Max(maxLengths[i], targetRow[i]?.Length ?? 0);
            }
            rows.Add(targetRow);
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

         foreach (var row in rows)
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

      public void WriteAnchor(string id)
      {
         // TODO PP (2020-08-26): Perhaps escape stuff in id?
         WriteRaw($"<a name=\"{id}\"></a>");
      }

      public void WriteHeading(int level, string text, string anchor = null)
      {
         if (level < 1 || level > 6)
            throw new ArgumentOutOfRangeException(nameof(level), $"Level must be between 1-6");

         EnsureNewLine();
         WriteRaw(new string('#', level));
         Write(' ');
         Write(text, MarkdownEscapeMode.Normal | MarkdownEscapeMode.NewLines);
         if (anchor != null)
            WriteAnchor(anchor);

         EnsureNewLine();
      }

      public void WriteBold(string text)
      {
         WriteRaw("**");
         Write(text);
         WriteRaw("**");
      }

      public void WriteItalic(string text)
      {
         WriteRaw("*");
         Write(text);
         WriteRaw("*");
      }

      public void WriteEmphasis(string text)
      {
         WriteRaw("***");
         Write(text);
         WriteRaw("***");
      }

      public void WriteBlockQuote(string text)
      {
         EnsureNewLine();
         StartBlockQuote();
         Write(text.Trim());
         EndBlockQuote();
      }

      public void StartBlockQuote()
      {
         if (m_blockQuotes == 0)
            EnsureNewLine();

         m_blockQuotes++;
      }

      public void EndBlockQuote()
      {
         if (m_blockQuotes-- < 0)
            m_blockQuotes = 0;
      }

      public void WriteBulletList(IEnumerable<string> items)
      {
         EnsureNewLine();
         foreach (var item in items)
         {
            WriteRaw("* ");
            Write(item.Trim().Replace("\n", "\n  "));
         }
         EnsureNewLine();
      }
      
      // TODO PP (2020-08-25): Add support for lists (ordered, and unordered, and perhaps definition lists)

      public class TableDefinition
      {
         private readonly IReadOnlyList<string> headers;
         private readonly List<Action<MarkdownWriter>[]> rows = new List<Action<MarkdownWriter>[]>();

         public TableDefinition(params string[] columns)
           : this(columns, null)
         {
         }

         public TableDefinition(IEnumerable<string> columns)
            : this(columns, null)
         {
         }

         public IReadOnlyList<string> Headers => headers;
         public IReadOnlyList<IReadOnlyList<Action<MarkdownWriter>>> Rows => rows;

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
            AddRow(row.Select(r => new Action<MarkdownWriter>((MarkdownWriter w) => w.Write(r))));
            //var targetRow = new string[headers.Count];
            //using (IEnumerator<string> enumerator = row.GetEnumerator())
            //{
            //   for (int i = 0; i < headers.Count; i++)
            //   {
            //      if (enumerator.MoveNext())
            //         targetRow[i] = Transform(enumerator.Current);
            //      else
            //         targetRow[i] = Transform(null);
            //   }
            //}

            //rows.Add(targetRow);
         }

         public void AddRow(IEnumerable<Action<MarkdownWriter>> row)
         {
            var targetRow = new Action<MarkdownWriter>[headers.Count];
            using (var enumerator = row.GetEnumerator())
            {
               for (int i = 0; i < headers.Count; i++)
               {
                  if (enumerator.MoveNext())
                     targetRow[i] = enumerator.Current;
                  else
                     targetRow[i] = null;
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

}
