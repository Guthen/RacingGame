using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RacingGame.Core
{
	public class GameEntity : Entity, IUpdate, IDrawable
	{
		public Vector2 Position;
		public Vector2 Scale = Vector2.One;
		public float Angle;

		public Rectangle Bounds
		{
			get
			{
				return new Rectangle( (int) Position.X, (int) Position.Y, (int) ( TextureSize.X * Scale.X ), (int) ( TextureSize.Y * Scale.Y ) );
			}
		}

		public Vector2 Origin;
		public Color Color = Color.White;
		public SpriteEffects Effects = SpriteEffects.None;

		public int Skin;
		public Rectangle[] Skins;
		protected Rectangle quad;
		protected Texture2D texture;
		public Vector2 TextureSize;

		protected List<string> debugProperties = new List<string>();

		protected void SetTexture( Texture2D _texture, Rectangle? _quad = null, bool splitToSkins = false )
		{
			texture = _texture;

			//  quad
			if ( _quad.HasValue )
				quad = _quad.Value;
			else
				quad = texture.Bounds;

			TextureSize.X = quad.Width;
			TextureSize.Y = quad.Height;

			Origin = TextureSize / 2;

			//  split to skins
			if ( splitToSkins )
				Skins = Assets.SplitTextureInQuads( texture, quad.Size );
		}

		public void SetSkin( int _skin = 0 )
		{
			if ( _skin < 0 || _skin >= Skins.Length ) return;

			Skin = _skin;
			quad = Skins[Skin];
		}

		public virtual void Update( float dt ) {}

		public virtual void Draw( SpriteBatch spriteBatch )
		{
			spriteBatch.Draw( texture, Position, quad, Color, Angle, Origin, Scale, Effects, 0 );

			#region Debug
			if ( Game.DebugLevel == DebugLevel.None ) return;

			float scale = .5f;
			Vector2 offset = new Vector2( 0f, 20f );
			foreach ( string name in debugProperties )
			{
				var value = GetType().GetField( name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( this );

				string text = name + ": " + value;
				Vector2 text_size = Game.Font.MeasureString( text ) * scale;
				spriteBatch.DrawString( Game.Font, text, Position - offset - new Vector2( text_size.X / 2, 0f ), Color.White, 0f, Vector2.Zero, Vector2.One * scale, SpriteEffects.None, 0f );
				offset.Y += text_size.Y - 2;
			}
			#endregion
		}
	}
}
