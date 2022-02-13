using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RacingGame.Entities;
using RacingGame.Entities.Scenes;
using RacingGame.Utils;
using System;

/*
 * COOL BUT NOT IDEAS TO MAKE RIGHT NOW:
 * - drift
 * - parts-based car : add a damage system; when a car collide with something (e.g: wall or other cars) it should be damaged accordingly
 *	 to impact position and car's speed, being able to rip off the engine or a wheel for example (or more commonly the bodywork)
 * 
 * TODO RIGHT NOW:
 * - implement walls collisions
 * - mouse pressed/released
 * - game loop: win
 * - ai system
 * - menu : main, skin selection & level selection
 * - 2 levels
 * 
 * DONE:
 * - keyboard pressed/released
 * - map system: 1 level
 * - game loop: laps
 * - Separating Axis Theorem collisions
 */

namespace RacingGame
{
	public class Game : Microsoft.Xna.Framework.Game, IInputReceiver
	{
		private SpriteBatch _spriteBatch;

		public Vector2 RenderSize = new Vector2( 320f, 180f );

		public static Scene Scene { get; protected set; }
		public static Game Instance { get; protected set; }
		public static Camera Camera { get; protected set; }
		public static SpriteFont Font;

		public static bool Debug;

		public Game()
		{
			new GraphicsDeviceManager( this );

			Content.RootDirectory = "Assets";
			Assets.Content = Content;
			Assets.GraphicsDevice = GraphicsDevice;

			IsMouseVisible = true;
			Window.AllowUserResizing = true;

			Inputz.AddReceiver( this );
		}

		public static void SetScene<T>() where T : Scene, new()
		{
			if ( !( Scene == null ) )
				Scene.QueueFree();
			EntityManager.Clear();

			Scene = new T();
		}

		protected override void LoadContent()
		{
			Console.WriteLine( "Game: Loading Content" );
			_spriteBatch = new SpriteBatch( GraphicsDevice );

			Font = Content.Load<SpriteFont>( "Fonts/default" );
			Assets.CacheDirectory<Texture2D>( "Images" );
		}

		protected override void Initialize()
		{
			base.Initialize();

			Console.WriteLine( "Game: Initialize" );
			Inputz.Initialize();
			Camera = new Camera( RenderSize, Window, .85f );

			SetScene<GameScene>();
		}

		protected override void Update( GameTime gameTime )
		{
			Inputz.Update();

			EntityManager.Update( (float) gameTime.ElapsedGameTime.TotalSeconds );
			EntityManager.UpdateDeletionQueue();

			base.Update( gameTime );
		}

		protected override void Draw( GameTime gameTime )
		{
			GraphicsDevice.Clear( Color.Black );

			//  game entities
			_spriteBatch.Begin( transformMatrix: Camera.Transform, samplerState: SamplerState.PointWrap );
			EntityManager.Draw( _spriteBatch );
			_spriteBatch.End();

			//  hud
			_spriteBatch.Begin( /*transformMatrix: Camera.ViewportMatrix*/ );
			_spriteBatch.DrawString( Font, string.Format( "{0} ents ({1} U; {2} D; {3} HUD)", EntityManager.Entities.Count, EntityManager.UpdateEntities.Count, EntityManager.DrawableEntities.Count, EntityManager.DrawableHUDs.Count ), Vector2.One * 6, Color.White );
			_spriteBatch.DrawString( Font, Inputz.InputReceivers.Count + " input receivers", new Vector2( 6, Font.MeasureString( "a" ).Y + 6 ), Color.White );
			EntityManager.DrawHUD( _spriteBatch );
			/*
			_spriteBatch.DrawLine( Vector2.Zero, new Vector2( Window.ClientBounds.Width, Window.ClientBounds.Height ), Color.White );
			_spriteBatch.DrawLine( new Vector2( 0f, Window.ClientBounds.Height ), new Vector2( Window.ClientBounds.Width, 0f ), Color.White );
			*/
			_spriteBatch.End();

			base.Draw( gameTime );
		}

		public void KeyPressed( Keys key )
		{
			if ( key == Keys.OemComma )
				Debug = !Debug;
		}
		public void KeyReleased( Keys key ) {}
	}
}
