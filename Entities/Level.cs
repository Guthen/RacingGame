using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using RacingGame.Utils;
using RacingGame.Utils.Jsonz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace RacingGame.Entities
{
	public enum Tiles
	{
		Road,
		VerticalFinishLine,
		HorizontalFinishLine,
		SpawnUp = 56,
		SpawnDown = 67,
		SpawnLeft = 55,
		SpawnRight = 68,
	}

	public class Level
	{
		public Point Size { get; protected set; }
		public int[,] MainLayer { get; protected set; }
		public int[,] WallLayer { get; protected set; }

		//[JsonConverter( typeof( Vector2ArrayConverter ) )]
		public Vector2[] SpawnPositions;
		public Vector2[] SpawnDirections;
		public Rectangle[] Checkpoints;

		public int Laps;

		public Tileset Tileset;
		public BoundingPolygon[] Colliders;

		public Level( int wide, int tall )
		{
			Size = new Point( wide, tall );

			//  layers
			MainLayer = new int[wide, tall];
			WallLayer = new int[wide, tall];

			//  spawns & checkpoints
			SpawnPositions = new Vector2[0];
			SpawnDirections = new Vector2[0];
			Checkpoints = new Rectangle[0];
		}

		public Vector2 GetSpawnPos( int i )
		{
			if ( i < 0 || i >= SpawnPositions.Length ) return Vector2.Zero;
			return SpawnPositions[i];
		}

		public Vector2 GetSpawnDir( int i )
		{
			if ( i < 0 || i >= SpawnDirections.Length ) return Vector2.Zero;
			return SpawnDirections[i];
		}

		public Rectangle GetCheckpoint( int i )
		{
			if ( i < 0 || i >= Checkpoints.Length ) return Rectangle.Empty;
			return Checkpoints[i];
		}

		public static Level ReadFile( string path )
		{
			#region LoadDocument
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load( path );
			}
			catch ( Exception e )
			{
				Console.WriteLine( "Failed to load XML level file at path {0}: {1}!", path, e.Message );
				return null;
			}
			#endregion

			#region ParseDocument
			Level level = new Level( int.Parse( doc.DocumentElement.GetAttribute( "width" ) ), int.Parse( doc.DocumentElement.GetAttribute( "height" ) ) );
			Point pos = new Point();

			#region ReadTileset
			string tileset_path = doc.DocumentElement.SelectSingleNode( "tileset" ).Attributes.GetNamedItem( "source" ).InnerText;
			Tileset tileset = Tiled.ReadTileset( Path.Combine( path, "../" + tileset_path ) );
			level.Tileset = tileset;
			#endregion

			#region ParseProperties
			XmlNode properties_node = doc.DocumentElement.SelectSingleNode( "properties" );
			if ( !( properties_node == null ) )
			{
				foreach ( XmlElement element in properties_node.ChildNodes )
				{
					switch ( element.GetAttribute( "name" ) )
					{
						case "Laps":
							level.Laps = int.Parse( element.GetAttribute( "value" ) ); 
							break;
					}
				}
			}
			#endregion

			#region ParseMainLayer
			XmlNode level_layer = doc.DocumentElement.SelectSingleNode( "layer[@name='level']" );

			List<Vector2> spawn_pos = new List<Vector2>();
			List<Vector2> spawn_dir = new List<Vector2>();

			foreach ( string data in level_layer.FirstChild.InnerText.Split( "," ) )
			{
				//  assign tile
				int tile_id = int.Parse( data ) - 1;
				level.MainLayer[pos.Y, pos.X] = tile_id;

				#region SpecialTiles
				switch ( (Tiles) tile_id )
				{
					case Tiles.SpawnUp:
						spawn_pos.Add( pos.ToVector2() );
						spawn_dir.Add( new Vector2( 0, -1 ) );
						break;
					case Tiles.SpawnDown:
						spawn_pos.Add( pos.ToVector2() );
						spawn_dir.Add( new Vector2( 0, 1 ) );
						break;
					case Tiles.SpawnLeft:
						spawn_pos.Add( pos.ToVector2() );
						spawn_dir.Add( new Vector2( -1, 0 ) );
						break;
					case Tiles.SpawnRight:
						spawn_pos.Add( pos.ToVector2() );
						spawn_dir.Add( new Vector2( 1, 0 ) );
						break;
				}
				#endregion

				//  next tile
				pos.X++;
				if ( pos.X >= level.Size.X )
				{
					pos.X = 0;
					pos.Y++;
				}
			}

			level.SpawnPositions = spawn_pos.ToArray();
			level.SpawnDirections = spawn_dir.ToArray();
			#endregion

			#region ParseWallLayer
			XmlNode wall_layer = doc.DocumentElement.SelectSingleNode( "layer[@name='walls']" );

			Dictionary<string, BoundingPolygon> colliders = new Dictionary<string, BoundingPolygon>();

			pos = new Point();
			foreach ( string data in wall_layer.FirstChild.InnerText.Split( "," ) )
			{
				//  assign tile
				int tile_id = int.Parse( data ) - 1;
				level.WallLayer[pos.Y, pos.X] = tile_id;
			
				#region Collider
				if ( tileset.CustomTiles.TryGetValue( tile_id, out Tile tile ) && tile.CollisionVertices.Length > 0 )
				{
					BoundingPolygon polygon = new BoundingPolygon( ( pos * tileset.TileSize ).ToVector2(), tile.CollisionVertices );
					if ( colliders.TryGetValue( GetTilePosID( pos.X - 1, pos.Y ), out BoundingPolygon left ) )
					{

					}
					else
						colliders.Add( GetTilePosID( pos.X, pos.Y ), polygon );	
				}
				#endregion

				//  next tile
				pos.X++;
				if ( pos.X >= level.Size.X )
				{
					pos.X = 0;
					pos.Y++;
				}
			}

			level.Colliders = colliders.Values.ToArray();
			Console.WriteLine( "created {0} collision polygons", level.Colliders.Length );
			#endregion

			#region ParseCheckpoints
			XmlNode checkpoints_layer = doc.DocumentElement.SelectSingleNode( "objectgroup[@name='checkpoints']" );

			level.Checkpoints = new Rectangle[checkpoints_layer.ChildNodes.Count];

			foreach ( XmlElement element in checkpoints_layer.ChildNodes )
			{
				Rectangle checkpoint = new Rectangle
				{
					X = int.Parse( element.GetAttribute( "x" ) ),
					Y = int.Parse( element.GetAttribute( "y" ) ),
					Width = int.Parse( element.GetAttribute( "width" ) ),
					Height = int.Parse( element.GetAttribute( "height" ) )
				};

				level.Checkpoints[int.Parse( element.GetAttribute( "name" ) )] = checkpoint;
			}

			#endregion
			#endregion

			return level;

			string GetTilePosID( int x, int y ) => x + ";" + y;
		}
	}
}
