using System;

namespace Additional_Card_Info.Core
{
    public static class Constants
    {
        public static int CoordinateLength = Enum.GetValues(typeof(ChaFileDefine.CoordinateType)).Length;
        public static int ClothingTypesLength = Enum.GetValues(typeof(ClothingTypes)).Length;
        public static int AdditonalClothingTypesLength = Enum.GetValues(typeof(ClothingTypes)).Length;
        public static int HStatesLength = Enum.GetValues(typeof(HStates)).Length;
        public static int ClubLength = Enum.GetValues(typeof(Club)).Length;
        public static int PersonalityLength = Enum.GetValues(typeof(Personality)).Length;
        public static int TraitsLength = Enum.GetValues(typeof(Traits)).Length;
        public static int SimplifiedCoordinateTypesLength = Enum.GetValues(typeof(SimplifiedCoordinateTypes)).Length;
        public static int HeightLength = Enum.GetValues(typeof(Height)).Length;
        public static int BreastsizeLength = Enum.GetValues(typeof(Breastsize)).Length;

        public enum ClothingTypes
        {
            Top,
            Bottom,
            Bra,
            Panties,
            Gloves,
            Pantyhose,
            Socks,
            Indoor_Shoes,
            Outdoor_Shoes
        }

        public enum AdditonalClothingTypes
        {
            Lingerie,
            Hair,
            AfterSchool_Accessories
        }

        public enum HStates
        {
            First_Time, //0
            Amateur, //1
            Pro, //2
            Lewd //3
        }

        public enum Club
        {
            Not_Club,//-1
            Going_Home_Club, //0
            Swim_Club, //1
            Manga_Club, //2
            Cheer_Club, //3
            Tea_Club, //4
            Track_Club //5
        }

        public enum Personality
        {
            Airhead,
            Angel,
            Athlete,
            Big_Sister,
            Bookworm,
            Classic_Heroine,
            Dark_Lord,
            Emotionless,
            Enigma,
            Extrovert,
            Fangirl,
            Flirt,
            Geek,
            Girl_Nextdoor,
            Heiress,
            Honor_Student,
            Introvert,
            Japanese_Ideal,
            Loner,
            Misfortune_Magnet,
            Motherfigure,
            Old_School,
            Perfectionist,
            Psycho_Stalker,
            Pure_Heart,
            Rebel,
            Returnee,
            Scaredy_Cat,
            Seductress,
            Ski,
            Slacker,
            Slangy,
            Snob,
            Sourpuss,
            Space_Case,
            Tomboy,
            Tough_Girl,
            Underclassman,
            Wild_Child,
        }

        public enum Traits
        {
            Pees_Often,
            Hungry,
            Insensitive,
            Simple,
            Slutty,
            Gloomy,
            Likes_Reading,
            Likes_Music,
            Lively,
            Passive,
            Friendly,
            Likes_Cleanliness,
            Lazy,
            Suddenly_Appears,
            Likes_Being_Aone,
            Likes_Excercising,
            Diligent,
            Likes_Girls
        }

        public enum SimplifiedCoordinateTypes
        {
            Uniform,
            Gym,
            Swimsuit,
            Club,
            Casual,
            Nightwear
        }

        public enum Height
        {
            Short,
            Average,
            Tall
        }

        public enum Breastsize
        {
            Small,
            Average,
            Large
        }

        public enum ClothesNot
        {
            NotBot,
            NotBra,
            NotShorts
        }
    }
}
