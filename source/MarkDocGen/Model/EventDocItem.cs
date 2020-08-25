using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class EventDocItem : EntityDocItem
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

      // TODO PP (2020-08-20): Remove commented code.
      //public override void WriteDocumentation(DocumentationWriter writer)
      //{
      //   writer.WriteHeader();
      //   writer.WritePageTitle($"{Parent.Name}.{Name}", "Event");

      //   writer.Write(this, Documentation.GetSummary());

      //   writer.WriteLine("```csharp");
      //   writer.WriteLine(CodeAmbience.ConvertSymbol(Event));
      //   writer.WriteLine("```");
      //   // attributes

      //   writer.WriteLine("#### Event type");
      //   writer.WriteLine(writer.GetTypeLink(Event.ReturnType));

      //   writer.Write("### Example", Documentation.GetExample(), this);
      //   writer.Write("### Remarks", Documentation.GetRemarks(), this);
      //}
   }
}
