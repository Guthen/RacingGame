using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RacingGame.Core;
using RacingGame.Utils;
using System;
using System.Collections.Generic;
using IDrawable = RacingGame.Core.IDrawable;

namespace RacingGame.Scenes
{
	class SATDemoScene : BaseScene, IUpdate, IDrawable
	{
		//private float rotationSpeed = 90f;

		private BoundingPolygon a;
		private BoundingPolygon b;
		private BoundingPolygon c;
		private BoundingPolygon d;
		private BoundingPolygon e;
		private BoundingPolygon f;
		private BoundingPolygon bc;

		private List<BoundingPolygon> polygons = new List<BoundingPolygon>();
		private Color[] colors = new Color[]
		{
			Color.Red,
			Color.Blue,
			Color.Green,
			Color.Aqua,
			Color.Violet,
			Color.Bisque,
			Color.CornflowerBlue,
			Color.Purple,
			Color.Brown,
			Color.DarkCyan,
			Color.Gray,
			Color.Azure,
		};

		public override void Initialize()
		{
			a = new BoundingPolygon( Vector2.Zero, new Vector2[] 
			{
				new Vector2( 0, 0 ),
				new Vector2( 70, 0 ),
				new Vector2( 0, 70 ),
			} );
			/*b = new BoundingPolygon( new Vector2( 100, 100 ), new Vector2[]
			{
				new Vector2( -40, 0 ),
				new Vector2( 0, -30 ),
				new Vector2( 40, 0 ),
				new Vector2( 25, 50 ),
			} );
			c = new BoundingPolygon( new Vector2( 175, 100 ), new Vector2[]
			{
				new Vector2( -35, 0 ),
				new Vector2( 0, 40 ),
				//new Vector2( 40, 0 ),
				new Vector2( -50, 50 ),
			} );*/

			/*b = new BoundingPolygon( new Vector2( 100, 100 ), new Vector2[]
			{
				new Vector2( 0, 0 ),
				new Vector2( 50, 0 ),
				new Vector2( 50, 100 ),
				new Vector2( 25, 100 ),
				new Vector2( 25, 50 ),
				new Vector2( 0, 50 ),
			} );*/

			b = BoundingPolygon.FromRectangle( new Rectangle( 100, 100, 50, 25 ) );
			c = BoundingPolygon.FromRectangle( new Rectangle( 150, 100, 50, 25 ) );
			d = BoundingPolygon.FromRectangle( new Rectangle( 200, 100, 50, 25 ) );
			/*b = new BoundingPolygon( new Vector2( 100, 100 ), new Vector2[] 
			{
				new Vector2( 16, 0 ),
				new Vector2( 16, 16 ),
				new Vector2( 0, 16 ),
			} );
			c = new BoundingPolygon( new Vector2( 100, 116 ), new Vector2[] 
			{
				new Vector2( 0, 0 ),
				new Vector2( 16, 0 ),
				new Vector2( 0, 16 ),
			} );
			d = new BoundingPolygon( new Vector2( 100 - 16, 116 ), new Vector2[] 
			{
				new Vector2( 16, 0 ),
				new Vector2( 16, 16 ),
				new Vector2( 0, 16 ),
			} );*/
			polygons.AddRange( new BoundingPolygon[] { a, b, c, d } );

			/*Vector2 pos = new Vector2( 100 - 16, 132 );
			for ( int i = 1; i < 3; i++ )
			{
				polygons.Add( 
					new BoundingPolygon( pos, new Vector2[] 
					{
						new Vector2( 0, 0 ),
						new Vector2( 16, 0 ),
						new Vector2( 0, 16 ),
					} ) 
				);

				pos.X -= 16;
				polygons.Add( 
					new BoundingPolygon( pos, new Vector2[] 
					{
						new Vector2( 16, 0 ),
						new Vector2( 16, 16 ),
						new Vector2( 0, 16 ),
					} )
				);

				pos.Y += 16;
			}*/

			/*Console.WriteLine( SAT.Intersect( a, b ) );
			Console.WriteLine( SAT.Intersect( a, c ) );
			Console.WriteLine( SAT.Intersect( b, c ) );*/
			/*Console.WriteLine( "C & B Intersecting? " + SAT.Intersect( c, b ) );
			Console.WriteLine( "B Convex? " + BoundingPolygon.IsConvex( b ) );
			Console.WriteLine( "C Convex? " + BoundingPolygon.IsConvex( c ) );*/

			/*Console.WriteLine( "Merged C & B? " + BoundingPolygon.TryMerge( b, c, out bc, false ) );
			if ( !( bc == null ) )
			{
				Console.WriteLine( "Merged BC & D? " + BoundingPolygon.TryMerge( bc, d, out BoundingPolygon temp_bc ) );
				if ( !( temp_bc == null ) )
					bc = temp_bc;
			}*/

			if ( BoundingPolygon.TryMerge( b, c, out bc ) )
				for ( int i = 3; i < polygons.Count; i++ )
					if ( !BoundingPolygon.TryMerge( bc, polygons[i], out bc ) )
						break;

			/*if ( !( bc == null ) )
			{
				Console.WriteLine( "BC & D Intersecting? " + SAT.Intersect( bc, d ) );
				Console.WriteLine( "BC Convex? " + BoundingPolygon.IsConvex( bc ) );
			}*/
		}

		public void Update( float dt )
		{
			MouseState state = Mouse.GetState();
			if ( state.LeftButton == ButtonState.Pressed )
				a.Position = Game.Camera.TranslateScreenPosition( state.Position.ToVector2() );
			if ( state.RightButton == ButtonState.Pressed )
				d.SetVertex( 0, Game.Camera.TranslatePosition( state.Position.ToVector2() - d.Position) /*- Game.Camera.TranslatePosition( d.Position )*/ );

			//d.Angle += rotationSpeed * dt;
		}

		public void Draw( SpriteBatch spriteBatch )
		{
			bool is_intersected = SAT.Intersect( a, d );
			spriteBatch.DrawString( Game.Font, is_intersected ? "Collision!" : "No collision", Vector2.One, is_intersected ? Color.Red : Color.Green );
			
			bool is_convex = BoundingPolygon.IsConvex( d.Vertices );
			spriteBatch.DrawString( Game.Font, is_convex ? "Convex" : "Concave", new Vector2( 1, 24 ), is_convex ? Color.Green : Color.Red );

			#region DrawPolygons
			for ( int i = 0; i < polygons.Count; i++ )
				spriteBatch.DrawPolygon( polygons[i], colors[i] );

			if ( Game.DebugLevel == DebugLevel.Colliders )
			{
				BoundingPolygon polygon = polygons[(int) ( Game.CurrentTime * 2 ) % polygons.Count];
				spriteBatch.DrawPolygonVertices( polygon, Color.WhiteSmoke );
			}
			else if ( Game.DebugLevel == DebugLevel.Checkpoints && !( bc == null ) )
			{
				spriteBatch.DrawPolygon( bc, Color.WhiteSmoke );
				spriteBatch.DrawPolygonVertex( (int) ( Game.CurrentTime * 3 ) % bc.Vertices.Length, bc, Color.WhiteSmoke );
			}
			#endregion
		}
	}
}
