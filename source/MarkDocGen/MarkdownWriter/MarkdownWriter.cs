using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DefaultDocumentation
{
   public class MarkdownWriter : IDisposable
   {
      #region Fields
      private static readonly Dictionary<State, State[]> s_stateTransitions = new Dictionary<State, State[]>
      {
         { State.Start, new [] { State.SimpleElement, State.Heading, State.BulletItem , State.OrderedItem, State.BlockQuote, State.Link, State.Table, State.Bold, State.Italic, State.Strikethrough, State.CodeBlock, State.Paragraph, State.HorizontalRule } },
         { State.Heading, new [] { State.SimpleElement } },
         { State.SimpleElement, Array.Empty<State>() },
         { State.BulletItem, new [] { State.SimpleElement, State.BulletItem, State.OrderedItem, State.BlockQuote, State.Link, State.Bold, State.Italic, State.Strikethrough, State.CodeBlock, State.Paragraph, State.Heading, State.HorizontalRule } },
         { State.OrderedItem, new [] { State.SimpleElement, State.BulletItem, State.OrderedItem, State.BlockQuote, State.Link, State.Bold, State.Italic, State.Strikethrough, State.CodeBlock, State.Paragraph, State.Heading, State.HorizontalRule } },
         { State.BlockQuote, new [] { State.SimpleElement, State.BulletItem, State.OrderedItem, State.BlockQuote, State.Link, State.Bold, State.Italic, State.Strikethrough, State.CodeBlock, State.Paragraph, State.Heading, State.HorizontalRule } },
         { State.Link, new [] { State.SimpleElement, State.Bold, State.Italic, State.Strikethrough } },
         { State.Table, new [] { State.TableRow } },
         { State.TableRow, new [] { State.TableCell } },
         { State.TableCell, new [] { State.SimpleElement, State.Link, State.Bold, State.Italic, State.Strikethrough, State.Paragraph } },
         { State.Bold, new [] { State.SimpleElement, State.Link, State.Italic, State.Strikethrough } },
         { State.Italic, new [] { State.SimpleElement, State.Link, State.Bold, State.Strikethrough } },
         { State.Strikethrough, new [] { State.SimpleElement, State.Link, State.Bold, State.Italic } },
         { State.CodeBlock, Array.Empty<State>() },
         { State.Paragraph, new [] { State.SimpleElement, State.Heading, State.BulletItem , State.OrderedItem, State.BlockQuote, State.Link, State.Table, State.Bold, State.Italic, State.Strikethrough, State.CodeBlock, State.Paragraph } },
         { State.HorizontalRule, Array.Empty<State>() }

      };
      private bool m_isDisposed;
      private bool m_isWhitespaceOnly = true;
      private bool m_lastLineEmpty = true;
      private readonly StringBuilder m_line;
      private bool m_lineHasOnlyDigits = false;
      private int m_orderedItemNumber;
      private State m_state;
      private readonly Stack<StateInfo> m_stateStack = new Stack<StateInfo>();
      private int m_tableCellPos = -1;
      private int m_tableColumnCount = -1;
      private int m_tableColumnIndex = -1;
      private List<TableColumnInfo> m_tableColumns;
      private int m_tableRowIndex = -1;
      private readonly TextWriter m_writer;
      private int m_lastBR = -1;

      #endregion

      #region Construction
      public MarkdownWriter(TextWriter writer, MarkdownWriterSettings settings = null)
      {
         m_writer = writer;
         Settings = settings ?? MarkdownWriterSettings.Default;
         m_line = new StringBuilder();
      }
      #endregion

      #region Properties
      public MarkdownWriterSettings Settings { get; }

      private TableColumnInfo CurrentColumn => m_tableColumns[m_tableColumnIndex];

      private bool IsFirstColumn => m_tableColumnIndex == 0;

      private bool IsLastColumn => m_tableColumnIndex == m_tableColumnCount - 1;
      #endregion


      public void Write(string text)
      {
         Write(text, ShouldEscape);
      }

      public void Write(char ch) => Write(ch, ShouldEscape);

      public void WriteLine()
      {
         Write(Environment.NewLine);
      }

      public void WriteLine(string text)
      {
         Write(text);
         Write(Environment.NewLine);
      }

      #region BlockQuote

      public void WriteBlockQuote(string content)
      {
         WriteStartBlockQuote();
         Write(content);
         WriteEndBlockQuote();
      }

      public void WriteStartBlockQuote()
      {
         try
         {
            Push(State.BlockQuote);
            EnsureNewLine();
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteEndBlockQuote()
      {
         try
         {
            ThrowIfCannotWriteEnd(State.BlockQuote);
            EnsureNewLine();
            Pop(State.BlockQuote);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      #endregion

      #region Bold

      public void WriteBold(string text)
      {
         WriteStartBold();
         Write(text);
         WriteEndBold();
      }

      public void WriteStartBold()
      {
         try
         {
            Push(State.Bold);
            WriteRaw("**");
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteEndBold()
      {
         try
         {
            ThrowIfCannotWriteEnd(State.Bold);
            WriteRaw("**");
            Pop(State.Bold);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }
      #endregion

      #region Italic

      public void WriteItalic(string text)
      {
         WriteStartItalic();
         Write(text);
         WriteEndItalic();
      }

      public void WriteStartItalic()
      {
         try
         {
            Push(State.Italic);
            WriteRaw('*');
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteEndItalic()
      {
         try
         {
            ThrowIfCannotWriteEnd(State.Italic);
            WriteRaw('*');
            Pop(State.Italic);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      #endregion

      #region Strikethrough

      public void WriteStrikeThrough(string text)
      {
         WriteStartStrikethrough();
         Write(text);
         WriteEndStrikethrough();
      }

      public void WriteStartStrikethrough()
      {
         try
         {
            Push(State.Strikethrough);
            WriteRaw('*');
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteEndStrikethrough()
      {
         try
         {
            ThrowIfCannotWriteEnd(State.Strikethrough);
            WriteRaw('*');
            Pop(State.Strikethrough);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      #endregion

      #region Link

      public void WriteLink(string text, string url, string title = null)
      {
         WriteStartLink();
         Write(text);
         WriteEndLink(url, title);
      }

      public void WriteStartLink()
      {
         try
         {
            Push(State.Link);
            WriteIndentation();
            WriteRaw('[');
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteEndLink(string url, string title = null)
      {
         try
         {
            if (url == null)
               throw new ArgumentNullException(nameof(url));

            if (url.Any(c => Char.IsWhiteSpace(c)))
               throw new ArgumentException($"URL cannot contain whitespace characters.", nameof(url));

            ThrowIfCannotWriteEnd(State.Link);

            WriteRaw("](");
            Write(url, ShouldEscapeInUrl);
            WriteLinkTitle(title);
            WriteRaw(")");

            Pop(State.Link);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      #endregion

      #region Heading

      public void WriteHeading(int level, string text)
      {
         WriteStartHeading(level);
         Write(text);
         WriteEndHeading();
      }

      public void WriteHeading1(string text) => WriteHeading(1, text);
      public void WriteHeading2(string text) => WriteHeading(2, text);
      public void WriteHeading3(string text) => WriteHeading(3, text);
      public void WriteHeading4(string text) => WriteHeading(4, text);
      public void WriteHeading5(string text) => WriteHeading(5, text);
      public void WriteHeading6(string text) => WriteHeading(6, text);

      public void WriteStartHeading(int level)
      {
         try
         {
            Push(State.Heading);
            if (m_line.Length > 0)
               FlushLine();

            if (!m_lastLineEmpty)
               FlushLine();

            for (int i = 0; i < level; i++)
            {
               WriteRaw('#');
            }

            WriteRaw(' ');
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteEndHeading()
      {
         try
         {
            Pop(State.Heading);
            FlushLine();
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }
      #endregion

      #region Inline Code

      public void WriteInlineCode(string text)
      {
         if (text == null)
            return;

         int length = text.Length;

         if (length == 0)
            return;

         try
         {
            Push(State.SimpleElement);

            int backtickLength = GetBacktickLength();

            for (int i = 0; i < backtickLength; i++)
               WriteRaw("`");

            if (text[0] == '`')
               WriteRaw(" ");

            Write(text, shouldEscape: _ => false);

            if (text[length - 1] == '`')
               WriteRaw(" ");

            for (int i = 0; i < backtickLength; i++)
               WriteRaw("`");

            Pop(State.SimpleElement);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }

         int GetBacktickLength()
         {
            int minLength = 0;
            int maxLength = 0;

            for (int i = 0; i < length; i++)
            {
               if (text[i] == '`')
               {
                  i++;

                  int len = 1;

                  while (i < length && text[i] == '`')
                  {
                     len++;
                     i++;
                  }

                  if (minLength == 0)
                  {
                     minLength = len;
                     maxLength = len;
                  }
                  else if (len < minLength)
                  {
                     minLength = len;
                  }
                  else if (len > maxLength)
                  {
                     maxLength = len;
                  }
               }
            }

            if (minLength == 1)
            {
               return maxLength + 1;
            }
            else
            {
               return 1;
            }
         }
      }

      #endregion

      #region Bullet Item

      public void WriteBulletItem(string text)
      {
         WriteStartBulletItem();
         Write(text);
         WriteEndBulletItem();
      }

      public void WriteStartBulletItem()
      {
         try
         {
            EnsureNewLine();
            WriteIndentation();
            WriteRaw('*');
            WriteRaw(' ');
            Push(State.BulletItem);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }


      public void WriteEndBulletItem()
      {
         try
         {
            Pop(State.BulletItem);
            EnsureNewLine();
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }
      #endregion

      #region Ordered List Item

      public void WriteOrderedListItem(int number, string content)
      {
         WriteStartOrderedListItem(number);
         Write(content);
         WriteEndOrderedListItem();
      }

      public void WriteStartOrderedListItem(int number)
      {
         // TODO PP (2020-08-30): Implement automatic numbering.
         if (number < 0)
            throw new ArgumentOutOfRangeException(nameof(number), number, "Item number must be greater than or equal to 0.");

         try
         {
            EnsureNewLine();
            WriteIndentation();
            WriteRaw(number.ToString());
            WriteRaw('.');
            WriteRaw(' ');
            Push(State.OrderedItem, number);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }


      public void WriteEndOrderedListItem()
      {
         try
         {
            Pop(State.OrderedItem);
            EnsureNewLine();
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }
      #endregion

      #region CodeBlock

      public void WriteCodeBlock(string content, string language = null)
      {
         WriteStartCodeBlock(language);
         Write(content);
         WriteEndCodeBlock();
      }

      public void WriteStartCodeBlock(string language = null)
      {
         try
         {
            Push(State.CodeBlock);
            EnsureNewLine();
            WriteIndentation();
            WriteRaw("```");
            if (language != null)
               WriteRaw(language);
            FlushLine();
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteEndCodeBlock()
      {
         try
         {
            ThrowIfCannotWriteEnd(State.CodeBlock);
            EnsureNewLine();
            WriteIndentation();
            WriteRaw("```");
            FlushLine();
            Pop(State.CodeBlock);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }
      #endregion

      #region Paragraph

      public void WriteParagraph(string content)
      {
         WriteStartParagraph();
         Write(content);
         WriteEndParagraph();
      }

      public void WriteStartParagraph()
      {
         try
         {
            Push(State.Paragraph);
            if (ShouldEscapeNewLine())
            {
               if (m_lastBR < m_line.Length)
                  WriteRaw("<br/>");
            }
            else
            {
               EnsureNewLine();
               if (!m_lastLineEmpty)
                  FlushLine();
            }
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteEndParagraph()
      {
         try
         {
            ThrowIfCannotWriteEnd(State.Paragraph);
            EnsureNewLine();
            if (!m_lastLineEmpty)
               FlushLine();
            Pop(State.Paragraph);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }
      #endregion

      #region Table

      public void WriteStartTable(int columnCount)
      {
         WriteStartTable(null, columnCount);
      }

      public void WriteStartTable(IReadOnlyList<TableColumnInfo> columns)
      {
         if (columns == null)
            throw new ArgumentNullException(nameof(columns));

         WriteStartTable(columns, columns.Count);
      }

      public void WriteStartTableCell()
      {
         try
         {
            Push(State.TableCell);

            m_tableColumnIndex++;

            WriteRaw('|');

            WriteRaw(' ');

            m_tableCellPos = m_line.Length;
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteStartTableRow()
      {
         try
         {
            Push(State.TableRow);
            m_tableRowIndex++;
            m_tableColumnIndex = -1;
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }


      public void WriteEndTable()
      {
         try
         {
            ThrowIfCannotWriteEnd(State.Table);
            m_tableRowIndex = -1;
            m_tableColumns.Clear();
            m_tableColumnCount = -1;

            EnsureNewLine();

            Pop(State.Table);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteEndTableCell()
      {
         try
         {
            ThrowIfCannotWriteEnd(State.TableCell);

            int width = m_line.Length - m_tableCellPos;

            TableColumnInfo currentColumn = CurrentColumn;

            if (currentColumn.Width == 0
                && width > 0)
            {
               m_tableColumns[m_tableColumnIndex] = currentColumn.WithWidth(width);
            }

            WritePadRight(width);

            WriteRaw(' ');

            m_tableCellPos = -1;
            Pop(State.TableCell);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteEndTableRow()
      {
         try
         {
            ThrowIfCannotWriteEnd(State.TableRow);

            WriteRaw('|');

            WriteLine();
            m_tableColumnIndex = -1;

            Pop(State.TableRow);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      public void WriteTableHeaderSeparator()
      {
         try
         {
            EnsureNewLine();

            WriteStartTableRow();

            int count = m_tableColumnCount;

            for (int i = 0; i < count; i++)
            {
               m_tableColumnIndex = i;

               WriteRaw('|');

               if (CurrentColumn.Alignment != HorizontalAlignment.Right)
               {
                  WriteRaw(':');
               }
               else
               {
                  WriteRaw(' ');
               }

               WriteRaw("---");

               WritePadRight(3, '-');

               if (CurrentColumn.Alignment != HorizontalAlignment.Left)
               {
                  WriteRaw(':');
               }
               else
               {
                  WriteRaw(" ");
               }
            }

            WriteEndTableRow();
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      #endregion

      #region Horizontal Rule

      public void WriteHorizontalRule(int count = 3, string separator = " ")
      {
         try
         {
            if (count < 3)
               throw new ArgumentOutOfRangeException(nameof(count), count, "Number of characters in horizontal rule must be greater than or equal to 3.");

            Push(State.HorizontalRule);

            EnsureNewLine();

            bool isFirst = true;

            string text = "-";

            for (int i = 0; i < count; i++)
            {
               if (isFirst)
               {
                  isFirst = false;
               }
               else
               {
                  WriteRaw(separator);
               }

               WriteRaw(text);
            }

            WriteLine();
            Pop(State.HorizontalRule);
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      #endregion

      public void Dispose()
      {
         Dispose(disposing: true);
         GC.SuppressFinalize(this);
      }


      #region Non-Public Methods

      protected virtual void Dispose(bool disposing)
      {
         if (!m_isDisposed)
         {
            if (disposing)
            {
               if (m_line.Length > 0)
                  m_writer.Write(m_line.ToString());

               m_line.Length = 0;
            }

            m_isDisposed = true;
         }
      }

      public void EnsureNewLine()
      {
         if (m_line.Length > 0)
         {
            if (m_isWhitespaceOnly)
               m_line.Length = 0;
            else
               FlushLine();
         }
      }

      public void EnsureNewParagraph()
      {
         EnsureNewLine();
         if (!m_lastLineEmpty)
            FlushLine();
      }
      private bool IsValidState(State newState)
      {
         return s_stateTransitions.TryGetValue(m_state, out var validStates) && validStates.Contains(newState);
      }

      private void Pop(State state)
      {
         if (m_stateStack.Count == 0 || m_state != state)
            throw new InvalidOperationException($"Cannot close state {state} when current state is {m_state}.");

         StateInfo stateInfo = m_stateStack.Pop();
         m_state = stateInfo.State;
         m_orderedItemNumber = stateInfo.OrderedItemNumber;
      }

      private void Push(State state, int orderedItemNumber = 0)
      {
         if (m_state == State.Closed)
            throw new InvalidOperationException($"Cannot write to a {nameof(MarkdownWriter)} after it has been closed.");

         if (m_state == State.Error)
            throw new InvalidOperationException($"The writer is in an error state.");

         if (!IsValidState(state))
            throw new InvalidOperationException($"Cannot write '{state}' when current state is '{m_state}'.");

         m_stateStack.Push(new StateInfo(m_state, m_orderedItemNumber));
         m_state = state;
         m_orderedItemNumber = orderedItemNumber;
      }

      private bool ShouldEscape(char ch)
      {
         if (m_state == State.CodeBlock)
            return false;

         switch (ch)
         {
            case '\\':
            case '`':
            case '_':
            case '{':
            case '}':
            case '[':
            case ']':
            //case '(':
            //case ')':
            case '*':
            //case '!':
            case '<':
            case '~':
               return true;

            case '#':
            case '+':
            case '-':
               return m_isWhitespaceOnly;

            case '.':
               return m_lineHasOnlyDigits;

            case '|':
               return m_isWhitespaceOnly || m_state == State.TableCell;

            default:
               return false;
         }
      }

      private bool ShouldEscapeInLinkTitle(char ch)
      {
         return ch == '"';
      }

      private bool ShouldEscapeInUrl(char ch)
      {
         switch (ch)
         {
            case '(':
            case ')':
               return true;
            default:
               return false;
         }
      }

      private bool ShouldEscapeNewLine()
      {
         switch (m_state)
         {
            case State.Heading:
            case State.TableCell:
               // TODO PP (2020-08-31): In tables we may actually not want this. Instead use a whitespace perhaps? Or error. Not sure.
               return true;
            default:
               return false;
         }
      }

      private void ThrowIfCannotWriteEnd(State state)
      {
         if (m_state != state)
            throw new InvalidOperationException($"Cannot close '{state}' when state is '{m_state}'.");

      }

      private void ThrowIfClosed()
      {
         if (m_isDisposed)
            throw new ObjectDisposedException(GetType().Name);
      }

      private void Write(string text, Predicate<char> shouldEscape)
      {
         if (text == null)
            return;

         WriteIndentation();
         foreach (var c in text)
            Write(c, shouldEscape);
      }

      private void Write(char ch, Predicate<char> shouldEscape)
      {
         if ((ch == '\r' || ch == '\n') && ShouldEscapeNewLine())
         {
            if (m_line.Length > 0 && m_line[m_line.Length - 1] == '\r')
               m_line.Length = m_line.Length - 1;

            WriteEscaped('\n');
            m_lastBR = m_line.Length;
         }
         else
         {
            if (m_line.Length == 0)
               WriteIndentation();

            if (shouldEscape(ch))
               WriteEscaped(ch);
            else
               WriteRaw(ch);
         }
      }

      private void WriteEscaped(char ch)
      {
         switch (ch)
         {
            case '\n':
            case '\r':
               WriteRaw("<br/>");
               break;
            case '<':
               WriteRaw("&lt;");
               break;
            case '>':
               WriteRaw("&gt;");
               break;
            case '&':
               WriteRaw("&amp;");
               break;

            default:
               WriteRaw('\\');
               WriteRaw(ch);
               break;
         }
      }

      protected void WriteIndentation()
      {
         if (m_line.Length > 0)
            return;

         foreach (var item in m_stateStack.Reverse())
         {
            WriteIndentation(item.State, item.OrderedItemNumber);
         }

         WriteIndentation(m_state, m_orderedItemNumber);

         void WriteIndentation(State state, int orderedItemNumber)
         {
            switch (state)
            {
               case State.BulletItem:
                  WriteRaw("  ");
                  break;
               case State.OrderedItem:
                  int count = GetDigitCount(orderedItemNumber) + 2;
                  m_line.Append(' ', count);
                  break;
               case State.BlockQuote:
                  m_line.Append("> ");
                  break;
            }
         }
      }

      private int GetDigitCount(int n)
      {
         if (n >= 0)
         {
            if (n < 10) return 1;
            if (n < 100) return 2;
            if (n < 1000) return 3;
            if (n < 10000) return 4;
            if (n < 100000) return 5;
            if (n < 1000000) return 6;
            if (n < 10000000) return 7;
            if (n < 100000000) return 8;
            if (n < 1000000000) return 9;
            return 10;
         }
         else
         {
            if (n > -10) return 2;
            if (n > -100) return 3;
            if (n > -1000) return 4;
            if (n > -10000) return 5;
            if (n > -100000) return 6;
            if (n > -1000000) return 7;
            if (n > -10000000) return 8;
            if (n > -100000000) return 9;
            if (n > -1000000000) return 10;
            return 11;
         }
      }

      private void WriteLinkTitle(string title)
      {
         if (string.IsNullOrEmpty(title))
            return;

         WriteRaw(" \"");
         Write(title, ShouldEscapeInLinkTitle);
         WriteRaw("\"");

      }

      private void FlushLine()
      {
         string value = m_line.ToString();
         if (String.IsNullOrWhiteSpace(value))
            m_lastLineEmpty = true;
         else
            m_lastLineEmpty = false;

         m_writer.WriteLine(m_line.ToString());
         m_line.Length = 0;
         m_isWhitespaceOnly = true;
         m_lineHasOnlyDigits = false;
         m_lastBR = -1;
      }

      private void WritePadRight(int width, char padding = ' ')
      {
         int totalWidth = Math.Max(CurrentColumn.Width, Math.Max(width, 3));

         for (int i = 0; i < totalWidth - width; i++)
         {
            WriteRaw(padding);
         }
      }

      public void WriteRaw(char ch)
      {
         if (m_line.Length > 0 && m_line[m_line.Length - 1] == '\r')
         {
            m_line.Length = m_line.Length - 1;

            FlushLine();
         }
         else if (ch == '\n')
         {
            FlushLine();
         }

         if (ch != '\n')
         {
            if (m_isWhitespaceOnly && Char.IsDigit(ch))
            {
               m_lineHasOnlyDigits = true;
            }
            else if (m_lineHasOnlyDigits && !Char.IsDigit(ch))
            {
               m_lineHasOnlyDigits = false;
            }

            if (m_isWhitespaceOnly && !Char.IsWhiteSpace(ch))
               m_isWhitespaceOnly = false;

            m_line.Append(ch);
         }

      }

      public void WriteRaw(string data)
      {
         ThrowIfClosed();
         foreach (var c in data)
            WriteRaw(c);
      }

      private void WriteStartTable(IReadOnlyList<TableColumnInfo> columns, int columnCount)
      {
         if (columnCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(columnCount), columnCount, "Table must have at least one column.");

         try
         {
            Push(State.Table);

            EnsureNewLine();

            if (!m_lastLineEmpty)
               FlushLine();

            if (m_tableColumns == null)
               m_tableColumns = new List<TableColumnInfo>(columnCount);

            if (columns != null)
            {
               m_tableColumns.AddRange(columns);
            }
            else
            {
               for (int i = 0; i < columnCount; i++)
               {
                  m_tableColumns.Add(new TableColumnInfo(HorizontalAlignment.Left, width: 0, isWhiteSpace: true));
               }
            }

            m_tableColumnCount = columnCount;
         }
         catch
         {
            m_state = State.Error;
            throw;
         }
      }

      #endregion

      #region Nested Types

      private enum State
      {
         Start = 0,
         Closed,
         Error,
         Heading,
         SimpleElement,
         BulletItem,
         OrderedItem,
         BlockQuote,
         Link,
         Table,
         TableRow,
         TableCell,
         Bold,
         Italic,
         Strikethrough,
         CodeBlock,
         Paragraph,
         HorizontalRule
      }

      private struct StateInfo
      {
         #region Construction
         public StateInfo(State state, int orderedItemNumber = 0)
         {
            State = state;
            OrderedItemNumber = orderedItemNumber;
         }
         #endregion

         #region Properties
         public int OrderedItemNumber { get; }

         public State State { get; }
         #endregion
      }

      #endregion
   }
}
