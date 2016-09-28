using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlantMan;

namespace PlantMan.Plants
{
    public class Plant
    {
        #region Constructors

        // Constructors
        public Plant(string Name)
        {
            // TODO: we should actually call an internal set for this, so that it goes
            // through validation...
            _name = Name;

            // now we set default values for some fields/properties
            Initialize();
        }

        // By default, the compiler will create a public parameterless constructor
        // behind our back. We don't want callers to use that constructor, so
        // we throw an exception.
        public Plant()
        {
            throw new InvalidOperationException("Use the other constructor.");
        }

        // Only called by the Constructor
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
            CNPS_Drainage = CNPS_DrainageType.Unassigned;
            SunRequirements = SunType.Unassigned;
            FloweringMonths = FloweringMonth.Unassigned;


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

        #endregion Constructors

        #region Public Properties and Fields

        // Our object has Public Properties and Public Fiels. We use Properties when
        // we want to control or react to access to a value. We use a field when we want
        // to allow unfettered access to a value.

        /// <summary>
        /// The Name of the plant represented by this object. Must not be blank.
        /// </summary>
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

        /// <summary>
        /// Type of plant (bush, fern, etc.)
        /// </summary>
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

        /// <summary>
        /// Months in which this plant can be expected to flower.
        /// </summary>
        public FloweringMonth FloweringMonths
        {
            get { return _floweringMonths; }

            set
            {
                if (!Helpers.IsValidFloweringMonthValue((int)value))
                {
                    throw new ArgumentException("Not a valid FloweringMonth value.", "FloweringMonths");
                }
                else
                {
                    _floweringMonths = value;
                }
            }
        }

        //// SunRequirements
        //public List<SunType> SunRequirements
        //{
        //    get
        //    {
        //        // Don't give them the actual list, give them a copy. We want to control
        //        // modifications to the list, so we force them to give us a whole new list
        //        // when they want to set this property. Alternative is to wrap list with
        //        // our own Add/Remove/Clear methods.
        //        List<SunType> k = _sunRequirements.ToList<SunType>();
        //        return k;
        //    }

        //    set
        //    {
        //        // Here the caller hands us an entire list.  We'll reject the list if
        //        // it contains any non-valid SunType values.
        //        if (value == null)
        //        {
        //            throw new ArgumentNullException("SunRequirements", "Value cannot be null.");
        //        }

        //        // we don't allow an empty list. Could auto-assign "Unassigned", 
        //        // but let's force caller to be explicit instead.
        //        if (value.Count == 0)
        //        {
        //            throw new ArgumentException("Empty list not allowed. Use Unassigned or Unknown or Not Applicable", "FloweringMonths");
        //        }

        //        // If present, Unassigned, Unknown and Not Applicable must be the ONLY value in list.
        //        if (value.Count > 1)
        //        {
        //            if (value.Contains<SunType>(SunType.Unassigned) ||
        //                    value.Contains<SunType>(SunType.Unassigned) ||
        //                    value.Contains<SunType>(SunType.NotApplicable)
        //                )
        //            {
        //                throw new ArgumentException("List cannot contain Unassigned or Unknown or NotApplicable AND another value.");
        //            }
        //        }

        //        // AllMonths values in list must be valid
        //        foreach (SunType v in value)
        //        {
        //            if (!Helpers.IsValidSunTypeValue((int)v))
        //            {
        //                throw new ArgumentException("Property not set. An item in list was not a valid SunType value: " + v.ToString());
        //            }
        //        }

        //        _sunRequirements = value;
        //    }
        //}

        // -- disabled until working
        /// <summary>
        /// 
        /// </summary>
        public SunType SunRequirements
        {
            get { return _sunRequirements; }

            set
            {
                if (!Helpers.IsValidSunTypeValue((int)value))
                {
                    throw new ArgumentException("Not a valid SunType value.", "SunRequirements");
                }
                else
                {
                    _sunRequirements = value;
                }
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
        public CNPS_DrainageType CNPS_Drainage
        {
            get { return _CNPS_Drainage; }

            set
            {
                if (Helpers.IsValidCNPS_DrainageValue((int)value))
                {
                    _CNPS_Drainage = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("_CNPS_Drainage.setter(): Not a valid enum value: " + value.ToString());
                    throw new ArgumentException("Not a valid enum value: " + value.ToString(), "_CNPS_Drainage");
                }
            }
        }

        // DocumentedAsGoodInContainers
        public YesNoMaybe DocumentedAsGoodInContainers
        {
            get { return _documentedAsGoodInContainers; }

            set
            {
                if (Helpers.IsValidYesNoMaybe(value))
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

        #region AttractsX

        // AttractsNativeBees
        public YesNoMaybe AttractsNativeBees
        {
            get { return _attractsNativeBees; }

            set
            {
                if (Helpers.IsValidYesNoMaybe(value))
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
                if (Helpers.IsValidYesNoMaybe(value))
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
                if (Helpers.IsValidYesNoMaybe(value))
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
                if (Helpers.IsValidYesNoMaybe(value))
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

        #endregion AttractsX

        #region County Stuff

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

        private void SetNativeToCounty(string county, YesNoMaybe value)
        {
            if (Helpers.IsValidYesNoMaybe(value))
            {
                _countyDict["v"] = value;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("SetNativeToCounty(): Not a valid enum value: " + value.ToString());
                throw new ArgumentException("Not a valid enum value: " + value.ToString(), "YesNoMaybe");
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

        #endregion County Stuff

        // Public fields
        // ---------------- //
        // Public fields can be directly read or written 
        // by the public, with no validation or intervention 
        // on our part.

        public string ScientificName;
        public string URL;
        public string Notes;
        public string CNPS_Soil;
        public string NotableVisuals;

        public DecimalLike MinHeight;
        public DecimalLike MaxHeight;
        public DecimalLike MinWidth;
        public DecimalLike MaxWidth;
        public DecimalLike MinRainfallInches;
        public DecimalLike MaxRainfallInches;
        public DecimalLike MinWinterTempF;
        public DecimalLike MinSoilpH;
        public DecimalLike MaxSoilpH;

        #endregion Public Properties and Fields

        #region Private Fields Which Back Public Properties

        // These are private fields, that hold the actual
        // values reported in public properties.
        private string _name;
        private PlantType _type;
        private FloweringMonth _floweringMonths;
        private SunType _sunRequirements;
        private WateringType _wateringRequirement;
        private CNPS_DrainageType _CNPS_Drainage;
        private YesNoMaybe _attractsNativeBees;
        private YesNoMaybe _attractsButterflies;
        private YesNoMaybe _attractsBirds;
        private YesNoMaybe _attractsHummingbirds;
        private YesNoMaybe _documentedAsGoodInContainers;

        #endregion Private Fields Which Back Public Properties

        /// <summary>
        /// A 'type' which accomodates our need to either hold a value, or indicate Unassigned / Unknown.
        /// Check HasNumericValue before operating on Value.
        /// </summary>
        public class DecimalLike
        {
            private decimal _value;
            private bool _isUnassigned;
            private bool _isUnknown;
            private bool _isNotApplicable;
            private bool _hasNumericValue;

            // HasValue will be false, Unassigned true, Unknown false
            public DecimalLike()
            {
                _isUnassigned = true;
            }

            // HasValue will be true, Unassigned false, Unknown false
            public DecimalLike(decimal Value)
            {
                _value = Value;
                _hasNumericValue = true;
                _isUnassigned = false;
                _isUnknown = true;
            }

            // HasValue false, Unassigned false, Unknown true
            public DecimalLike(bool Unknown)
            {
                _isUnknown = true;
            }

            // Value
            public decimal Value
            {
                get
                {
                    if (!_hasNumericValue)
                    {
                        throw new InvalidOperationException("HasNumericValue is false; there is no meaningful value here.");
                    }
                    else
                    {
                        return _value;
                    }
                }

                set
                {
                    _hasNumericValue = false;
                    _isUnknown = false;
                    _isNotApplicable = false;
                    _isUnassigned = true;
                    _value = value;
                }
            }

            public void SetValue(decimal value)
            {
                this.Value = value; 
            }

            public void SetUnassigned()
            {
                _value = decimal.Zero;
                _hasNumericValue = false;
                _isUnknown = false;
                _isUnassigned = true;
                _isNotApplicable = false;
            }

            public void SetUnknown()
            {
                _value = decimal.Zero;
                _hasNumericValue = false;
                _isUnknown = false;
                _isUnassigned = true;
            }

            public void SetNotApplicable()
            {
                _value = decimal.Zero;
                _hasNumericValue = false;
                _isUnknown = false;
                _isUnassigned = false;
                _isNotApplicable = true;
            }

            public bool HasNumericValue
            {
                get { return _hasNumericValue; }
            }

            public bool IsUnassigned
            {
                get { return _isUnassigned; }
            }

            public bool IsUnknown
            {
                get { return _isUnknown; }
            }

            public bool IsNotApplicable
            {
                get { return _isNotApplicable; }
            }

        } // class DecimalAndBeyond

    } // class Plant

} //
