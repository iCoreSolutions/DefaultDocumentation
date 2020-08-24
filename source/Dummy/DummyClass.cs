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
   /// SumDummy is a class deriving from a nested class.
   /// </summary>
   /// <remarks>
   /// <para>This is the remarks for the <see cref="SubDummy{T}"/> class.</para>
   /// <para>It contains some paragraphs, with a small <c>code</c> tag.</para>
   /// <para>And finally one last paragraph, with a <see cref="IEnumerable{T}"/> (see) and also a <see langword="false"/>.</para>
   /// </remarks>
   class SubDummy<T> : DummyClass.DummyNested<T>
   {
      /// <summary>
      /// This returns a value tuple.
      /// </summary>
      public (int Apa, string Bepa) ValueTupleProp { get; }

      /// <summary>
      /// This returns a normal tuple.
      /// </summary>
      public Tuple<int, string> TupleProp { get; }


      /// <summary>
      /// This is a method with an example.
      /// </summary>
      /// <example>      
      /// This example illustrates accessing this method.
      /// <code>
      /// SubDummy&lt;int&gt; a = new SumDummy&lt;int&gt;();
      /// a.Method1(3);
      /// </code>
      /// </example>
      /// <remarks>
      /// <para>This is the remarks for the <see cref="SubDummy{T}"/> class.</para>
      /// <para>It contains some paragraphs, with a small <c>code</c> tag.</para>
      /// <para>And finally one last paragraph, with a <see cref="IEnumerable{T}"/> (see) and also a <see langword="false"/>.</para>
      /// </remarks>
      /// <exception cref="ArgumentException">Thrown when an argument is out of whack</exception>
      /// <exception cref="ArgumentNullException">Thrown when an argument is out of whack</exception>
      /// <seealso cref="DummyClass.DummyNested{T}"/>
      /// <seealso cref="StringComparer.GetHashCode(string)"/>
      /// <param name="a">This is the value to pass in to the function. It is called <c>a</c>.</param>
      /// <returns>This method actually always returns <see langword="null"/>.</returns>
      public string Method1(int a)
      {
         return null;
      }

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
      public void Overloaded(string s) { }

      /// <summary>
      /// dummy
      /// </summary>
      /// <param name="s">dummy</param>
      public void Overloaded(int i) { }

      /// <summary>
      /// dummy
      /// </summary>
      /// <param name="s">dummy</param>
      public void Overloaded(double s, int a) { }
   }
}
