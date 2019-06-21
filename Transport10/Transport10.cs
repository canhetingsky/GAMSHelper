using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using GAMS;
using System.Diagnostics;
using System.IO;


namespace TransportSeq
{
    class Transport10
    {
        static void Main(string[] args)
        {
            // Reading input data from workbook
            var excelApp = new Excel.Application();
            Excel.Workbook wb = excelApp.Workbooks.Open(Directory.GetCurrentDirectory() + @"\..\..\..\..\Data\transport.xls");

            Excel.Range range;

            Excel.Worksheet capacity = (Excel.Worksheet)wb.Worksheets.get_Item("capacity");
            range = capacity.UsedRange;
            Array capacityData = (Array)range.Cells.Value;
            int iCount = capacity.UsedRange.Columns.Count;

            Excel.Worksheet demand = (Excel.Worksheet)wb.Worksheets.get_Item("demand");
            range = demand.UsedRange;
            Array demandData = (Array)range.Cells.Value;
            int jCount = range.Columns.Count;

            Excel.Worksheet distance = (Excel.Worksheet)wb.Worksheets.get_Item("distance");
            range = distance.UsedRange;
            Array distanceData = (Array)range.Cells.Value;

            // number of markets/plants have to be the same in all spreadsheets
            Debug.Assert((range.Columns.Count - 1) == jCount && (range.Rows.Count - 1) == iCount,
                         "Size of the spreadsheets doesn't match");
            wb.Close();

            // Creating the GAMSDatabase and fill with the workbook data
            GAMSWorkspace ws;
            if (Environment.GetCommandLineArgs().Length > 1)
                ws = new GAMSWorkspace(systemDirectory: Environment.GetCommandLineArgs()[1]);
            else
                ws = new GAMSWorkspace();
            GAMSDatabase db = ws.AddDatabase();

            GAMSSet i = db.AddSet("i", 1, "Plants");
            GAMSSet j = db.AddSet("j", 1, "Markets");
            GAMSParameter capacityParam = db.AddParameter("a", 1, "Capacity");
            GAMSParameter demandParam = db.AddParameter("b", 1, "Demand");
            GAMSParameter distanceParam = db.AddParameter("d", 2, "Distance");

            for (int ic = 1; ic <= iCount; ic++)
            {
                i.AddRecord((string)capacityData.GetValue(1, ic));
                capacityParam.AddRecord((string)capacityData.GetValue(1, ic)).Value = (double)capacityData.GetValue(2, ic);
            }
            for (int jc = 1; jc <= jCount; jc++)
            {
                j.AddRecord((string)demandData.GetValue(1, jc));
                demandParam.AddRecord((string)demandData.GetValue(1, jc)).Value = (double)demandData.GetValue(2, jc);
                for (int ic = 1; ic <= iCount; ic++)
                {
                    distanceParam.AddRecord((string)distanceData.GetValue(ic + 1, 1), (string)distanceData.GetValue(1, jc + 1)).Value = (double)distanceData.GetValue(ic + 1, jc + 1);
                }
            }

            // Create and run the GAMSJob
            using (GAMSOptions opt = ws.AddOptions())
            {
                GAMSJob t10 = ws.AddJobFromString(GetModelText());
                opt.Defines.Add("gdxincname", db.Name);
                opt.AllModelTypes = "xpress";
                t10.Run(opt, db);
                foreach (GAMSVariableRecord rec in t10.OutDB.GetVariable("x"))
                    Console.WriteLine("x(" + rec.Keys[0] + "," + rec.Keys[1] + "): level=" + rec.Level + " marginal=" + rec.Marginal);
            }
        }

        static String GetModelText()
        {
            String model = @"
  Sets
       i   canning plants
       j   markets

  Parameters
       a(i)   capacity of plant i in cases
       b(j)   demand at market j in cases
       d(i,j) distance in thousands of miles
  Scalar f  freight in dollars per case per thousand miles /90/;

$if not set gdxincname $abort 'no include file name for data file provided'
$gdxin %gdxincname%
$load i j a b d
$gdxin

  Parameter c(i,j)  transport cost in thousands of dollars per case ;

            c(i,j) = f * d(i,j) / 1000 ;

  Variables
       x(i,j)  shipment quantities in cases
       z       total transportation costs in thousands of dollars ;

  Positive Variable x ;

  Equations
       cost        define objective function
       supply(i)   observe supply limit at plant i
       demand(j)   satisfy demand at market j ;

  cost ..        z  =e=  sum((i,j), c(i,j)*x(i,j)) ;

  supply(i) ..   sum(j, x(i,j))  =l=  a(i) ;

  demand(j) ..   sum(i, x(i,j))  =g=  b(j) ;

  Model transport /all/ ;

  Solve transport using lp minimizing z ;

  Display x.l, x.m ;
";

            return model;
        }
    }
}
