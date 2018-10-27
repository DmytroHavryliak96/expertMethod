using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public enum Marks {VeryLow, Low, Middle, GreaterThanMiddle, High};

    class StartupEstimation
    {
        private List<double[]> marks; // список оцінок по категоріям
        private double[] desirableMarks; // список бажаних оцінок по групам категорій
        private double[] sum; // сума балів відповідей градаційної шкали для групи критеріїв
        private double[] a; // згортка суми мінімальних балів
        private double[] b; // згортка суми максимальних балів
        private double[] x; // Функція належності бальної оцінки
        private double[] alpha; // Функція належності «бажаних значень»
        private int[][] UTerms; // лінгвістичні змінні, які визначають різницю між 
        // бажаними та реальними оцінками по групах
        private int[] desirableUTerms; // бажані терми
        private double[][] miuUTerm; // Функція належності лінгвістичних змінних
        private double[] miuOTerm; // оцінки відносно отриманих та бажаних термів
        private int[] weight; // вагові коефіцієнти по кожній групі критеріїв
        private double[] normWeight; // нормовані вагові коефіцінти
        private double totalMark; // агрегована оцінка
        private Marks mark; // лінгвістична оцінка ідеї
        private const int NumberOfGroups = 5; // константа - кількість груп критеріїв

        public StartupEstimation(IEnumerable<double[]> _marks, double[] _desirableMarks, int[] _desirableUTerms, int[] _weight)
        {
            // ініціалізація списку оцінок
            if (_marks == null)
                throw new ArgumentException("the list of marks cannot be null");
            if (_marks.Count() != 5 || _desirableMarks.Count() != 5 || _desirableUTerms.Count() != 5 || _weight.Count() != 5)
                throw new ArgumentException("the number of criterion groups' must be " + NumberOfGroups.ToString());

            marks = new List<double[]>();

            foreach (var item in _marks)
            {
                double[] arr = new double[item.Count()];
                for (int i = 0; i < item.Count(); i++)
                    arr[i] = item[i];

                marks.Add(arr);
            }

            // ініціалізація бажаних оцінок
            desirableMarks = new double[NumberOfGroups];
            for (int i = 0; i < NumberOfGroups; i++)
                desirableMarks[i] = _desirableMarks[i];

            // ініціалізація множини сум
            sum = new double[NumberOfGroups];

            // ініціалізація згорток сум мінімальних та максимальних балів (константи згідно анкети)
            a = new double[NumberOfGroups] { 20, 15, 10, 50, 25};
            b = new double[NumberOfGroups] { 115, 60, 50, 225, 90};

            // ініціалізація функцій належності бальної оцінки та "бажаної оцінки"
            x = new double[NumberOfGroups];
            alpha = new double[NumberOfGroups];

            // ініціалізація лінгвістичних змінних
            UTerms = new int[NumberOfGroups][];

            // ініціалізація достовірностей терму
            miuUTerm = new double[NumberOfGroups][];

            // ініціалізація бажаних термів
            desirableUTerms = new int[NumberOfGroups];
            for (int i = 0; i < NumberOfGroups; i++)
                desirableUTerms[i] = _desirableUTerms[i];

            // ініціалізація оцінок відносно отриманих та бажаних термів
            miuOTerm = new double[NumberOfGroups];

            // ініціалізація ваг
            weight = new int[NumberOfGroups];
            normWeight = new double[NumberOfGroups];
            for (int i = 0; i < NumberOfGroups; i++)
                weight[i] = _weight[i];

            // ініціалізація суми
            totalMark = 0;
        }


        public void Calculate()
        {
            Sum();

            x = CalculateMembership(sum);
            alpha = CalculateMembership(desirableMarks);

            CalculateUTermMembership();

            CalculateMiuOTerm();

            NormalizeWeight();

            CalculateTotalMark();

            CalculateStringMark();
        }
        // знаходження суми балів відповідей градаційної шкали для групи критеріїв
        private void Sum()
        {
            for(int i = 0; i < marks.Count(); i++)
            {
                sum[i] = marks[i].Sum();
            }
        }

        // обчислення функції належності
        private double[] CalculateMembership(double[] totalMarks)
        {
            double[] func = new double[NumberOfGroups];

            for(int i = 0; i < NumberOfGroups; i++)
            {
                if (totalMarks[i] <= a[i])
                    func[i] = 0;
                else if (a[i] < totalMarks[i] && totalMarks[i] <= (a[i] + b[i]) / 2.0)
                    func[i] = 2 * Math.Pow((totalMarks[i] - a[i]) / (b[i] - a[i]), 2);
                else if ((a[i] + b[i]) / 2.0 < totalMarks[i] && totalMarks[i] < b[i])
                    func[i] = 1 - 2 * Math.Pow((b[i] - totalMarks[i]) / (b[i] - a[i]), 2);
                else if (totalMarks[i] >= b[i])
                    func[i] = 1;
            }

            return func;
        }

        // обчислення функції належності для термів
        private void CalculateUTermMembership()
        {
            for (int i = 0; i < NumberOfGroups; i++)
            {
                List<double> _miuTerms = new List<double>();
                List<int> _UTerms = new List<int>();

                if (x[i] <= alpha[i] - alpha[i] / 4.0) {
                    _UTerms.Add(1);
                    if (x[i] <= alpha[i] - alpha[i] / 2.0)
                        _miuTerms.Add(1);
                    else
                        _miuTerms.Add((3 * alpha[i] - 4 * x[i]) / alpha[i]); 
                   }

                if (alpha[i] - alpha[i]/2.0 < x[i] && x[i] <= alpha[i])
                {
                    _UTerms.Add(2);
                    if (alpha[i] - alpha[i] / 2.0 < x[i] && x[i] <= alpha[i] - alpha[i] / 4.0)
                        _miuTerms.Add((4 * x[i] - 2 * alpha[i]) / alpha[i]);
                    else
                        _miuTerms.Add((4*alpha[i] - 4*x[i])/alpha[i]);
                        
                }

                if (alpha[i] - alpha[i]/4.0 < x[i] && x[i] <= alpha[i] + alpha[i] / 4.0)
                {
                    _UTerms.Add(3);
                    if (alpha[i] - alpha[i] / 4.0 < x[i] && x[i] <= alpha[i])
                        _miuTerms.Add((4 * x[i] - 3 * alpha[i]) / alpha[i]);
                    else
                        _miuTerms.Add((5*alpha[i] - 4*x[i])/alpha[i]);
                }

                if(alpha[i] < x[i] && x[i] <= alpha[i] + alpha[i]/2.0)
                {
                    _UTerms.Add(4);
                    if (alpha[i] < x[i] && x[i] <= alpha[i] + alpha[i] / 4.0)
                        _miuTerms.Add((4 * x[i] - 4 * alpha[i]) / alpha[i]);
                    else
                        _miuTerms.Add((6*alpha[i] - 4*x[i])/alpha[i]);

                }
                
                if(alpha[i] + alpha[i] / 4.0 < x[i] && x[i] <= alpha[i] + alpha[i] / 2.0)
                {
                    _UTerms.Add(5);
                    _miuTerms.Add((4 * x[i] - 5 * alpha[i]) / alpha[i]);
                }
                if(x[i] >= alpha[i] + alpha[i]/2.0)
                {
                    _UTerms.Add(5);
                    _miuTerms.Add(1);
                }

                UTerms[i] = new int[_UTerms.Count()];
                miuUTerm[i] = new double[_miuTerms.Count()];

                for (int j = 0; j < _UTerms.Count(); j++)
                {
                    UTerms[i][j] = _UTerms[j];
                    miuUTerm[i][j] = _miuTerms[j];
                }
                
            }
        }

        // обчислення оцінок відносно отриманих та бажаних термів
        private void CalculateMiuOTerm()
        {
            for(int i = 0; i < NumberOfGroups; i++)
            {
                List<double> vector = new List<double>();

                for(int j = 0; j < UTerms[i].Length; j++)
                {
                    if (UTerms[i][j] == desirableUTerms[i])
                        vector.Add(miuUTerm[i][j]);
                    else if (UTerms[i][j] + 1 == desirableUTerms[i] || UTerms[i][j] - 1 == desirableUTerms[i])
                        vector.Add(miuUTerm[i][j] / 2.0);
                    else
                        vector.Add(0);
                }

                miuOTerm[i] = vector.Max();
            }
        }

        private void NormalizeWeight()
        {
            int sumWeight = weight.Sum();
            for(int i = 0; i < NumberOfGroups; i++)
                normWeight[i] = (double)weight[i] / (double)sumWeight;

        }

        // побудови агрегованої оцінки
        private void CalculateTotalMark()
        {
            for(int i = 0; i < NumberOfGroups; i++)
            {
                totalMark += normWeight[i] * miuOTerm[i];
            }
        }

        // визначення лінгвістичної оцінки
        private void CalculateStringMark()
        {
            if (0 <= totalMark && totalMark <= 0.021)
                mark = Marks.VeryLow;
            else if (0.021 < totalMark && totalMark <= 0.36)
                mark = Marks.Low;
            else if (0.36 < totalMark && totalMark <= 0.47)
                mark = Marks.Middle;
            else if (0.47 < totalMark && totalMark <= 0.67)
                mark = Marks.GreaterThanMiddle;
            else
                mark = Marks.High;
        }

        public void ShowResults()
        {
            Console.WriteLine("Membership Table");
            Console.WriteLine("Criterion Groups".PadRight(17) + "Score".PadRight(6) + "Mark Func".PadRight(11) + "Desiriable".PadRight(11) + "Desiriable Func".PadRight(16));
            for(int i = 0; i < NumberOfGroups; i++)
            {
                Console.WriteLine((i+1).ToString().PadRight(17) + Formate(sum[i], 6) + Formate(x[i], 11) + Formate(desirableMarks[i], 11) + Formate(alpha[i], 16));
            }

            Console.WriteLine("UTerms Table");
            Console.WriteLine("Criterion Groups".PadRight(17) + "Calculated UTerm".PadRight(17) + "Miu U Term".PadRight(25) + "Desiriable Miu U Term".PadRight(22));
            for (int i = 0; i < NumberOfGroups; i++)
            {
                string text = "";
                text += (i + 1).ToString().PadRight(17);

                string terms = "";

                for(int j = 0; j < UTerms[i].Length; j++)
                    terms += "U" + (i+1) + UTerms[i][j] + " OR ";

                ;
       
                text += terms.Remove(terms.Count() - 4).PadRight(17);

                string miuTerms = "";
                for (int j = 0; j < miuUTerm[i].Length; j++)
                    miuTerms += "Mu" + (i+1) + UTerms[i][j] + "=" + Math.Round(miuUTerm[i][j], 2) + " OR ";

                text += miuTerms.Remove(miuTerms.Count() - 4).PadRight(25);
                text += ("U" + (i + 1) + desirableUTerms[i]).PadRight(22);
                Console.WriteLine(text);
            }

            Console.WriteLine("Calculated Marks Table");
            Console.WriteLine("Criterion Groups".PadRight(17) + "Result Mark".PadRight(17));
            for (int i = 0; i < NumberOfGroups; i++)
                Console.WriteLine((i + 1).ToString().PadRight(17) + Formate(miuOTerm[i], 17));
            
            Console.WriteLine("Final Mark = " + mark);



        }

        private string Formate(double value, int rightPadding)
        {
            return Math.Round(value, 2).ToString().PadRight(rightPadding);
        }

    }
}
