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
		public string Name = "RaceCar";

		public float ForwardSpeed = 200f;
		public float BackwardSpeed = 100f;

		public float TurnSpeed = 5f;
		public float WheelTurnSpeed = 3f;
		public float AccelerationSpeed = 3f;
		public float DeccelerationSpeed = 1.5f;
		public float BrakeSpeed = 1f;

		public Vector2 InputDirection;
		public bool IsBraking = false;

		[DebugField]
		public int CheckpointId = 0;
		public Rectangle NextCheckpoint { get; protected set; }

		public bool IsFinished => Lap > GameScene.Map.Level.Laps;
		public float FinishTime = 0f;

		public float LastTimeCheckpointPassed = -1f;
		[DebugField]
		public int Lap = 1;

		public BoundingPolygon Collider;
		
		[DebugField]
		public bool IsStuck = false;
		public BoundingPolygon LastHitCollider;

		protected float currentThrottle = 0f;
		protected float currentTurnAxis = 0f;

		private static readonly Color colliderColor = new Color( .4f, 1f, .6f, .4f );

		public RaceCarEntity()
		{
			SetTexture( Assets.GetAsset<Texture2D>( "Images/race_cars" ), new Rectangle( 0, 0, 16, 16 ), true );
			SetSkin( 4 );

			Collider = BoundingPolygon.FromRectangle( new Rectangle( 1, 5, 14, 6 ) );
		}

		public void Move( float dt, Vector2 input_dir, bool is_braking = false )
		{
			Vector2 dir = Angle.Direction();
			float dot = Vector2.Dot( input_dir, dir );

			#region UpdateAngle
			float turn_axis_target = Vector2.Dot( input_dir.Rotate90(), dir );
			currentTurnAxis = MathUtils.Lerp( dt * WheelTurnSpeed, currentTurnAxis, turn_axis_target );
			//  clamp to target when near enough
			if ( Math.Abs( currentTurnAxis - turn_axis_target ) <= .1f )
				currentTurnAxis = turn_axis_target;

			Collider.Angle = ( Angle + TurnSpeed * currentTurnAxis * currentThrottle * dt ).Modulo( MathF.PI * 2 );
			#endregion

			#region UpdateThrottle
			float throttle = dot;
			if ( is_braking )
				currentThrottle = MathUtils.Approach( dt * BrakeSpeed, currentThrottle, 0f );
			else
				currentThrottle = MathUtils.Lerp( dt * ( throttle > currentThrottle ? AccelerationSpeed : DeccelerationSpeed ), currentThrottle, throttle );
			#endregion

			#region UpdatePosition
			float speed = currentThrottle > 0 ? ForwardSpeed : BackwardSpeed;
			Collider.Position = Position + dir * speed * currentThrottle * dt;
			#endregion

			//  check for collisions
			LastHitCollider = MapEntity.Main.IsColliding( Collider );
			if ( !( LastHitCollider == null ) )
			{
				currentThrottle = MathUtils.Approach( dt * 5f, currentThrottle, 0f );
				IsStuck = true;
			}
			//  apply position & angle
			else
			{
				Position = Collider.Position;
				Angle = Collider.Angle;
				IsStuck = false;
			}
		}

		public override void Update( float dt )
		{
			if ( !GameScene.Instance.IsStarted ) return;

			//  movement
			#region HandleMovement
			InputDirection = new Vector2();
			ProcessInput( dt );
			if ( InputDirection.LengthSquared() > 0f )
				InputDirection.Normalize();

			Move( dt, InputDirection, IsBraking );
			#endregion

			//  update collider position & angle
			Collider.UpdateTo( Position, Angle );

			#region PassCheckpoint
			int next_checkpoint_id = ( CheckpointId + 1 ) % GameScene.Map.Level.Checkpoints.Length;
			NextCheckpoint = GameScene.Map.Level.GetCheckpoint( next_checkpoint_id );

			if ( NextCheckpoint.Intersects( Bounds ) )
			{
				CheckpointId = next_checkpoint_id;
				if ( !IsFinished && CheckpointId == 0 )
				{
					Lap++;
					OnLapDone();

					if ( Lap > GameScene.Map.Level.Laps )
						FinishTime = Game.CurrentTime;
				}

				LastTimeCheckpointPassed = Game.CurrentTime;
			}
			#endregion
		}

		public virtual void OnLapDone() {}

		public virtual void ProcessInput( float dt ) {}

		public override void Draw( SpriteBatch spriteBatch )
		{
			base.Draw( spriteBatch );

			float scale = .35f;
			spriteBatch.DrawString( Game.Font, Name, Position - Vector2.UnitY * 12f - Game.Font.MeasureString( Name ) * scale / 2f, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f );
		}

		public override void DebugDraw( SpriteBatch spriteBatch )
		{
			base.DebugDraw( spriteBatch );

			spriteBatch.DrawLine( Position, Position + InputDirection * 10f, Color.Red );

			if ( Game.DebugLevel == DebugLevel.Colliders )
				spriteBatch.DrawPolygon( Collider, colliderColor );
		}
	}
}
