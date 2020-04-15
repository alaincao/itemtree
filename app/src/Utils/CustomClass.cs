
namespace ItemTTT
{
	public static partial class Utils
	{
		public class CustomClass1<T1>
		{
			public T1 A { get; set; }

			public override int GetHashCode()
			{
				return A.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var cc = obj as CustomClass1<T1>;
				if( cc == null )
					return false;
				if( ! cc.A.Equals(this.A) )
					return false;
				return true;
			}

			public override string ToString()
			{
				return "A=" + A;
			}
		}

		public class CustomClass2<T1,T2>
		{
			public T1 A { get; set; }
			public T2 B { get; set; }

			public override int GetHashCode()
			{
				return A.GetHashCode() ^ B.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var cc = obj as CustomClass2<T1,T2>;
				if( cc == null )
					return false;
				if( ! cc.A.Equals(this.A) )
					return false;
				if( ! cc.B.Equals(this.B) )
					return false;
				return true;
			}

			public override string ToString()
			{
				return "A=" + A + " ; B=" + B;
			}
		}

		public class CustomClass3<T1,T2,T3>
		{
			public T1 A { get; set; }
			public T2 B { get; set; }
			public T3 C { get; set; }

			public override int GetHashCode()
			{
				return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var cc = obj as CustomClass3<T1,T2,T3>;
				if( cc == null )
					return false;
				if( ! cc.A.Equals(this.A) )
					return false;
				if( ! cc.B.Equals(this.B) )
					return false;
				if( ! cc.C.Equals(this.C) )
					return false;
				return true;
			}

			public override string ToString()
			{
				return "A=" + A + " ; B=" + B + " ; C=" + C;
			}
		}

		public class CustomClass4<T1,T2,T3,T4>
		{
			public T1 A { get; set; }
			public T2 B { get; set; }
			public T3 C { get; set; }
			public T4 D { get; set; }

			public override int GetHashCode()
			{
				return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode() ^ D.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var cc = obj as CustomClass4<T1,T2,T3,T4>;
				if( cc == null )
					return false;
				if( ! cc.A.Equals(this.A) )
					return false;
				if( ! cc.B.Equals(this.B) )
					return false;
				if( ! cc.C.Equals(this.C) )
					return false;
				if( ! cc.D.Equals(this.D) )
					return false;
				return true;
			}
		}

		public class CustomClass5<T1,T2,T3,T4,T5>
		{
			public T1 A { get; set; }
			public T2 B { get; set; }
			public T3 C { get; set; }
			public T4 D { get; set; }
			public T5 E { get; set; }

			public override int GetHashCode()
			{
				return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode() ^ D.GetHashCode() ^ E.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var cc = obj as CustomClass5<T1,T2,T3,T4,T5>;
				if( cc == null )
					return false;
				if( ! cc.A.Equals(this.A) )
					return false;
				if( ! cc.B.Equals(this.B) )
					return false;
				if( ! cc.C.Equals(this.C) )
					return false;
				if( ! cc.D.Equals(this.D) )
					return false;
				if( ! cc.E.Equals(this.E) )
					return false;
				return true;
			}

			public override string ToString()
			{
				return "A=" + A + " ; B=" + B + " ; C=" + C + " ; D=" + D + " ; E=" + E;
			}
		}
	}
}
