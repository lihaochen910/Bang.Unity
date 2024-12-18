using System.Collections.Generic;
using UnityEngine;


namespace Bang.Unity.Utilities {
    
	public static class ColorUtils
    {

        ///<summary>The color with alpha</summary>
        public static Color WithAlpha(this Color color, float alpha) {
            color.a = alpha;
            return color;
        }

        ///<summary>A greyscale color</summary>
        public static Color Grey(float value) {
            return new Color(value, value, value, 1);
        }

        ///<summary>Convert Color to Hex.</summary>
        private static Dictionary<Color32, string> colorHexCache = new Dictionary<Color32, string>();
        public static string ColorToHex(Color32 color) {
#if UNITY_EDITOR
            {
                if ( !UnityEditor.EditorGUIUtility.isProSkin ) {
                    if ( color == Color.white ) {
                        return "#000000";
                    }
                }
            }
#endif
            
            if ( colorHexCache.TryGetValue(color, out var result) ) {
                return result;
            }
            result = ( "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") ).ToUpper();
            return colorHexCache[color] = result;
        }

        ///<summary>Convert Hex to Color.</summary>
        private static Dictionary<string, Color> hexColorCache = new Dictionary<string, Color>(System.StringComparer.OrdinalIgnoreCase);
        public static Color HexToColor(string hex) {
            if ( hexColorCache.TryGetValue(hex, out var result) ) {
                return result;
            }

            var startOffset = 0;
            if ( hex.StartsWith( '#' ) ) {
                startOffset++;
            }
            if ( hex.Length > 7 ) {
                throw new System.Exception("Invalid length for hex color provided");
            }
            var r = byte.Parse(hex.Substring(0 + startOffset, 2), System.Globalization.NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2 + startOffset, 2), System.Globalization.NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4 + startOffset, 2), System.Globalization.NumberStyles.HexNumber);
            result = new Color32(r, g, b, 255);
            return hexColorCache[hex] = result;
        }

    }

}