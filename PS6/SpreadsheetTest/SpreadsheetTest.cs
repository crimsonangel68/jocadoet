using SS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SpreadsheetUtilities;

namespace SpreadsheetTest
{
    /// <summary>
    ///This is a test class for SpreadsheetTest and is intended
    ///to contain all SpreadsheetTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SpreadsheetTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        //==================================================================================================================Constructor Tests
        /// <summary>
        ///A test for Spreadsheet Constructor
        ///</summary>
        [TestMethod()]
        public void SpreadsheetConstructorTest()
        {
            Spreadsheet target = new Spreadsheet();
        }

        //==================================================================================================================GetCellContentsTest
        /// <summary>
        ///A test for GetCellContents
        ///</summary>
        [TestMethod()]
        public void GetCellContentsTest()
        {
            Spreadsheet target = new Spreadsheet(); 
            string name = "A1";
            target.SetCellContents(name, 0.0);
            target.SetCellContents(name, "text");
            target.SetCellContents(name, new Formula("a3+4", s=> true, s=>s));
            object expected = new Formula("a3+4", s => true, s => s); 
            object actual;
            actual = target.GetCellContents(name);
            Assert.AreEqual(expected, actual);
        }
        
        /// <summary>
        ///A test for GetCellContents
        ///</summary>
        [TestMethod()]
        public void GetCellContentsTest1()
        {
            Spreadsheet target = new Spreadsheet();
            string name = "A1";
            target.SetCellContents(name, "hello");
            object expected = "hello";
            object actual;
            actual = target.GetCellContents(name);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetCellContents
        ///</summary>
        [TestMethod()]
        public void GetCellContentsTest2()
        {
            Spreadsheet target = new Spreadsheet();
            string name = "Ab31";
            target.SetCellContents(name, new Formula("4+4", s => true, s => s));
            object expected = new Formula("4+4", s => true, s => s);
            object actual;
            actual = target.GetCellContents(name);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetCellContents
        ///</summary>
        [TestMethod()]
        public void GetCellContentsTest3()
        {
            Spreadsheet target = new Spreadsheet();
            string name = "aA21";
            string formula = "A2 * B4";
            target.SetCellContents(name, new Formula(formula, s => true, s => s));
            object expected = new Formula(formula, s => true, s => s);
            object actual;
            actual = target.GetCellContents(name);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetCellContents
        ///</summary>
        [TestMethod()]
        public void GetCellContentsTest4()
        {
            Spreadsheet target = new Spreadsheet();
            string name = "vdsA321";
            string formula = "A2 / B4 + 4.5";
            target.SetCellContents(name, new Formula(formula, s => true, s => s));
            object expected = new Formula(formula, s => true, s => s);
            object actual;
            actual = target.GetCellContents(name);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetCellContents
        ///</summary>
        [TestMethod()]
        public void GetCellContentsTest5()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                object expected = 0.0;
                object actual;
                actual = target.GetCellContents(null);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for GetCellContents
        ///</summary>
        [TestMethod()]
        public void GetCellContentsTest6()
        {
            try
            {
            Spreadsheet target = new Spreadsheet();
            string name = String.Empty;
            target.SetCellContents(name, 0);
            object expected = 0;
            object actual;
            actual = target.GetCellContents(name);
            Assert.AreEqual(expected, actual);
            }
            catch(Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for GetCellContents
        ///</summary>
        [TestMethod()]
        public void GetCellContentsTest7()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = "vdsA321bf";
                object expected = 0;
                object actual;
                actual = target.GetCellContents(name);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        //==================================================================================================================GetDirectDependents Tests
        /// <summary>
        ///A test for GetDirectDependents
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SS.dll")]
        public void GetDirectDependentsTest()
        {
            try
            {
            Spreadsheet_Accessor target = new Spreadsheet_Accessor();
            string name = null;
            IEnumerable<string> expected = null;
            IEnumerable<string> actual;
            actual = target.GetDirectDependents(name);
            Assert.AreEqual(expected, actual);
            }
            catch(Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for GetDirectDependents
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SS.dll")]
        public void GetDirectDependentsTest1()
        {
            try
            {
                Spreadsheet_Accessor target = new Spreadsheet_Accessor();
                string name = "A1b";
                IEnumerable<string> expected = null;
                IEnumerable<string> actual;
                actual = target.GetDirectDependents(name);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for GetDirectDependents
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SS.dll")]
        public void GetDirectDependentsTest2()
        {
            Spreadsheet_Accessor target = new Spreadsheet_Accessor();
            string name = "A1";
            target.SetCellContents("A1", 3);
            target.SetCellContents("B1", new Formula("A1 * A1", s => true, s => s));
            target.SetCellContents("C1", new Formula("B1 + A1", s => true, s => s));
            target.SetCellContents("D1", new Formula("B1 - C1", s => true, s => s));
            SortedSet<string> expected = new SortedSet<string> {"B1", "C1"};
            IEnumerable<string> a;
            a = target.GetDirectDependents(name);
            SortedSet<string> actual = new SortedSet<string>();
            foreach (string s in a)
            {
                actual.Add(s);
            }
            CollectionAssert.AreEqual(actual, expected);
        }

        //==================================================================================================================GetNamesOfAllNonemptyCells Tests
        /// <summary>
        ///A test for GetNamesOfAllNonemptyCells
        ///</summary>
        [TestMethod()]
        public void GetNamesOfAllNonemptyCellsTest()
        {
            Spreadsheet target = new Spreadsheet();
            target.SetCellContents("A1", 3);
            target.SetCellContents("B1", "text");
            target.SetCellContents("C1", new Formula("3 - 4", s => true, s => s));
            SortedSet<string> expected = new SortedSet<string> {"A1", "B1", "C1"};
            IEnumerable<string> a;
            a = target.GetNamesOfAllNonemptyCells();
            SortedSet<string> actual = new SortedSet<string>();
            foreach (string s in a)
            {
                actual.Add(s);
            }
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetNamesOfAllNonemptyCells
        ///</summary>
        [TestMethod()]
        public void GetNamesOfAllNonemptyCellsTest1()
        {
            Spreadsheet target = new Spreadsheet();
            target.SetCellContents("A1", 3);
            target.SetCellContents("B1", "text");
            target.SetCellContents("C1", new Formula("3 - 4", s => true, s => s));
            SortedSet<string> expected = new SortedSet<string> { "A1", "B1", "C1" };
            IEnumerable<string> a;
            a = target.GetNamesOfAllNonemptyCells();
            SortedSet<string> actual = new SortedSet<string>();
            foreach (string s in a)
            {
                actual.Add(s);
            }
            CollectionAssert.AreEqual(expected, actual);
        }


        //==================================================================================================================SetCellContents Tests(string)
        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestString()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = null;
                string text = "a";
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(null, text);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestString1()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = String.Empty;
                string text = "a";
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(name, text);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestString2()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = "A1b";
                string text = "a";
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(name, text);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestString3()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = "A1";
                string text = null;
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(name, text);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new ArgumentNullException());
            }
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestString4()
        {
            Spreadsheet target = new Spreadsheet(); 
            string name = "A1"; 
            double number = 3.0;
            target.SetCellContents("B1", "A1 * A1");
            target.SetCellContents("C1", "B1 + A1");
            target.SetCellContents("D1", "B1 - C1");
            SortedSet<string> expected = new SortedSet<string> {"A1"}; 
            ISet<string> a;
            a = target.SetCellContents(name, number);
            SortedSet<string> actual = new SortedSet<string>();
            foreach (string s in a)
            {
                actual.Add(s);
            }
            CollectionAssert.AreEqual(actual, expected);
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestString5()
        {
            Spreadsheet target = new Spreadsheet();
            string name = "A1";
            target.SetCellContents("B1", 1.0);
            target.SetCellContents("C1", 1.0);
            SortedSet<string> expected = new SortedSet<string> { "A1" };
            ISet<string> a;
            a = target.SetCellContents(name, new Formula("B1 + C1", s => true, s => s));
            SortedSet<string> actual = new SortedSet<string>();
            foreach (string s in a)
            {
                actual.Add(s);
            }
            CollectionAssert.AreEqual(actual, expected);
        }

        //==================================================================================================================SetCellContents Tests(double)
        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestDouble()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = null;
                double number = 3.0;
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(name, number);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestDouble1()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = String.Empty;
                double number = 0;
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(name, number);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }
        
        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestDouble2()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = "A1b";
                double number = 3.0; ;
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(name, number);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

         /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestDouble3()
        {
            Spreadsheet target = new Spreadsheet();
            string name = "A1";
            double number = 3.0; 
            target.SetCellContents("B1", 3.0);
            target.SetCellContents("A1", 4.0);
            target.SetCellContents("A1", 5.0);
            SortedSet<string> expected = new SortedSet<string> {"A1"};
            ISet<string> a;
            a = target.SetCellContents(name, number);
            SortedSet<string> actual = new SortedSet<string>();
            foreach (string s in a)
            {
                actual.Add(s);
            }
            CollectionAssert.AreEqual(actual, expected);
            
        }

        //==================================================================================================================SetCellContents Tests(formula)
        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestFormula()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = null;
                Formula form = new Formula("a3", s => true, s => s);
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(name, form);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestFormula1()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = string.Empty;
                Formula form = new Formula("a3", s => true, s => s);
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(name, form);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestFormula2()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = "A1";
                Formula form = null;
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(name, form);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new ArgumentNullException());
            }
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestFormula3()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = null;
                Formula form = new Formula("a3", s => true, s => s);
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(name, form);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestFormula4()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                string name = "a3f";
                Formula form = new Formula("a3", s => true, s => s);
                ISet<string> expected = null;
                ISet<string> actual;
                actual = target.SetCellContents(name, form);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception e)
            {
                AssertFailedException.Equals(e, new InvalidNameException());
            }
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestFormula5()
        {
            Spreadsheet target = new Spreadsheet();
            string name = "A1";
            SortedSet<string> expected = new SortedSet<string>{"A1", "B1", "C1"};
            ISet<string> a;
            target.SetCellContents("B1", new Formula("A1 * 2", s => true, s => s));
            target.SetCellContents("C1", new Formula("B1 + A1", s => true, s => s));

            a = target.SetCellContents(name, 3);
            SortedSet<string> actual = new SortedSet<string>();
            foreach (string s in a)
            {
                actual.Add(s);
            }
            CollectionAssert.AreEqual(expected, actual);       
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTestFormula6()
        {
            Spreadsheet target = new Spreadsheet();
            string name = "A1";
            SortedSet<string> expected = new SortedSet<string> { "A2" };
            ISet<string> a;
            target.SetCellContents("B1", new Formula("2", s => true, s => s));
            target.SetCellContents("C1", new Formula("3", s => true, s => s));

            a = target.SetCellContents("A2", new Formula("B1 + C1", s => true, s => s));
            SortedSet<string> actual = new SortedSet<string>();
            foreach (string s in a)
            {
                actual.Add(s);
            }
            CollectionAssert.AreEqual(expected, actual);
        }

        //==================================================================================================================Circular Test
        ///<summary>
        ///
        ///</summary>
        [TestMethod()]
        public void CircularExceptionTest()
        {
            try
            {
                Spreadsheet target = new Spreadsheet();
                target.SetCellContents("A1", new Formula("B1 + A1", s => true, s => s));
                target.SetCellContents("B1", new Formula("C1 * 4", s => true, s => s));
                target.SetCellContents("C1", new Formula("D1 - 2", s => true, s => s));
                ISet<string> a;
                a = target.SetCellContents("D1", new Formula("A1 /2", s => true, s => s));
                SortedSet<string> expected = new SortedSet<string> { "A1", "B1", "C1", "D1" };
                SortedSet<string> actual = new SortedSet<string>();
                foreach (string s in a)
                {
                    actual.Add(s);
                }
                CollectionAssert.AreEqual(expected, actual);
            }
            catch (CircularException)
            {
                Assert.IsTrue(true);
            }
        }
    }
}
