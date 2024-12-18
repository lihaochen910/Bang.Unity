using Bang.Contexts;
using Bang.Systems;


namespace Bang.Unity.Graphics {
	
	public interface IImGuiSystem : IRenderSystem {

		/// <summary>
		/// Called before rendering starts.
		/// </summary>
		public abstract void DrawImGui( Context context );

	}

}
