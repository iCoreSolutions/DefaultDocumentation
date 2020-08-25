namespace MarkDocGen
{
   class ExternalLinkModel : ILinkModel
   {
      public ExternalLinkModel(string url, string text)
      {
         Url = url;
         Text = text;
      }

      public string Url { get; }
      public string Text { get; }

      public virtual LinkType LinkType => LinkType.External;

      public virtual ILinkModel WithText(string text)
      {
         return new ExternalLinkModel(Url, text);
      }
   }
}
