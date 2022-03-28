using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Core;
using RacingGame.Utils;
using System;
using IDrawable = RacingGame.Core.IDrawable;

namespace RacingGame.Gameplay
{
	public class MapEntity : Entity, IDrawable
	{
		public static MapEntity Main { get; protected set; }

		public Level Level;
		public Point Size;

		public Point QuadSize = new Point( 16, 16 );
		protected static Rectangle[] quads;
		protected static Texture2D texture;

		private static readonly Color highlightColliderColor = new Color( 1f, 1f, 1f, 1f );
		private static readonly Color colliderColor = new Color( 1f, 1f, 1f, .4f );

		public MapEntity() => Main = this;

		public void SetTexture( Texture2D image )
		{
			texture = image;
			quads = Assets.SplitTextureInQuads( texture, QuadSize );
		}

		public Vector2 GridToWorld( Vector2 grid_pos, bool centered = false )
		{
			Vector2 world_pos = new Vector2( grid_pos.X * QuadSize.X, grid_pos.Y * QuadSize.Y );
			if ( centered )
			{
				world_pos.X += QuadSize.X / 2;
				world_pos.Y += QuadSize.Y / 2;
			}

			return world_pos;
		}
		public Vector2 WorldToGrid( Vector2 world_pos ) => new Vector2( MathF.Floor( world_pos.X / QuadSize.X ), MathF.Floor( world_pos.Y / QuadSize.Y ) );

		public BoundingPolygon IsColliding( BoundingPolygon polygon )
		{
			foreach ( BoundingPolygon collider in Level.Colliders )
				if ( SAT.Intersect( polygon, collider ) )
					return collider;

			return null;
		}

		public void Clear( int wide, int tall )
		{
			Level = new Level( wide, tall );
			Size.X = wide;
			Size.Y = tall;
		}

		public void Clear( Level _level )
		{
			Level = _level;
			Size.X = Level.Size.X;
			Size.Y = Level.Size.Y;
		}

		public void Load( string path )
		{
			Level = Level.ReadFile( path );
			Size.X = Level.Size.X;
			Size.Y = Level.Size.Y;

			QuadSize = Level.Tileset.TileSize;
			SetTexture( Level.Tileset.Image );
		}

		private void DrawLayers( SpriteBatch spriteBatch, params int[][,] layers )
		{
			for ( int y = 0; y < Size.Y; y++ )
				for ( int x = 0; x < Size.X; x++ )
					foreach ( int[,] layer in layers )
						if ( layer[y, x] >= 0 )
							spriteBatch.Draw( texture, new Vector2( x * QuadSize.X, y * QuadSize.Y ), quads[layer[y, x]], Color.White );
		}

		public void Draw( SpriteBatch spriteBatch )
		{
			DrawLayers( spriteBatch, Level.MainLayer, Level.WallLayer );

			#region DrawDebugCheckpoints
			if ( Game.DebugLevel == DebugLevel.Checkpoints )
				for ( int i = 0; i < Level.Checkpoints.Length; i++ )
				{
					Rectangle checkpoint = Level.Checkpoints[i];
					spriteBatch.DrawPolygon( checkpoint, Color.LightGreen );
					spriteBatch.DrawString( Game.Font, i.ToString(), checkpoint.Center.ToVector2(), Color.LightGreen );
				}
			#endregion

			#region DrawDebugColliders
			if ( Game.DebugLevel == DebugLevel.Colliders )
			{
				//  highlight a polygon
				BoundingPolygon polygon = Level.Colliders[(int) ( Game.CurrentTime ) % Level.Colliders.Length];
				spriteBatch.DrawPolygon( polygon, highlightColliderColor );
				spriteBatch.DrawPolygonVertices( polygon, colliderColor, 2, false );
				spriteBatch.DrawPolygonVertex( (int) ( Game.CurrentTime * polygon.Vertices.Length ) % polygon.Vertices.Length, polygon, highlightColliderColor );
			
				//  draw other polygon in alpha
				foreach ( BoundingPolygon current in Level.Colliders )
				{
					if ( current == polygon ) continue;

					spriteBatch.DrawPolygon( current, colliderColor );
					spriteBatch.DrawPolygonVertices( current, colliderColor, 2, false );
				}
			}
			#endregion
		}
	}
}
