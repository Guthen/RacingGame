using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RacingGame.Core;
using RacingGame.Scenes;
using RacingGame.Utils;
using System;

/*
 * COOL BUT NOT IDEAS TO MAKE RIGHT NOW:
 * - drift
 * - parts-based car : add a damage system; when a car collide with something (e.g: wall or other cars) it should be damaged accordingly
 *	 to impact position and car's speed, being able to rip off the engine or a wheel for example (or more commonly the bodywork)
 * 
 * TODO RIGHT NOW:
 * 
 * DONE:
 * - game loop: win
 * - implement walls collisions
 * - ai system
 * - keyboard pressed/released
 * - map system: 1 level
 * - game loop: laps
 * - Separating Axis Theorem collisions
 */

namespace RacingGame
{
	public enum DebugLevel
	{
		None,
		Colliders,
		Checkpoints,
		Variables,
	}

	public class Game : Microsoft.Xna.Framework.Game, IInputReceiver
	{
		private SpriteBatch spriteBatch;

		public Vector2 RenderSize = new Vector2( 320f, 180f );

		public static BaseScene Scene { get; protected set; }
		public static Game Instance { get; protected set; }
		public static Camera Camera { get; protected set; }
		public static SpriteFont Font { get; protected set; }
		public static SpriteFont BigFont { get; protected set; }
		public static Random Random { get; protected set; }

		public static DebugLevel DebugLevel;
		public static float CurrentTime { get; protected set; }
		public static float TimeFactor = 1f;

		public Game()
		{
			Random = new Random( DateTime.Now.Millisecond );

			new GraphicsDeviceManager( this );

			Content.RootDirectory = "Assets";
			Assets.Content = Content;
			Assets.GraphicsDevice = GraphicsDevice;

			IsMouseVisible = true;
			Window.AllowUserResizing = true;

			InputManager.AddReceiver( this );
		}

		public static void SetScene<T>() where T : BaseScene, new()
		{
			if ( !( Scene == null ) )
				Scene.QueueFree();
			EntityManager.Clear();

			Scene = new T();
		}

		protected override void LoadContent()
		{
			Console.WriteLine( "Game: Loading Content" );
			spriteBatch = new SpriteBatch( GraphicsDevice );

			Font = Content.Load<SpriteFont>( "Fonts/default" );
			BigFont = Content.Load<SpriteFont>( "Fonts/big" );
			Assets.CacheDirectory<Texture2D>( "Images" );
		}

		protected override void Initialize()
		{
			base.Initialize();

			Console.WriteLine( "Game: Initialize" );
			InputManager.Initialize();
			Camera = new Camera( RenderSize, Window, .75f );

			SetScene<GameScene>();
		}

		protected override void Update( GameTime gameTime )
		{
			float dt = (float) gameTime.ElapsedGameTime.TotalSeconds * TimeFactor;
			CurrentTime += dt;

			//  update
			InputManager.Update();

			EntityManager.Update( dt );
			EntityManager.UpdateDeletionQueue();

			base.Update( gameTime );
		}

		protected override void Draw( GameTime gameTime )
		{
			GraphicsDevice.Clear( Color.Black );

			//  game entities
			spriteBatch.Begin( blendState: BlendState.NonPremultiplied, transformMatrix: Camera.Transform, samplerState: SamplerState.PointWrap );
			EntityManager.Draw( spriteBatch );
			spriteBatch.End();

			//  hud
			spriteBatch.Begin( /*transformMatrix: Camera.ViewportMatrix*/ );
			if ( !( DebugLevel == DebugLevel.None ) )
			{
				spriteBatch.DrawString( Font, string.Format( "{0} ents ({1} U; {2} D; {3} HUD)", EntityManager.Entities.Count, EntityManager.UpdateEntities.Count, EntityManager.DrawableEntities.Count, EntityManager.DrawableHUDs.Count ), Vector2.One * 6, Color.White );
				spriteBatch.DrawString( Font, InputManager.InputReceivers.Count + " input receivers", new Vector2( 6, Font.MeasureString( "a" ).Y + 6 ), Color.White );
				spriteBatch.DrawString( Font, "Debug: " + Enum.GetName( typeof( DebugLevel ), DebugLevel ), new Vector2( 6, Font.MeasureString( "a" ).Y * 2 + 6 ), Color.White );
			}
			EntityManager.DrawHUD( spriteBatch );
			spriteBatch.End();

			base.Draw( gameTime );
		}

		public void KeyPressed( Keys key )
		{
			switch ( key )
			{
				//  debug mode
				case Keys.OemComma:
					Array values = Enum.GetValues( typeof( DebugLevel ) );

					for ( int i = 0; i < values.Length; i++ )
					{
						DebugLevel level = (DebugLevel) values.GetValue( i );
						if ( level == DebugLevel )
						{
							DebugLevel = (DebugLevel) values.GetValue( ( i + 1 ) % values.Length );
							break;
						}	
					}
					break;
				//  slow-mo time 
				case Keys.CapsLock:
					TimeFactor = TimeFactor == 1f ? .1f : 1f;
					break;
			}
		}
		public void KeyReleased( Keys key ) {}
	}
}
