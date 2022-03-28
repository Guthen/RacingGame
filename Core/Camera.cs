using Microsoft.Xna.Framework;
using System;

namespace RacingGame.Core
{
	public class Camera : Entity, IUpdate
	{
		public Vector2 Position;
		public Vector2 Offset;
		public float Zoom = 1f;

		public Matrix Transform;
		public Matrix InvertedTransform;
		public Matrix ScaleMatrix;
		public Matrix InvertedScaleMatrix;
		public Matrix ViewportMatrix;

		public Vector2 WindowSize { get => windowSize; }
		public Vector2 RenderSize { get => renderSize; }

		private Vector2 screenScale;
		private Vector2 renderSize;
		private GameWindow window;
		private Vector2 windowSize;

		private GameEntity _target;

		public Camera( Vector2 renderSize, GameWindow window, float zoom = 1f )
		{
			IsStatic = true;
			this.renderSize = renderSize;

			this.window = window;
			this.window.ClientSizeChanged += UpdateScreenScale;

			Zoom = zoom;

			UpdateScreenScale();
		}

		public Vector2 TranslatePosition( Vector2 pos ) => Vector2.Transform( pos, InvertedTransform );
		public Vector2 TranslateScreenPosition( Vector2 pos ) => Vector2.Transform( pos, InvertedScaleMatrix );
		public void SetTarget( GameEntity target ) => _target = target;
		public void SetZoom( float zoom )
		{
			Zoom = zoom;
			UpdateScaleTransform();
		}

		public void CenterTo( Vector2 pos )
		{
			Vector2 last_position = Position;

			Position = pos - TranslateScreenPosition( windowSize / 2 ) + Offset;
			
			if ( !( last_position == Position ) )
				UpdateTransform();
		}

		private void UpdateScreenScale( object sender = null, EventArgs e = null )
		{
			float scale = MathF.Max( window.ClientBounds.Width / renderSize.X, window.ClientBounds.Height / renderSize.Y );
			screenScale.X = scale;
			screenScale.Y = scale;

			windowSize.X = window.ClientBounds.Width;
			windowSize.Y = window.ClientBounds.Height;

			UpdateScaleTransform();
			UpdateTransform();

			Console.WriteLine( "Camera: Update Screen Scale (x{0})", scale );
		}

		private void UpdateTransform()
		{
			Transform = Matrix.CreateTranslation( -Position.X, -Position.Y, 0f ) * ScaleMatrix;
			InvertedTransform = Matrix.Invert( Transform );
		}

		private void UpdateScaleTransform()
		{
			ViewportMatrix = Matrix.CreateScale( screenScale.X, screenScale.Y, 1f );
			ScaleMatrix = Matrix.CreateScale( screenScale.X * Zoom, screenScale.Y * Zoom, 1f );
			InvertedScaleMatrix = Matrix.Invert( ScaleMatrix );
		}

		public void Update( float dt )
		{
			if ( !( _target == null ) )
				CenterTo( _target.Position );
		}
	}
}
