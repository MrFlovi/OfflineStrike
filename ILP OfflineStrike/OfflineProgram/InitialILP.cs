using Gurobi;

namespace OfflineProgram
{
    public static class InitialILP
    {
        public static void Run(string fileName)
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string file = Path.Combine(currentDir, @$"..\..\..\{fileName}");
            List<string> text = File.ReadLines(file).ToList();

            GRBEnv env = new GRBEnv();
            env.Start();
            var model = new GRBModel(env);

            int n = Int32.Parse(text[0]); // there are n people
            int m = Int32.Parse(text[1]);  // there are m days

            int[] seats = new int[m];
            double[] pricePlane = new double[m];         // { 500, 100, 1 };
            double[] priceHotel = new double[m];         // { 250, 150, 100 };
            GRBVar[] leavingVariables = new GRBVar[m];
            GRBVar[] stayingVariables = new GRBVar[m];

            for (int i = 2; i < text.Count; i++)
            {
                string[] textLine = text[i].Split(", ");
                seats[i - 2] = Int32.Parse(textLine[0]);
                pricePlane[i - 2] = double.Parse(textLine[1]);
                priceHotel[i - 2] = double.Parse(textLine[2]);
            }


            for (int i = 0; i < m; i++)
            {
                leavingVariables[i] = model.AddVar(0, n, 0, GRB.INTEGER, $"people_leaving_on_day_{i}");
                stayingVariables[i] = model.AddVar(0, n, 0, GRB.INTEGER, $"people_staying_on_day_{i}");
            }

            for (int i = 0; i < m; i++)
            {
                model.AddConstr(leavingVariables[i], GRB.LESS_EQUAL, seats[i], $"leaving_seats_day_{i}");
            }

            GRBLinExpr sum = 0;
            foreach (GRBVar leavingVar in leavingVariables)
            {
                sum.AddTerm(1, leavingVar);
            }
            model.AddConstr(sum, GRB.EQUAL, n, "all_people_should_leave");

            GRBLinExpr basecase = 0;
            basecase.AddTerm(1, leavingVariables[0]);
            basecase.AddTerm(1, stayingVariables[0]);
            model.AddConstr(basecase, GRB.EQUAL, n, "basecase");

            for (int i = 1; i < m; i++)
            {
                GRBLinExpr inductive = 0;
                inductive.AddTerm(1, leavingVariables[i]);
                inductive.AddTerm(1, stayingVariables[i]);
                model.AddConstr(inductive, GRB.EQUAL, stayingVariables[i - 1], $"same_as_staying_yesterday_{i}");
            }

            model.Update();

            GRBLinExpr objectiveExpression = 0;
            objectiveExpression.AddTerms(pricePlane, leavingVariables);
            objectiveExpression.AddTerms(priceHotel, stayingVariables);

            model.SetObjective(objectiveExpression, GRB.MINIMIZE);
            model.Update();
            model.Optimize();

            //Console.WriteLine("Cost: ", model.ObjVal);

            for (int i = 0; i < m; i++)
            {
                Console.WriteLine($"{leavingVariables[i].X}, {stayingVariables[i].X}");
            }

            Console.ReadLine();
        }
    }
}
