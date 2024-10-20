using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression representing a color
    /// </summary>
    public class TokenExpressionConstantColor : TokenExpressionConstant, IEquatable<TokenExpressionConstantColor>
    {
        // IConvertible value is stored as HEX

        /// <summary>
        /// Red component (0-1)
        /// </summary>
        public double R { get; }

        /// <summary>
        /// Red component (0-1)
        /// </summary>
        public double G { get; }

        /// <summary>
        /// Red component (0-1)
        /// </summary>
        public double B { get; }

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(object);

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionConstantColor"/> class using 0-255 values
        /// </summary>
        public TokenExpressionConstantColor(int r, int g, int b) : base("#" + ToHex(r, g, b))
        {
            this.R = r / 255.0;
            this.G = g / 255.0;
            this.B = b / 255.0;
        }

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionConstantColor"/> class using 0.0-1.0 values
        /// </summary>
        public TokenExpressionConstantColor(double r, double g, double b) : base("#" + ToHex(r, g, b))
        {
            if (r < 0 || r > 1.0) throw new ArgumentException(nameof(r), $"Color components must be in the range 0-1 {r}");
            if (g < 0 || g > 1.0) throw new ArgumentException(nameof(g), $"Color components must be in the range 0-1 {g}");
            if (b < 0 || b > 1.0) throw new ArgumentException(nameof(b), $"Color components must be in the range 0-1 {b}");
            this.R = r;
            this.G = g;
            this.B = b;
        }

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionConstantColor"/> class using a hex string value
        /// </summary>
        public TokenExpressionConstantColor(string hexcolor) : base(hexcolor)
        {
            if (string.IsNullOrEmpty(hexcolor))
                throw new ArgumentNullException(nameof(hexcolor));

            hexcolor = hexcolor.Replace("#", string.Empty).Trim();

            if (hexcolor.Length != 6)
                throw new ArgumentException("a hex color should contains 6 characters", nameof(hexcolor));

            int red = int.Parse(hexcolor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            int green = int.Parse(hexcolor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            int blue = int.Parse(hexcolor.Substring(4, 2), NumberStyles.AllowHexSpecifier);

            R = red / 255.0;
            G = green / 255.0;
            B = blue / 255.0;
        }

        /// <summary>
        /// Accepts the visitor
        /// </summary>
        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return $"RGB({R},{G},{B})";
        }

        /// <summary>
        /// Compare to another TokenExpressionConstantColor
        /// </summary>
        public bool Equals(TokenExpressionConstantColor? other)
        {
            if (other is null) return false;
            return this.R.Equals(other.R) && this.G.Equals(other.G) && this.B.Equals(other.B);
        }

        /// <summary>
        /// Compare to another TokenExpression
        /// </summary>
        public override bool Equals(TokenExpression? other)
        {
            if (other is TokenExpressionConstantColor color)
            {
                return (this.R, this.G, this.B) == (color.R, color.G, color.B);
            }
            return false;
        }

        /// <summary>
        /// Compare to another TokenExpression
        /// </summary>
        public override bool Equals(object? other)
        {
            return other is TokenExpressionConstantColor t && Equals(t);
        }

        /// <summary>
        /// Get hash code
        /// </summary>
        public override int GetHashCode()
        {
            return (this.R, this.G, this.B).GetHashCode();
        }

        /// <summary>
        /// Returns the color as a six-digit hexadecimal string, in the form RRGGBB.
        /// </summary>
        public static string ToHex(double r, double g, double b)
        {
            int red = (int)(r * 255.9);
            int green = (int)(g * 255.9);
            int blue = (int)(b * 255.9);
            return $"{red:X2}{green:X2}{blue:X2}";
        }

        /// <summary>
        /// Returns the color as a six-digit hexadecimal string, in the form RRGGBB.
        /// </summary>
        public static string ToHex(int r, int g, int b)
        {
            return $"{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// Get the hue from an HSL model of color
        /// </summary>
        public double Hue
        {
            get
            {
                if (this.R == this.G && this.G == this.B)
                    return 0.0;

                double hue;

                var min = Math.Min(R, Math.Min(G, B));
                var max = Math.Max(R, Math.Min(G, B));

                var delta = max - min;

                if (Math.Abs(max - R) < float.Epsilon)
                    hue = (G - B) / delta; // between yellow & magenta
                else if (Math.Abs(max - G) < float.Epsilon)
                    hue = 2 + (B - R) / delta; // between cyan & yellow
                else
                    hue = 4 + (R - G) / delta; // between magenta & cyan

                hue *= 60; // degrees

                if (hue < 0)
                    hue += 360;

                return hue * 182.04f;
            }
        }

        /// <summary>
        /// Get the saturation from an HSL model of color (0 - 1.0)
        /// </summary>
        public double Saturation
        {
            get
            {
                var min = Math.Min(R, Math.Min(G, B));
                var max = Math.Max(R, Math.Max(G, B));

                if (Math.Abs(max - min) < float.Epsilon)
                    return 0;
                return (Math.Abs(max) < float.Epsilon) ? 0f : 1f - (1f * min / max);
            }
        }

        /// <summary>
        /// Get the luminance from an HSL model of color (0 - 1.0)
        /// </summary>
        public double Luminance
        {
            get
            {
                return Math.Max(R, Math.Max(G, B));
            }
        }

        /// <summary>
        /// Calculate the perceptual distance between two colors
        /// </summary>
        public double Distance(TokenExpressionConstantColor other)
        {
            return Distance(other.R, other.G, other.B);
        }

        /// <summary>
        /// Calculate the perceptual distance between two colors
        /// </summary>
        public double Distance(double r, double g, double b)
        {
            double deltaRSquared = Math.Pow(this.R - r, 2);
            double deltaGSquared = Math.Pow(this.G - g, 2);
            double deltaBSquared = Math.Pow(this.B - b, 2);

            return Math.Sqrt(2 * deltaRSquared + 4 * deltaGSquared + 3 * deltaBSquared);
        }

        /// <summary>
        /// Get the closest named color
        /// </summary>
        public static TokenExpressionConstant GetClosestNamedColor(double r, double g, double b)
        {
            var closest = AllColors.First();
            double distance = closest.Distance(r, g, b);
            foreach (TokenExpressionConstantColor c in AllColors)
            {
                double newDistance = c.Distance(r, g, b);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    closest = c;
                }
            }
            return closest;
        }

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor INDIANRED = new TokenExpressionConstantColor(205, 92, 92) { Text = "indian red" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTCORAL = new TokenExpressionConstantColor(240, 128, 128) { Text = "light coral" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SALMON = new TokenExpressionConstantColor(250, 128, 114) { Text = "salmon" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKSALMON = new TokenExpressionConstantColor(233, 150, 122) { Text = "dark salmon" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTSALMON = new TokenExpressionConstantColor(255, 160, 122) { Text = "light salmon" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor CRIMSON = new TokenExpressionConstantColor(220, 20, 60) { Text = "crimson" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor RED = new TokenExpressionConstantColor(255, 0, 0) { Text = "red" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor FIREBRICK = new TokenExpressionConstantColor(178, 34, 34) { Text = "fire brick" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKRED = new TokenExpressionConstantColor(139, 0, 0) { Text = "dark red" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor PINK = new TokenExpressionConstantColor(255, 192, 203) { Text = "pink" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTPINK = new TokenExpressionConstantColor(255, 182, 193) { Text = "light pink" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor HOTPINK = new TokenExpressionConstantColor(255, 105, 180) { Text = "hot pink" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DEEPPINK = new TokenExpressionConstantColor(255, 20, 147) { Text = "deep pink" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MEDIUMVIOLETRED = new TokenExpressionConstantColor(199, 21, 133) { Text = "medium violet red" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor PALEVIOLETRED = new TokenExpressionConstantColor(219, 112, 147) { Text = "pale violet red" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor CORAL = new TokenExpressionConstantColor(255, 127, 80) { Text = "coral" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor TOMATO = new TokenExpressionConstantColor(255, 99, 71) { Text = "tomato" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor ORANGERED = new TokenExpressionConstantColor(255, 69, 0) { Text = "orange red" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKORANGE = new TokenExpressionConstantColor(255, 140, 0) { Text = "dark orange" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor ORANGE = new TokenExpressionConstantColor(255, 165, 0) { Text = "orange" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor GOLD = new TokenExpressionConstantColor(255, 215, 0) { Text = "gold" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor YELLOW = new TokenExpressionConstantColor(255, 255, 0) { Text = "yellow" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTYELLOW = new TokenExpressionConstantColor(255, 255, 224) { Text = "light yellow" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LEMONCHIFFON = new TokenExpressionConstantColor(255, 250, 205) { Text = "lemon chiffon" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTGOLDENRODYELLOW = new TokenExpressionConstantColor(250, 250, 210) { Text = "light goldenrod yellow" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor PAPAYAWHIP = new TokenExpressionConstantColor(255, 239, 213) { Text = "papaya whip" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MOCCASIN = new TokenExpressionConstantColor(255, 228, 181) { Text = "moccasin" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor PEACHPUFF = new TokenExpressionConstantColor(255, 218, 185) { Text = "peachpuff" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor PALEGOLDENROD = new TokenExpressionConstantColor(238, 232, 170) { Text = "pale goldenrod" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor KHAKI = new TokenExpressionConstantColor(240, 230, 140) { Text = "khaki" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKKHAKI = new TokenExpressionConstantColor(189, 183, 107) { Text = "dark khaki" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LAVENDER = new TokenExpressionConstantColor(230, 230, 250) { Text = "lavender" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor THISTLE = new TokenExpressionConstantColor(216, 191, 216) { Text = "thistle" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor PLUM = new TokenExpressionConstantColor(221, 160, 221) { Text = "plum" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor VIOLET = new TokenExpressionConstantColor(238, 130, 238) { Text = "violet" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor ORCHID = new TokenExpressionConstantColor(218, 112, 214) { Text = "orchid" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor FUCHSIA = new TokenExpressionConstantColor(255, 0, 255) { Text = "fuchsia" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MAGENTA = new TokenExpressionConstantColor(255, 0, 255) { Text = "magenta" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MEDIUMORCHID = new TokenExpressionConstantColor(186, 85, 211) { Text = "medium orchid" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MEDIUMPURPLE = new TokenExpressionConstantColor(147, 112, 219) { Text = "medium purple" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor REBECCAPURPLE = new TokenExpressionConstantColor(102, 51, 153) { Text = "rebecca purple" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor BLUEVIOLET = new TokenExpressionConstantColor(138, 43, 226) { Text = "blue violet" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKVIOLET = new TokenExpressionConstantColor(148, 0, 211) { Text = "dark violet" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKORCHID = new TokenExpressionConstantColor(153, 50, 204) { Text = "dark orchid" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKMAGENTA = new TokenExpressionConstantColor(139, 0, 139) { Text = "dark magenta" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor PURPLE = new TokenExpressionConstantColor(128, 0, 128) { Text = "purple" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor INDIGO = new TokenExpressionConstantColor(75, 0, 130) { Text = "indigo" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SLATEBLUE = new TokenExpressionConstantColor(106, 90, 205) { Text = "slate blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKSLATEBLUE = new TokenExpressionConstantColor(72, 61, 139) { Text = "dark slate blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MEDIUMSLATEBLUE = new TokenExpressionConstantColor(123, 104, 238) { Text = "medium slate blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor GREENYELLOW = new TokenExpressionConstantColor(173, 255, 47) { Text = "green yellow" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor CHARTREUSE = new TokenExpressionConstantColor(127, 255, 0) { Text = "chartreuse" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LAWNGREEN = new TokenExpressionConstantColor(124, 252, 0) { Text = "lawn green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIME = new TokenExpressionConstantColor(0, 255, 0) { Text = "lime" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIMEGREEN = new TokenExpressionConstantColor(50, 205, 50) { Text = "lime green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor PALEGREEN = new TokenExpressionConstantColor(152, 251, 152) { Text = "pale green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTGREEN = new TokenExpressionConstantColor(144, 238, 144) { Text = "light green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MEDIUMSPRINGGREEN = new TokenExpressionConstantColor(0, 250, 154) { Text = "medium spring green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SPRINGGREEN = new TokenExpressionConstantColor(0, 255, 127) { Text = "spring green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MEDIUMSEAGREEN = new TokenExpressionConstantColor(60, 179, 113) { Text = "medium sea green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SEAGREEN = new TokenExpressionConstantColor(46, 139, 87) { Text = "sea green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor FORESTGREEN = new TokenExpressionConstantColor(34, 139, 34) { Text = "forest green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor GREEN = new TokenExpressionConstantColor(0, 128, 0) { Text = "green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKGREEN = new TokenExpressionConstantColor(0, 100, 0) { Text = "dark green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor YELLOWGREEN = new TokenExpressionConstantColor(154, 205, 50) { Text = "yellow green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor OLIVEDRAB = new TokenExpressionConstantColor(107, 142, 35) { Text = "olive drab" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor OLIVE = new TokenExpressionConstantColor(128, 128, 0) { Text = "olive" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKOLIVEGREEN = new TokenExpressionConstantColor(85, 107, 47) { Text = "dark olive green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MEDIUMAQUAMARINE = new TokenExpressionConstantColor(102, 205, 170) { Text = "medium aquamarine" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKSEAGREEN = new TokenExpressionConstantColor(143, 188, 139) { Text = "dark sea green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTSEAGREEN = new TokenExpressionConstantColor(32, 178, 170) { Text = "light sea green" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKCYAN = new TokenExpressionConstantColor(0, 139, 139) { Text = "dark cyan" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor TEAL = new TokenExpressionConstantColor(0, 128, 128) { Text = "teal" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor AQUA = new TokenExpressionConstantColor(0, 255, 255) { Text = "aqua" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor CYAN = new TokenExpressionConstantColor(0, 255, 255) { Text = "cyan" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTCYAN = new TokenExpressionConstantColor(224, 255, 255) { Text = "light cyan" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor PALETURQUOISE = new TokenExpressionConstantColor(175, 238, 238) { Text = "pale turquoise" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor AQUAMARINE = new TokenExpressionConstantColor(127, 255, 212) { Text = "aquamarine" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor TURQUOISE = new TokenExpressionConstantColor(64, 224, 208) { Text = "turquoise" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MEDIUMTURQUOISE = new TokenExpressionConstantColor(72, 209, 204) { Text = "medium turquoise" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKTURQUOISE = new TokenExpressionConstantColor(0, 206, 209) { Text = "dark turquioise" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor CADETBLUE = new TokenExpressionConstantColor(95, 158, 160) { Text = "cadet blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor STEELBLUE = new TokenExpressionConstantColor(70, 130, 180) { Text = "steel blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTSTEELBLUE = new TokenExpressionConstantColor(176, 196, 222) { Text = "light steel blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor POWDERBLUE = new TokenExpressionConstantColor(176, 224, 230) { Text = "powder blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTBLUE = new TokenExpressionConstantColor(173, 216, 230) { Text = "light blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SKYBLUE = new TokenExpressionConstantColor(135, 206, 235) { Text = "sky blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTSKYBLUE = new TokenExpressionConstantColor(135, 206, 250) { Text = "light sky blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DEEPSKYBLUE = new TokenExpressionConstantColor(0, 191, 255) { Text = "deep sky blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DODGERBLUE = new TokenExpressionConstantColor(30, 144, 255) { Text = "dodge blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor CORNFLOWERBLUE = new TokenExpressionConstantColor(100, 149, 237) { Text = "cornflower blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor ROYALBLUE = new TokenExpressionConstantColor(65, 105, 225) { Text = "royal blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MEDIUMBLUE = new TokenExpressionConstantColor(0, 0, 205) { Text = "medium blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKBLUE = new TokenExpressionConstantColor(0, 0, 139) { Text = "dark blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor NAVY = new TokenExpressionConstantColor(0, 0, 128) { Text = "navy" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MIDNIGHTBLUE = new TokenExpressionConstantColor(25, 25, 112) { Text = "midnight blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor CORNSILK = new TokenExpressionConstantColor(255, 248, 220) { Text = "cornsilk" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor BLANCHEDALMOND = new TokenExpressionConstantColor(255, 235, 205) { Text = "blanched almond" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor BISQUE = new TokenExpressionConstantColor(255, 228, 196) { Text = "bisque" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor NAVAJOWHITE = new TokenExpressionConstantColor(255, 222, 173) { Text = "navajo white" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor WHEAT = new TokenExpressionConstantColor(245, 222, 179) { Text = "wheat" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor BURLYWOOD = new TokenExpressionConstantColor(222, 184, 135) { Text = "burly wood" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor TAN = new TokenExpressionConstantColor(210, 180, 140) { Text = "tan" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor ROSYBROWN = new TokenExpressionConstantColor(188, 143, 143) { Text = "rosy brown" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SANDYBROWN = new TokenExpressionConstantColor(244, 164, 96) { Text = "sandy brown" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor GOLDENROD = new TokenExpressionConstantColor(218, 165, 32) { Text = "goldenrod" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKGOLDENROD = new TokenExpressionConstantColor(184, 134, 11) { Text = "dark goldenrod" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor PERU = new TokenExpressionConstantColor(205, 133, 63) { Text = "peru" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor CHOCOLATE = new TokenExpressionConstantColor(210, 105, 30) { Text = "chocolate" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SADDLEBROWN = new TokenExpressionConstantColor(139, 69, 19) { Text = "saddle brown" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SIENNA = new TokenExpressionConstantColor(160, 82, 45) { Text = "sienna" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor BROWN = new TokenExpressionConstantColor(165, 42, 42) { Text = "brown" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MAROON = new TokenExpressionConstantColor(128, 0, 0) { Text = "maroon" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor WHITE = new TokenExpressionConstantColor(255, 255, 255) { Text = "white" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SNOW = new TokenExpressionConstantColor(255, 250, 250) { Text = "snow" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor HONEYDEW = new TokenExpressionConstantColor(240, 255, 240) { Text = "honeydew" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MINTCREAM = new TokenExpressionConstantColor(245, 255, 250) { Text = "mint cream" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor AZURE = new TokenExpressionConstantColor(240, 255, 255) { Text = "azure" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor ALICEBLUE = new TokenExpressionConstantColor(240, 248, 255) { Text = "alice blue" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor GHOSTWHITE = new TokenExpressionConstantColor(248, 248, 255) { Text = "ghost white" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor WHITESMOKE = new TokenExpressionConstantColor(245, 245, 245) { Text = "white smoke" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SEASHELL = new TokenExpressionConstantColor(255, 245, 238) { Text = "sea shell" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor BEIGE = new TokenExpressionConstantColor(245, 245, 220) { Text = "beige" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor OLDLACE = new TokenExpressionConstantColor(253, 245, 230) { Text = "oldlace" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor FLORALWHITE = new TokenExpressionConstantColor(255, 250, 240) { Text = "floral white" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor IVORY = new TokenExpressionConstantColor(255, 255, 240) { Text = "ivory" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor ANTIQUEWHITE = new TokenExpressionConstantColor(250, 235, 215) { Text = "antique white" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LINEN = new TokenExpressionConstantColor(250, 240, 230) { Text = "linen" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LAVENDERBLUSH = new TokenExpressionConstantColor(255, 240, 245) { Text = "lavender blush" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor MISTYROSE = new TokenExpressionConstantColor(255, 228, 225) { Text = "misty rose" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor GAINSBORO = new TokenExpressionConstantColor(220, 220, 220) { Text = "gainsboro" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTGRAY = new TokenExpressionConstantColor(211, 211, 211) { Text = "light gray" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SILVER = new TokenExpressionConstantColor(192, 192, 192) { Text = "silver" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKGRAY = new TokenExpressionConstantColor(169, 169, 169) { Text = "dark gray" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor GRAY = new TokenExpressionConstantColor(128, 128, 128) { Text = "gray" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DIMGRAY = new TokenExpressionConstantColor(105, 105, 105) { Text = "dim gray" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor LIGHTSLATEGRAY = new TokenExpressionConstantColor(119, 136, 153) { Text = "light slate gray" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor SLATEGRAY = new TokenExpressionConstantColor(112, 128, 144) { Text = "slate gray" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor DARKSLATEGRAY = new TokenExpressionConstantColor(47, 79, 79) { Text = "dark slate gray" };

        /// <summary>
        /// Color
        ///</summary>
        public static readonly TokenExpressionConstantColor BLACK = new TokenExpressionConstantColor(0, 0, 0) { Text = "black" };

        /// <summary>
        /// All named colors
        /// </summary>
        public static readonly List<TokenExpressionConstantColor> AllColors = new List<TokenExpressionConstantColor>
        {
            INDIANRED,
            LIGHTCORAL,
            SALMON,
            DARKSALMON,
            LIGHTSALMON,
            CRIMSON,
            RED,
            FIREBRICK,
            DARKRED,
            PINK,
            LIGHTPINK,
            HOTPINK,
            DEEPPINK,
            MEDIUMVIOLETRED,
            PALEVIOLETRED,
            CORAL,
            TOMATO,
            ORANGERED,
            DARKORANGE,
            ORANGE,
            GOLD,
            YELLOW,
            LIGHTYELLOW,
            LEMONCHIFFON,
            LIGHTGOLDENRODYELLOW,
            PAPAYAWHIP,
            MOCCASIN,
            PEACHPUFF,
            PALEGOLDENROD,
            KHAKI,
            DARKKHAKI,
            LAVENDER,
            THISTLE,
            PLUM,
            VIOLET,
            ORCHID,
            FUCHSIA,
            MAGENTA,
            MEDIUMORCHID,
            MEDIUMPURPLE,
            REBECCAPURPLE,
            BLUEVIOLET,
            DARKVIOLET,
            DARKORCHID,
            DARKMAGENTA,
            PURPLE,
            INDIGO,
            SLATEBLUE,
            DARKSLATEBLUE,
            MEDIUMSLATEBLUE,
            GREENYELLOW,
            CHARTREUSE,
            LAWNGREEN,
            LIME,
            LIMEGREEN,
            PALEGREEN,
            LIGHTGREEN,
            MEDIUMSPRINGGREEN,
            SPRINGGREEN,
            MEDIUMSEAGREEN,
            SEAGREEN,
            FORESTGREEN,
            GREEN,
            DARKGREEN,
            YELLOWGREEN,
            OLIVEDRAB,
            OLIVE,
            DARKOLIVEGREEN,
            MEDIUMAQUAMARINE,
            DARKSEAGREEN,
            LIGHTSEAGREEN,
            DARKCYAN,
            TEAL,
            AQUA,
            CYAN,
            LIGHTCYAN,
            PALETURQUOISE,
            AQUAMARINE,
            TURQUOISE,
            MEDIUMTURQUOISE,
            DARKTURQUOISE,
            CADETBLUE,
            STEELBLUE,
            LIGHTSTEELBLUE,
            POWDERBLUE,
            LIGHTBLUE,
            SKYBLUE,
            LIGHTSKYBLUE,
            DEEPSKYBLUE,
            DODGERBLUE,
            CORNFLOWERBLUE,
            ROYALBLUE,
            MEDIUMBLUE,
            DARKBLUE,
            NAVY,
            MIDNIGHTBLUE,
            CORNSILK,
            BLANCHEDALMOND,
            BISQUE,
            NAVAJOWHITE,
            WHEAT,
            BURLYWOOD,
            TAN,
            ROSYBROWN,
            SANDYBROWN,
            GOLDENROD,
            DARKGOLDENROD,
            PERU,
            CHOCOLATE,
            SADDLEBROWN,
            SIENNA,
            BROWN,
            MAROON,
            WHITE,
            SNOW,
            HONEYDEW,
            MINTCREAM,
            AZURE,
            ALICEBLUE,
            GHOSTWHITE,
            WHITESMOKE,
            SEASHELL,
            BEIGE,
            OLDLACE,
            FLORALWHITE,
            IVORY,
            ANTIQUEWHITE,
            LINEN,
            LAVENDERBLUSH,
            MISTYROSE,
            GAINSBORO,
            LIGHTGRAY,
            SILVER,
            DARKGRAY,
            GRAY,
            DIMGRAY,
            LIGHTSLATEGRAY,
            SLATEGRAY,
            DARKSLATEGRAY,
            BLACK
        };
    }
}
