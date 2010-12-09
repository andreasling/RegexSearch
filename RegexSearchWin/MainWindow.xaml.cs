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
                var indexBuilder = args.Argument as IndexBuilder;
                args.Result = indexBuilder.BuildIndex();
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                this.index = args.Result as Index;

                //index.SetSearcher(new BinarySearcher());
                index.SetSearcher(new RegexSearcher());

                rebuildIndexButton.IsEnabled = true;
            };

            try
            {
                var regex = new Regex(indexPatternTextBox.Text, RegexOptions.Compiled | RegexOptions.Singleline);
                var ib = new RegexIndexBuilder(folderPathTextBox.Text, filePatternTextBox.Text, regex);
                worker.RunWorkerAsync(ib);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);

                rebuildIndexButton.IsEnabled = true;
            }
        }

        private string searchPattern = string.Empty;
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
                searchButton.Background = Brushes.LightGreen;
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

        private void browseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = "select folder to index", 
                    SelectedPath = folderPathTextBox.Text, 
                    ShowNewFolderButton = false
                };

            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderPathTextBox.Text = folderBrowser.SelectedPath;
            }
        }

        private void AutoIndexButton_Click(object sender, RoutedEventArgs e)
        {
            var worker = new BackgroundWorker();

            worker.DoWork += (s, args) => 
            {
                var ps = args.Argument as string[];

                FileSystemWatcher watcher = new FileSystemWatcher(ps[0], ps[1])
                {
                    IncludeSubdirectories = true, 
                    NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite
                };

                while (true)
                {
                    var results = watcher.WaitForChanged(WatcherChangeTypes.All, 1000);

                    if (!results.TimedOut)
                    {
                        System.Diagnostics.Debug.WriteLine("watcher.WaitForChanged({0}): {1} -> {2}", results.ChangeType, results.OldName, results.Name);

                        this.Dispatcher.Invoke((Action)(() => 
                        {
                            RebuildIndex();
                            searchButton.Background = Brushes.Pink;
                        }));
                    }
                }
            };

            worker.RunWorkerAsync(new[] {folderPathTextBox.Text, filePatternTextBox.Text});
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
