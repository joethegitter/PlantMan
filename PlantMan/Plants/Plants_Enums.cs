using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlantMan;

namespace PlantMan.Plants
{
    // WARNING:
    // Do not change enums without changing the corresponding IsValidXXXX() helper method.
    // Do not change values or order of fields in source data without modifyng 
    //      the ValueOfFieldInSource and IndexOfFieldInSource classes.

    // single vals
    public enum PlantType
    {
        Unassigned = 1, Unknown = 2, NotApplicable = 4,
        Annual_herb = 8, Bush = 16, Fern = 32, Grass = 64, Perennial_herb = 128, Tree = 256, Vine = 512
    }

    public enum WateringType
    {
        Unassigned = 1, Unknown = 2, NotApplicable = 4,
        Regular = 8, Moderate = 16, Occasional = 32, Infrequent = 64, Drought_tolerant
    }

    public enum YesNoMaybe { Unassigned = 1, Unknown = 2, NotApplicable = 4, Yes = 8, No = 16 };

    // [Flags] attribute tells the compiler to treat this enum as a bitfield (for OR'ing, etc)
    [Flags]         
    public enum FloweringMonth
    {
        Unassigned = 1, Unknown = 2, NotApplicable = 4,
        Jan = 8, Feb = 16, Mar = 32, Apr = 64, May = 128, Jun = 256,
        Jul = 512, Aug = 1024, Sep = 2048, Oct = 4096, Nov = 8192, Dec = 16384,
        AllMonths = (Jan | Feb | Mar | Apr | May | Jun | Jul | Aug | Sep | Oct | Nov | Dec)
    }

    [Flags]
    public enum SunType
    {
        Unassigned = 1, Unknown = 2, NotApplicable = 4,
        Full = 8, Partial = 16, Shade = 32,
        AllSunTypes = (Full | Partial | Shade)
    }

    [Flags]
    public enum CNPS_DrainageType
    {
        Unassigned = 1, Unknown = 2, NotApplicable = 4,
        Fast = 8, Medium = 16, Slow = 32, Standing = 64,
        AllDrainageType = (Fast | Medium | Slow | Standing)
    }



    public static class Helpers
    {
        public static bool IsValidPlantType(PlantType value)
        {
            switch (value)
            {
                case PlantType.Unassigned:
                case PlantType.Annual_herb:
                case PlantType.Bush:
                case PlantType.Fern:
                case PlantType.Grass:
                case PlantType.Perennial_herb:
                case PlantType.Tree:
                case PlantType.Vine:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("IsValidPlantType(): Not a valid enum value: " + value.ToString());
                    return false;
            }
            return true;
        }

        public static bool IsValidWateringType(WateringType value)
        {
            switch (value)
            {
                case WateringType.Unassigned:
                case WateringType.Regular:
                case WateringType.Moderate:
                case WateringType.Occasional:
                case WateringType.Infrequent:
                case WateringType.Drought_tolerant:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("IsValidWateringType(): Not a valid enum value: " + value.ToString());
                    return false;
            }
            return true;
        }

        public static bool IsValidYesNoMaybe(YesNoMaybe value)
        {
            switch (value)
            {
                case YesNoMaybe.Unassigned:
                case YesNoMaybe.Unknown:
                case YesNoMaybe.Yes:
                case YesNoMaybe.No:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("IsValidYesNoMaybe(): Not a valid enum value: " + value.ToString());
                    return false;
            }
            return true;
        }

        public static bool IsValidFloweringMonthValue(int value)
        {
            FloweringMonth incoming = (FloweringMonth)value;

            FloweringMonth AllVals = (FloweringMonth.Unassigned | FloweringMonth.Unknown |
                FloweringMonth.NotApplicable | FloweringMonth.Jan |
                FloweringMonth.Feb | FloweringMonth.Mar | FloweringMonth.Apr |
                FloweringMonth.May | FloweringMonth.Jun | FloweringMonth.Jul |
                FloweringMonth.Aug | FloweringMonth.Sep | FloweringMonth.Oct | FloweringMonth.Nov | FloweringMonth.Dec);

            // determine if any of the FW enums contained in incoming are
            // also contained within the entire set.
            bool HasMembersOfAll = AllVals.HasFlag(incoming);
            if (!HasMembersOfAll) return false;

            // determine if any of the FW enums contained in incoming are
            // also contained within the month-only-set.
            bool HasMembersOfMonths = FloweringMonth.AllMonths.HasFlag(incoming);

            // determine if any of the FW enums contained in incoming are
            // also contained within the there-can-be-only-one set.
            FloweringMonth TCBOO = (FloweringMonth.Unassigned | FloweringMonth.Unknown | FloweringMonth.NotApplicable);
            bool IsOneOfExcl = TCBOO.HasFlag((FloweringMonth)value);

            if (HasMembersOfMonths && IsOneOfExcl) return false;  // bad combo

            // determine if incoming contains more than one of the values
            // from the TCBOO set.
            if (incoming.HasFlag(FloweringMonth.Unassigned) && incoming.HasFlag(FloweringMonth.Unknown)) return false;
            if (incoming.HasFlag(FloweringMonth.Unassigned) && incoming.HasFlag(FloweringMonth.NotApplicable)) return false;
            if (incoming.HasFlag(FloweringMonth.Unknown) && incoming.HasFlag(FloweringMonth.NotApplicable)) return false;

            return true;
        }

        public static bool IsValidSunTypeValue(int value)
        {
            SunType incoming = (SunType)value;

            SunType AllVals = (SunType.Unassigned | SunType.Unknown |
                SunType.NotApplicable | SunType.Full |
                SunType.Partial | SunType.Shade);

            // determine if any of the FW enums contained in incoming are
            // also contained within the entire set.
            bool HasMembersOfAll = AllVals.HasFlag(incoming);
            if (!HasMembersOfAll) return false;

            // determine if any of the FW enums contained in incoming are
            // also contained within the suntypes-only-set.
            bool HasMembersOfSunTypes = SunType.AllSunTypes.HasFlag(incoming);

            // determine if any of the FW enums contained in incoming are
            // also contained within the there-can-be-only-one set.
            SunType TCBOO = (SunType.Unassigned | SunType.Unknown | SunType.NotApplicable);
            bool IsOneOfExcl = TCBOO.HasFlag((SunType)value);

            if (HasMembersOfSunTypes && IsOneOfExcl) return false;  // bad combo

            // determine if incoming contains more than one of the values
            // from the TCBOO set.
            if (incoming.HasFlag(SunType.Unassigned) && incoming.HasFlag(SunType.Unknown)) return false;
            if (incoming.HasFlag(SunType.Unassigned) && incoming.HasFlag(SunType.NotApplicable)) return false;
            if (incoming.HasFlag(SunType.Unknown) && incoming.HasFlag(SunType.NotApplicable)) return false;

            return true;
        }

        public static bool IsValidCNPS_DrainageValue(int value)
        {
            CNPS_DrainageType incoming = (CNPS_DrainageType)value;

            CNPS_DrainageType AllVals = (CNPS_DrainageType.Unassigned | CNPS_DrainageType.Unknown |
                CNPS_DrainageType.NotApplicable | CNPS_DrainageType.Fast |
                CNPS_DrainageType.Medium | CNPS_DrainageType.Slow | CNPS_DrainageType.Standing );

            // determine if any of the FW enums contained in incoming are
            // also contained within the entire set.
            bool HasMembersOfAll = AllVals.HasFlag(incoming);
            if (!HasMembersOfAll) return false;

            // determine if any of the FW enums contained in incoming are
            // also contained within the CNPS_DrainageTypes-only-set.
            bool HasMembersOfCNPS_DrainageTypes = CNPS_DrainageType.AllDrainageType.HasFlag(incoming);

            // determine if any of the FW enums contained in incoming are
            // also contained within the there-can-be-only-one set.
            CNPS_DrainageType TCBOO = (CNPS_DrainageType.Unassigned | CNPS_DrainageType.Unknown | CNPS_DrainageType.NotApplicable);
            bool IsOneOfExcl = TCBOO.HasFlag((CNPS_DrainageType)value);

            if (HasMembersOfCNPS_DrainageTypes && IsOneOfExcl) return false;  // bad combo

            // determine if incoming contains more than one of the values
            // from the TCBOO set.
            if (incoming.HasFlag(CNPS_DrainageType.Unassigned) && incoming.HasFlag(CNPS_DrainageType.Unknown)) return false;
            if (incoming.HasFlag(CNPS_DrainageType.Unassigned) && incoming.HasFlag(CNPS_DrainageType.NotApplicable)) return false;
            if (incoming.HasFlag(CNPS_DrainageType.Unknown) && incoming.HasFlag(CNPS_DrainageType.NotApplicable)) return false;

            return true;
        }
    }

    /// <summary>
    /// These are the values that the parser expects to see in a CSV file.
    /// </summary>
    public static class ValueOfFieldInSource
    {
        public const string UnivUnassigned = "";
        public const string UnivUnknown = "?";
        public const string UnivNotApplicable = "NA";

        // HACK: do not change the order, we have code which assumes the index of UnivUnassigned
        public static string[] PlantType = { UnivUnassigned, UnivNotApplicable, UnivUnknown,
            "Annual herb", "Bush", "Fern", "Grass", "Perennial herb", "Tree", "Vine" };
        public static string[] WateringType = { UnivUnassigned, UnivNotApplicable, UnivUnknown,
            "Regular", "Moderate", "Occasional", "Infrequent", "Drought tolerant" }; 
        public static string[] CNPS_Drainage = { UnivUnassigned, UnivNotApplicable, UnivUnknown,
            "Fast", "Medium", "Occasional", "Slow", "Standing" };
        // these enums are used in multiple selection properties
        public static string[] Month = { UnivUnassigned, UnivNotApplicable, UnivUnknown,
            "Jan", "Feb", "Mar", "Apr", "May", "June", "July", "Aug", "Sep", "Oct", "Nov", "Dec" };
        public static string[] SunType = { UnivUnassigned, UnivNotApplicable, UnivUnknown,
            "Full", "Partial", "Shade" };
        // this enum is shared by many properties
        public static string[] YesNoMaybe = { UnivUnassigned, UnivNotApplicable, UnivUnknown,
            "Yes", "No" };
    }

    /// <summary>
    /// This class maps the layout of a CSV file which can be parsed into plants.
    /// The name of each index (int) maps to the name of each property, but that
    /// is just informative, not canonical or important.
    /// </summary>
    public static class IndexOfFieldInSource
    {
        public const int Name = 0;
        public const int URL = 1;
        public const int ScientificName = 2;
        public const int Type = 3;
        public const int MinHeight = 4;
        public const int MaxHeight = 5;
        public const int MinWidth = 6;
        public const int MaxWidth = 7;
        public const int FloweringMonths = 8;
        public const int SunRequirements = 9;
        public const int WateringRequirements = 10;
        public const int MinRainfallInches = 11;
        public const int MaxRainfallInches = 12;
        public const int NotableVisuals = 13;
        public const int MinWinterTempF = 14;
        public const int CNPS_Soil = 15;
        public const int MinSoilpH = 16;
        public const int MaxSoilpH = 17;
        public const int CNPS_Drainage = 18;
        public const int Alameda = 19;
        public const int Contra_Costa = 20;
        public const int Marin = 21;
        public const int Napa = 22;
        public const int San_Francisco = 23;
        public const int San_Mateo = 24;
        public const int Santa_Clara = 25;
        public const int Solano = 26;
        public const int Sonoma = 27;
        public const int AttractsNativeBees = 28;
        public const int AttractsButterflies = 29;
        public const int AttractsHummingbirds = 30;
        public const int AttractsBirds = 31;
        public const int Notes = 32;
        public const int DocumentedAsGoodInContainers = 33;
        public const int InShoppingList_DO_NOT_USE = 34;
    }
}
