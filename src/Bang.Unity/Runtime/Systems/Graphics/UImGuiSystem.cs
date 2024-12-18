using System;
using Bang.Contexts;
using Bang.Systems;
using ImGuiNET;
using UImGui;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Bang.Unity.Graphics {

	[Filter]
	public class UImGuiSystem : IStartupSystem, IExitSystem {

		private GameObject _uimguiGameObject;
		private Action< UImGui.UImGui > _actionOnUImGuiLayout;

		public void Start( Context context ) {
			var uimguiObj = Object.Instantiate( Resources.Load< GameObject >( "UImGui/UImGuiCamera" ) );
			uimguiObj.name = "UImGui";
			_uimguiGameObject = uimguiObj;

			OnUImGuiInitialize();

			var world = context.World as UnityWorld;
			_actionOnUImGuiLayout = imgui => OnUImGuiLayout( world, imgui );
			UImGuiUtility.Layout += _actionOnUImGuiLayout;
		}

		public void Exit( Context context ) {
			UImGuiUtility.Layout -= _actionOnUImGuiLayout;
			
			Object.Destroy( _uimguiGameObject );
		}

		private void OnUImGuiInitialize() {
			
			var io = ImGui.GetIO();
			var style = ImGui.GetStyle();
			var defaultScale = 2f;

			if (Application.platform == RuntimePlatform.Android ||
				Application.platform == RuntimePlatform.IPhonePlayer)
			{
				defaultScale = 2f * 2.5f;
			}

			io.DisplaySize = new Vector2( defaultScale, defaultScale );
			// style.FrameRounding = 0.0f;
			// style.FrameBorderSize = 0.5f;
			style.ScaleAllSizes(defaultScale);
            
			var font = io.Fonts.Fonts[0];
			font.Scale = defaultScale;

			InitTheme();
		}

		private void OnUImGuiLayout( UnityWorld world, UImGui.UImGui _ ) {
			world.DrawImGui();
		}
		
		private static Color HexToColor( string hex ) {
			if ( hex.StartsWith( "#" ) ) {
				hex = hex.Substring( 1, hex.Length - 1 );
			}
		
			int rgbInt = Convert.ToInt32( hex, 16 );
			byte r = ( byte )( ( rgbInt >> 16 ) & 255 );
			byte g = ( byte )( ( rgbInt >> 8 ) & 255 );
			byte b = ( byte )( rgbInt & 255 );
			return new Color32( r, g, b, 255 );
		}
		
		private void InitTheme() {
			ImGui.StyleColorsDark();
            var dark = ImGui.GetStyle();

			var Bg = HexToColor( "#282a36" );
			var BgFaded = HexToColor( "#44475a" );
			var Foreground = HexToColor( "#9260ab" );
			var HighAccent = HexToColor( "#ff79c6" );
			var Accent = HexToColor( "#bd93f9" );
			var RedFaded = HexToColor( "#5A444BFF" );
			var Faded = HexToColor( "#6272a4" );
			var Red = HexToColor( "#ff5545" );
			var Green = HexToColor( "#42ff22" );
			var Warning = HexToColor( "#eb8e42" );
			var White = HexToColor( "#f8f8f2" );
			var GenericAsset = new Vector4(1f, 0.4f, 0.6f, 1);
			var Yellow = HexToColor( "#f1fa8c" );
            
            dark.FrameRounding = 3;
            dark.PopupRounding = 3;
            dark.WindowRounding = 6;
            dark.Colors[(int)ImGuiCol.Text] = White;

            dark.Colors[(int)ImGuiCol.PopupBg] = Bg;
            dark.Colors[(int)ImGuiCol.WindowBg] = Bg;
            dark.Colors[(int)ImGuiCol.TitleBg] = BgFaded;
            dark.Colors[(int)ImGuiCol.TitleBgActive] = Faded;
            dark.Colors[(int)ImGuiCol.TextSelectedBg] = Accent;
            dark.Colors[(int)ImGuiCol.ChildBg] = Bg;
            dark.Colors[(int)ImGuiCol.PopupBg] = Bg;
            dark.Colors[(int)ImGuiCol.Header] = Faded;
            dark.Colors[(int)ImGuiCol.HeaderActive] = Accent;
            dark.Colors[(int)ImGuiCol.HeaderHovered] = Accent;

            dark.Colors[(int)ImGuiCol.Tab] = BgFaded;
            dark.Colors[(int)ImGuiCol.TabHovered] = HighAccent;
            // dark.Colors[(int)ImGuiCol.TabDimmed] = BgFaded;
            dark.Colors[(int)ImGuiCol.TableHeaderBg] = BgFaded;
            // dark.Colors[(int)ImGuiCol.TabDimmedSelected] = HighAccent;
            // dark.Colors[(int)ImGuiCol.TabDimmedSelectedOverline] = Accent;
            // dark.Colors[(int)ImGuiCol.TabSelected] = Accent;
            // dark.Colors[(int)ImGuiCol.TabSelectedOverline] = Accent;
			
            dark.Colors[(int)ImGuiCol.DockingEmptyBg] = BgFaded;
            dark.Colors[(int)ImGuiCol.DockingPreview] = Faded;
            dark.Colors[(int)ImGuiCol.Button] = Foreground;
            dark.Colors[(int)ImGuiCol.ButtonActive] = HighAccent;
            dark.Colors[(int)ImGuiCol.ButtonHovered] = Accent;
            dark.Colors[(int)ImGuiCol.FrameBg] = BgFaded;
            dark.Colors[(int)ImGuiCol.FrameBgActive] = Bg;
            dark.Colors[(int)ImGuiCol.FrameBgHovered] = Bg;
            dark.Colors[(int)ImGuiCol.SeparatorActive] = Accent;
            dark.Colors[(int)ImGuiCol.ButtonActive] = HighAccent;
		}

	}

}
