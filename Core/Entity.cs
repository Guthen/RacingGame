using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace RacingGame.Core
{
	public interface IUpdate
	{
		public abstract void Update( float dt );
	}

	public interface IDrawable
	{
		public abstract void Draw( SpriteBatch spriteBatch );
	}

	public interface IDrawableHUD
	{
		public abstract void DrawHUD( SpriteBatch spriteBatch );
	}

	public class Entity
	{
		public bool IsStatic = false;
		public bool IsDeletionQueued = false;

		public Entity()
		{
			if ( this is IUpdate )
				EntityManager.UpdateEntities.Add( (IUpdate) this );
			if ( this is IDrawable )
				EntityManager.DrawableEntities.Add( (IDrawable) this );
			if ( this is IDrawableHUD )
				EntityManager.DrawableHUDs.Add( (IDrawableHUD) this );

			EntityManager.Add( this );
		}

		public void QueueFree() => EntityManager.QueueFree( this );
		public virtual void OnFree() {}

		~Entity()
		{
			Console.WriteLine( "Entity destroyed" );
		}
	}

	public static class EntityManager
	{
		public static List<Entity> Entities = new List<Entity>();

		public static List<Entity> entitiesDeletionQueue = new List<Entity>();
		public static bool queueUpdateNeeded = false;

		public static List<IUpdate> UpdateEntities = new List<IUpdate>();
		public static List<IDrawable> DrawableEntities = new List<IDrawable>();
		public static List<IDrawableHUD> DrawableHUDs = new List<IDrawableHUD>();

		public static void Add( Entity entity ) => Entities.Add( entity );
		public static void Call( string method, params object[] args )
		{
			for ( int i = 0; i < Entities.Count; i++ )
			{
				Entity entity = Entities[i];
				entity.GetType().GetMethod( method ).Invoke( entity, args );
			}
		}
		public static void Update( float dt )
		{
			foreach ( IUpdate entity in UpdateEntities )
				entity.Update( dt );
		}
		public static void Draw( SpriteBatch spriteBatch )
		{
			foreach ( IDrawable entity in DrawableEntities ) 
				entity.Draw( spriteBatch );
		}
		public static void DrawHUD( SpriteBatch spriteBatch )
		{
			foreach ( IDrawableHUD entity in DrawableHUDs ) 
				entity.DrawHUD( spriteBatch );
		}
		public static void Clear()
		{
			foreach ( Entity entity in Entities )
				if ( !entity.IsStatic )
					QueueFree( entity );
		}

		public static void QueueFree( Entity entity )
		{
			if ( entity.IsDeletionQueued ) return;

			entity.IsDeletionQueued = true;
			entitiesDeletionQueue.Add( entity );
			queueUpdateNeeded = true;
		}

		public static void UpdateDeletionQueue()
		{
			if ( !queueUpdateNeeded ) return;

			foreach ( Entity entity in entitiesDeletionQueue )
			{
				Console.WriteLine( "Deleting " + entity );
				entity.OnFree();

				//  remove from interfaces list
				if ( entity is IUpdate )
					UpdateEntities.Remove( (IUpdate) entity );
				if ( entity is IDrawable )
					DrawableEntities.Remove( (IDrawable) entity );
				if ( entity is IDrawableHUD )
					DrawableHUDs.Remove( (IDrawableHUD) entity );

				//  remove from entities
				Entities.Remove( entity );
			}

			entitiesDeletionQueue.Clear();
			queueUpdateNeeded = false;
		}
	}
}
