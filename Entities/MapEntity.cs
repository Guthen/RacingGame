using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Utils;
using System;

namespace RacingGame.Entities
{
	public class MapEntity : Entity, IDrawable
	{
		public Level Level;
		public Point Size;

		public Point QuadSize = new Point( 16, 16 );
		protected static Rectangle[] quads;
		protected static Texture2D texture;

		public MapEntity()
		{
			//SetTexture( texture == null ? Assets.GetAsset<Texture2D>( "Images/tileset" ) : texture );
		}

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

		private void DrawLayer( SpriteBatch spriteBatch, int[,] layer )
		{
			for ( int y = 0; y < Size.Y; y++ )
				for ( int x = 0; x < Size.X; x++ )
					if ( layer[y, x] >= 0 )
						spriteBatch.Draw( texture, new Vector2( x * QuadSize.X, y * QuadSize.Y ), quads[layer[y, x]], Color.White );
		}

		public void Draw( SpriteBatch spriteBatch )
		{
			DrawLayer( spriteBatch, Level.MainLayer );
			DrawLayer( spriteBatch, Level.WallLayer );

			if ( Game.Debug )
			{
				#region DrawDebugCheckpoints
				foreach ( Rectangle checkpoint in Level.Checkpoints )
					spriteBatch.DrawPolygon( checkpoint, Color.LightGreen );
				#endregion

				#region DrawDebugColliders
				foreach ( BoundingPolygon collider in Level.Colliders )
					spriteBatch.DrawPolygon( collider, Color.LightGray );
				#endregion
			}
		}
	}
}
