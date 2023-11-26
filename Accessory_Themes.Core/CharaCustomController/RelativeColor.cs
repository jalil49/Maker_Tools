using System.Collections.Generic;

namespace Accessory_Themes
{
    internal class RelativeColor
    {
        public ThemeData Theme { get; set; }
        public int ColorNum { get; set; }

        public RelativeColor(ThemeData theme, int colorNum)
        {
            Theme = theme;
            this.ColorNum = colorNum;
        }

        public override bool Equals(object obj)
        {
            return obj is RelativeColor color &&
                   EqualityComparer<ThemeData>.Default.Equals(Theme, color.Theme) &&
                   ColorNum == color.ColorNum;
        }

        public override int GetHashCode()
        {
            var hashCode = -1395371734;
            hashCode = hashCode * -1521134295 + EqualityComparer<ThemeData>.Default.GetHashCode(Theme);
            hashCode = hashCode * -1521134295 + ColorNum.GetHashCode();
            return hashCode;
        }
    }
}
