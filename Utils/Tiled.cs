using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace RacingGame.Utils
{
	public struct Tile
	{
		public int ID;
		public Vector2[] CollisionVertices;
	}

	public struct Tileset
	{
		public Texture2D Image;
		public Point TileSize;
		public Dictionary<int, Tile> CustomTiles;
	}

	public static class Tiled
	{
		public static Tileset ReadTileset( string path )
		{
			#region LoadDocument
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load( path );
			}
			catch ( Exception e )
			{
				Console.WriteLine( "Failed to load XML tileset file at path {0}: {1}!", path, e.Message );
				return default;
			}
			#endregion

			#region ParseDocument
			Tileset tileset = new Tileset();

			//  read basic values
			tileset.TileSize = new Point( int.Parse( doc.DocumentElement.GetAttribute( "tilewidth" ) ), int.Parse( doc.DocumentElement.GetAttribute( "tileheight" ) ) );

			//  read image
			string image_path = Path.Combine( Path.GetDirectoryName( path ), "../" + doc.DocumentElement.SelectSingleNode( "image" ).Attributes.GetNamedItem( "source" ).InnerText ); //  get path from document
			image_path = Path.GetFullPath( image_path ).Substring( Directory.GetCurrentDirectory().Length + 1 ).Replace( '\\', '/' ); //  get relative path
			image_path = Path.ChangeExtension( image_path, string.Empty ).Replace( ".", string.Empty ); //  remove extension
			tileset.Image = Assets.GetAsset<Texture2D>( image_path );

			//  read custom tiles
			#region CustomTiles
			tileset.CustomTiles = new Dictionary<int, Tile>();

			foreach ( XmlElement tile_element in doc.DocumentElement.SelectNodes( "tile" ) )
			{
				//  init tile
				int tile_id = int.Parse( tile_element.GetAttribute( "id" ) );
				Tile tile = new Tile() { ID = tile_id };

				//  add polygon collision
				XmlNode polygon_node = tile_element.SelectSingleNode( "objectgroup/object/polygon" );
				if ( !( polygon_node == null ) )
				{
					string[] points = polygon_node.Attributes.GetNamedItem( "points" ).InnerText.Split( " " );
					tile.CollisionVertices = new Vector2[points.Length];

					for ( int i = 0; i < points.Length; i++ )
					{
						string[] coordinates = points[i].Split( "," );
						Vector2 vertex = new Vector2( float.Parse( coordinates[0] ), float.Parse( coordinates[1] ) );
						tile.CollisionVertices[i] = vertex;
					}
				}

				//  register tile
				tileset.CustomTiles.Add( tile_id, tile );
			}
			#endregion
			#endregion

			return tileset;
		}
	}
}
