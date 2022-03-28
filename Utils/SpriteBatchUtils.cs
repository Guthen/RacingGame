using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*
 * Intended for debug
 * Part of the code can be found on : https://community.monogame.net/t/line-drawing/6962/3
 */

namespace RacingGame.Utils
{
    public static class SpriteBatchUtils
	{
        private static Texture2D _texture;
        private static Texture2D GetTexture( SpriteBatch spriteBatch )
        {
            if ( _texture == null )
            {
                _texture = new Texture2D( spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color );
                _texture.SetData( new[] { Color.White } );
            }

            return _texture;
        }

        public static void DrawLine( this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f )
        {
            var distance = Vector2.Distance( point1, point2 );
            var angle = (float) Math.Atan2( point2.Y - point1.Y, point2.X - point1.X );
            DrawLine( spriteBatch, point1, distance, angle, color, thickness );
        }

        public static void DrawLine( this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f )
        {
            var origin = new Vector2( 0f, 0.5f );
            var scale = new Vector2( length, thickness );
            spriteBatch.Draw( GetTexture( spriteBatch ), point, null, color, angle, origin, scale, SpriteEffects.None, 0 );
        }


        public static void DrawRectangle( this SpriteBatch spriteBatch, Rectangle rect, Color color, float angle = 0f, Vector2 origin = new Vector2() )
		{
            spriteBatch.Draw( GetTexture( spriteBatch ), rect, null, color, angle, origin, SpriteEffects.FlipHorizontally, 0 );
		}

        #region Polygons
         public static void DrawPolygonVertex( this SpriteBatch spriteBatch, int id, BoundingPolygon polygon, Color color, int size = 2, bool draw_id = true )
		{
            Vector2 pos = polygon.Vertices[id];

            int x = (int) pos.X - size / 2;
            int y = (int) pos.Y - size / 2;
            spriteBatch.DrawRectangle( new Rectangle( x, y, size, size ), color );

            if ( draw_id )
                spriteBatch.DrawString( Game.Font, id.ToString(), new Vector2( x + 5, y ), color, 0f, Vector2.Zero, .5f, SpriteEffects.None, 0f );
		}

        public static void DrawPolygonVertices( this SpriteBatch spriteBatch, BoundingPolygon polygon, Color color, int size = 2, bool draw_id = true )
		{
            for ( int i = 0; i < polygon.Vertices.Length; i++ )
                spriteBatch.DrawPolygonVertex( i, polygon, color, size, draw_id );
		}

        public static void DrawPolygon( this SpriteBatch spriteBatch, BoundingPolygon polygon, Color color, float thickness = 1f )
		{
            Vector2 last_pos = polygon.Vertices[0];
            for ( int i = 1; i < polygon.Vertices.Length; i++ )
			{
                Vector2 pos = polygon.Vertices[i];

                spriteBatch.DrawLine( last_pos, pos, color, thickness );

                last_pos = pos;
			}

            spriteBatch.DrawLine( polygon.Vertices[0], polygon.Vertices[polygon.Vertices.Length - 1], color, thickness );
		}

        public static void DrawPolygon( this SpriteBatch spriteBatch, Rectangle rect, Color color, float thickness = 1f )
		{
            Vector2 top_left = rect.Location.ToVector2();
            Vector2 top_right = new Vector2( rect.X + rect.Width, rect.Y );
            Vector2 bottom_right = new Vector2( rect.X + rect.Width, rect.Y + rect.Height );
            Vector2 bottom_left = new Vector2( rect.X, rect.Y + rect.Height );

            spriteBatch.DrawLine( top_left, top_right, color, thickness );
            spriteBatch.DrawLine( top_right, bottom_right, color, thickness );
            spriteBatch.DrawLine( bottom_right, bottom_left, color, thickness );
            spriteBatch.DrawLine( bottom_left, top_left, color, thickness );
		}
        #endregion
    }
}
