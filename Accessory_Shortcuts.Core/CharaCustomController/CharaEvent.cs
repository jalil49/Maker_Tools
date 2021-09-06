using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;

namespace Accessory_Shortcuts
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private ChaFileAccessory.PartsInfo[] Parts => ChaControl.nowCoordinate.accessory.parts;

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        { }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (!MakerAPI.InsideMaker)
            {
                return;
            }
            Constants.Default_Dict();
        }
    }
}
