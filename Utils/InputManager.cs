using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RacingGame.Utils
{
	public enum KeyState
	{
		Up,
		Down,
		Pressed,
		Released,
	}

	public interface IInputReceiver
	{
		public abstract void KeyPressed( Keys key );
		public abstract void KeyReleased( Keys key );
	}

	public static class InputManager
	{
		public static List<IInputReceiver> InputReceivers = new List<IInputReceiver>();
		private static Dictionary<Keys, KeyState> keyStates = new Dictionary<Keys, KeyState>();
		//private static Dictionary<Keys, KeyState> mouseStates = new Dictionary<Keys, KeyState>();

		private static KeyboardState lastKeyboardState;
		//private static MouseState lastMouseState;


		public static void Initialize()
		{
			//  reset keys
			foreach ( Keys key in Enum.GetValues( typeof( Keys ) ) )
			{
				keyStates[key] = KeyState.Up;
			}
		}

		public static void AddReceiver( IInputReceiver inputReceiver ) => InputReceivers.Add( inputReceiver );
		public static void RemoveReceiver( IInputReceiver inputReceiver ) => InputReceivers.Remove( inputReceiver );

		public static KeyState GetKeyState( Keys key )
		{
			if ( keyStates.TryGetValue( key, out KeyState value ) )
				return value;

			return KeyState.Up;
		}
		public static bool IsKeyPressed( Keys key ) => GetKeyState( key ) == KeyState.Pressed;
		public static bool IsKeyReleased( Keys key ) => GetKeyState( key ) == KeyState.Released;
		public static bool IsKeyDown( Keys key )
		{
			KeyState state = GetKeyState( key );
			return state == KeyState.Down || state == KeyState.Pressed;
		}
		public static bool IsKeyUp( Keys key )
		{
			KeyState state = GetKeyState( key );
			return state == KeyState.Up || state == KeyState.Released;
		}

		internal static void Call( string method_name, params object[] args )
		{
			int count = InputReceivers.Count;
			for ( int i = 0; i < count; i++ )
			{
				IInputReceiver inputReceiver = InputReceivers[i];
				inputReceiver.GetType().GetMethod( method_name ).Invoke( inputReceiver, args );
			}
		}

		public static void Update()
		{
			KeyboardState keyboard = Keyboard.GetState();
			//  TODO: implement mouse callbacks
			//MouseState mouse = Mouse.GetState();

			Keys[] keys = new Keys[keyStates.Count];
			 keyStates.Keys.CopyTo( keys, 0 );
			foreach ( Keys key in keys )
			{
				bool last_key_down = lastKeyboardState.IsKeyDown( key );

				//  press or down keys
				if ( keyboard.IsKeyDown( key ) )
					if ( last_key_down )
						keyStates[key] = KeyState.Down;
					else
					{
						keyStates[key] = KeyState.Pressed;
						Call( "KeyPressed", key );
					}
				//  release keys
				else if ( last_key_down )
				{
					keyStates[key] = KeyState.Released;
					Call( "KeyReleased", key );
				}
				//  revert keys to up
				else if ( !( keyStates[key] == KeyState.Up ) )
					keyStates[key] = KeyState.Up;
			}

			lastKeyboardState = keyboard;
		}
	}
}
