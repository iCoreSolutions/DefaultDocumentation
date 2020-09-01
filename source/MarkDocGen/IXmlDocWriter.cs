namespace MarkDocGen
{
   interface IXmlDocWriter
   {
      void WriteText(RenderingContext context, string text);
      void WriteStartParagraph();
      void WriteEndParagraph();
      void WriteLink(RenderingContext context, ILinkModel link);
      void WriteInlineCode(RenderingContext context, string content);
      void WriteStartCodeBlock(string language);
      void WriteCodeBlock(RenderingContext context, string value, string language = null);
      
      void WriteStartList(ListType type);
      void WriteStartListItem(int itemNumber, ListType type);
      void WriteEndListItem(ListType type);
      void WriteEndList(ListType type);
      void WriteListItemTerm(string value);
      void WriteStartTableHeader();
      void WriteEndTableCell();
      void WriteStartTableCell();
      void WriteStartTable(int columnCount);
      void WriteEndTableHeader();
      void WriteStartTableRow();
      void WriteEndTableRow();
      void WriteEndTable();
      void WriteTypeParamRef(RenderingContext context, string value);
      void WriteParamRef(RenderingContext context, string name);
      void WriteEndItalic();
      void WriteStartItalic();
      void WriteEndBold();
      void WriteStartBold();
   }

   public enum ListType
   {
      Bullet,
      Number
   }
}
