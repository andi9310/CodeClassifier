#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using CodeClassifier.Classifiers;
using CodeClassifier.Classifiers.Bayess;
using CodeClassifier.Classifiers.KNN;
using Microsoft.Win32;

#endregion

namespace CodeClassifier
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const double Alpha = 1.0;
        private readonly List<IClassifier> _classifiers;
        public int ParsingProgress { get; set; } = 10;
        public int TeachingProgress { get; set; } = 0;
        private string[] _files;
        private bool _isPaused;
        private bool _isStopped;
        private long _lastParsed;
        private bool _isParsingInProgress;
        private string _name;
        private ConcurrentBag<KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>>> _learningSet;


        readonly object _progresslock = new object();

        public int ItemsToTeach { get; private set; }
        public int ItemsToTeachByClassifiers { get; private set; }

        //private double Progress { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _classifiers = new List<IClassifier> { new BayesianClassifier() };
            for (var i = 1; i <= 130; i *= 2)
            {
                _classifiers.Add(new KNearestNeighboursClassifier(i));
            }
            ClassifierSelector.ItemsSource = _classifiers;
            ClassifierSelector.SelectedIndex = 0;
            IsFileOpen = false;
        }


        public bool IsFileOpen { get; set; }

        [DllImport("Kernel32")]
        private static extern void AllocConsole();

        [DllImport("Kernel32")]
        private static extern void FreeConsole();

        /*
                public string DataBaseFileName { get; set; }
        */

        private void TeachClassifier(string author)
        {
            _learningSet = new ConcurrentBag<KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>>>();
            // _progress = 0.0;
            _isStopped = false;

            _name = author;
            var ofd = new OpenFileDialog { Multiselect = true };
            var result = ofd.ShowDialog();
            if (result != true) return;
            _files = ofd.FileNames;
            Dispatcher.Invoke(() => Pb.Value = 0);
            Dispatcher.Invoke(() => Pb2.Value = 0);
            Dispatcher.Invoke(() => Pb.Maximum = _files.Length);
            Dispatcher.Invoke(() => Pb2.Maximum = Pb.Maximum * _classifiers.Count);
            ItemsToTeach = _files.Length;
            ItemsToTeachByClassifiers = ItemsToTeach * _classifiers.Count;
            _isParsingInProgress = true;
            var parsingLoopResult = ParseAll();
            _lastParsed += parsingLoopResult.LowestBreakIteration ?? (ItemsToTeach - _lastParsed);
            _isParsingInProgress = false;
            if (_lastParsed == ItemsToTeach)
            {
                TeachAll();
            }
        }

        private void TeachAll()
        {

            Parallel.ForEach(_classifiers, classifier =>
            {
                foreach (var keyValuePair in _learningSet)
                {
                    classifier.Teach(keyValuePair);
                    lock (_progresslock)
                    {
                        Dispatcher.Invoke(() => Pb2.Value++);

                    }
                }
            });
        }

        private ParallelLoopResult ParseAll()
        {
            var x =  Parallel.ForEach
                (
                    _files.Skip((int)_lastParsed),
                    (file, state) =>
                    {
                        if (_isPaused)
                        {
                            state.Break();
                        }
                        if (_isStopped)
                        {
                            state.Stop();
                        }
                        else
                        {
                            using (var sr = new StreamReader(File.Open(file, FileMode.Open)))
                            {
                                var parser = new Parser(sr);
                                _learningSet.Add(new KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>>(_name, parser.Parse()));

                            }
                            lock (_progresslock)
                            {
                                Dispatcher.Invoke(() => Pb.Value++);
                            }
                        }

                    });
            return x;
        }

        private async void Browse_Click(object sender, RoutedEventArgs e)
        {
            DoneLabel.Visibility = Visibility.Collapsed;
            var author = AuthorBox.Text;
            var task = new Task(() => TeachClassifier(author));

            task.Start();
            await task;

            DoneLabel.Visibility = Visibility.Visible;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            var result = ofd.ShowDialog();
            if (result != true) return;
            using (var sr = new StreamReader(ofd.OpenFile()))
            {
                var p = new Parser(sr);
                var item = ClassifierSelector.SelectionBoxItem as IClassifier;
                if (item != null)
                {
                    ResultLabel.Content = item.Classify(p.Parse());
                }
            }
        }

        private void TextModeButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            AllocConsole();


            var folder = Console.ReadLine();
            FileStream ostrm;
            StreamWriter writer;
            var oldOut = Console.Out;
            try
            {
                ostrm = new FileStream("./Redirect.txt", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Cannot open Redirect.txt for writing");
                Console.WriteLine(ex.Message);
                return;
            }
            Console.SetOut(writer);

            if (folder != null)
            {
                var di = new DirectoryInfo(folder);
                var classifiersResults = new Dictionary<string, List<double>>();
                var programmersResultAllTests = new List<Dictionary<string, List<KeyValuePair<string, double>>>>();
                for (var testNum = 0; testNum < 1; testNum++)
                {
                    var random = new Random();
                    var learningSet =
                        new ConcurrentBag<KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>>>();
                    var testSet = new ConcurrentBag<KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>>>();

                    foreach (var programmerFolder in di.EnumerateDirectories())
                    {
                        var list = programmerFolder.EnumerateFiles().OrderBy(i => random.Next()).ToList();

                        var folder1 = programmerFolder;
                        Parallel.ForEach(list.Take((int)(list.Count * 0.7)), file =>
                        {
                            using (var sr = new StreamReader(file.Open(FileMode.Open)))
                            {
                                learningSet.Add(
                                    new KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>>(
                                        folder1.Name, new Parser(sr).Parse()));
                            }
                        });

                        var programmerFolder1 = programmerFolder;
                        Parallel.ForEach(list.Skip((int)(list.Count * 0.7)), file =>
                        {
                            using (var sr = new StreamReader(file.Open(FileMode.Open)))
                            {
                                testSet.Add(
                                    new KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>>(
                                        programmerFolder1.Name, new Parser(sr).Parse()));
                            }
                        });
                    }
                    Parallel.ForEach(_classifiers, classifier =>
                    {
                        foreach (var keyValuePair in learningSet)
                        {
                            classifier.Teach(keyValuePair);
                        }
                    });

                    var resultsNumbersDictionary =
                        new Dictionary<string, Dictionary<string, ResultsInfo>>();
                    foreach (var classifier in _classifiers)
                    {

                        resultsNumbersDictionary.Add(classifier.ToString(),
                            new Dictionary<string, ResultsInfo>());

                        foreach (var directory in di.EnumerateDirectories())
                        {
                            resultsNumbersDictionary[classifier.ToString()].Add(directory.Name,
                                new ResultsInfo());
                        }

                    }

                    foreach (var classifier in _classifiers)
                    {
                        foreach (var test in testSet)
                        {
                            var s = classifier.Classify(test.Value);
                            if (s == test.Key)
                            {
                                resultsNumbersDictionary[classifier.ToString()][s].Relevant++;
                                resultsNumbersDictionary[classifier.ToString()][s].RelevantRetrieved++;
                                resultsNumbersDictionary[classifier.ToString()][s].Retrieved++;
                            }
                            else
                            {
                                resultsNumbersDictionary[classifier.ToString()][test.Key].Relevant++;
                                resultsNumbersDictionary[classifier.ToString()][s].Retrieved++;
                            }
                            //  Console.WriteLine("test skonczony");
                        }
                        Console.Error.WriteLine("klasifajer skonczony");

                    }
                    var programmersResults = new Dictionary<string, List<KeyValuePair<string, double>>>();
                    foreach (var classifier in resultsNumbersDictionary)
                    {
                        var listF = (from result in classifier.Value
                                     let precision = result.Value.RelevantRetrieved / (double)result.Value.Retrieved
                                     let recall = result.Value.RelevantRetrieved / (double)result.Value.Relevant
                                     select
                                         new KeyValuePair<string, double>(result.Key,
                                             result.Value.Retrieved != 0
                                                 ? (1 + Alpha) * precision * recall / (Alpha * precision + recall)
                                                 : 0)).ToList();
                        try
                        {
                            classifiersResults[classifier.Key].Add(listF.Select(i => i.Value).Mean());
                        }
                        catch (KeyNotFoundException)
                        {
                            classifiersResults.Add(classifier.Key, new List<double> { listF.Select(i => i.Value).Mean() });
                        }
                        try
                        {
                            programmersResults[classifier.Key] = listF;
                        }
                        catch (KeyNotFoundException)
                        {
                            programmersResults.Add(classifier.Key, listF);
                        }
                    }
                    programmersResultAllTests.Add(programmersResults);
                }
                foreach (var classifiersResult in classifiersResults)
                {
                    var result = classifiersResult;
                    var x = programmersResultAllTests.Select(i => i[result.Key]);
                    var byProgrammers = new Dictionary<string, List<double>>();
                    foreach (var programmer in x.SelectMany(test => test))
                    {
                        try
                        {
                            byProgrammers[programmer.Key].Add(programmer.Value);
                        }
                        catch (KeyNotFoundException)
                        {
                            byProgrammers.Add(programmer.Key, new List<double> { programmer.Value });
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine(@"{0} {1}", classifiersResult.Key, classifiersResult.Value.Mean());
                    foreach (var programmer in byProgrammers)
                    {
                        Console.WriteLine(@"{0} {1}", programmer.Key, programmer.Value.Mean());
                    }
                }
            }
            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
            FreeConsole();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isParsingInProgress)
            {
                _isPaused = true;
            }
        }

        private async void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isPaused) return;
            var task = new Task(() =>
            {
                _isPaused = false;
                _isParsingInProgress = true;
                var parsingLoopResult = ParseAll();
                _lastParsed += parsingLoopResult.LowestBreakIteration ?? (ItemsToTeach - _lastParsed);
                _isParsingInProgress = false;
                if (_lastParsed != ItemsToTeach) return;
                TeachAll();
                _lastParsed = 0;
            });

            task.Start();
            await task;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isParsingInProgress)
            {
                _isStopped = true;
            }
        }
    }
}