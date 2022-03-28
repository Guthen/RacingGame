using Microsoft.Xna.Framework;
using System;

namespace RacingGame.Utils
{
	public static class MathUtils
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

		public static Vector2 Direction( this float angle ) => new Vector2( MathF.Cos( angle ), MathF.Sin( angle ) );
		public static float Angle( this Vector2 vector ) => MathF.Atan2( vector.Y, vector.X );
		public static Vector2 GetNormalized( this Vector2 vector )
		{
			vector.Normalize();
			return vector;
		}
		public static Vector2 Limit( this Vector2 vector, float max_length )
		{
			float length = vector.Length();
			return vector.GetNormalized() * MathF.Min( length, max_length ) / length;
		}

		public static Vector2 Rotate90( this Vector2 vector, bool is_clockwise = true ) 
			=> is_clockwise ? new Vector2( vector.Y, -vector.X ) : new Vector2( -vector.Y, vector.X );
		public static Vector2 Rotate180( this Vector2 vector ) => new Vector2( -vector.X, -vector.Y );

		public static Vector2 ToVector2( this Vector3 vector ) => new Vector2( vector.X, vector.Y );

		/* 
		 * C# '%' operator is a remainder and not a modulo (so doesn't work with negative number), that's bad but anyways:
		 * https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
		 */
		public static int Modulo( this int a, int b ) => ( a % b + b ) % b;
		public static float Modulo( this float a, float b ) => ( a % b + b ) % b;

		public static float Distance( this Point point, Point target ) => MathF.Sqrt( ( point.X - target.X ) ^ 2 + ( point.Y - target.Y ) ^ 2 );
		public static float Distance( this Point point, Vector2 target ) => MathF.Sqrt( MathF.Pow( point.X - target.X, 2f ) + MathF.Pow( point.Y - target.Y, 2f ) );
	
		/*
		 * Sources: 
		 * - https://stackoverflow.com/questions/11907947/how-to-check-if-a-point-lies-on-a-line-between-2-other-points
		 * - https://stackoverflow.com/questions/17692922/check-is-a-point-x-y-is-between-two-points-drawn-on-a-straight-line
		*/
		public static bool IsInLine( this Vector2 c, Vector2 a, Vector2 b )
		{
			if ( a.X == b.X ) return a.X == c.X; //  horizontal
			if ( a.Y == b.Y ) return a.Y == c.Y; //  vertical

			float ix = ( c.X - a.X ) / ( b.X - a.X );
			float iy = ( c.Y - a.Y ) / ( b.Y - a.Y );
			return ix == iy 
				&& ( ix >= 0 && ix <= 1 ) 
				&& ( iy >= 0 && iy <= 1 );
		}
	}
}
