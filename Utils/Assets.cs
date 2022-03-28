using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace RacingGame.Utils
{
	public static class Assets
	{
		public static ContentManager Content;
		public static GraphicsDevice GraphicsDevice;

		private static Dictionary<string, object> assets = new Dictionary<string, object>();

		public static void Cache<T>( string path )
		{
			SetAsset( path, Content.Load<T>( path ) );
		}

		public static void CacheDirectory<T>( string dir_path )
		{
			DirectoryInfo dir = new DirectoryInfo( Content.RootDirectory + "/" + dir_path );
			foreach ( FileInfo file in dir.GetFiles() )
			{
				string path = dir.Name + "/" + file.Name.Replace( ".xnb", "" );
				Cache<T>( path );
			}
		}

		public static T GetAsset<T>( string path )
		{
			if ( !assets.ContainsKey( path ) ) return default;
			return (T) assets[path];
		}

		public static void SetAsset( string path, object asset )
		{
			assets.Add( path, asset );
		}

		public static Texture2D[] SplitTexture( Texture2D texture, Point quad_size )
		{
			//  init array
			int count_x = texture.Bounds.Width / quad_size.X;
			int count_y = texture.Bounds.Height / quad_size.Y;
			Texture2D[] quads = new Texture2D[count_x * count_y];

			int quad_id = 0;
			for ( int y = 0; y < count_y; y++ )
			{
				for ( int x = 0; x < count_x; x++ )
				{
					//  init quad
					Rectangle quad_bounds = new Rectangle( x * quad_size.X, y * quad_size.Y, quad_size.X, quad_size.Y );
					Texture2D quad = new Texture2D( GraphicsDevice, quad_size.X, quad_size.Y );

					//  copy & paste color data
					Color[] data = new Color[quad_size.X * quad_size.Y];
					texture.GetData( 0, quad_bounds, data, 0, data.Length );
					quad.SetData( data );

					//  assign quad
					quads[quad_id] = quad;
					quad_id++;
				}
			}

			return quads;
		}

		public static Rectangle[] SplitTextureInQuads( Texture2D texture, Point quad_size )
		{
			//  init array
			int count_x = texture.Bounds.Width / quad_size.X;
			int count_y = texture.Bounds.Height / quad_size.Y;
			Rectangle[] quads = new Rectangle[count_x * count_y];

			int quad_id = 0;
			for ( int y = 0; y < count_y; y++ )
			{
				for ( int x = 0; x < count_x; x++ )
				{
					//  assign quad
					quads[quad_id] = new Rectangle( x * quad_size.X, y * quad_size.Y, quad_size.X, quad_size.Y );
					quad_id++;
				}
			}

			return quads;
		}
	}
}
