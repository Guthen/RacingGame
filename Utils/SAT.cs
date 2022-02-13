using Microsoft.Xna.Framework;
using RacingGame.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

/*
 * Implementation of the Separating Axis Theorem by myself (Vyrkx A.K.A. Guthen A.K.A. Arthur CATHELAIN)
 * Sources:
 * ─ https://gamedevelopment.tutsplus.com/tutorials/collision-detection-using-the-separating-axis-theorem--gamedev-169
 * ─ http://programmerart.weebly.com/separating-axis-theorem.html
 * ─ https://github.com/JuantAldea/Separating-Axis-Theorem	
*/

namespace RacingGame.Utils
{
	public class BoundingPolygon
	{
		public Vector2 Position { 
			get => parent == null ? position : parent.Position; 
			set {
				if ( value == position ) return;
				position = value;

				//  update vertices
				UpdateVertices();
			}
		}
		public Vector2[] Vertices; //  transformed vertices in world coordinates
		public float Angle { 
			get => angle; 
			set {
				if ( value == angle ) return;
				angle = value;

				//  update vertices
				UpdateVertices();
			} 
		}

		private Vector2[] vertices; //  non-transformed vertices in relative coordinates
		private Vector2 position; //  position of polygon
		private float angle;
		private GameEntity parent; //  TODO: update vertices when attached to parent

		public BoundingPolygon( Vector2 pos, int n_verts, float ang = 0f )
		{
			position = pos;
			angle = ang;

			Vertices = new Vector2[n_verts];
			vertices = new Vector2[n_verts];
		}

		public BoundingPolygon( Vector2 pos, Vector2[] verts, float ang = 0f )
		{
			position = pos;
			angle = ang;

			vertices = verts;
			Vertices = new Vector2[verts.Length];
			UpdateVertices();
		}

		public void SetVertex( int id, Vector2 pos )
		{
			vertices[id] = pos;
			UpdateVertex( id );
		}

		public void AttachTo( GameEntity entity ) => parent = entity;
		public void UpdateVertex( int id )
		{
			Vector2 pos = vertices[id];
			float ang = Angle * Mathz.DegToRad;

			Vertices[id].X = Position.X + MathF.Cos( ang ) * pos.X - MathF.Sin( ang ) * pos.Y;
			Vertices[id].Y = Position.Y + MathF.Sin( ang ) * pos.X + MathF.Cos( ang ) * pos.Y;
		}
		public void UpdateVertices() {
			for( int i = 0; i < vertices.Length; i++ ) 
				UpdateVertex( i );
		}

		/// <summary>
		/// Compute normal vectors of polygon edges used for SAT Algorithm
		/// </summary>
		/// <param name="axes">Array to push normals</param>
		public virtual Vector2[] GetAxes()
		{
			Vector2[] axes = new Vector2[Vertices.Length];
			if ( Vertices.Length == 0 ) return axes;

			//  add edges normals
			for ( int i = 0; i < Vertices.Length; i++ )
			{
				Vector2 edge = Vertices[( i + 1 ) % Vertices.Length] - Vertices[i];

				Vector2 normal = new Vector2( edge.Y, -edge.X );
				normal.Normalize();
				axes[i] = normal;
			}

			return axes;
		}

		public static BoundingPolygon FromRectangle( Rectangle rect )
		{
			Vector2 center = new Vector2( rect.Center.X, rect.Center.Y );
			return new BoundingPolygon( center, new Vector2[]
			{
				new Vector2( rect.Left, rect.Bottom ) - center,
				new Vector2( rect.Left, rect.Top ) - center,
				new Vector2( rect.Right, rect.Top ) - center,
				new Vector2( rect.Right, rect.Bottom ) - center,
			} );
		}

		/*
		 * My own & simple algorithm to merge two polygon with common vertices in one
		 */
		/// <summary>
		/// Try merging two polygons into one if at least two vertices are common
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="result"></param>
		/// <param name="convex">If resulted polygon must be convex</param>
		/// <returns>If merge was successful</returns>
		public static bool TryMerge( BoundingPolygon a, BoundingPolygon b, out BoundingPolygon result, bool convex = true )
		{
			List<Vector2> vertices = new List<Vector2>();

			int current_id = 0;
			BoundingPolygon current = a;
			BoundingPolygon other = b;
			for ( int i = 0; i < a.Vertices.Length + b.Vertices.Length; i++ )
			{
				Vector2 vertex = current.Vertices[current_id];
				bool is_common = false;

				//  check for a common vertex
				for ( int j = 0; j < other.Vertices.Length; j++ )
				{
					if ( other.Vertices[j] == vertex )
					{
						is_common = true;

						//  switch polygons
						BoundingPolygon temp = current;
						current = other;
						other = temp;

						vertices.Add( vertex - a.Position );
						current_id = ( j + 1 ) % current.Vertices.Length;
						break;
					}
				}

				if ( !is_common )
				{
					vertices.Add( vertex - a.Position );
					current_id++;
				}
			}

			//  check convexity
			Vector2[] vertices_array = vertices.ToArray();
			if ( convex && !IsConvex( vertices_array ) )
			{
				result = null;
				return false;
			}

			//  compute result
			result = new BoundingPolygon( a.Position, vertices_array );
			return true;
		}

		/*
		 * Simple algorithm to know if a polygon is convex or not.
		 * Basically, it loop over all edges and checks if the direction (left or right) of the next edge is different from the previous one
		 * Source:
		 * ─ http://www.sunshine2k.de/coding/java/Polygon/Convex/polygon.htm#:~:text=polygon%20is%20convex%2C%20I%20came,polygon%20does%20not%20cross%20itself
		 */
		public static bool IsConvex( Vector2[] vertices )
		{
			if ( vertices.Length < 3 ) throw new ArgumentException( "There should be at least 3 vertices!" );
			if ( vertices.Length == 3 ) return true; //  triangles are always convex

			float old_dot = 0f;

			for ( int i = 0; i < vertices.Length; i++ )
			{
				Vector2 p = vertices[i];
				Vector2 u = vertices[( i + 2 ) % vertices.Length];

				Vector2 v = vertices[( i + 1 ) % vertices.Length] - p;
				
				float dot = u.X * v.Y - u.Y * v.X + v.X * p.Y - v.Y * p.X;
				if ( i == 0 )
					old_dot = dot;
				else if ( ( dot < 0 && old_dot > 0 ) || ( dot > 0 && old_dot < 0 ) )
					return false;
			}

			return true;
		}
		public static bool IsConvex( BoundingPolygon polygon ) => IsConvex( polygon.vertices );
	}

	public static class SAT
	{
		public static bool Intersect( BoundingPolygon a, BoundingPolygon b )
		{
			Vector2[] axes = a.GetAxes().Concat( b.GetAxes() ).ToArray();

			foreach ( Vector2 axis in axes )
			{
				Vector2 a_projection = Project( a.Vertices, axis );
				Vector2 b_projection = Project( b.Vertices, axis );

				if ( !Overlap( a_projection, b_projection ) )
					return false;
			}
			return true;
		}

		private static bool Overlap( Vector2 a_projection, Vector2 b_projection )
		{
			return MathF.Min( a_projection.X, a_projection.Y ) <= MathF.Max( b_projection.X, b_projection.Y )
				&& MathF.Min( b_projection.X, b_projection.Y ) <= MathF.Max( a_projection.X, a_projection.Y );
		}

		private static Vector2 Project( Vector2[] vertices, Vector2 axis )
		{
			Vector2 projection = new Vector2( float.PositiveInfinity, float.NegativeInfinity );

			foreach ( Vector2 vertex in vertices )
			{
				float dot = Vector2.Dot( vertex, axis );
				projection.X = MathF.Min( projection.X, dot );
				projection.Y = MathF.Max( projection.Y, dot );
			}

			return projection;
		}
	}
}
