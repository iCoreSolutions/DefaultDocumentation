using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class EventDocItem : MemberDocItem
   {
      private static readonly CSharpAmbience CodeAmbience = new CSharpAmbience
      {
         ConversionFlags =
              ConversionFlags.ShowAccessibility
              | ConversionFlags.ShowBody
              | ConversionFlags.ShowDefinitionKeyword
              | ConversionFlags.ShowModifiers
      };

      public IEvent Event { get; }

      public EventDocItem(TypeDocItem parent, IEvent @event, XElement documentation)
          : base(parent, @event, documentation)
      {
         Event = @event;
      }

      public override DocItemKind Kind => DocItemKind.Event;

      public string Name => Event.Name;

   }
}
