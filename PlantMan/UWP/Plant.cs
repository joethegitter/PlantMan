using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants
{
    class Plant
    {
        // Constructors
        public Plant(string Name)
        {
            // instead of assigning value to the private field _name,
            // we set the public property, which causes validation
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
            // The Name Property is read only; it is set at creation in the constructor,
            // because we require any Plant object to have a name. Later, when trying
            // to add a Plant to our database, Name will be our "key" property which
            // enforces uniqueness between records.

            get { return _name; }
        }

        // I am leaving ScientificName below as an example of how to convert public
        // fields to properties. Eventually you really do want to enforce these;
        // but for now I'll provide a ScientificName public field, which you can
        // remove when you convert to property.

        //// ScientificName
        //public string ScientificName
        //{
        //    // We do not allow a blank value to be "set";
        //    // to allow "under construction" records and protect ourselves,
        //    // we will initalize this value in the constructor call to Initialize()

        //    get { return _scientificName; }

        //    set
        //    {
        //        if (!String.IsNullOrEmpty(value) && !String.IsNullOrWhiteSpace(value))
        //        {
        //            _scientificName = value;
        //        }
        //        else
        //        {
        //            throw new ArgumentException("Property cannot be set to null or empty or whitespace", "ScientificName");
        //        }
        //    }
        //}

        // Type
        public PlantType Type
        {
            get { return _type; }

            set
            {
                if (Enums.IsValidPlantType(value))
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
                    if (!Enums.IsValidFloweringMonth(v))
                    {
                        throw new ArgumentException("Property not set. An item in list was not a valid FloweringMonth value: " + v.ToString());
                    }
                }

                _floweringMonths = value;
            }
        }

        // SunRequirement
        public SunType SunRequirement
        {
            get { return _sunRequirement; }

            set
            {
                if (Enums.IsValidSunType(value))
                {
                    _sunRequirement = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SunRequirement.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "SunType");
                }
            }
        }

        // WateringRequirement
        public WateringType WateringRequirement
        {
            get { return _wateringRequirement; }

            set
            {
                if (Enums.IsValidWateringType(value))
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
                if (Enums.IsValidWateringType(value))
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

        // Attracts_Butterflies
        public AttractorOf Attracts_Butterflies
        {
            get { return _attractsButterflies; }

            set
            {
                if (Enums.IsValidAttractorOf(value))
                {
                    _attractsButterflies = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Attracts_Butterflies.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "Attracts_Butterflies");
                }
            }
        }

        // Attracts_Birds
        public AttractorOf Attracts_Birds
        {
            get { return _attractsBirds; }

            set
            {
                if (Enums.IsValidAttractorOf(value))
                {
                    _attractsBirds = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Attracts_Birds.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "Attracts_Birds");
                }
            }
        }

        // Attracts_Hummingbirds
        public AttractorOf Attracts_Hummingbirds
        {
            get { return _attractsHummingbirds; }

            set
            {
                if (Enums.IsValidAttractorOf(value))
                {
                    _attractsHummingbirds = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Attracts_Hummingbirds.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "Attracts_Hummingbirds");
                }
            }
        }

        // NativeToCounty Stuff
            // Do not add or remove NativeToCounty properties without editing
            // the _countyName list. Do not edit the spelling of county names 
            // without also editing the value passed by each NativeToCounty property.
        private List<string> _countyNames = new List<string>()
        { "Alameda", "ContraCosta", "Marin", "Napa", "SanFrancisco", "SanMateo", "SantaClara", "Solano", "Sonoma" };

        private Dictionary<string, NativeToCounty> _countyDict =
            new Dictionary<string, NativeToCounty>();

        private void InitializeCountyDictionary()       // will be called in object Initialize()
        {
            foreach (string s in _countyNames)
            {
                _countyDict.Add(s, NativeToCounty.Unassigned);
            }
        }

        public NativeToCounty NativeTo_Alameda
        {
            get { return _countyDict["Alameda"]; }

            set { SetNativeToCounty("Alameda", value); }
        }

        public NativeToCounty NativeTo_Contra_Costa
        {
            get { return _countyDict["ContraCosta"]; }

            set { SetNativeToCounty("ContraCosta", value); }
        }

        public NativeToCounty NativeTo_Marin
        {
            get { return _countyDict["Marin"]; }

            set { SetNativeToCounty("Marin", value); }
        }

        public NativeToCounty NativeTo_Napa
        {
            get { return _countyDict["Napa"]; }

            set { SetNativeToCounty("Napa", value); }
        }

        public NativeToCounty NativeTo_SanFrancisco
        {
            get { return _countyDict["SanFrancisco"]; }

            set { SetNativeToCounty("SanFrancisco", value); }
        }

        public NativeToCounty NativeTo_SanMateo
        {
            get { return _countyDict["SanMateo"]; }

            set { SetNativeToCounty("SanMateo", value); }
        }

        public NativeToCounty NativeTo_Santa_Clara
        {
            get { return _countyDict["SantaClara"]; }

            set { SetNativeToCounty("SantaClara", value); }
        }

        public NativeToCounty NativeTo_Solano
        {
            get { return _countyDict["Solano"]; }

            set { SetNativeToCounty("Solano", value); }
        }

        public NativeToCounty NativeTo_Sonoma
        {
            get { return _countyDict["Sonoma"]; }

            set { SetNativeToCounty("Sonoma", value); }
        }

        private void SetNativeToCounty(string county, NativeToCounty value)
        {
            if (Enums.IsValidNativeToCounty(value))
            {
                _countyDict["v"] = value;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("SetNativeToCounty(): Not a valid enum value: " + value.ToString());
                throw new ArgumentException("Not a valid enum value: " + value.ToString(), "NativeToCounty");
            }
        }

        // Public fields
        // ---------------- //
        // Public fields can be directly read or written by the public,
        // with no validation or intervention on our part

        // TODO: thes should be turned into properties, with enforcment. My opinion.
        public string ScientificName;
        public string URL;
        public string Notes;
        public string CNPS_Soil;
        public string NotableVisual;

        public decimal MinHeight;
        public decimal MaxHeight;
        public decimal MinWidth;
        public decimal MaxWidth;
        public decimal MinRainfallInches;
        public decimal MaxRainfallInches;
        public decimal MinWinterTempF;
        public decimal MinSoilpH;
        public decimal MaxSoilpH;
        public bool DocumentedAsGoodInContainers;

        // Here are private fields, that hold values which back
        // the public properties.
        private string _name;
        private PlantType _type;
        private List<FloweringMonth> _floweringMonths;  // multiple values allowed
        private SunType _sunRequirement;
        private WateringType _wateringRequirement;
        private CNPS_Drainage _CNPS_Drainage;
        private AttractorOf _attractsButterflies;
        private AttractorOf _attractsBirds;
        private AttractorOf _attractsHummingbirds;

        // Mike: I would not overload AttractorOf to also mean HealthHazardTo.
        // Instead, maintain separate values...

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
            NotableVisual = "<Unassigned>";
            ScientificName = "<Unassigned>";

            // use the public property setters to set the public properties
            Type = PlantType.Unassigned;
            SunRequirement = SunType.Unassigned;
            WateringRequirement = WateringType.Unassigned;
            CNPS_Drainage = CNPS_Drainage.Unassigned;

            FloweringMonths = new List<FloweringMonth>();
            FloweringMonths.Add(FloweringMonth.Unassigned);

            Attracts_Birds = AttractorOf.Unassigned;
            Attracts_Butterflies = AttractorOf.Unassigned;
            Attracts_Hummingbirds = AttractorOf.Unassigned;

            NativeTo_Alameda = NativeToCounty.Unassigned;
            NativeTo_Contra_Costa = NativeToCounty.Unassigned;
            NativeTo_Napa = NativeToCounty.Unassigned;
            NativeTo_Santa_Clara = NativeToCounty.Unassigned;
            NativeTo_SanFrancisco = NativeToCounty.Unassigned;
            NativeTo_SanMateo = NativeToCounty.Unassigned;
            NativeTo_Solano = NativeToCounty.Unassigned;
            NativeTo_Sonoma = NativeToCounty.Unassigned;
            NativeTo_Marin = NativeToCounty.Unassigned;
        }

    }
}
