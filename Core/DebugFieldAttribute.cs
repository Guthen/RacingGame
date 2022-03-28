using System;

namespace RacingGame.Core
{
	[AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
	public class DebugFieldAttribute : Attribute
	{
		public DebugFieldAttribute() {}

		public virtual string FormatValue( object value ) => value.ToString();
	}

	[AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
	public class DebugFloatFieldAttribute : DebugFieldAttribute
	{
		public int Digits = 2;

		public DebugFloatFieldAttribute() {}

		public override string FormatValue( object value ) => MathF.Round( (float) value, Digits ).ToString();
	}
}
