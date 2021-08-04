using System;
using System.Collections.Generic;

namespace Additional_Card_Info
{
    public static class Constants
    {
        public static int ClothingTypesLength = Enum.GetValues(typeof(ClothingTypes)).Length;
        public static int AdditonalClothingTypesLength = Enum.GetValues(typeof(AdditonalClothingTypes)).Length;
        public static int HStatesLength = Enum.GetValues(typeof(HStates)).Length;
        public static int ClubLength = Enum.GetValues(typeof(Club)).Length;
        public static int PersonalityLength = Enum.GetValues(typeof(Personality)).Length;
        public static int TraitsLength = Enum.GetValues(typeof(Traits)).Length;
        public static int SimplifiedCoordinateTypesLength = Enum.GetValues(typeof(SimplifiedCoordinateTypes)).Length;
        public static int HeightLength = Enum.GetValues(typeof(Height)).Length;
        public static int BreastsizeLength = Enum.GetValues(typeof(Breastsize)).Length;
        public static int InterestLength = Enum.GetValues(typeof(Interests)).Length;

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
            AfterSchool_Accessories,
            Swimming_Accessories,
            Bathroom_Accessories
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
            Island_Girl
        }

        public enum Traits
        {
#if !KKS
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
            Likes_Girls,
            Information,
            Love_Love,
            Lonely
#else
            Hungry,
            Simple,
            Likes_Reading,
            Likes_Music,
            Insensitive,
            Friendly,
            Likes_Cleanliness,
            Suddenly_Appears,
            Likes_Being_Aone,
            Likes_Excercising,
            Diligent,
            Information,
            Love_Love,
            Lively,
            Passive,
            Lazy,
            Pees_Often,
            Likes_Girls,
            Slutty,
            Gloomy,
            Lonely
#endif
        }

        public readonly static SortedDictionary<int, int> Koi_to_Sun_Traits = new SortedDictionary<int, int>() {
            {0, 16},
            {1, 0 },
            {2, 5 },
            {3, 1 },
            {4, 18 },
            {5, 19},
            {6, 2},
            {7, 3},
            {8, 13},
            {9, 14},
            {10, 5},
            {11, 6},
            {12, 15},
            {13, 7},
            {14, 8},
            {15, 9},
            {16, 10},
            {17, 18},
            {18, 11},
            {19, 12},
            {20, 20}
        };

        public enum SimplifiedCoordinateTypes
        {
            Uniform,
            Gym,
            Swimsuit,
            Club,
            Casual,
            Nightwear,
            Bathroom
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

        public enum Interests
        {
            Play,
            Nature,
            Fish,
            Sightseeing,
            H
        }
    }
}
