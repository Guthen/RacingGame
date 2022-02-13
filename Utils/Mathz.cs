using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace RacingGame.Utils
{
	public static class Mathz
	{
		public const float RadToDeg = 180f / MathF.PI;
		public const float DegToRad = MathF.PI / 180f;

		public static float Lerp( float t, float a, float b ) => ( 1 - t ) * a + b * t;
		public static float Approach( float inc, float current, float target ) 
		{
			if ( target <= current )
				return Math.Max( target, current - inc );
			else if ( target >= current )
				return Math.Min( target, current + inc );

			return target;
		}

		public static float Angle( this Vector2 vector ) => MathF.Atan2( vector.Y, vector.X );

		/* 
		 * C# '%' operator is a remainder and not a modulo (so doesn't work with negative number), that's bad but anyways:
		 * https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
		 */
		public static int Modulo( this int a, int b ) => ( a % b + b ) % b;

		public static float Distance( this Point point, Point target ) => MathF.Sqrt( ( point.X - target.X ) ^ 2 + ( point.Y - target.Y ) ^ 2 );
		public static float Distance( this Point point, Vector2 target ) => MathF.Sqrt( MathF.Pow( point.X - target.X, 2f ) + MathF.Pow( point.Y - target.Y, 2f ) );
	}
}
