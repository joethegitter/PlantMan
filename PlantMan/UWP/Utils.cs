using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Diagnostics;

using PlantMan.Plants;
using TextFileParsers;
using JKCo.Utility;

namespace PlantMan
{
    public struct VersionNumber
    {
        public int Major;
        public int Minor;
        public int Build;
        public int Revision;

        public VersionNumber(int major, int minor, int build, int revision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        public override string ToString()
        {
            return Major.ToString() + "." + Minor.ToString() + "." +
                Build.ToString() + "." + Revision.ToString();
        }
    }

    public class Utils
    {
        /// <summary>
        /// If necessary, downloads the new data.
        /// </summary>
        /// <param name="local"></param>
        /// <returns>Returns a string containing the path and name of the resource containing the data.</returns>
        async public static Task<string> DownloadNewUpdateIfWeNeedIt(VersionNumber localVN, string currentResource)
        {
            // "async" in our method signature above means that this method may "await"
            // other async methods that it calls, so callers to this method
            // should "await" this method when they call it, if they want to remain 
            // async themselves. The return type for async methods is Task<return type>.

            // "await" here below causes the method to pause and wait for the return of the API.
            // All code in the caller chain up to here also pauses. However, the rest of
            // the code in our app is not paused, and our app remains responsive
            // to events and user interactions.
            Tuple<bool, VersionNumber> myTupe = await APIQueryGetNewVersion("dataMain", localVN);
            bool fNeedNew = myTupe.Item1;
            VersionNumber targetVN = myTupe.Item2;
            string theNewData = "";
            string whereIsIt = "";

            if (myTupe.Item1 == true)  // server told us to get the new version specified in targetV
            {
                // and we "await" again, since web calls can be slow.
                theNewData = await APIDownloadDataMain(myTupe.Item2);

                // Now store the new data somewhere, and return to caller where to find it.
                // For mocking purposes, this project already has it stored.
                // Fake putting theNewData in a resource, and just return the path to that resource.
                whereIsIt = "CSVData.PlantsFixed.csv";
            }
            else
            {
                whereIsIt = currentResource;
            }

            return whereIsIt;
        }

        // In reality, any method starting with "API" here would really be an API hosted on
        // our server. We just mock them here, for educational and testing purposes.

        /// <summary>
        /// Returns a string containing the newly downloaded data.
        /// </summary>
        /// <param name="versionNumber">The version number of the data file to get.</param>
        /// <returns>String containing the resource name and path from which to get the CSV file.</returns>
        async private static Task<string> APIDownloadDataMain(VersionNumber versionNumber)
        {
            // remember, we're mocking this.
            return "There's no data here, buddy. Fake it.";
        }

        /// <summary>
        /// Queries server to see if server wants us to get new version of an object.
        /// </summary>
        /// <param name="whichVersionableThing">Which object are we talking about.</param>
        /// <param name="localV">The version number of the data the client already has access to.</param>
        /// <param name="targetV">Parameter where we want the server to put the newer version number.</param>
        /// <returns></returns>
        async private static Task<Tuple<bool, VersionNumber>> APIQueryGetNewVersion(string whichVersionableThing, object localV)
        {
            // Why the Tuple?
            // In an ordinary method where we wanted to indicate both bool and some data,
            // we would use an "out" or "ref" parameter modifier, so we could send the
            // data back in a passed variable. However, this is not alllowed for async methods.
            // So, we Tuple it.

            // On the server, compare the "localV" version number, decide if client
            // needs a newer version of the data. If so, return true, and give them
            // version number they should request.

            // create a tuple<bool, VersionNumber> and put the return values in them.
            bool retBool = true;
            VersionNumber retVN = new VersionNumber(1, 2, 3, 4);

            Tuple<bool, VersionNumber> retTuple = new Tuple<bool, VersionNumber>(retBool, retVN);
            return retTuple;
        }

        public static Dictionary<string, Plant> ParseDataIntoStore(object currentApp, string resourceName, bool dataHasHeaderLine = false)
        {
            // create a dictionary of KeyValuePairs, where KVP.Key is name of plant,
            // and KVP.Value is the actual Plant object. Well, intialize one.
            Dictionary<string, Plant> PlantDic = new Dictionary<string, Plant>();

            Assembly assembly = currentApp.GetType().GetTypeInfo().Assembly;
            string resource = resourceName;
            string assName = assembly.GetName().ToString();

            // string[] resNames = assembly.GetManifestResourceNames();

            // TODO:
            // HACK:
            // fuck it, i boned this code a bit. Just insert the correct rsource name now
            resource = "UWP_TestBed.CSVData.PlantsFixed.csv";


            // Read stream and remove quotes                       
            string theUnQuotedVersion = "";
            using (Stream streamCSV = assembly.GetManifestResourceStream(resource))
            {
                // TODO: joe go get that code which verified the resource existed
                if (streamCSV == null)
                {
                    throw new ArgumentException("Could not open a stream from that resource. Misspelled?");
                }

                theUnQuotedVersion = JKCo.Utility.UnQuoter.UnQuote(streamCSV);
            }

            using (Stream stream = UnQuoter.GenerateStreamFromString(theUnQuotedVersion))
            {
                using (DelimitedFieldParser parser = new DelimitedFieldParser(stream))
                {
                    // set the properties of the DelimitedFieldParser object
                    parser.SetDelimiters(new char[] { ',' });   // NEW added
                    parser.HasFieldsEnclosedInQuotes = true;    // NEW addeds

                    bool headerConsumed = false;

                    int lineCounter = 0;

                    while (!parser.EndOfFile)
                    {
                        // Read the line into "TextFields" (specific to this parser)
                        TextFields tfs = parser.ReadFields();
                        lineCounter++;

                        // if useHeader, ignore over first line
                        if (dataHasHeaderLine && lineCounter == 1) { headerConsumed = true; continue; }

                        // convert TextFields into List of strings
                        string[] tfa = tfs.ToArray();
                        List<string> temp = tfa.ToList<string>();

                        // Put the quotes back into each field
                        List<string> theFields = new List<string>();
                        foreach (string s in temp)
                        {
                            string y = UnQuoter.ReQuoteField(s);
                            Debug.WriteLine("Line " + lineCounter.ToString() + ": " + y);
                            theFields.Add(y);
                        }

                        Plant pl = PlantFromListOfFields(theFields, lineCounter);

                        if (pl == null)
                        {
                            throw new InvalidOperationException("PlantFromTextFields returned a null Plant.");
                        }

                        if (pl.Name == null)
                        {
                            throw new InvalidOperationException("PlantFromTextFields returned a Plant with a null name.");
                        }

                        try
                        {
                            PlantDic.Add(pl.Name, pl);
                        }
                        catch (ArgumentException ae)
                        {
                            throw new InvalidOperationException("Tried to add a Plant already in the Dictionary. See Inner.", ae);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("Exception thrown adding Plant to Dictionary. See Inner.", ex);
                        }
                    }
                }
            }
            return PlantDic;
        }

        private void ContinuableException(Exception ex)
        {
            try
            {
                throw ex;
            }
            catch
            {
                // do nothing, let it continue
            }
        }

        /// <summary>
        /// If the value in the field cannot be parsed into a number, returns Decimal.Null
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="outVal"></param>
        /// <param name="lineNumber"></param>
        /// <param name="suppressException"></param>
        /// <returns></returns>
        private static bool GetDecimalValue(List<string> list, int index, out decimal? outVal, int lineNumber, bool suppressException = false)
        {
            string start = "ParseError: Line " + lineNumber.ToString() + " Field " + index.ToString() + " - ";
            decimal? retDec = null;
            bool retBool = false;

            if (list[index] == null)
            {
                Debug.Assert(false, start + "value in list == null");
                outVal = null;
                return false;
            }

            if (string.IsNullOrWhiteSpace(list[index]))
            {
                Debug.Assert(false, start + "value in list == whitespace");
                outVal = null;
                return false;
            }

            if (string.IsNullOrEmpty(list[index]))
            {
                Debug.Assert(false, start + "value in list != null && !IsNullOrEmpty(value)");
                outVal = null;
                return false;
            }

            // if we are here, we have chars of some type. let's see if they are numbers
            bool success = false;
            decimal outie;
            success = decimal.TryParse(list[index], out outie);
            if (!success)
            {
                Debug.Assert(false, start + "not a number, value in list = " + list[index]);
                outVal = null;
                return false;
            }
            else
            {
                retDec = outie;
                retBool = true;
            }
            outVal = retDec;
            return retBool;
        }

        private static bool GetIntegerValue(List<string> list, int index, out int outVal, int lineNumber, bool suppressException = false)
        {
            string start = "ParseError: Line " + lineNumber.ToString() + " Field " + index.ToString() + " - ";
            int retDec = 0;
            bool retBool = false;

            if (list[index] == null)
            {
                Debug.Assert(false, start + "value in list == null");
                outVal = 0;
                return false;
            }

            if (string.IsNullOrWhiteSpace(list[index]))
            {
                Debug.Assert(false, start + "value in list == whitespace");
                outVal = 0;
                return false;
            }

            if (string.IsNullOrEmpty(list[index]))
            {
                Debug.Assert(false, start + "value in list != null && !IsNullOrEmpty(value)");
                outVal = 0;
                return false;
            }

            // if we are here, we have chars of some type. let's see if they are numbers
            bool success = false;
            int outie;
            success = int.TryParse(list[index], out outie);
            if (!success)
            {
                Debug.Assert(false, start + "not a number, value in list = " + list[index]);
                outVal = 0;
                return false;
            }
            else
            {
                retDec = outie;
                retBool = true;
            }
            outVal = retDec;
            return retBool;
        }

        private static string GetStringValue(List<string> list, int index, int lineNumber, bool suppressException = false)
        {
            string start = "ParseError: Line " + lineNumber.ToString() + " Field " + index.ToString() + " - ";

            string retVal = "";

            if (list[index] == null)
            {
                Debug.WriteLine(start + "field == null.");
            }
            else if (string.IsNullOrWhiteSpace(list[index]))
            {
                Debug.WriteLine(start + "field == whitespace only.");
            }
            else if (string.IsNullOrEmpty(list[index]))
            {
                Debug.WriteLine(start + "field == string.Empty().");
            }

            string ARealString = list[index].Trim();
            if (string.IsNullOrEmpty(ARealString))
            {
                Debug.WriteLine(start + "is Empty after Trim().");
                retVal = "";
            }
            else
            {
                retVal = ARealString;
            }
            return retVal;
        }

        /// <summary>
        /// Reads the text value of a field representing a value from an enjm... JOE write
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="suppressException"></param>
        /// <returns>-1 for Unassigned </returns>
        private static int GetIndexOfSingleEnumValue(List<string> list, int index, int lineNumber, bool suppressException = false)
        {
            string start = "ParseNotation: Line " + lineNumber.ToString() + " Field " + index.ToString() + " - ";

            int indexIntoArray = -1;
            string trimmed = "";

            // Translate all variants of null or empty or whitespace to "";
            // but articulate what the actual value was here.
            if (list[index] == null)
            {
                Debug.Assert(false, start + "value in list == null");
                trimmed = "";
            }
            else if (string.IsNullOrWhiteSpace(list[index]))
            {
                Debug.Assert(false, start + "value in list == whitespace");
                trimmed = "";
            }
            else if (string.IsNullOrEmpty(list[index]))
            {
                Debug.Assert(false, start + "value in list == not null but IsNullOrEmpty() fails.");
                trimmed = "";
            }
            else
            {
                trimmed = list[index].Trim();
            }

            // Let each enum type decide how to handle empty value.

            // Determine which static array we should look in to find the value stored in list[index].
            // Once you determine which array to look in, return the index of that array where the field
            // value was found. If not found, return -1.
            switch (index)
            {
                case IndexOfFieldInSource.Type:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.PlantType, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.CNPS_Drainage:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.CNPS_Drainage, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        // that was a valid value for this field, so return the
                        // index into array of valid values for this field.
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.SunRequirement:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.SunType, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.WateringRequirements:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.SunType, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.AttractsBirds:
                case IndexOfFieldInSource.AttractsButterflies:
                case IndexOfFieldInSource.AttractsHummingbirds:
                case IndexOfFieldInSource.AttractsNativeBees:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.SunType, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.Alameda:
                case IndexOfFieldInSource.Contra_Costa:
                case IndexOfFieldInSource.Marin:
                case IndexOfFieldInSource.Napa:
                case IndexOfFieldInSource.Santa_Clara:
                case IndexOfFieldInSource.San_Francisco:
                case IndexOfFieldInSource.San_Mateo:
                case IndexOfFieldInSource.Solano:
                case IndexOfFieldInSource.Sonoma:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.SunType, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.DocumentedAsGoodInContainers:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.YesNoMaybe, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                default:
                    {
                        Debug.WriteLine("Umappable value at Field Index: " + index.ToString());
                        if (!suppressException) throw new InvalidOperationException("Umappable value at Field Index: " + index.ToString());
                        return indexIntoArray; // should still be -1
                    }
            }
            return indexIntoArray;
        }


        /// <summary>
        /// Returns NULL if invalid name passed, Plant could not be created.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        private static Plant PlantFromListOfFields(List<string> list, int lineNumber)
        {
            // we cannot create an actual Plant object yet
            // because we have not yet read the name from the csv
            Plant pl;
            string strVal = "";
            string UNASSIGNED = "<Value has not been assigned.>";
            string start = "Line " + lineNumber.ToString() + " Field ";


            if (list == null) { throw new ArgumentNullException("list"); }

            #region String Properties

            // Name
            strVal = GetStringValue(list, IndexOfFieldInSource.Name, lineNumber);
            if (strVal == "")
            {
                Debug.WriteLine(start + "Name - INVALID NAME, record will be skipped.");
                try
                {
                    throw new InvalidOperationException(start + "Name - INVALID NAME, record will be skipped.");
                }
                catch
                {
                    // we threw the exception for visibility during testing, we can proceed
                    return null;
                }
            }

            try
            {
                pl = new Plant(list[0]);
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.WriteLine(start + "Name? - could not create Plant, record will be skipped.");
                    throw new InvalidOperationException(start + "Name? - could not create Plant, record will be skipped; name = {" + strVal + "}", ex);
                }
                catch
                {
                    // we only threw exception for visibility, we can proceed
                    return null;
                }
            }

            // URL
            strVal = GetStringValue(list, IndexOfFieldInSource.URL, lineNumber);
            if (strVal == "")
            {
                Debug.WriteLine(start + "URL - was empty, will become Unassigned.");
                pl.URL = UNASSIGNED;
            }
            else
            {
                pl.URL = strVal;
            }

            // ScientificName
            strVal = GetStringValue(list, IndexOfFieldInSource.ScientificName, lineNumber);
            if (strVal == "")
            {
                Debug.WriteLine(start + "ScientificName - was empty, will become Unassigned.");
                pl.ScientificName = UNASSIGNED;
            }
            else
            {
                pl.ScientificName = strVal;
            }

            // NotableVisuals
            strVal = GetStringValue(list, IndexOfFieldInSource.NotableVisuals, lineNumber);
            pl.NotableVisuals = strVal;

            // Notes
            strVal = GetStringValue(list, IndexOfFieldInSource.Notes, lineNumber);
            pl.Notes = strVal;

            // CNPS_Soil
            strVal = GetStringValue(list, IndexOfFieldInSource.CNPS_Soil, lineNumber);
            if (strVal == "")
            {
                Debug.WriteLine(start + "ScientificName - was empty, will become Unassigned.");
                pl.CNPS_Soil = UNASSIGNED;
            }
            else
            {
                pl.CNPS_Soil = strVal;
            }

            #endregion String Properties

            #region Numeric Fields
            // numerics
            // If we cannot read a numeric value from the CSV, we set the property to NULL
            // (YES! Because it's a nullable decimal type. Weird, I know...
            Decimal? decVal;
            if (GetDecimalValue(list, IndexOfFieldInSource.MaxHeight, out decVal, lineNumber))
            {
                pl.MaxHeight_Nullable = decVal;
            }
            else
            {
                Debug.WriteLine(start + "MaxHeight_Nullable - could not be parsed, will become null.");
                pl.MaxHeight_Nullable = null;
            }

            if (GetDecimalValue(list, IndexOfFieldInSource.MinHeight, out decVal, lineNumber))
            {
                pl.MinHeight_Nullable = decVal;
            }
            else
            {
                Debug.WriteLine(start + "MinHeight_Nullable - could not be parsed, will become null.");
                pl.MinHeight_Nullable = null;
            }

            if (GetDecimalValue(list, IndexOfFieldInSource.MaxWidth, out decVal, lineNumber))
            {
                pl.MaxWidth_Nullable = decVal;
            }
            else
            {
                Debug.WriteLine(start + "MaxWidth - could not be parsed, will become null.");
                pl.MaxWidth_Nullable = null;
            }

            if (GetDecimalValue(list, IndexOfFieldInSource.MinWidth, out decVal, lineNumber))
            {
                pl.MinWidth_Nullable = decVal;
            }
            else
            {
                Debug.WriteLine(start + "MinWidth_Nullable - could not be parsed, will become null.");
                pl.MinWidth_Nullable = null;
            }

            if (GetDecimalValue(list, IndexOfFieldInSource.MaxRainfallInches, out decVal, lineNumber))
            {
                pl.MaxRainfallInches_Nullable = decVal;
            }
            else
            {
                Debug.WriteLine(start + "MaxRainfallInches - could not be parsed, will become null.");
                pl.MaxRainfallInches_Nullable = null;
            }

            if (GetDecimalValue(list, IndexOfFieldInSource.MaxSoilpH, out decVal, lineNumber))
            {
                pl.MaxSoilpH_Nullable = decVal;
            }
            else
            {
                Debug.WriteLine(start + "MaxSoilpH_Nullable - could not be parsed, will become null.");
                pl.MaxSoilpH_Nullable = null;
            }

            if (GetDecimalValue(list, IndexOfFieldInSource.MinSoilpH, out decVal, lineNumber))
            {
                pl.MinSoilpH_Nullable = decVal;
            }
            else
            {
                Debug.WriteLine(start + "MinSoilpH_Nullable - could not be parsed, will become null.");
                pl.MinSoilpH_Nullable = null;
            }

            if (GetDecimalValue(list, IndexOfFieldInSource.MinRainfallInches, out decVal, lineNumber))
            {
                pl.MinRainfallInches_Nullable = decVal;
            }
            else
            {
                Debug.WriteLine(start + "MinRainfallInches_Nullable - could not be parsed, will become null.");
                pl.MinRainfallInches_Nullable = null;
            }

            if (GetDecimalValue(list, IndexOfFieldInSource.MinWinterTempF, out decVal, lineNumber))
            {
                pl.MinWinterTempF_Nullable = decVal;
            }
            else
            {
                Debug.WriteLine(start + "MinWinterTempF_Nullable - could not be parsed, will become null.");
                pl.MinWinterTempF_Nullable = null;
            }

            #endregion Numeric Fields

            #region Enum Properties

            // Single Enum values
            // indexOfCanonicalValue is the index into the specific array of strings
            // containing the valid canonical values for this field. If the value in 
            // the field could not be mapped to a canonical value, the result will be
            // this field's version of Unassigned.
            int indexOfCanonicalValue;
            string canonicalVal = "";

            // Type
            PlantType tt = PlantType.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.Type, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.PlantType[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "Annual_herb", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Annual_herb;
            }
            else if (string.Equals(canonicalVal, "Bush", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Bush;
            }
            else if (string.Equals(canonicalVal, "Shade", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Fern;
            }
            else if (string.Equals(canonicalVal, "Grass", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Grass;
            }
            else if (string.Equals(canonicalVal, "Perennial_herb", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Perennial_herb;
            }
            else if (string.Equals(canonicalVal, "Tree", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Tree;
            }
            else if (string.Equals(canonicalVal, "Vine", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Vine;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Unknown;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.NotApplicable;
            }
            pl.Type = tt;

            // WateringRequirement
            WateringType wt = WateringType.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.WateringRequirements, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.WateringType[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "Drought_tolerant", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Drought_tolerant;
            }
            else if (string.Equals(canonicalVal, "Infrequent", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Infrequent;
            }
            else if (string.Equals(canonicalVal, "Moderate", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Moderate;
            }
            else if (string.Equals(canonicalVal, "Occasional", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Occasional;
            }
            else if (string.Equals(canonicalVal, "Regular", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Regular;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Unknown;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.NotApplicable;
            }
            pl.WateringRequirement = wt;

            // CNPS_Drainage
            CNPS_Drainage ct = CNPS_Drainage.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.CNPS_Drainage, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.CNPS_Drainage[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "Fast", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Fast;
            }
            else if (string.Equals(canonicalVal, "Medium", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Medium;
            }
            else if (string.Equals(canonicalVal, "Slow", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Slow;
            }
            else if (string.Equals(canonicalVal, "Standing", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Standing;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Unknown;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.NotApplicable;
            }
            pl.CNPS_Drainage = ct;

            // Yes / No

            // AttractsBirds
            YesNoMaybe yn = YesNoMaybe.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.AttractsBirds, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.YesNoMaybe[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "No", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.No;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.NotApplicable;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unknown;
            }
            else if (string.Equals(canonicalVal, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Yes;
            }
            pl.AttractsBirds = yn;

            // AttractsButterflies
            yn = YesNoMaybe.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.AttractsButterflies, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.YesNoMaybe[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "No", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.No;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.NotApplicable;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unknown;
            }
            else if (string.Equals(canonicalVal, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Yes;
            }
            pl.AttractsButterflies = yn;


            // AttractsHummingbirds
            yn = YesNoMaybe.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.AttractsHummingbirds, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.YesNoMaybe[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "No", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.No;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.NotApplicable;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unknown;
            }
            else if (string.Equals(canonicalVal, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Yes;
            }
            pl.AttractsHummingbirds = yn;

            // AttractsNativeBees
            yn = YesNoMaybe.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.AttractsNativeBees, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.YesNoMaybe[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "No", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.No;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.NotApplicable;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unknown;
            }
            else if (string.Equals(canonicalVal, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Yes;
            }
            pl.AttractsNativeBees = yn;

            // DocumentedAsGoodInContainers
            yn = YesNoMaybe.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.DocumentedAsGoodInContainers, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.YesNoMaybe[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "No", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.No;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.NotApplicable;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unknown;
            }
            else if (string.Equals(canonicalVal, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Yes;
            }
            pl.DocumentedAsGoodInContainers = yn;

            // Multiple Value Properties


            // FloweringMonths


            // SunRequirements
            // TODO: hack now, complete later
            // By default this is set to empty list

            // Joe: backing field for FloweringMonths and SunRequirements should be a flagged enum
            // structure. User facing can remain lists, but have them to resolve to OR'd values.
            // use the 
            // EnumType value = EnumType.Enum.Parse(typeOf(EnumType), "mar,apr,jun") to have them
            // automatically Ord. See page 807-808



            #endregion Enum Properties

            return pl;

        }
    }
}
