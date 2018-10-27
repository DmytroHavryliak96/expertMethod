using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<double[]> marks = new List<double[]>{
                new double[] { 20, 20, 25, 20},
                new double[] { 5, 0, 10},
                new double[] { 15, 5},
                new double[] { 5, 10, 10, 10, 5, 10, 20},
                new double[] { 10, 10, 5, 5, 0}
            };


            double[] desiarable = new double[5] { 100, 25, 25, 100, 40 };
            int[] desirableUTerms = new int[5] { 3, 3, 2, 2, 2};
            int[] weight = new int[5] { 10, 7, 5, 8, 6};

            var calculator = new StartupEstimation(marks, desiarable, desirableUTerms, weight);
            calculator.Calculate();
            calculator.ShowResults();
            Console.ReadLine();
        }
    }
}
