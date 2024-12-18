using Bang.Contexts;
using Bang.Systems;


namespace Bang.Unity.Graphics {
	
	public interface IDrawSystem : IRenderSystem {

		/// <summary>
		/// Called before rendering starts.
		/// </summary>
		public abstract void Draw( Context context );

	}

}
