using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RacingGame.Utils;
using RacingGame.Core;
using RacingGame.Gameplay;
using System.Collections.Generic;
using System;

namespace RacingGame.Scenes
{
	public class GameScene : BaseScene, IUpdate, IDrawableHUD
	{
		public static GameScene Instance { get; protected set; }
		public static PlayerRaceCarEntity Player { get; protected set; }
		public static MapEntity Map { get; protected set; }

		public static List<RaceCarEntity> SortedCarsInPosition;

		private float currentStartTime;
		public bool IsStarted = false;
		public readonly float StartTime = 3.5f;

		public float RaceTime = 0f;

		private float currentLeaderboardRefreshTime;
		private readonly float LeaderboardRefreshTime = 1f;

		private int blinkState = 0;
		private float currentBlinkTime;
		private readonly float blinkTime = .5f;

		public GameScene() => Instance = this;

		public override void Initialize()
		{
			//  create entities
			Map = new MapEntity();
			Map.Load( "Assets/Levels/monaco.tmx" );

			SortedCarsInPosition = new List<RaceCarEntity>();

			//  ai
			for ( int i = 0; i < Map.Level.SpawnPositions.Length - 1; i++ )
			{
				AIRaceCarEntity car = new AIRaceCarEntity
				{
					Name = string.Format( "Bot{0:00}", i ),
					Position = Map.GridToWorld( Map.Level.GetSpawnPos( i ), true ),
					Angle = Map.Level.GetSpawnDir( i ).Angle()
				};
				car.SetSkin( i % car.Skins.Length );
				SortedCarsInPosition.Add( car );
			}

			//  player
			Player = new PlayerRaceCarEntity
			{
				Name = "Player",
				Position = Map.GridToWorld( Map.Level.GetSpawnPos( Map.Level.SpawnPositions.Length - 1 ), true ),
				Angle = Map.Level.GetSpawnDir( Map.Level.SpawnPositions.Length - 1 ).Angle(),
			};
			SortedCarsInPosition.Add( Player );

			Game.Camera.SetTarget( Player );
		}

		public override void KeyPressed( Keys key )
		{
			if ( key == Keys.Escape )
				Game.SetScene<GameScene>();
		}

		public void Update( float dt )
		{
			//  start timer
			if ( !IsStarted )
			{
				currentStartTime += dt;
				if ( currentStartTime >= StartTime )
					IsStarted = true;
			}
			//  race time
			else
				RaceTime += dt;

			//  blink timer
			currentBlinkTime += dt;
			if ( currentBlinkTime >= blinkTime )
			{
				blinkState = ( blinkState + 1 ) % 2;
				currentBlinkTime = 0f;
			}

			//  sort positions of cars
			currentLeaderboardRefreshTime += dt;
			if ( currentLeaderboardRefreshTime >= LeaderboardRefreshTime )
			{
				SortedCarsInPosition.Sort(
					delegate ( RaceCarEntity a, RaceCarEntity b )
					{
						if ( a.IsFinished && b.IsFinished ) return (int) ( a.FinishTime - b.FinishTime );
						else
						{
							if ( a.IsFinished ) return -1;
							if ( b.IsFinished ) return 1;
						}

						if ( !( a.Lap == b.Lap ) ) return b.Lap - a.Lap;
						if ( !( a.CheckpointId == b.CheckpointId ) ) return b.CheckpointId - a.CheckpointId;
						return (int) (
							Vector2.DistanceSquared( a.NextCheckpoint.Center.ToVector2(), a.Position )
							- Vector2.DistanceSquared( b.NextCheckpoint.Center.ToVector2(), b.Position )
						);
					}
				);
				currentLeaderboardRefreshTime = 0f;
			}
		}

		public void DrawHUD( SpriteBatch spriteBatch )
		{
			string text;
			int player_position = 0;

			#region StartHUD
			if ( !IsStarted )
			{
				int time_left = (int) ( StartTime - currentStartTime );
				if ( time_left == 0 )
					text = "GO!";
				else
					text = time_left.ToString();

				spriteBatch.DrawString( Game.BigFont, text, Game.Camera.WindowSize / 2 - Game.BigFont.MeasureString( text ) / 2, Color.White );
			}
			#endregion

			#region LeaderboardHUD
			float pos_y = 115f;
			drawTextLine( pos_y, "Leaderboard:", Color.White );
			pos_y += 15f;

			for ( int i = 0; i < SortedCarsInPosition.Count; i++ )
			{
				RaceCarEntity car = SortedCarsInPosition[i];

				Color color = car.IsFinished && blinkState == 0 ? Color.LightBlue : Color.White;
				if ( car is PlayerRaceCarEntity )
				{
					color = car.IsFinished && blinkState == 0 ? Color.LightBlue : Color.White;
					player_position = i + 1;
				}

				drawTextLine( pos_y, string.Format( "{0}{1}{2}", car is PlayerRaceCarEntity ? "- " : "", car.IsFinished ? "FINISHED " : "", car.Name ), color );
				pos_y += 15f;
			}
			#endregion

			#region TopRightHUD
			drawTextLine( 10f, string.Format( "Lap {0}/{1}", Player.Lap, Map.Level.Laps ), Color.White );
			drawTextLine( 25f, string.Format( "Position {0}/{1}", player_position, SortedCarsInPosition.Count ), Color.White );
			
			drawTimeLine( 55f, "Race: {0}", TimeSpan.FromSeconds( RaceTime ), Color.White );
			drawTimeLine( 70f, "Current Lap: {0}", TimeSpan.FromSeconds( Player.LapTime ), Color.White );
			drawTimeLine( 85f, "Best Lap: {0}", TimeSpan.FromSeconds( Player.BestLapTime ), Color.White );
			#endregion

			#region FinishHUD
			if ( Player.IsFinished )
			{
				float scale = .5f;

				text = string.Format( "{0} place!", StringUtils.AddOrdinal( player_position ) );
				spriteBatch.DrawString( Game.BigFont, text, Game.Camera.WindowSize / 2 - Game.BigFont.MeasureString( text ) * scale / 2 - Vector2.UnitY * 6f / scale, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f );
			
				scale = .35f;
				text = "Press Escape to start over!";
				spriteBatch.DrawString( Game.BigFont, text, Game.Camera.WindowSize / 2 - Game.BigFont.MeasureString( text ) * scale / 2 + Vector2.UnitY * 12f / scale, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f );
			}
			#endregion

			void drawTextLine( float y, string text, Color color )
				=> spriteBatch.DrawString( Game.Font, text, new Vector2( Game.Camera.WindowSize.X - Game.Font.MeasureString( text ).X - 10, y ), color );
			void drawTimeLine( float y, string format_text, TimeSpan time, Color color )
			{
				string formatted_time = string.Format( "{0:00}:{1:00}:{2:000}", time.Minutes, time.Seconds, time.Milliseconds );
				drawTextLine( y, string.Format( format_text, formatted_time ), color );
			}
		}
	}
}
