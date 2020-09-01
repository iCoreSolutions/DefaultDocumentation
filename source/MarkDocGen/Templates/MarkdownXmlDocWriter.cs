using System;
using DefaultDocumentation;

namespace MarkDocGen
{
   class MarkdownXmlDocWriter : IXmlDocWriter
   {
      private readonly MarkdownWriter m_writer;
      private readonly Action<RenderingContext, ILinkModel, MarkdownWriter> m_renderLink;

      public MarkdownXmlDocWriter(MarkdownWriter writer, Action<RenderingContext, ILinkModel, MarkdownWriter> renderLink)
      {
         m_writer = writer;
         m_renderLink = renderLink;
      }

      public void WriteCodeBlock(RenderingContext context, string value, string language)
      {
         m_writer.WriteCodeBlock(value, language);
      }

      public void WriteEndBold()
      {
         m_writer.WriteEndBold();
      }

      public void WriteEndItalic()
      {
         m_writer.WriteEndItalic();
      }

      public void WriteEndList(ListType type)
      {
      }

      public void WriteEndListItem(ListType type)
      {
         if (type == ListType.Bullet)
            m_writer.WriteEndBulletItem();
         else
            m_writer.WriteEndOrderedListItem();
      }

      public void WriteEndParagraph()
      {
         m_writer.WriteEndParagraph();
      }

      public void WriteEndTable()
      {
         m_writer.WriteEndTable();
      }

      public void WriteEndTableCell()
      {
         m_writer.WriteEndTableCell();
      }

      public void WriteEndTableHeader()
      {
         m_writer.WriteEndTableRow();
         m_writer.WriteTableHeaderSeparator();
      }

      public void WriteEndTableRow()
      {
         m_writer.WriteEndTableRow();
      }

      public void WriteInlineCode(RenderingContext context, string content)
      {
         m_writer.WriteInlineCode(content);
      }

      public void WriteLink(RenderingContext context, ILinkModel link)
      {
         m_renderLink(context, link, m_writer);
      }

      public void WriteListItemTerm(string value)
      {
         m_writer.WriteBold(value);
      }

      public void WriteParamRef(RenderingContext context, string name)
      {
         m_writer.WriteInlineCode(name);
      }

      public void WriteStartBold()
      {
         m_writer.WriteStartBold();
      }

      public void WriteStartCodeBlock(string language)
      {
         m_writer.WriteStartCodeBlock(language);
      }

      public void WriteStartItalic()
      {
         m_writer.WriteStartItalic();
      }

      public void WriteStartList(ListType type)
      {
      }

      public void WriteStartListItem(int itemNumber, ListType type)
      {
         if (type == ListType.Bullet)
            m_writer.WriteStartBulletItem();
         else
            m_writer.WriteStartOrderedListItem(itemNumber);
      }

      public void WriteStartParagraph()
      {
         m_writer.WriteStartParagraph();
      }

      public void WriteStartTable(int columnCount)
      {
         m_writer.WriteStartTable(columnCount);
      }

      public void WriteStartTableCell()
      {
         m_writer.WriteStartTableCell();
      }

      public void WriteStartTableHeader()
      {
         m_writer.WriteStartTableRow();
      }

      public void WriteStartTableRow()
      {
         m_writer.WriteStartTableRow();
      }

      public void WriteText(RenderingContext context, string text)
      {
         m_writer.Write(text);
      }

      public void WriteTypeParamRef(RenderingContext context, string value)
      {
         m_writer.WriteInlineCode(value);
      }
   }
}
