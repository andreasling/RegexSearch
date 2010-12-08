using System;
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
            InitializeComponent();

            Results = new ObservableCollection<ResultListViewItem>();
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
            Results.Clear();
            searchTextBox.Background = Brushes.White;

            if (string.IsNullOrEmpty(searchTextBox.Text))
            {
                return;
            }

            try
            {
                var regex = new Regex(searchTextBox.Text);

                var results =
                    //index.BinarySearch(searchTextBox.Text);
                    index.RegexSearch(regex);

                foreach (var r in results)
                {
                    Results.Add(new ResultListViewItem() { FullPath = r });
                }
            }
            catch (ArgumentException)
            {
                searchTextBox.Background = Brushes.Pink;
            }
        }

        private void resultsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int index = resultsListView.SelectedIndex;

            var file = Results[index].FullPath;

            var text = File.ReadAllText(file);

            var highLights =
                from Match m in Regex.Matches(text, searchTextBox.Text)
                select new HighLight() { Start = m.Index, Count = m.Length };

            var dfw = new DisplayFileWindow(file, text, highLights);
            dfw.Title = "Matches for: " + searchTextBox.Text;
            dfw.Show();
        }
    }
}
