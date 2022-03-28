using Microsoft.Xna.Framework.Input;
using RacingGame.Utils;

namespace RacingGame.Core
{
	public abstract class BaseScene : Entity, IInputReceiver
	{
		public BaseScene()
		{
			IsStatic = true;
			InputManager.AddReceiver( this );

			Initialize();
		}

		public abstract void Initialize();

		public virtual void KeyPressed( Keys key ) {}
		public virtual void KeyReleased( Keys key ) {}

		public virtual void MousePressed() {}
		public virtual void MouseReleased() {}

		public override void OnFree() => InputManager.RemoveReceiver( this );
	}
}
