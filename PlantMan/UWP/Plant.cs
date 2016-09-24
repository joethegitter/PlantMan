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
            this.Name = Name;

            // now we set default values for all fields/properties
            Initialize();
        }

        // By default, compiler will create a public parameterless constructor.
        // We don't want that, so we block that creation with our own.
        public Plant()
        {
            this.Name = "<ERROR! UNITITALIZED!>";
        }



        // A method for validating Enums
        public static bool IsValidFloweringMonth(typeof(FloweringMonth) x)
        {
            // TODO: write this - find that method
            return true;
        }

        // When parsing the CSV file, use these values to recognize meaning
        public static class ValInCsv
        {
            public static string[] Unknown = { "?" };
            public static string[] Yes = { "Y", "y", "Yes", "yes", "x" };
            public static string[] No = { "N", "n", "No", "no", "<NullAsString>" };
            public static string[] Unassigned = { "<NullAsString>" };
        }

        // Public fields - these elements can be read or written directly by the public

        // Name is read only; it is set at creation in the constructor, and 
        // our database will assume its value is unique
        public string Name
        {
            get { return _name; }

            internal set  // internal, so Name can only be set from within the object code itself
            {
                if (!String.IsNullOrEmpty(value) && !String.IsNullOrWhiteSpace(value))
                {
                    _name = value;
                }
                else
                {
                    throw new ArgumentException("Property cannot be set to null or empty or whitespace", "Name");
                }
            }
        }

        // ScientificName
        // We force a value here; for "under construction" records,
        // parser should put a placeholder name in, like "Unassigned"
        public string ScientificName
        {
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

        // URL
        // We force a value here; for "under construction" records,
        // parser should put a placeholder name in, like "Unassigned"
        public string URL
        {
            get { return _URL; }

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

        // Type
        public PlantType Type
        {
            get { return _type; }

            set
            {
                PlantType x = PlantType.Bush;
                if (IsValidEnum(x,value))
                {
                    _type = value;
                }
                else
                {
                    throw new ArgumentException("Not a valid PlantType value.", "PlantType");
                }
            }
        }

        // Height and Width
        public decimal MinHeight
        {
            get { return _minHeight; }
            set { _minHeight = value; }
        }

        public decimal MaxHeight
        {
            get { return _maxHeight; }
            set { _maxHeight = value; }
        }

        public decimal MinWidth
        {
            get { return _minWidth; }
            set { _minWidth = value; }
        }

        public decimal MaxWidth
        {
            get { return _maxWidth; }
            set { _maxWidth = value; }
        }

        // Flowering months
        public IReadOnlyList<FloweringMonth> FloweringMonths
        {
            get
            {
                // We store the values which make up this property in a list. We
                // don't want to hand that list to the caller, though, because then
                // they will be able to edit the list without us knowing it. So we
                // will instead hand them a ReadOnly reference to that list. When they
                // want to change the contents of the list, they can get this property,
                // create a new list, copy over each item (as they want), add or subtract
                // items from that list, and then use the Set accessor to replace the 
                // entire list that we have stored.
                IReadOnlyList<FloweringMonth> k = _floweringMonths;
                return k;
            }

            set
            {
                // Here the caller hands us an entire list.  We'll reject the list if
                // it contains any non-valid FloweringMonthvalues.

                JOE this is where we are
                if (IsValidEnum(x, value))
                {
                    _type = value;
                }
                else
                {
                    throw new ArgumentException("Not a valid PlantType value.", "PlantType");
                }
            }
        }


        // Here are private fields, that hold values which can only be internally accessed
        private string _name;
        private string _URL;
        private string _scientificName;
        private PlantType _type;
        private decimal _minHeight;
        private decimal _maxHeight;
        private decimal _minWidth;
        private decimal _maxWidth;
        private List<FloweringMonth> _floweringMonths;
        private List<SunType> _sun;
        private List<WateringType> _wateringRequirement;
        private decimal _minRainfallInches;
        private decimal _maxRainfallInches;
        private string _notableVisuals;
        private decimal _minWinterTempF;
        private string _CNPS_Soil;
        private decimal _minSoilpH;
        private decimal _maxSoilpH;
        private CNPS_Drainage _CNPS_Drainage;
        private NativeToCounty _Alameda;
        private NativeToCounty _Contra_Costa;
        private NativeToCounty _Marin;
        private NativeToCounty _Napa;
        private NativeToCounty _San_Francisco;
        private NativeToCounty _San_Mateo;
        private NativeToCounty _Santa_Clara;
        private NativeToCounty _Solano;
        private NativeToCounty _Sonoma;
        enum AttractorOf { Unassigned, Unknown, Yes, No }
        private AttractorOf _butterflies;
        private AttractorOf _birds;
        private AttractorOf _hummingbirds;
        private string _notes;
        private bool _documentedAsGoodInContainers;

        // I would not overload AttractorOf to also mean HealthHazardTo. Instead, sep vals:
        //enum HealthHazardTo { Unassigned, Unknown, Yes, No }
        //private HealthHazardTo _butterflies;
        //private HealthHazardTo _birds;
        //private HealthHazardTo _hummingbirds;




        // Here are the constructors, which are called when somebody does "new Plant()"



        // Here are publically accessible accessors (get/set properties)


        // Here are publically callable methods



        // Here are private methods which typically do the real work, 
        // as they are called by the public methods and properties.
        private void Initialize()
        {
            throw new NotImplementedException();
        }


    }
}
