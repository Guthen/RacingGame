using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Core;
using RacingGame.Utils;
using RacingGame.Scenes;
using System;

namespace RacingGame.Gameplay
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

		public BoundingPolygon Collider;
		
		public bool IsStuck = false;

		protected float currentThrottle = 0f;
		protected float currentTurnAxis = 0f;
		protected Rectangle nextCheckpoint;

		private static readonly Color colliderColor = new Color( .4f, 1f, .6f, .4f );

		public RaceCarEntity()
		{
			SetTexture( Assets.GetAsset<Texture2D>( "Images/race_cars" ), new Rectangle( 0, 0, 16, 16 ), true );
			SetSkin( 4 );

			Collider = BoundingPolygon.FromRectangle( new Rectangle( 1, 5, 14, 6 ) );

			debugProperties.Add( "CheckpointId" );
			debugProperties.Add( "Lap" );
			debugProperties.Add( "IsStuck" );
		}

		public void Move( float dt, float throttle, float turn_axis, bool is_braking = false )
		{
			#region UpdateThrottle
			//  update throttle (-1 to 1)
			if ( is_braking )
				currentThrottle = MathUtils.Approach( dt * BrakeSpeed, currentThrottle, 0f );
			else
				currentThrottle = MathUtils.Lerp( dt * ( throttle > currentThrottle ? AccelerationSpeed : DeccelerationSpeed ), currentThrottle, throttle );
			#endregion

			#region TurnLeftOrRight
			currentTurnAxis = MathUtils.Lerp( dt * WheelTurnSpeed, currentTurnAxis, turn_axis );

			float new_angle = ( Angle + TurnSpeed * currentTurnAxis * currentThrottle * dt ).Modulo( MathF.PI * 2 );
			Collider.Angle = new_angle;
			#endregion

			#region MoveForwardOrBackward
			//  get our speed
			float speed = currentThrottle > 0f ? ForwardSpeed : BackwardSpeed;

			//  compute new position
			Vector2 new_pos = Position;
			new_pos.X += MathF.Cos( Angle ) * speed * currentThrottle * dt;
			new_pos.Y += MathF.Sin( Angle ) * speed * currentThrottle * dt;
			Collider.Position = new_pos;
			#endregion

			//  check for collisions
			if ( MapEntity.Main.IsColliding( Collider ) )
			{
				currentThrottle = MathUtils.Approach( dt * 5f, currentThrottle, 0f );
				IsStuck = true;
			}
			//  apply position & angle
			else
			{
				Position = new_pos;
				Angle = new_angle;
				IsStuck = false;
			}
		}

		public override void Update( float dt )
		{
			//  update collider position & angle
			Collider.UpdateTo( Position, Angle );

			#region PassCheckpoint
			int next_checkpoint_id = ( CheckpointId + 1 ) % GameScene.Map.Level.Checkpoints.Length;
			nextCheckpoint = GameScene.Map.Level.GetCheckpoint( next_checkpoint_id );
			/*if ( this == GameScene.Player )
				Console.WriteLine( "Need Checkpoint: " + next_checkpoint_id + " " + checkpoint.Center.Distance( position ) );*/

			if ( nextCheckpoint.Intersects( Bounds ) )
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
			#endregion
		}

		public override void Draw( SpriteBatch spriteBatch )
		{
			base.Draw( spriteBatch );

			#region Debug
			if ( Game.DebugLevel == DebugLevel.Colliders )
				spriteBatch.DrawPolygon( Collider, colliderColor );
			#endregion
		}
	}
}
