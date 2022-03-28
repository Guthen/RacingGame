using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RacingGame.Utils;
using RacingGame.Scenes;
using System;

namespace RacingGame.Gameplay
{
	public class PlayerRaceCarEntity : RaceCarEntity
	{
		public float LapTime = 0f;
		public float BestLapTime = 0f;

		public override void ProcessInput( float dt )
		{
			KeyboardState keyboard = Keyboard.GetState();

			//  movement
			#region DirectionInput
			if ( keyboard.IsKeyDown( Keys.Z ) )
				InputDirection.Y += 1f;
			if ( keyboard.IsKeyDown( Keys.S ) )
				InputDirection.Y -= 1f;
			if ( keyboard.IsKeyDown( Keys.Q ) )
				InputDirection.X += 1f;
			if ( keyboard.IsKeyDown( Keys.D ) )
				InputDirection.X -= 1f;

			if ( !( InputDirection == Vector2.Zero ) )
				InputDirection = Right * InputDirection.X + Forward * InputDirection.Y; //  translate our relative direction to world
			#endregion

			//  braking
			IsBraking = keyboard.IsKeyDown( Keys.Space );
		}

		public override void OnLapDone()
		{
			if ( BestLapTime == 0f )
				BestLapTime = LapTime;
			else
				BestLapTime = Math.Min( BestLapTime, LapTime );

			LapTime = 0f;
		}

		public override void Update( float dt )
		{
			base.Update( dt );

			if ( GameScene.Instance.IsStarted )
				LapTime += dt;

			Game.Camera.Offset = Vector2.Lerp( Game.Camera.Offset, Angle.Direction() * 35f * currentThrottle, dt * 10f );
		}
	}
}
