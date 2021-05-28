using KKAPI;
using KKAPI.Chara;

namespace Generic.Core
{
    partial class Dummy : CharaCustomFunctionController
    {
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            //Dummy Exists so data from this registered GUID isn't disposed of outside of Maker if Dll isn't installed
        }
    }
}
