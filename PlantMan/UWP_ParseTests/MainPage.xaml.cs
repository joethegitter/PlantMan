using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

using PlantMan.Plants;
using CSVtoPlant;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWP_ParseTests
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Because I have no idea how to manipulate page objects
            // in UWP, we're going to treat this pretty much as a 
            // console project. I'll call my code here, while still
            // in the page constructor.


        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ParseResourceIntoTextBox()
        {
            // Create a Dict of Plants
            Dictionary<string, Plant> PlantDict = new Dictionary<string, Plant>();
            
           

        }
    }
}
