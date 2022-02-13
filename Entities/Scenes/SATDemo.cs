using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RacingGame.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace RacingGame.Entities.Scenes
{
	class SATDemo : Scene, IUpdate, IDrawable
	{
		private float rotationSpeed = 90f;

		private BoundingPolygon a;
		private BoundingPolygon b;
		private BoundingPolygon c;
		private BoundingPolygon d;
		private BoundingPolygon bc;

		public override void Initialize()
		{
			a = new BoundingPolygon( Vector2.Zero, new Vector2[] 
			{
				new Vector2( 0, 0 ),
				new Vector2( 70, 0 ),
				new Vector2( 0, 70 ),
			} );
			b = new BoundingPolygon( new Vector2( 100, 100 ), new Vector2[]
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
			} );
			d = BoundingPolygon.FromRectangle( new Rectangle( 250, 100, 50, 25 ) );
			//d.Position = Vector2.Zero;

			/*Console.WriteLine( SAT.Intersect( a, b ) );
			Console.WriteLine( SAT.Intersect( a, c ) );
			Console.WriteLine( SAT.Intersect( b, c ) );*/
			Console.WriteLine( "Merged C & B? " + BoundingPolygon.TryMerge( b, c, out bc ) );
			Console.WriteLine( "B Convex? " + BoundingPolygon.IsConvex( b ) );
			Console.WriteLine( "C Convex? " + BoundingPolygon.IsConvex( c ) );
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

			spriteBatch.DrawPolygon( a, Color.Red );
			spriteBatch.DrawPolygon( b, Color.Green );
			spriteBatch.DrawPolygon( c, Color.Blue );
			spriteBatch.DrawPolygon( d, Color.Purple );

			if ( !( bc == null ) )
				spriteBatch.DrawPolygon( bc, Color.Azure );
		}
	}
}
