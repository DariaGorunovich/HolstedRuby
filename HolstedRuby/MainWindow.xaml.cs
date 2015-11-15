using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace HolstedRuby
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _sourceText = "";
        private readonly SortedSet<string> _uniqueOperands = new SortedSet<string>();
        private readonly SortedSet<string> _uniqueOperators = new SortedSet<string>();
        private int _operatorsCounter = 0, _operandsCounter = 0;
        private List<string> _symbols = new List<string>() {"<=", ">=", "==","!=","\\\"", "\\'","\\=", "\\.", "\\+", "\\-", "\\*", "\\/", "\\#", "\\,", "\\(", "\\)", "\\[", "\\]"};

        private SortedSet<string> _keyWords = new SortedSet<string>()
        {
            "def",
            "to_s",
            "to_i",
            "puts",
            "length",
            "end",
            "gets",
            "strip",
            "join",
            "while",
            "puts"
        };

        public MainWindow()
        {
            InitializeComponent();

        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                InitialDirectory = @"W:\VisualSt\MSiSInf\Laba1\HolstedRuby\",
                Title = "Open File"
            };
            if (ofd.ShowDialog(Holsted) != true) return;
            var source = new StreamReader(ofd.OpenFile());
            Code.Text = source.ReadToEnd();
            source.Close();
        }

        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            LabelforAnalyze.Content = "";
            _sourceText = Code.Text;
            

            var regquotes = new Regex("\"(\"|[^\"]*)\"");                                // реджекс для литерала(ковычки)
            var qmatches = regquotes.Match(_sourceText);
            while (qmatches.Success)
            {
                _sourceText = _sourceText.Substring(0, qmatches.Index + 1) + " " +
                             _sourceText.Substring(qmatches.Index + qmatches.Length - 1);
                qmatches = regquotes.Match(_sourceText, qmatches.Index + "\" \"".Length);
            }

            var regonesquotes = new Regex("\'(\'|[^\']*)\'");                           // реджекс для литерала(одинарные ковычки)
            var qonesmatches = regonesquotes.Match(_sourceText);
            while (qonesmatches.Success)
            {
                _sourceText = _sourceText.Substring(0, qonesmatches.Index + 1) + " " +
                             _sourceText.Substring(qonesmatches.Index + qonesmatches.Length - 1);
                qonesmatches = regonesquotes.Match(_sourceText, qonesmatches.Index + "\" \"".Length);
            }

            var regsharp = new Regex(@"(#.+)");                                          //реджекс для #    
            var sharpmatches = regsharp.Match(_sourceText);
            while (sharpmatches.Success)
            {
                _sourceText = _sourceText.Substring(0, sharpmatches.Index + 1) + _sourceText.Substring(sharpmatches.Index + sharpmatches.Length);
                sharpmatches = regsharp.Match(_sourceText, sharpmatches.Index + "#".Length);
            }
           
            foreach (var element in _symbols)                                            //замена на пробелы всех символов 
            {
                var elementsreg = new Regex(element);
                var elementmatches = elementsreg.Match(_sourceText);
                if (elementmatches.Success)
                    _uniqueOperators.Add(element);
                while (elementmatches.Success)
                {
                    _operatorsCounter++;
                    _sourceText = _sourceText.Substring(0, elementmatches.Index) + " " +
                                  _sourceText.Substring(elementmatches.Index + elementmatches.Length);
                    elementmatches = elementsreg.Match(_sourceText);
                }
            }

            var proc = false;
            var regword = new Regex(@"\b\w+\b");
            var wordmatches = regword.Matches(_sourceText);
            foreach (Match word in wordmatches)
            {
                if (_keyWords.Contains(word.Value) || proc)
                {
                    if (!_keyWords.Contains(word.Value)) _keyWords.Add(word.Value);
                    _operatorsCounter++;
                    _uniqueOperators.Add(word.Value);
                    proc = false;
                }
                else
                {
                    _operandsCounter++;
                    _uniqueOperands.Add(word.Value);
                }
                if (word.Value == "def")
                {
                    proc = true;
                }
            }

            //1)словарь программы n=n1+n2,n1 - число уникальных операторов программы,n2 - число уникальных операндов программы
            var generalUniqueValue = _uniqueOperators.Count + _uniqueOperands.Count;
            //2)длину программы N=N1+N2, N1 - общее число операторов в программе, N2 - общее число операндов в программе
            var generalValue = _operatorsCounter + _operandsCounter;
            //3)объем программы V=N*log2(n) (бит)
            var capacityValue = generalValue * (Math.Log(generalValue) / Math.Log(2));
            //n* - теоретический словарь программы n* = n1* + n2*, V* = n* * log2 n*
            var programVoc = 3 + _uniqueOperands.Count + _uniqueOperands.Count;
            var potencialCapacity = programVoc*(Math.Log(programVoc)/Math.Log(2));
            //1.теоретической длины программы N^ = n1*log2(n1) + n2*log2(n2),n1 - словарь операторов; n2 - словарь операндов программы
            var programLength = _uniqueOperators.Count * (Math.Log(_uniqueOperators.Count) / Math.Log(2)) +
                                _uniqueOperands.Count * (Math.Log(_uniqueOperands.Count) / Math.Log(2));
            //2.уровень качества программирования L, L=V*/V, V =N*log2(n), V*=n* * log2 n*
            var qualityLevel = potencialCapacity/capacityValue;
            var Vstar = programVoc*(Math.Log(programVoc)/Math.Log(2));
            //3.уровень программы, L^ = 2*n2 / (n1*N2)
            var programLevel = (2*_uniqueOperands.Count)/(_uniqueOperators.Count*_operandsCounter);
            //4. интеллектуальное содержание конкретного алгоритма, I = L^ * V
            var intellect = programLevel * capacityValue;
            //оценка необходимых интеллектуальных усилий, E = N^ * log2(n/L)
            var intellectStrengh = programLength*(Math.Log(generalUniqueValue/qualityLevel) / Math.Log(2));
            //затраты на восприятие готовой программы E' = N * log2(n/L)
            var intellectFinishProgram = generalValue*(Math.Log(generalUniqueValue / qualityLevel) / Math.Log(2));
            //E = V * V / V*
            var E =capacityValue*capacityValue/Vstar;

            //вывод в лейбл
            LabelforAnalyze.Content = "1)Словарь программы: " + generalUniqueValue + "\n" +
                                      "2)Длина программы: " + generalValue + "\n" +
                                      "3)Объем программы: " + capacityValue + " (бит) " + "\n" +
                                      "Теоретический словарь программы: " + programVoc + "\n" +
                                      "Потенциальный объем программы: " + potencialCapacity + "\n" +
                                      "Теоретическая длина программы: " + programLength + "\n" +
                                      "Уровень качества программирования: " + qualityLevel + "\n" +
                                      "Уровень программы: " + programLevel + "\n" +
                                      "Интеллектуальное содержание алгоритма: " + intellect + "\n" +
                                      "Оценка необходимых интеллектуальный усилий: " + intellectStrengh + "\n" +
                                      "Затраты на восприятие готовой программы: " + intellectFinishProgram + "\n" +
                                      "E: " + E;
        }
    }
}

