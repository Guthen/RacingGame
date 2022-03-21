using System;
using Microsoft.Xna.Framework;
using RacingGame.Utils;

namespace RacingGame.Gameplay
{
	public class AIRaceCarEntity : RaceCarEntity
	{
		private float throttle;
		private float turnAxis;

		public AIRaceCarEntity()
		{
			debugProperties.Add( "turnAxis" );
		}

		public override void Update( float dt )
		{
			throttle = MathUtils.Approach( dt * 1.5f, throttle, 1f );
			if ( IsStuck )
				throttle = -1f;

			//  turn axis
			Vector2 dir = ( nextCheckpoint.Center.ToVector2() - Position );
			dir.Normalize();

			float dir_angle = Angle.Direction().Angle() - dir.Angle();
			turnAxis = MathUtils.Approach( dt * 4, turnAxis, dir_angle < 0f ? 1f : -1f );

			//  move
			Move( dt, throttle, turnAxis, false );

			base.Update( dt );
		}
	}
}
