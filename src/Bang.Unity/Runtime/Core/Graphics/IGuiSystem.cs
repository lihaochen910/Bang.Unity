using Bang.Contexts;
using Bang.Systems;


namespace Bang.Unity.Graphics {

	public interface IGuiSystem : IRenderSystem {

		/// <summary>
		/// Called before rendering starts.
		/// </summary>
		public abstract void DrawGui( Context context );

	}
	
}
