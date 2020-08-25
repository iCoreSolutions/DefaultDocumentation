namespace MarkDocGen
{
   class NoLinkModel : ILinkModel
   {
      public NoLinkModel(string text)
      {
         Text = text;
      }

      public LinkType LinkType => LinkType.NoLink;

      public string Text { get; }

      public ILinkModel WithText(string text)
      {
         return new NoLinkModel(text);
      }
   }
}
