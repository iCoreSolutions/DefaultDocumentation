using System.Xml.Linq;
using ICSharpCode.Decompiler.Documentation;
using ICSharpCode.Decompiler.TypeSystem;

namespace DefaultDocumentation.Model
{
   /*
    * Introducing an AssemblyDocItem might be a bit problematic.  GetIdString() in documentation does *not* contain any information about the assembly in which the item is found I think. This means we cannot resolve the correct
    * type by this Id, *if* a type with the same FullName exists in two assemblies.  However... this is likely never going to happen, because it would just be stupid to be honest. So for now, we ignore assembly name in ID generation, and just 
    * keep the generated ID.
    * 
    * */
   /*
    * Several parts to this:
    * 1. Determine which files to generate
    * 2. Render the content of a page or a DocItem, this will need to reference parts of the XmlDocumentation.
    * 3. Render the content of the XmlDocumentation elements, such as <c>, <see cref=""> etc.
    * 
    * 
    * 
    * 
    * 
    */

   internal abstract class EntityDocItem : SymbolDocItem
   {
      protected EntityDocItem(DocItem parent, IEntity entity, XElement documentation) 
         : base(parent, entity, entity.GetIdString(), documentation)
      {
         Entity = entity;
      }

      public IEntity Entity { get; }

   }

   internal abstract class MemberDocItem : EntityDocItem
   {
      protected MemberDocItem(DocItem parent, IMember member, XElement documentation)
         : base(parent, member, documentation)
      {
         Member = member;
      }

      public IMember Member { get; }
   }
}
