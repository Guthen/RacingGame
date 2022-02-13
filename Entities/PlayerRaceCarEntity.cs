using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RacingGame.Entities
{
	public class PlayerRaceCarEntity : RaceCarEntity
	{
		public PlayerRaceCarEntity() {}

		public override void Update( float dt )
		{
			KeyboardState keyboard = Keyboard.GetState();

			//  move
			bool is_braking = false;
			float throttle = 0f;
			if ( keyboard.IsKeyDown( Keys.Z ) )
				throttle += 1f;
			if ( keyboard.IsKeyDown( Keys.S ) )
				throttle -= 1f;
			if ( keyboard.IsKeyDown( Keys.Space ) )
				is_braking = true;

			//  turn
			float turn_axis = 0f;
			if ( keyboard.IsKeyDown( Keys.Q ) )
				turn_axis -= 1f;
			if ( keyboard.IsKeyDown( Keys.D ) )
				turn_axis += 1f;

			Move( dt, throttle, turn_axis, is_braking );
			Game.Camera.Offset = Vector2.Lerp( Game.Camera.Offset, new Vector2( MathF.Cos( Angle ), MathF.Sin( Angle ) ) * 35f * _currentThrottle, dt * 10f );
		
			base.Update( dt );
		}
	}
}
