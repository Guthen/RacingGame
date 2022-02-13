using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Utils;

namespace RacingGame.Entities
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
		}
	}
}
