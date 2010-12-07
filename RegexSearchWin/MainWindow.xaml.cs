﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RegexSearch;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.IO;

namespace RegexSearchWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IndexBuilder indexBuilder = null;
        private Index index = Index.Empty;

        public ObservableCollection<ResultListViewItem> Results { get; set; }

        public MainWindow()
        {
            Results = new ObservableCollection<ResultListViewItem>();
            InitializeComponent();

            DataContext = this;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var path =
                //@"C:\Treserva\Dev\Prestanda1\S3\Arende\Source\Process";
                //@"C:\Treserva\Dev\Prestanda1\S3\Arende\Source\";
                //@"C:\Treserva\Dev\Prestanda1\S3\";
                @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\";
            var pattern = "*.cs";

            var regex = new Regex(
                //@"(?:(?<=(?:class|namespace)\s+?)[^\s:{]+?(?=[\s:{]))|(?:())", 
                //"class", 
                    @"(?:(?<=(?:class|namespace)\s+?)(?<identifier>[a-zA-Z_][\w_]*?)(?=[\s:{]))|(?:(?<=(?<before>[{};]\s*?)(?<type>(?:[a-zA-Z_][\w_]*?\.)*?(?:[a-zA-Z_][\w_]*?)(?:\<[^\>]+?\>)?)\s+?)(?<identifier>[a-zA-Z_][\w_]*?)(?=(?<after>\s*?[;=])))",
                    RegexOptions.Compiled | RegexOptions.Singleline);

            indexBuilder = new RegexIndexBuilder(path, pattern, regex);
        }

        private void rebuildIndexButton_Click(object sender, RoutedEventArgs e)
        {
            this.index = indexBuilder.BuildIndex();
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTextBox.Text))
                {
                    Results.Clear();
                    return;
                }

                var regex = new Regex(searchTextBox.Text, RegexOptions.Compiled | RegexOptions.Singleline);

                searchTextBox.Background = Brushes.White;

                var results =
                    //index.BinarySearch(searchTextBox.Text);
                    index.RegexSearch(regex);

                //resultsListView.Items.Clear();

                //foreach (var item in results)
                //{

                //    resultsListView.Items.Add(

                //        new { File = System.IO.Path.GetFileName(item), Path = System.IO.Path.GetDirectoryName(item) });		 
                //}

                //Results = new ObservableCollection<ResultListViewItem>(
                //    from r in results select new ResultListViewItem() { FullPath = r });
                Results.Clear();
                foreach (var r in results)
                {
                    Results.Add(new ResultListViewItem() { FullPath = r });
                }
            }
            catch (ArgumentException)
            {
                Results.Clear();
                searchTextBox.Background = Brushes.LightPink;
            }
        }

        private void resultsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int index = resultsListView.SelectedIndex;

            if (index < 0)
            {
                return;
            }

            var file = Results[index].FullPath;

            var text = File.ReadAllText(file);

            var highLights =
                from Match m in Regex.Matches(text, searchTextBox.Text)
                select new HighLight() { Start = m.Index, Count = m.Length };

            new DisplayFileWindow(text, highLights).Show();
        }
    }
}
