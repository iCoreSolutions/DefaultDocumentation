namespace MarkDocGen
{
   interface ILinkModel
   {
      LinkType LinkType { get; }
      string Text { get; }

      ILinkModel WithText(string text);
      //      void Render(MyTemplate template, TextWriter writer);
   }
}
