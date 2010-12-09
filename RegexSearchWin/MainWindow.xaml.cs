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
using System.ComponentModel;

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
            RebuildIndex();
        }

        private void RebuildIndex()
        {
            rebuildIndexButton.IsEnabled = false;

            var worker = new BackgroundWorker();

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                args.Result = indexBuilder.BuildIndex();
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                this.index = args.Result as Index;

                //index.SetSearcher(new BinarySearcher());
                index.SetSearcher(new RegexSearcher());

                rebuildIndexButton.IsEnabled = true;
            };

            worker.RunWorkerAsync();
        }

        private string searchPattern = null;
        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchPattern = searchTextBox.Text;

            if (IsValidRegex(searchTextBox.Text))
            {
                searchTextBox.Background = Brushes.White;
                searchButton.IsEnabled = true;
            }
            else
            {
                searchTextBox.Background = Brushes.Pink;
                searchButton.IsEnabled = false;
            }
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            searchButton.IsEnabled = false;

            var worker = new BackgroundWorker();

            worker.DoWork += (object s, DoWorkEventArgs args) =>
            {
                var tempList = new List<ResultListViewItem>();

                var searchRegex = new Regex(searchPattern, RegexOptions.Compiled | RegexOptions.Singleline);

                var results =
                    index.Search(searchPattern);

                foreach (var result in results)
                {
                    tempList.Add(new ResultListViewItem() { FullPath = result });
                }

                args.Result = tempList;
            };

            worker.RunWorkerCompleted += (object s, RunWorkerCompletedEventArgs args) =>
            {
                if (args.Error != null)
                {
                    System.Windows.Forms.MessageBox.Show(args.Error.Message);
                }

                Results.Clear();

                if (args.Result != null)
                {
                    foreach (var results in args.Result as List<ResultListViewItem>)
                    {
                        Results.Add(results);
                    }
                }

                textBlock1.Text = string.Format("files: {0}", Results.Count);

                searchButton.IsEnabled = true;
            };

            worker.RunWorkerAsync();
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

            var dfw = new DisplayFileWindow(file, text, highLights);
            dfw.Title = "Matches for: " + searchTextBox.Text;
            dfw.Show();
        }

        private bool IsValidRegex(string pattern)
        {
            try
            {
                new Regex(pattern);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
