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
    public enum PlantType { Unassigned, Unknown, NotApplicable,  Annual_herb, Bush, Fern, Grass, Perennial_herb, Tree, Vine }
    public enum WateringType { Unassigned, Unknown, NotApplicable,  Regular, Moderate, Occasional, Infrequent, Drought_tolerant }
    public enum CNPS_Drainage { Unassigned, Unknown, NotApplicable,  Fast, Medium, Slow, Standing }
    public enum YesNoMaybe { Unassigned, Unknown, NotApplicable,  Yes, No };

    // multiple vals
    public enum FloweringMonth { Unassigned, Unknown, NotApplicable, Jan, Feb, Mar, Apr, May, June, July, Aug, Sep, Oct, Nov, Dec }
    public enum SunType { Unassigned, Unknown, NotApplicable, Full, Partial, Shade }

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

        public static bool IsValidFloweringMonth(FloweringMonth value)
        {
            switch (value)
            {
                case FloweringMonth.Unassigned:
                case FloweringMonth.Unknown:
                case FloweringMonth.NotApplicable:
                case FloweringMonth.Jan:
                case FloweringMonth.Feb:
                case FloweringMonth.Mar:
                case FloweringMonth.Apr:
                case FloweringMonth.May:
                case FloweringMonth.June:
                case FloweringMonth.July:
                case FloweringMonth.Aug:
                case FloweringMonth.Sep:
                case FloweringMonth.Oct:
                case FloweringMonth.Nov:
                case FloweringMonth.Dec:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("IsValidFloweringMonth(): Not a valid enum value: " + value.ToString());
                    return false;
            }
            return true;
        }

        public static bool IsValidSunType(SunType value)
        {
            switch (value)
            {
                case SunType.Unassigned:
                case SunType.Full:
                case SunType.Partial:
                case SunType.Shade:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("IsValidSunType(): Not a valid enum value: " + value.ToString());
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

        public static bool IsValidWateringType(CNPS_Drainage value)
        {
            switch (value)
            {
                case CNPS_Drainage.Unassigned:
                case CNPS_Drainage.Unknown:
                case CNPS_Drainage.Fast:
                case CNPS_Drainage.Medium:
                case CNPS_Drainage.Slow:
                case CNPS_Drainage.Standing:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("IsValidWateringType(): Not a valid enum value: " + value.ToString());
                    return false;
            }
            return true;
        }

        public static bool IsValidNativeToCounty(YesNoMaybe value)
        {
            switch (value)
            {
                case YesNoMaybe.Unassigned:
                case YesNoMaybe.Unknown:
                case YesNoMaybe.Yes:
                case YesNoMaybe.No:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("IsValidNativeToCounty(): Not a valid enum value: " + value.ToString());
                    return false;
            }
            return true;
        }

        public static bool IsValidAttractorOf(YesNoMaybe value)
        {
            switch (value)
            {
                case YesNoMaybe.Unassigned:
                case YesNoMaybe.Unknown:
                case YesNoMaybe.Yes:
                case YesNoMaybe.No:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("IsValidAttractorOf(): Not a valid enum value: " + value.ToString());
                    return false;
            }
            return true;
        }
    }

    // When parsing the CSV file, these are the values we expect to find
    public static class ValueOfFieldInSource
    {
        public const string UnivUnassigned = "";
        public const string UnivUnknown = "?";
        public const string UnivNotApplicable = "NA";

        // HACK: do not change the order, we have code which assumes the index of UnivUnassigned
        public static string[] PlantType = { UnivUnassigned, UnivNotApplicable, UnivUnknown, "Annual herb", "Bush", "Fern", "Grass", "Perennial herb", "Tree", "Vine" };
        public static string[] WateringType = { UnivUnassigned, UnivNotApplicable, UnivUnknown, "Regular", "Moderate", "Occasional", "Infrequent", "Drought_tolerant" }; 
        public static string[] CNPS_Drainage = { UnivUnassigned, UnivNotApplicable, UnivUnknown, "Fast", "Medium", "Occasional", "Slow", "Standing" };
        // these enums are used in multiple selection properties
        public static string[] Month = { UnivUnassigned, UnivNotApplicable, UnivUnknown, "Jan", "Feb", "Mar", "Apr", "May", "June", "July", "Aug", "Sep", "Oct", "Nov", "Dec" };
        public static string[] SunType = { UnivUnassigned, UnivNotApplicable, UnivUnknown, "Full", "Partial", "Shade" };
        // this enum is shared by many properties
        public static string[] YesNoMaybe = { UnivUnassigned, UnivNotApplicable, UnivUnknown, "Yes", "No" };
    }

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
        public const int SunRequirement = 9;
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
