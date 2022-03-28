using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Core;
using RacingGame.Utils;

namespace RacingGame.Gameplay
{
	public class AIRaceCarEntity : RaceCarEntity
	{
		private float throttle;
		[DebugFloatField( Digits = 2 )]
		private float turnAxis;
		private float targetTurnAxis;
		[DebugFloatField( Digits = 2 )]
		private float diffAngle;


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
