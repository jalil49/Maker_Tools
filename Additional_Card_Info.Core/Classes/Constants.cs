using System;
using System.Collections.Generic;

namespace Additional_Card_Info
{
    public static class Constants
    {
        public enum AdditonalClothingTypes
        {
            Lingerie,
            Hair,
            AfterSchoolAccessories,
            SwimmingAccessories,
            BathroomAccessories
        }

        public enum Breastsize
        {
            Small,
            Average,
            Large
        }

        public enum ClothingTypes
        {
            Top,
            Bottom,
            Bra,
            Panties,
            Gloves,
            Pantyhose,
            Socks,
            IndoorShoes,
            OutdoorShoes
        }

        public enum Club
        {
            GoingHomeClub, //0
            SwimClub, //1
            MangaClub, //2
            CheerClub, //3
            TeaClub, //4
            TrackClub //5
        }

        public enum Height
        {
            Short,
            Average,
            Tall
        }

        public enum HStates
        {
            FirstTime, //0
            Amateur, //1
            Pro, //2
            Lewd //3
        }

        public enum Interests
        {
            Play,
            Nature,
            Fish,
            Sightseeing,
            H
        }

        public enum Personality
        {
            Airhead,
            Angel,
            Athlete,
            BigSister,
            Bookworm,
            ClassicHeroine,
            DarkLord,
            Emotionless,
            Enigma,
            Extrovert,
            Fangirl,
            Flirt,
            Geek,
            GirlNextdoor,
            Heiress,
            HonorStudent,
            Introvert,
            JapaneseIdeal,
            Loner,
            MisfortuneMagnet,
            Motherfigure,
            OldSchool,
            Perfectionist,
            PsychoStalker,
            PureHeart,
            Rebel,
            Returnee,
            ScaredyCat,
            Seductress,
            Ski,
            Slacker,
            Slangy,
            Snob,
            Sourpuss,
            SpaceCase,
            Tomboy,
            ToughGirl,
            Underclassman,
            WildChild,
            IslandGirl
        }

        public enum SimplifiedCoordinateTypes
        {
            SchoolUniform,
            Gym,
            Swimsuit,
            Club,
            Casual,
            Nightwear,
            Bathroom
        }

        public enum Traits
        {
#if KK
            PeesOften,
            Hungry,
            Insensitive,
            Simple,
            Slutty,
            Gloomy,
            LikesReading,
            LikesMusic,
            Lively,
            Passive,
            Friendly,
            LikesCleanliness,
            Lazy,
            SuddenlyAppears,
            LikesBeingAone,
            LikesExcercising,
            Diligent,
            LikesGirls,
            Information,
            LoveLove,
            Lonely
#elif KKS
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

        public static readonly int ClothingTypesLength = Enum.GetValues(typeof(ClothingTypes)).Length;
        public static int AdditonalClothingTypesLength = Enum.GetValues(typeof(AdditonalClothingTypes)).Length;
        public static int HStatesLength = Enum.GetValues(typeof(HStates)).Length;
        public static int ClubLength = Enum.GetValues(typeof(Club)).Length;
        public static readonly int PersonalityLength = Enum.GetValues(typeof(Personality)).Length;
        public static readonly int TraitsLength = Enum.GetValues(typeof(Traits)).Length;
        public static readonly int SimplifiedCoordinateTypesLength = Enum.GetValues(typeof(SimplifiedCoordinateTypes)).Length;
        public static readonly int HeightLength = Enum.GetValues(typeof(Height)).Length;
        public static readonly int BreastsizeLength = Enum.GetValues(typeof(Breastsize)).Length;
        public static readonly int InterestLength = Enum.GetValues(typeof(Interests)).Length;

        public static readonly SortedDictionary<int, int> KoiToSunTraits = new SortedDictionary<int, int>
        {
            [0] = 16,
            [1] = 0,
            [2] = 5,
            [3] = 1,
            [4] = 18,
            [5] = 19,
            [6] = 2,
            [7] = 3,
            [8] = 13,
            [9] = 14,
            [10] = 5,
            [11] = 6,
            [12] = 15,
            [13] = 7,
            [14] = 8,
            [15] = 9,
            [16] = 10,
            [17] = 18,
            [18] = 11,
            [19] = 12,
            [20] = 20
        };
    }
}