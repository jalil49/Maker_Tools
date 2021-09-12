using System.Collections.Generic;

namespace CardUpdateTool
{
    public static class Constants
    {
        public static Dictionary<string, ReadableInfo> ReadableGuid = new Dictionary<string, ReadableInfo>()
        {
            ["Additional_Card_Info"] = new ReadableInfo("Additional Card Info") { KnownVersion = 1 },
            ["Accessory_Parents"] = new ReadableInfo("Accessory Parents") { KnownVersion = 1 },
            ["Accessory_States"] = new ReadableInfo("Accessory States") { KnownVersion = 1 },
            ["Accessory_Themes"] = new ReadableInfo("Accessory Themes") { KnownVersion = 1 },
            ["madevil.kk.ca"] = new ReadableInfo("Madevil Character Acc", false, true) { KnownVersion = 3 },
            ["madevil.kk.ass"] = new ReadableInfo("Acc State Sync", false, true) { KnownVersion = 6 },
            ["KSOX"] = new ReadableInfo("Koi Skin Overlay", true, false) { KnownVersion = 2 },
            ["KCOX"] = new ReadableInfo("Koi Cloth Overlay") { KnownVersion = 1 },
            ["KKABMPlugin.ABMData"] = new ReadableInfo("ABMX") { KnownVersion = 2 },
            ["com.deathweasel.bepinex.autosave"] = new ReadableInfo("Auto Save", false),
            ["com.deathweasel.bepinex.hairaccessorycustomizer"] = new ReadableInfo("Hair Acc. Customizer"),
            ["com.deathweasel.bepinex.materialeditor"] = new ReadableInfo("Material Editor"),
            ["com.deathweasel.bepinex.hcharaadjustment"] = new ReadableInfo("H Chara Adjustment", true, false),
            ["com.deathweasel.bepinex.pushup"] = new ReadableInfo("Push Up"),
            ["com.deathweasel.bepinex.clothingunlocker"] = new ReadableInfo("Clothing Unlocker", true, false),
            ["com.deathweasel.bepinex.colliders"] = new ReadableInfo("Colliders", false),
            ["com.deathweasel.bepinex.eyecontrol"] = new ReadableInfo("Eye Control", false),
            ["com.deathweasel.bepinex.eyeshaking"] = new ReadableInfo("Eye Shaking", false),
            ["com.deathweasel.bepinex.moreoutfits"] = new ReadableInfo("More Outfits", true, false),
            ["com.deathweasel.bepinex.dynamicboneeditor"] = new ReadableInfo("Dynamic Bone Editor"),
            ["com.deathweasel.bepinex.uncensorselector"] = new ReadableInfo("Uncensor Selector", true, false),
            ["orange.spork.advikplugin"] = new ReadableInfo("Advanced IK", true, false),
            ["picolet21.koikatsu.EroStatus"] = new ReadableInfo("EroStatus", true, false),
            ["Sauceke.SexFaces"] = new ReadableInfo("Sex Faces", true, false),
            ["com.gebo.BepInEx.GameDialogHelper"] = new ReadableInfo("GameDialogHelper", true, false),
            ["com.gebo.bepinex.translationhelper.chara"] = new ReadableInfo("Translation Helper Chara", true, false),
            ["kokaiinum.KKExpandMaleMaker"] = new ReadableInfo("Expand Male Maker", true, false),
            ["marco.authordata"] = new ReadableInfo("Author Data", false) { KnownVersion = 1 },
            ["Marco.SkinEffects"] = new ReadableInfo("Skin Effects", false),
            ["BonerStateSync"] = new ReadableInfo("Boner State Sync", false) { KnownVersion = 1 },
            ["Bulge"] = new ReadableInfo("Bulge", false) { KnownVersion = 1 },
            ["KK_Pregnancy"] = new ReadableInfo("Pregnancy", false) { KnownVersion = 1 },
            ["KK_PregnancyPlus"] = new ReadableInfo("Pregnancy Plus", false) { KnownVersion = 1 },
            ["moreAccessories"] = new ReadableInfo("More Accessories") { KnownVersion = 2 },
        };

        public class ReadableInfo
        {
            public string Name;
            public bool Charavisible;
            public bool Outfitvisible;
            public int KnownVersion = 0;

            public ReadableInfo(string _name, bool _chara, bool _outfit)
            {
                Name = _name;
                Charavisible = _chara;
                Outfitvisible = _outfit;
            }

            public ReadableInfo(string _name, bool _showboth = true)
            {
                Name = _name;
                Outfitvisible = Charavisible = _showboth;
            }
        }
    }
}
