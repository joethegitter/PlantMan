using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using PlantMan;
using PlantMan.Plants;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWP_TestBed
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Basically treating this like a console app; just going to do work,
            // send it out through Debug.WriteLine, then stop doing anything

            // load up the database, either from our local store, or
            // from newer versions of the data on the net
            LoadUpTheDataStoreWouldYouJeeves();
        }

        async private void LoadUpTheDataStoreWouldYouJeeves()
        {
            // Give me a string to the latest version of the data, please.
            // ---------------------------------------------------------- //
            // (In reality, we would read the versionNumber of our "current" data
            // from our local storage, but for now, we'll just fake that),
            VersionNumber localV = new VersionNumber(0, 0, 0, 0);

            // This method handles all server calls, makes decisions, and
            // returns to us a string with the path and name of the resource
            // where the "new" data is now stored. We actually feed it the
            // path and name to the current resource, so that it can feed that
            // back to us if it is the most current. This removes logic on our
            // end.
            string resourceName = await Utils.DownloadNewUpdateIfWeNeedIt(localV, "CSVData.PlantsFixed.csv");
            // ---------------------------------------------------------- //

            // Now parse that data into our "database", if you would my good man.

            // We need to pass "this" here ("this" being "the application object currently running",
            // in this case) because our utility code will need to know which assembly to in for the
            // specified resource.
            Utils.ParseDataIntoStore(this, resourceName, true);
        }
    }
}
