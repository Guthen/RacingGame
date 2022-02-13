using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Entities.Scenes;
using RacingGame.Utils;
using System;

namespace RacingGame.Entities
{
	public class RaceCarEntity : GameEntity
	{
		public float ForwardSpeed = 200f;
		public float BackwardSpeed = 100f;

		public float TurnSpeed = 5f;
		public float WheelTurnSpeed = 3f;
		public float AccelerationSpeed = 3f;
		public float DeccelerationSpeed = 1.5f;
		public float BrakeSpeed = 1f;

		public int CheckpointId = 0;
		public int Lap = 1;

		protected float _currentThrottle = 0f;
		protected float _currentTurnAxis = 0f;

		public RaceCarEntity()
		{
			SetTexture( Assets.GetAsset<Texture2D>( "Images/race_cars" ), new Rectangle( 0, 0, 16, 16 ), true );
			SetSkin( 4 );
		}

		public void Move( float dt, float throttle, float turn_axis, bool is_braking = false )
		{
			//  update throttle (-1 to 1)
			if ( is_braking )
				_currentThrottle = Mathz.Approach( dt * BrakeSpeed, _currentThrottle, 0f );
			else
				_currentThrottle = Mathz.Lerp( dt * ( throttle > _currentThrottle ? AccelerationSpeed : DeccelerationSpeed ), _currentThrottle, throttle );

			//  turn left or right
			_currentTurnAxis = Mathz.Lerp( dt * WheelTurnSpeed, _currentTurnAxis, turn_axis );
			Angle += TurnSpeed * _currentTurnAxis * _currentThrottle * dt;

			//  move forward or backward
			float speed = _currentThrottle > 0f ? ForwardSpeed : BackwardSpeed;
			Position.X += MathF.Cos( Angle ) * speed * _currentThrottle * dt ;
			Position.Y += MathF.Sin( Angle ) * speed * _currentThrottle * dt;
		}

		public override void Update( float dt )
		{
			int next_checkpoint_id = ( CheckpointId + 1 ) % GameScene.Map.Level.Checkpoints.Length;
			Rectangle checkpoint = GameScene.Map.Level.GetCheckpoint( next_checkpoint_id );
			/*if ( this == GameScene.Player )
				Console.WriteLine( "Need Checkpoint: " + next_checkpoint_id + " " + checkpoint.Center.Distance( position ) );*/

			if ( checkpoint.Intersects( Bounds ) )
			{
				CheckpointId = next_checkpoint_id;
				if ( CheckpointId == 0 )
				{
					Lap++;
					Console.WriteLine( "Lap: " + Lap );

					if ( Lap > GameScene.Map.Level.Laps )
						Console.WriteLine( "Finish Race" );
				}
				Console.WriteLine( "Pass Checkpoint: " + next_checkpoint_id );
			}
		}
	}
}
