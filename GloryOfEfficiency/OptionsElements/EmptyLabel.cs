using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace GloryOfEfficiency.OptionsElements
{
    internal class EmptyLabel : LabelComponent
    {
        public EmptyLabel() : base("") { }
        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null) {}
    }
}
