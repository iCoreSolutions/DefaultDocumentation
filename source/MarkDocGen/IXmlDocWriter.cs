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
   }
}
