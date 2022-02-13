using Microsoft.Xna.Framework.Input;
using RacingGame.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace RacingGame.Entities.Scenes
{
	public class Scene : Entity, IInputReceiver
	{
		public Scene()
		{
			Clearable = false;
			Inputz.AddReceiver( this );

			Initialize();
		}

		public virtual void Initialize() {}

		public virtual void KeyPressed( Keys key ) {}
		public virtual void KeyReleased( Keys key ) {}

		public virtual void MousePressed() {}
		public virtual void MouseReleased() {}

		public override void OnFree() => Inputz.RemoveReceiver( this );
	}
}
