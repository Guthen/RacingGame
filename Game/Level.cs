using Microsoft.Xna.Framework;
using RacingGame.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace RacingGame.Gameplay
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

			Dictionary<BoundingPolygon, List<BoundingPolygon>> colliders_adjacents = new Dictionary<BoundingPolygon, List<BoundingPolygon>>();
			Dictionary<string, BoundingPolygon> colliders_by_pos = new Dictionary<string, BoundingPolygon>();
			//Dictionary<BoundingPolygon, List<string>> tile_ids_by_collider = new Dictionary<BoundingPolygon, List<string>>();

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
					colliders_by_pos.Add( getTilePosID( pos.X, pos.Y ), polygon );

					#region ListAdjacentPolygons
					List<BoundingPolygon> adjacents = new List<BoundingPolygon>();
					if ( colliders_by_pos.TryGetValue( getTilePosID( pos.X - 1, pos.Y ), out BoundingPolygon temp ) )
					{
						adjacents.Add( temp );
						colliders_adjacents[temp].Add( polygon );
					}
					if ( colliders_by_pos.TryGetValue( getTilePosID( pos.X, pos.Y - 1 ), out temp ) )
					{
						adjacents.Add( temp );
						colliders_adjacents[temp].Add( polygon );
					}
					colliders_adjacents.Add( polygon, adjacents );
					#endregion
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

			Console.WriteLine( "Level: Created {0} colliders", colliders_by_pos.Count );
			level.Colliders = colliders_by_pos.Values.ToArray();
			level.MergeColliders( colliders_adjacents );
			#endregion

			#region ParseCheckpoints
			XmlNode checkpoints_layer = doc.DocumentElement.SelectSingleNode( "objectgroup[@name='checkpoints']" );

			level.Checkpoints = new Rectangle[checkpoints_layer.ChildNodes.Count];

			foreach ( XmlElement element in checkpoints_layer.ChildNodes )
			{
				try
				{
					Rectangle checkpoint = new Rectangle
					{
						X = (int) float.Parse( element.GetAttribute( "x" ), CultureInfo.InvariantCulture ),
						Y = (int) float.Parse( element.GetAttribute( "y" ), CultureInfo.InvariantCulture ),
						Width = (int) float.Parse( element.GetAttribute( "width" ), CultureInfo.InvariantCulture ),
						Height = (int) float.Parse( element.GetAttribute( "height" ), CultureInfo.InvariantCulture )
					};

					level.Checkpoints[int.Parse( element.GetAttribute( "name" ) )] = checkpoint;
				}
				catch ( Exception e )
				{
					throw new FormatException( string.Format( "Checkpoint #{0} contains unexpected values, please verify that the attributes 'x', 'y', 'width' & 'height' are numbers.", element.GetAttribute( "name" ) ), e );
				}
			}

			#endregion
			#endregion

			return level;
			
			static string getTilePosID( int x, int y ) => x + ";" + y;
		}

		private BoundingPolygon[] MergeColliders( BoundingPolygon[] colliders, Dictionary<BoundingPolygon, List<BoundingPolygon>> adjacents )
		{
			HashSet<BoundingPolygon> merged_colliders = new HashSet<BoundingPolygon>();

			List<BoundingPolygon> result_colliders = new List<BoundingPolygon>();
			for ( int i = 0; i < colliders.Length; i++ )
			{
				BoundingPolygon collider = colliders[i];
				if ( merged_colliders.Contains( collider ) ) continue;
				merged_colliders.Add( collider );

				int n_vertices = collider.Vertices.Length;
				foreach ( BoundingPolygon adjacent in adjacents[collider] )
					mergeCollider( ref collider, adjacent, n_vertices );

				result_colliders.Add( collider );
			}

			Console.WriteLine( "Level: Optimized to {0} colliders (reduced by {1}%)", result_colliders.Count, (float) colliders.Length / result_colliders.Count * 100 );
			return result_colliders.ToArray();

			void mergeCollider( ref BoundingPolygon collider, BoundingPolygon current, int n_vertices )
			{
				if ( merged_colliders.Contains( current ) ) return;
				if ( !( current.Vertices.Length == n_vertices ) ) return;  //  only merge polygons with the same number of vertices

				if ( BoundingPolygon.TryMerge( collider, current, out BoundingPolygon temp ) )
				{
					collider = temp;
					merged_colliders.Add( current );

					//  merge adjacents (recursive)
					foreach ( BoundingPolygon adjacent in adjacents[current] )
						mergeCollider( ref collider, adjacent, n_vertices );
				}
			}
		}
		private void MergeColliders( Dictionary<BoundingPolygon, List<BoundingPolygon>> adjacents )
			=> Colliders = MergeColliders( Colliders, adjacents );
	}
}
