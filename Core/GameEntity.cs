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

		public Vector2 Forward => Angle.Direction();
		public Vector2 Right => Forward.Rotate90();

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

		private Dictionary<FieldInfo, DebugFieldAttribute> debugAttributes;

		public GameEntity()
		{
			//  cache debug field attributes
			debugAttributes = new Dictionary<FieldInfo, DebugFieldAttribute>();
			foreach ( FieldInfo field in GetType().GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) )
				foreach ( Attribute attribute in field.GetCustomAttributes() )
				{
					if ( !( attribute is DebugFieldAttribute ) ) continue;
					debugAttributes.Add( field, (DebugFieldAttribute) attribute );
				}
		}

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

		public virtual void Update( float dt ) { }

		public virtual void Draw( SpriteBatch spriteBatch )
		{
			spriteBatch.Draw( texture, Position, quad, Color, Angle, Origin, Scale, Effects, 0 );

			if ( Game.DebugLevel == DebugLevel.None ) return;
			DebugDraw( spriteBatch );
		}

		public virtual void DebugDraw( SpriteBatch spriteBatch )
		{
			if ( !( Game.DebugLevel == DebugLevel.Variables ) ) return;

			//  draw all debugged fields
			float scale = .5f;
			Vector2 offset = new Vector2( 0f, 20f );

			foreach ( FieldInfo field in debugAttributes.Keys )
			{
				DebugFieldAttribute attribute = debugAttributes[field]; 
				string text = field.Name + ": " + attribute.FormatValue( field.GetValue( this ) );

				Vector2 text_size = Game.Font.MeasureString( text ) * scale;
				spriteBatch.DrawString( Game.Font, text, Position - offset - new Vector2( text_size.X / 2, 0f ), Color.White, 0f, Vector2.Zero, Vector2.One * scale, SpriteEffects.None, 0f );
				offset.Y += text_size.Y - 2;
			}
		}
	}
}
