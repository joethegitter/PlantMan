using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlantMan;

namespace PlantMan.Plants
{
    public class Plant : IInstanceIDable
    {
        // Constructors
        public Plant(string Name)
        {
            // TODO: we should actually call an internal set for this, so that it goes
            // through validation...
            _name = Name;

            // now we set default values for some fields/properties
            Initialize();
        }

        // By default, compiler will create a public parameterless constructor
        // behind our back. We don't want callers to use that constructor, so
        // we throw an exception.
        public Plant()
        {
            throw new InvalidOperationException("Use the other constructor.");
        }

        // Public Properties
        // ----------------------- //

        // Use properties when you want to react to or control 
        // the reading and writing of these values.

        // Name
        public string Name
        {
            // The Name Property is read only outside of this object; the value is 
            // set in the constructor, when a new Plant object is created. This is 
            // how we ensure that any Plant object created actually has a name.
            // Later, when trying to add a Plant to our database, Name will be our 
            // "key" property which enforces uniqueness between records.
            get { return _name; }

            private set
            {
                // TODO: should also makes sure the string is not just unprintable chars or
                // anything that would cause a problem for database storage...
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
                {
                    _name = value;
                }
                else
                {
                    throw new ArgumentException("Property cannot be set to null or empty or whitespace", "Name");
                }
            }
        }

        public int InstanceID
        {
            get { return _instanceID; }
            private set { _instanceID = value; }
        }

        // I am leaving ScientificName below as an example of how to convert public
        // fields to properties. Eventually you really do want to enforce these;
        // but for now I'll provide a ScientificName public field, which you can
        // remove when you convert to property.

        // ScientificName
        /*
       public string ScientificName
       {
           // We do not allow a blank value to be "set";
           // to allow "under construction" records and protect ourselves,
           // we will initalize this value in the constructor call to Initialize()

           get { return _scientificName; }

           set
           {
               if (!String.IsNullOrEmpty(value) && !String.IsNullOrWhiteSpace(value))
               {
                   _scientificName = value;
               }
               else
               {
                   throw new ArgumentException("Property cannot be set to null or empty or whitespace", "ScientificName");
               }
           }
       }
       */

        // Type
        public PlantType Type
        {
            get { return _type; }

            set
            {
                if (Helpers.IsValidPlantType(value))
                {
                    _type = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Type.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "PlantType");
                }
            }
        }

        // Flowering months
        public List<FloweringMonth> FloweringMonths
        {
            get
            {
                // Don't give them the actual list, give them a copy. We want to control
                // modifications to the list, so we force them to give us a whole new list
                // when they want to set this property. Alternative is to wrap list with
                // our own Add/Remove/Clear methods.
                List<FloweringMonth> k = _floweringMonths.ToList<FloweringMonth>();
                return k;
            }

            set
            {
                // Here the caller hands us an entire list.  We'll reject the list if
                // it contains any non-valid FloweringMonthvalues.
                if (value == null)
                {
                    throw new ArgumentNullException("FloweringMonths", "Value cannot be null.");
                }

                // we don't allow an empty list. Could auto-assign "Unassigned", 
                // but let's force caller to be explicit instead.
                if (value.Count == 0)  
                {
                    throw new ArgumentException("Empty list not allowed. Use Unassigned or Unknown or Not Applicable", "FloweringMonths");
                }

                // If present, Unassigned, Unknown and Not Applicable must be the ONLY value in list.
                if (value.Count > 1)
                {
                    if (value.Contains<FloweringMonth>(FloweringMonth.Unassigned) ||
                            value.Contains<FloweringMonth>(FloweringMonth.Unassigned) ||
                            value.Contains<FloweringMonth>(FloweringMonth.NotApplicable)
                        )
                    {
                        throw new ArgumentException("List cannot contain Unassigned or Unknown or NotApplicable AND another value.");
                    }
                }

                // All values in list must be valid
                foreach (FloweringMonth v in value)
                {
                    if (!Helpers.IsValidFloweringMonth(v))
                    {
                        throw new ArgumentException("Property not set. An item in list was not a valid FloweringMonth value: " + v.ToString());
                    }
                }

                _floweringMonths = value;
            }
        }

        // SunRequirement
        public List<SunType> SunRequirements
        {
            get
            {
                // Don't give them the actual list, give them a copy. We want to control
                // modifications to the list, so we force them to give us a whole new list
                // when they want to set this property. Alternative is to wrap list with
                // our own Add/Remove/Clear methods.
                List<SunType> k = _sunRequirements.ToList<SunType>();
                return k;
            }

            set
            {
                // Here the caller hands us an entire list.  We'll reject the list if
                // it contains any non-valid SunType values.
                if (value == null)
                {
                    throw new ArgumentNullException("SunRequirements", "Value cannot be null.");
                }

                // we don't allow an empty list. Could auto-assign "Unassigned", 
                // but let's force caller to be explicit instead.
                if (value.Count == 0)
                {
                    throw new ArgumentException("Empty list not allowed. Use Unassigned or Unknown or Not Applicable", "FloweringMonths");
                }

                // If present, Unassigned, Unknown and Not Applicable must be the ONLY value in list.
                if (value.Count > 1)
                {
                    if (value.Contains<SunType>(SunType.Unassigned) ||
                            value.Contains<SunType>(SunType.Unassigned) ||
                            value.Contains<SunType>(SunType.NotApplicable)
                        )
                    {
                        throw new ArgumentException("List cannot contain Unassigned or Unknown or NotApplicable AND another value.");
                    }
                }

                // All values in list must be valid
                foreach (SunType v in value)
                {
                    if (!Helpers.IsValidSunType(v))
                    {
                        throw new ArgumentException("Property not set. An item in list was not a valid SunType value: " + v.ToString());
                    }
                }

                _sunRequirements = value;
            }
        }

        // WateringRequirement
        public WateringType WateringRequirement
        {
            get { return _wateringRequirement; }

            set
            {
                if (Helpers.IsValidWateringType(value))
                {
                    _wateringRequirement = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WateringRequirement.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "WateringRequirement");
                }
            }
        }

        // CNPS_Drainage
        public CNPS_Drainage CNPS_Drainage
        {
            get { return _CNPS_Drainage; }

            set
            {
                if (Helpers.IsValidWateringType(value))
                {
                    _CNPS_Drainage = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("CNPS_Drainage.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "CNPS_Drainage");
                }
            }
        }

        // AttractsNativeBees
        public YesNoMaybe AttractsNativeBees
        {
            get { return _attractsNativeBees; }

            set
            {
                if (Helpers.IsValidAttractorOf(value))
                {
                    _attractsNativeBees = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AttractsButterflies.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "AttractsNativeBees");
                }
            }
        }

        // AttractsButterflies
        public YesNoMaybe AttractsButterflies
        {
            get { return _attractsButterflies; }

            set
            {
                if (Helpers.IsValidAttractorOf(value))
                {
                    _attractsButterflies = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AttractsButterflies.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "AttractsButterflies");
                }
            }
        }

        // AttractsBirds
        public YesNoMaybe AttractsBirds
        {
            get { return _attractsBirds; }

            set
            {
                if (Helpers.IsValidAttractorOf(value))
                {
                    _attractsBirds = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AttractsBirds.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "AttractsBirds");
                }
            }
        }

        // AttractsHummingbirds
        public YesNoMaybe AttractsHummingbirds
        {
            get { return _attractsHummingbirds; }

            set
            {
                if (Helpers.IsValidAttractorOf(value))
                {
                    _attractsHummingbirds = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AttractsHummingbirds.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "AttractsHummingbirds");
                }
            }
        }

        // County Stuff
            // Do not add or remove county properties without editing
            // the _countyName list. Do not edit the spelling of county names 
            // without also editing the values used in the county property Get/Set accessors
        private List<string> _countyNames = new List<string>()
        { "Alameda", "Contra Costa", "Marin", "Napa", "San Francisco", "San Mateo", "Santa Clara", "Solano", "Sonoma" };

        private Dictionary<string, YesNoMaybe> _countyDict =
            new Dictionary<string, YesNoMaybe>();

        private void InitializeCountyDictionary()       // will be called in object Initialize()
        {
            foreach (string s in _countyNames)
            {
                _countyDict.Add(s, YesNoMaybe.Unassigned);
            }
        }

        public YesNoMaybe NativeTo_Alameda
        {
            get { return _countyDict["Alameda"]; }

            set { SetNativeToCounty("Alameda", value); }
        }

        public YesNoMaybe NativeTo_Contra_Costa
        {
            get { return _countyDict["Contra Costa"]; }

            set { SetNativeToCounty("Contra Costa", value); }
        }

        public YesNoMaybe NativeTo_Marin
        {
            get { return _countyDict["Marin"]; }

            set { SetNativeToCounty("Marin", value); }
        }

        public YesNoMaybe NativeTo_Napa
        {
            get { return _countyDict["Napa"]; }

            set { SetNativeToCounty("Napa", value); }
        }

        public YesNoMaybe NativeTo_San_Francisco
        {
            get { return _countyDict["San Francisco"]; }

            set { SetNativeToCounty("San Francisco", value); }
        }

        public YesNoMaybe NativeTo_SanMateo
        {
            get { return _countyDict["San Mateo"]; }

            set { SetNativeToCounty("San Mateo", value); }
        }

        public YesNoMaybe NativeTo_Santa_Clara
        {
            get { return _countyDict["Santa Clara"]; }

            set { SetNativeToCounty("Santa Clara", value); }
        }

        public YesNoMaybe NativeTo_Solano
        {
            get { return _countyDict["Solano"]; }

            set { SetNativeToCounty("Solano", value); }
        }

        public YesNoMaybe NativeTo_Sonoma
        {
            get { return _countyDict["Sonoma"]; }

            set { SetNativeToCounty("Sonoma", value); }
        }

        private void SetNativeToCounty(string county, YesNoMaybe value)
        {
            if (Helpers.IsValidNativeToCounty(value))
            {
                _countyDict["v"] = value;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("SetNativeToCounty(): Not a valid enum value: " + value.ToString());
                throw new ArgumentException("Not a valid enum value: " + value.ToString(), "YesNoMaybe");
            }
        }

        // DocumentedAsGoodInContainers
        public YesNoMaybe DocumentedAsGoodInContainers
        {
            get { return _documentedAsGoodInContainers; }

            set
            {
                if (Helpers.IsValidAttractorOf(value))
                {
                    _documentedAsGoodInContainers = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DocumentedAsGoodInContainers.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "DocumentedAsGoodInContainers");
                }
            }
        }


        // debug
        public bool Debug_ContainsErrors
        {
            get; set;
        }

        public string CompilationOfErrors
        {
            get { return _compilationOfErrors; }

            set
            {
                _compilationOfErrors += value + Environment.NewLine;
            }
        }

        private string _compilationOfErrors = "";


        // Public fields
        // ---------------- //
        // Public fields can be directly read or written by the public,
        // with no validation or intervention on our part

        public string ScientificName;
        public string URL;
        public string Notes;
        public string CNPS_Soil;
        public string NotableVisuals;

        public decimal? MinHeight_Nullable;
        public decimal? MaxHeight_Nullable;
        public decimal? MinWidth_Nullable;
        public decimal? MaxWidth_Nullable;
        public decimal? MinRainfallInches_Nullable;
        public decimal? MaxRainfallInches_Nullable;
        public decimal? MinWinterTempF_Nullable;
        public decimal? MinSoilpH_Nullable;
        public decimal? MaxSoilpH_Nullable;

        // Here are private fields, that hold values which back
        // the public properties.
        private string _name;
        private PlantType _type;
        private List<FloweringMonth> _floweringMonths;  // multiple values allowed
        private List<SunType> _sunRequirements;
        private WateringType _wateringRequirement;
        private CNPS_Drainage _CNPS_Drainage;
        private YesNoMaybe _attractsNativeBees;
        private YesNoMaybe _attractsButterflies;
        private YesNoMaybe _attractsBirds;
        private YesNoMaybe _attractsHummingbirds;
        private YesNoMaybe _documentedAsGoodInContainers;

        // Mike: I recommend that you not try and overload the AttractsX fields
        // to also mean IsAHealthHazard. Leave them as YesNoMaybe, and have a separate
        // HealthHazardTo field, as below. It might make data entry slower, but the parsing code
        // and other logic is MUCH MUCH simpler.

        // Remember: "Debug Data, Not Code!"

        //enum HealthHazardTo { Unassigned, Unknown, Yes, No }
        //private HealthHazardTo _attractsButterflies;
        //private HealthHazardTo _birds;
        //private HealthHazardTo _hummingbirds;

        // Here are private methods which typically do the real work, 
        // as they are called by the public methods and properties.

        private void Initialize()
        {
            // do any work that needs to be done before using the public properties
            InitializeCountyDictionary();

            // public field types that require intializations
            URL = "<Unassigned>";
            Notes = "<Unassigned>";         
            CNPS_Soil = "<Unassigned>";     
            NotableVisuals = "<Unassigned>";
            ScientificName = "<Unassigned>";

            // use the public property setters to set the public properties
            Type = PlantType.Unassigned;
            WateringRequirement = WateringType.Unassigned;
            CNPS_Drainage = CNPS_Drainage.Unassigned;

            List<FloweringMonth> flowerMonths = new List<FloweringMonth>();
            flowerMonths.Add(FloweringMonth.Unassigned);
            FloweringMonths = flowerMonths;

            List<SunType> sunTypes = new List<SunType>();
            sunTypes.Add(SunType.Unassigned);
            SunRequirements = sunTypes;

            AttractsNativeBees = YesNoMaybe.Unassigned;
            AttractsBirds = YesNoMaybe.Unassigned;
            AttractsButterflies = YesNoMaybe.Unassigned;
            AttractsHummingbirds = YesNoMaybe.Unassigned;

            NativeTo_Alameda = YesNoMaybe.Unassigned;
            NativeTo_Contra_Costa = YesNoMaybe.Unassigned;
            NativeTo_Napa = YesNoMaybe.Unassigned;
            NativeTo_Santa_Clara = YesNoMaybe.Unassigned;
            NativeTo_San_Francisco = YesNoMaybe.Unassigned;
            NativeTo_SanMateo = YesNoMaybe.Unassigned;
            NativeTo_Solano = YesNoMaybe.Unassigned;
            NativeTo_Sonoma = YesNoMaybe.Unassigned;
            NativeTo_Marin = YesNoMaybe.Unassigned;

            DocumentedAsGoodInContainers = YesNoMaybe.Unassigned;
            

            // Nine decimal fields init automatically to zero
            // so not listed here.
        }

        // Static members

        // The value of a static field/property is shared across all instances
        // of an object. Any change to that value is seen by all instances of 
        // the object. To avoid concurrency issues, we use a "lock" when changing
        // any static value.
        private static int _instanceID = 0;
        private object IIDLock = new object();
        private int GetNewInstanceID()
        {
            lock (IIDLock)
            {
                _instanceID++;
                return _instanceID;
            }
        }

        // Update this when we change the format of the object
        public static VersionNumber VersionNumber
        {
            get { return new VersionNumber(1, 0, 0, 0); }
        }
    }
}
