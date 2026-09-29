using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.SqlClient;
using CalcClassBr;

namespace TestCalculator
{
    [TestClass]
    public class MultTests
    {
        private static DataTable table = new DataTable();

        [ClassInitialize]
        public static void LoadDataFromSql(TestContext context)
        {
            string connectionString = "Data Source=.\\SQLDEVELOPER;Initial Catalog=CalculatorTestsDB;Integrated Security=True;TrustServerCertificate=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id, TestCaseName, ParamA, ParamB, ExpectedResult, ExpectedError FROM dbo.TestData_Mult ORDER BY Id ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(table);
                }
            }
        }

        private void ExecuteRowTest(int rowId)
        {
            DataRow[] foundRows = table.Select("Id = " + rowId);
            Assert.IsTrue(foundRows.Length > 0, "Строка с Id=" + rowId + " не найдена в базе данных.");
            DataRow row = foundRows[0];

            string testCaseName = row["TestCaseName"] != DBNull.Value ? row["TestCaseName"].ToString() : "";
            long a = Convert.ToInt64(row["ParamA"]);
            long b = Convert.ToInt64(row["ParamB"]);

            object rawRes = row["ExpectedResult"];
            int? expectedResult = (rawRes != DBNull.Value && rawRes != null) ? Convert.ToInt32(rawRes) : (int?)null;

            object rawErr = row["ExpectedError"];
            string expectedError = (rawErr != DBNull.Value && rawErr != null) ? rawErr.ToString() : null;

            int actualResult = 0;
            bool threwException = false;

            try
            {
                if (a > int.MaxValue || a < int.MinValue || b > int.MaxValue || b < int.MinValue)
                {
                    threwException = true;
                }
                else
                {
                    actualResult = CalcClass.Mult((int)a, (int)b);
                }
            }
            catch (Exception)
            {
                threwException = true;
            }

            if (!string.IsNullOrEmpty(expectedError))
            {
                bool hasError = threwException || CalcClass.lastError == expectedError;
                Assert.IsTrue(hasError, "[" + testCaseName + "] Ожидалась ошибка '" + expectedError + "'.");
            }
            else
            {
                Assert.IsFalse(threwException, "[" + testCaseName + "] Метод завершился исключением.");
                Assert.IsNotNull(expectedResult, "[" + testCaseName + "] ExpectedResult не должен быть пустым.");
                Assert.AreEqual(expectedResult.Value, actualResult, "[" + testCaseName + "] Ошибка в умножении: " + a + " * " + b);
            }
        }

        [TestMethod] public void Test_01_Positive() => ExecuteRowTest(1);
        [TestMethod] public void Test_02_NegativeByPositive() => ExecuteRowTest(2);
        [TestMethod] public void Test_03_PositiveByNegative() => ExecuteRowTest(3);
        [TestMethod] public void Test_04_NegativeByNegative() => ExecuteRowTest(4);
        [TestMethod] public void Test_05_ParamA_Zero() => ExecuteRowTest(5);
        [TestMethod] public void Test_06_ParamB_Zero() => ExecuteRowTest(6);
        [TestMethod] public void Test_07_BothZero() => ExecuteRowTest(7);
        [TestMethod] public void Test_08_MultiplyByOne() => ExecuteRowTest(8);
        [TestMethod] public void Test_09_MultiplyByMinusOne() => ExecuteRowTest(9);
        [TestMethod] public void Test_10_MaxInt_ByOne() => ExecuteRowTest(10);
        [TestMethod] public void Test_11_MinInt_ByOne() => ExecuteRowTest(11);
        [TestMethod] public void Test_12_Overflow_Positive() => ExecuteRowTest(12);
        [TestMethod] public void Test_13_Overflow_Negative() => ExecuteRowTest(13);
        [TestMethod] public void Test_14_Square_MaxInt() => ExecuteRowTest(14);
    }
}