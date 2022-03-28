using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Utils;
using RacingGame.Scenes;

namespace RacingGame.Gameplay
{
	public class AIRaceCarEntity : RaceCarEntity
	{
		public Vector2 currentDirection;
		private Vector2 lastDirection;

		private bool isMovingAwayFromCollision = false;
		private Vector2 directionToMoveAway;
		private float currentTimeMovingAway;
		private readonly float timeMovingAway = .7f;

		private Vector2 nextCheckpointDir => ( NextCheckpoint.Center.ToVector2() - Position ).GetNormalized();

		private float minGroupSeparationDist = 17f;

		public override void ProcessInput( float dt )
		{
			//  move
			currentDirection = nextCheckpointDir;
			if ( IsStuck )
			{
				directionToMoveAway = lastDirection.Rotate180();
				isMovingAwayFromCollision = true;
				currentTimeMovingAway = timeMovingAway + (float) Game.Random.NextDouble() * -2f;
			}
			if ( isMovingAwayFromCollision )
			{
				currentDirection = directionToMoveAway;

				currentTimeMovingAway += dt;
				if ( currentTimeMovingAway >= timeMovingAway )
					isMovingAwayFromCollision = false;
			}

			//  slowing down after reaching a checkpoint to allow a better rotation to the next checkpoint
			if ( Game.CurrentTime - LastTimeCheckpointPassed <= 1f )
				currentDirection = currentDirection - Forward * .2f;
			
			lastDirection = currentDirection;
			InputDirection = ( currentDirection + GetGroupSeparationDirection() * .25f ).GetNormalized();
		}

		public Vector2 GetGroupSeparationDirection()
		{
			Vector2 dir = new Vector2();
			int count = 0;

			foreach ( RaceCarEntity car in GameScene.SortedCarsInPosition )
			{
				if ( car == this ) continue;

				float dist = Vector2.Distance( Position, car.Position );
				if ( dist >= minGroupSeparationDist ) continue;

				dir += ( Position - car.Position ).GetNormalized();
				count++;
			}

			if ( dir == Vector2.Zero )
				return Vector2.Zero;
			return dir / count;
		}

		public override void DebugDraw( SpriteBatch spriteBatch )
		{
			base.DebugDraw( spriteBatch );

			//Vector2 dir = Angle.Direction();
			//spriteBatch.DrawLine( Position, Position + dir * 5f, Color.White );
			//spriteBatch.DrawLine( Position, Position + new Vector2( -dir.Y, dir.X ) * targetTurnAxis * 10f, Color.Red );
			spriteBatch.DrawLine( Position, Position + currentDirection * 15f, Color.Purple );

			//spriteBatch.DrawLine( Position, Position + nextCheckpointDir * 20f, Color.Blue );
		}
	}
}
