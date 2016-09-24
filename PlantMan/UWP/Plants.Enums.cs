using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants
{
    // WARNING:
    // Do not change enums without changing the corresponding IsValidXXXX() method.

    public enum PlantType { Unassigned, Annual_herb, Bush, Fern, Grass, Perennial_herb, Tree, Vine }
    public enum FloweringMonth { Unassigned, Unknown, NotApplicable, Jan, Feb, Mar, Apr, May, June, July, Aug, Sep, Oct, Nov, Dec }
    public enum SunType { Unassigned, Full, Partial, Shade }
    public enum WateringType { Unassigned, Regular, Moderate, Occasional, Infrequent, Drought_tolerant }
    public enum CNPS_Drainage { Unassigned, Unknown, Fast, Medium, Slow, Standing }
    public enum NativeToCounty { Unassigned, Unknown, Yes, No };
    public enum AttractorOf { Unassigned, Unknown, Yes, No }


    public static class Enums
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

        public static bool IsValidNativeToCounty(NativeToCounty value)
        {
            switch (value)
            {
                case NativeToCounty.Unassigned:
                case NativeToCounty.Unknown:
                case NativeToCounty.Yes:
                case NativeToCounty.No:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("IsValidNativeToCounty(): Not a valid enum value: " + value.ToString());
                    return false;
            }
            return true;
        }

        public static bool IsValidAttractorOf(AttractorOf value)
        {
            switch (value)
            {
                case AttractorOf.Unassigned:
                case AttractorOf.Unknown:
                case AttractorOf.Yes:
                case AttractorOf.No:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("IsValidAttractorOf(): Not a valid enum value: " + value.ToString());
                    return false;
            }
            return true;
        }
    }

    // When parsing the CSV file, use these values to recognize meaning
    public static class ValInCsv
    {
        public static string[] Unknown = { "?" };
        public static string[] Yes = { "Y", "y", "Yes", "yes", "x" };
        public static string[] No = { "N", "n", "No", "no", "<NullAsString>" };
        public static string[] Unassigned = { "<NullAsString>" };
    }

}
