using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RacingGame.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace RacingGame.Entities.Scenes
{
	public class GameScene : Scene, IDrawableHUD
	{
		public static PlayerRaceCarEntity Player { get; protected set; } 
		public static MapEntity Map { get; protected set; }

		public override void Initialize()
		{
			//  create entities
			Map = new MapEntity();
			Map.Load( "Assets/Levels/monaco.tmx" );

			//  player
			Player = new PlayerRaceCarEntity
			{
				Position = Map.GridToWorld( Map.Level.GetSpawnPos( 0 ), true ),
				Angle = Map.Level.GetSpawnDir( 0 ).Angle()
			};

			//  ai
			for ( int i = 1; i < Map.Level.SpawnPositions.Length; i++ )
			{
				RaceCarEntity car = new RaceCarEntity
				{
					Position = Map.GridToWorld( Map.Level.GetSpawnPos( i ), true ),
					Angle = Map.Level.GetSpawnDir( i ).Angle()
				};
				car.SetSkin( i % car.Skins.Length );
			}

			Game.Camera.SetTarget( Player );
		}

		public override void KeyPressed( Keys key )
		{
			if ( key == Keys.Escape )
				Game.SetScene<GameScene>();
		}

		public void DrawHUD( SpriteBatch spriteBatch )
		{
			string text = string.Format( "Laps {0}/{1}", Player.Lap, Map.Level.Laps );
			spriteBatch.DrawString( Game.Font, text, new Vector2( Game.Camera.WindowSize.X - Game.Font.MeasureString( text ).X - 10, 10 ), Color.White );
		}
	}
}
