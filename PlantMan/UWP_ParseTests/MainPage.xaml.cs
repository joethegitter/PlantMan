﻿using System;
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
using System.Reflection;

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

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ParseResourceIntoTextBox();
        }

        private void ParseResourceIntoTextBox()
        {
            Assembly myAss = this.GetType().GetTypeInfo().Assembly;
            string myPath = "UWP_ParseTests.Assets.PlantsFixed.csv";
            PlantImporter pi = new PlantImporter(myAss, myPath);
            pi.DebugOutput = PlantImporter.TDO.None;
            List<Plant> myPlants = pi.GetPlantList();

            // at this point, if you know how to make the UI work
            // just loop through the plants, spitting out Name + ScientificName
        }
    }
}
