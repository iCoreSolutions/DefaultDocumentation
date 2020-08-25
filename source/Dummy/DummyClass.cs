using DummyRef1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dummy
{
   /// <summary>
   /// This is an interface.
   /// </summary>
   public interface IDummyIfc
   {
      /// <summary>
      /// This is a property in the interface.
      /// </summary>
      MyClassInDummy DummyCls { get; }

   }

   /// <summary>
   /// MyDict
   /// </summary>
   public class MyDictionary : Dictionary<KeyValuePair<string, bool>, string>
   {
   }

   /// <summary>
   /// Dummy class description. See <see cref="System.Linq.Enumerable.LastOrDefault{TSource}(System.Collections.Generic.IEnumerable{TSource}, Func{TSource, bool})"/>.
   /// 
   /// Also try with <see cref="Enumerable.LastOrDefault{TSource}(System.Collections.Generic.IEnumerable{TSource}, Func{TSource, bool})">LastOrDefault Specific Method</see>
   /// </summary>
   public class DummyClass : IDummyIfc
   {
      public global::DuplicateNs.Class1 c1 { get; set; }

        /// <summary>
        /// dummy <c>test</c>
        /// linebreak
        /// <code>
        /// example
        /// yep
        /// </code>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <remarks>
        /// pouet <see cref="System.StringComparer.CurrentCulture"/>
        /// </remarks>
        public class DummyNested<T>
        {
            /// <summary>
            /// dummy
            /// </summary>
            public event Action<T> Action;
        }

      /// <summary>Gets or sets the dummy cls. <see cref="String"/> and <see cref="MyClassInDummy"/>.</summary>
      /// <value>The dummy cls.</value>
      public MyClassInDummy DummyCls { get; set; }

        /// <summary>
        /// dummy
        /// </summary>
        public int DummyField;

        /// <summary>
        /// dummy
        /// </summary>
        public int DummyProperty { get; }

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public dynamic this[int index]
        {
            get => index;
        }

        /// <summary>
        /// dummy <see cref="DummyProperty"/> and <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T">dummy</typeparam>
        /// <param name="value">dummy</param>
        /// <returns>dummy</returns>
        public async Task<dynamic> DummyAsync<T>(T value)
        {
            await Task.Delay(0);
            return value;
        }

        /// <summary>
        /// dummy
        /// </summary>
        public DummyClass()
        { }

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="p">dummy</param>
        /// <returns>dummy</returns>
        public unsafe int** Unsafe(void* p) => (int**)&p;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="pouet">kikoo</param>
        /// <typeparam name="T2">lol</typeparam>
        public void DummyMethod<T2>(T2 pouet)
        {
            var t = this;
            t += 0;
        }

        /// <summary>
        /// dummy
        /// </summary>
        public TaskContinuationOptions DummyOption { get; }

        /// <summary>
        /// dummy
        /// </summary>
        /// <typeparam name="T2">dummy</typeparam>
        /// <param name="pouet">dummy</param>
        /// <returns>dummy</returns>
        public (int, DummyClass) DummyTuple<T2>(T2 pouet) => (42, this);

        /// <summary>
        /// dummy
        /// </summary>
        /// <typeparam name="T2">dummy</typeparam>
        /// <param name="pouet">dummy</param>
        /// <returns>dummy</returns>
        public ValueTuple<int, DummyClass> DummyExplicitTuple<T2>(T2 pouet) => (42, this);

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static DummyClass operator +(DummyClass a, int b) => a;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="c"></param>
        public static implicit operator int(DummyClass c) => 0;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="c"></param>
        public static explicit operator double(DummyClass c) => 0;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="c"></param>
        public static explicit operator DummyClass(int c) => null;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator ==(DummyClass a, DummyClass b) => true;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator !=(DummyClass a, DummyClass b) => false;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator -(DummyClass a, DummyClass b) => true;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator *(DummyClass a, DummyClass b) => true;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator /(DummyClass a, DummyClass b) => true;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator &(DummyClass a, DummyClass b) => true;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator |(DummyClass a, DummyClass b) => true;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator ~(DummyClass a) => true;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator ^(DummyClass a, DummyClass b) => true;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <returns>dummy</returns>
        public static DummyClass operator ++(DummyClass a) => a;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <returns>dummy</returns>
        public static DummyClass operator --(DummyClass a) => a;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator <(DummyClass a, DummyClass b) => false;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator >(DummyClass a, DummyClass b) => false;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator <=(DummyClass a, DummyClass b) => false;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator >=(DummyClass a, DummyClass b) => false;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <returns>dummy</returns>
        public static DummyClass operator -(DummyClass a) => a;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <returns>dummy</returns>
        public static DummyClass operator +(DummyClass a) => a;
        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="b">dummy</param>
        /// <returns>dummy</returns>
        public static bool operator %(DummyClass a, DummyClass b) => false;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="i">dummy</param>
        /// <returns>dummy</returns>
        public static DummyClass operator <<(DummyClass a, int i) => a;

        /// <summary>
        /// dummy
        /// </summary>
        /// <param name="a">dummy</param>
        /// <param name="i">dummy</param>
        /// <returns>dummy</returns>
        public static DummyClass operator >>(DummyClass a, int i) => a;


      /// <summary>
      /// dummy
      /// </summary>
      /// <param name="s">dummy</param>
      /// <remarks>
      ///<para>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam eget luctus dui, nec blandit arcu. Cras ullamcorper suscipit ornare. Nullam euismod placerat arcu ac pharetra. Nulla blandit vestibulum mi vitae sollicitudin. Donec orci nibh, venenatis non velit sed, gravida malesuada dolor. Nam vestibulum ullamcorper dui in faucibus. Fusce rutrum varius tortor, vel laoreet dolor pulvinar sit amet. Phasellus a neque ultricies, egestas nunc eu, facilisis ante. Vestibulum viverra lacus id libero iaculis mattis. Cras ut odio metus.</para> 
      ///<para>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam eget luctus dui, nec blandit arcu. Cras ullamcorper suscipit ornare. Nullam euismod placerat arcu ac pharetra. Nulla blandit vestibulum mi vitae sollicitudin. Donec orci nibh, venenatis non velit sed, gravida malesuada dolor. Nam vestibulum ullamcorper dui in faucibus. Fusce rutrum varius tortor, vel laoreet dolor pulvinar sit amet. Phasellus a neque ultricies, egestas nunc eu, facilisis ante. Vestibulum viverra lacus id libero iaculis mattis. Cras ut odio metus.</para> 
      ///<para>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam eget luctus dui, nec blandit arcu. Cras ullamcorper suscipit ornare. Nullam euismod placerat arcu ac pharetra. Nulla blandit vestibulum mi vitae sollicitudin. Donec orci nibh, venenatis non velit sed, gravida malesuada dolor. Nam vestibulum ullamcorper dui in faucibus. Fusce rutrum varius tortor, vel laoreet dolor pulvinar sit amet. Phasellus a neque ultricies, egestas nunc eu, facilisis ante. Vestibulum viverra lacus id libero iaculis mattis. Cras ut odio metus.</para> 
      ///</remarks>
      public void Overloaded(string s) { }

      /// <summary>
      /// dummy
      /// </summary>
      /// <param name="s">dummy</param>
      /// <remarks>
      ///<para>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam eget luctus dui, nec blandit arcu. Cras ullamcorper suscipit ornare. Nullam euismod placerat arcu ac pharetra. Nulla blandit vestibulum mi vitae sollicitudin. Donec orci nibh, venenatis non velit sed, gravida malesuada dolor. Nam vestibulum ullamcorper dui in faucibus. Fusce rutrum varius tortor, vel laoreet dolor pulvinar sit amet. Phasellus a neque ultricies, egestas nunc eu, facilisis ante. Vestibulum viverra lacus id libero iaculis mattis. Cras ut odio metus.</para> 
      ///<para>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam eget luctus dui, nec blandit arcu. Cras ullamcorper suscipit ornare. Nullam euismod placerat arcu ac pharetra. Nulla blandit vestibulum mi vitae sollicitudin. Donec orci nibh, venenatis non velit sed, gravida malesuada dolor. Nam vestibulum ullamcorper dui in faucibus. Fusce rutrum varius tortor, vel laoreet dolor pulvinar sit amet. Phasellus a neque ultricies, egestas nunc eu, facilisis ante. Vestibulum viverra lacus id libero iaculis mattis. Cras ut odio metus.</para> 
      ///<para>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam eget luctus dui, nec blandit arcu. Cras ullamcorper suscipit ornare. Nullam euismod placerat arcu ac pharetra. Nulla blandit vestibulum mi vitae sollicitudin. Donec orci nibh, venenatis non velit sed, gravida malesuada dolor. Nam vestibulum ullamcorper dui in faucibus. Fusce rutrum varius tortor, vel laoreet dolor pulvinar sit amet. Phasellus a neque ultricies, egestas nunc eu, facilisis ante. Vestibulum viverra lacus id libero iaculis mattis. Cras ut odio metus.</para> 
      ///</remarks>
      public void Overloaded(int i) { }

      /// <summary>
      /// dummy
      /// </summary>
      /// <param name="s">dummy</param>
      /// <remarks>
      ///<para>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam eget luctus dui, nec blandit arcu. Cras ullamcorper suscipit ornare. Nullam euismod placerat arcu ac pharetra. Nulla blandit vestibulum mi vitae sollicitudin. Donec orci nibh, venenatis non velit sed, gravida malesuada dolor. Nam vestibulum ullamcorper dui in faucibus. Fusce rutrum varius tortor, vel laoreet dolor pulvinar sit amet. Phasellus a neque ultricies, egestas nunc eu, facilisis ante. Vestibulum viverra lacus id libero iaculis mattis. Cras ut odio metus.</para> 
      ///<para>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam eget luctus dui, nec blandit arcu. Cras ullamcorper suscipit ornare. Nullam euismod placerat arcu ac pharetra. Nulla blandit vestibulum mi vitae sollicitudin. Donec orci nibh, venenatis non velit sed, gravida malesuada dolor. Nam vestibulum ullamcorper dui in faucibus. Fusce rutrum varius tortor, vel laoreet dolor pulvinar sit amet. Phasellus a neque ultricies, egestas nunc eu, facilisis ante. Vestibulum viverra lacus id libero iaculis mattis. Cras ut odio metus.</para> 
      ///<para>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam eget luctus dui, nec blandit arcu. Cras ullamcorper suscipit ornare. Nullam euismod placerat arcu ac pharetra. Nulla blandit vestibulum mi vitae sollicitudin. Donec orci nibh, venenatis non velit sed, gravida malesuada dolor. Nam vestibulum ullamcorper dui in faucibus. Fusce rutrum varius tortor, vel laoreet dolor pulvinar sit amet. Phasellus a neque ultricies, egestas nunc eu, facilisis ante. Vestibulum viverra lacus id libero iaculis mattis. Cras ut odio metus.</para> 
      ///</remarks>
      public void Overloaded(double s, int a) { }
   }
}
