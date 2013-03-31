/*
 *
 * This project was originally an exercise in CS3500 Fall 2012 at the University of Utah completed
 * by Joshua Boren.
 * 
 * The project is now being extended to be used with a server that is being built in our CS3505
 * Spring 2013 course.  Changes will be made by Joshua Boren, Calvin Kern, Doug Hitchcock and 
 * Ethan Hayes to allow this front end of the spreadsheet to have collaboration functionality
 * for users across networks.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using System.Xml;

namespace SS
{
    /// <summary>
    /// A representation of a Spreadsheet.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        private Dictionary<string, Cell> sheet;
        private DependencyGraph dg;
        private bool modified;

        //-----------------------------------------------------------------------------------------Constructor  
        /// <summary>
        /// Creates a new Spreadsheet object.
        /// </summary>
        public Spreadsheet()
            : base(valid => true, s => s, "default")
        {
            sheet = new Dictionary<string, Cell>();
            dg = new DependencyGraph();
            Changed = false;
        }

        //---------------------------------------------------------------------------------------------------------------------------3-arg constructor
        /// <summary>
        /// Three-argument constructor.  Takes in a validity delegate, normalizing delegate and a version name.
        /// </summary>
        /// <param name="valid"></param>
        /// <param name="norm"></param>
        /// <param name="vers"></param>
        public Spreadsheet(Func<string, bool> valid, Func<string, string> norm, string vers)
            : base(valid, norm, vers)
        {
            sheet = new Dictionary<string, Cell>();
            dg = new DependencyGraph();
            Changed = false;
            Version = vers;
        }

        //----------------------------------------------------------------------------------------------------------------------------4-arg constructor
        /// <summary>
        /// Four-argument constructor.  Takes in a filepath validity delegate, normalizing delegate and a version name.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="valid"></param>
        /// <param name="norm"></param>
        /// <param name="vers"></param>
        public Spreadsheet(string filepath, Func<string, bool> valid, Func<string, string> norm, string vers)
            : base(valid, norm, vers)
        {
            sheet = new Dictionary<string, Cell>();
            dg = new DependencyGraph();
            Changed = false;
            GetSavedVersion(filepath);
        }

        //-----------------------------------------------------------------------------------------GetNamesOfAllNonemptyCells()
        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet. 
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return sheet.Keys;
        }

        //-----------------------------------------------------------------------------------------GetCellContents(string)
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            string s = Normalize(name);
            if (s == null)
                throw new InvalidNameException();
            if (!validName(s))
                throw new InvalidNameException();

            Cell temp;
            if (sheet.TryGetValue(s, out temp))
                return temp.CellContent;
            else
                return "";
        }

        //-----------------------------------------------------------------------------------------SetCellContents(string, double)
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            if (name == null)
                throw new InvalidNameException();
            if (!validName(name))
                throw new InvalidNameException();

            if (!sheet.ContainsKey(name))
            {
                sheet.Add(name, new Cell(number));
            }
            else
            {
                sheet[name].CellContent = number;
            }

            dg.ReplaceDependees(name, new HashSet<string>());

            HashSet<string> result = new HashSet<string>();
            foreach (string s in GetCellsToRecalculate(name))
            {
                result.Add(s);
                Recalculate(s);
            }
            return result;
        }

        //-----------------------------------------------------------------------------------------SetCellContents(string, text)
        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            if (text == null)
                throw new ArgumentNullException();
            
			if (name == null)
                throw new InvalidNameException();
            
			if (name.Equals(String.Empty))
            {
                sheet.Remove(name);
                dg.ReplaceDependees(name, new HashSet<string>());
            }
            else if (!validName(name))
            {
                throw new InvalidNameException();
            }
            
			if (text == "")
            {
                sheet.Remove(name);
                HashSet<string> empty = new HashSet<string>();
                return empty;
            }
            
			if (!sheet.ContainsKey(name))
                sheet.Add(name, new Cell(text));
            else
                sheet[name].CellContent = text;

            dg.ReplaceDependees(name, new HashSet<string>());

            ISet<string> result = new HashSet<string>();
            foreach (string d in GetCellsToRecalculate(name))
            {
                result.Add(d);
                Recalculate(d);
            }
            return result;
        }

        //-----------------------------------------------------------------------------------------SetCellContents(name, Formula)
        /// <summary>
        /// If formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            try
            {
                if (formula == null)
                    throw new ArgumentNullException();
            }
            catch (NullReferenceException)
            {
                throw new ArgumentNullException();
            }
            
			if (name == null)
                throw new InvalidNameException();
            
			if (!validName(name))
                throw new InvalidNameException();

            List<string> oldDependees = new List<string>(dg.GetDependees(name));

            dg.ReplaceDependees(name, formula.GetVariables(IsValid, Normalize));

            ISet<string> result;
            try
            {
                result = new HashSet<string>(GetCellsToRecalculate(name));
            }
            catch (CircularException)
            {
                dg.ReplaceDependees(name, oldDependees);
                throw new CircularException();
            }

            if (!sheet.ContainsKey(name))
            {
                sheet.Add(name, new Cell(formula));
            }
            else
            {
                sheet[name].CellContent = "=" + formula;
            }
            return result;
        }

        //-----------------------------------------------------------------------------------------GetDirectDependents(string)
        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1  
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException();
            if (!validName(name))
                throw new InvalidNameException();

            return dg.GetDependents(name);
        }

        //-----------------------------------------------------------------------------------------validName(string)
        /// <summary>
        /// Determines if the input string is a valid name for a cell.
        /// </summary>
        /// <param name="s">The name of the cell</param>
        /// <returns>True if the input string is a valid name, false otherwise.</returns>
        private bool validName(string s)
        {
            Regex r = new Regex(@"^[a-zA-Z]+[0-9]+$");
            if (r.IsMatch(s))
                return true && IsValid(s);
            else
                return false;
        }

        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the true if the version has been changed and false if it has not since the last save.
        /// </summary>
        public override bool Changed
        {
            get
            {
                return modified;
            }
            protected set
            {
                modified = value;
            }
        }

        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public override string GetSavedVersion(string filename)
        {
            bool CellSet = false;
            string readVersion = "";
            try
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    string name = null;
                    string content = null;

                    while (reader.Read())
                    {
                        if (CellSet)
                        {
                            name = null;
                            content = null;
                            CellSet = false;
                        }
						
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "spreadsheet":
                                    readVersion = reader["version"];
                                    break;

                                case "cell":
                                    break;

                                case "name":
                                    reader.Read();
                                    name = reader.Value;
                                    break;

                                case "contents":
                                    reader.Read();
                                    content = reader.Value;
                                    break;
                            }
                        }

                        if (name != null && content != null)
                        {
                            SetContentsOfCell(name, content);
                            CellSet = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string s = e.Message;
                throw new SpreadsheetReadWriteException();
            }

            if (readVersion != "default" && Version == "default")
            {
                return readVersion;
            }
            else if (readVersion != Version)
            {
                throw new SpreadsheetReadWriteException();
            }
            else
            {
                return readVersion;
            }
        }

        //-----------------------------------------------------------------------------------------Working
        /// <summary>
        /// Saves the current spreadsheet to the given filename.
        /// </summary>
        /// <param name="filename"></param>
        public override void Save(string filename)
        {
            try
            {
                using (XmlWriter writer = XmlWriter.Create(filename))
                {
                    string newLine = writer.Settings.NewLineChars;
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");

                    writer.WriteAttributeString("version", Version);
                    writer.WriteString(newLine);
                    IEnumerable<string> names = GetNamesOfAllNonemptyCells();

                    foreach (string name in names)
                    {
                        writer.WriteString(newLine);
                        writer.WriteStartElement("cell");
                        writer.WriteString(newLine);
                        writer.WriteElementString("name", name);
                        writer.WriteString(newLine);

                        if (GetCellContents(name) is Formula)
                        {
                            writer.WriteElementString("contents", "=" + GetCellContents(name).ToString());
                            writer.WriteString(newLine);
                        }
                        else
                        {
                            writer.WriteElementString("contents", GetCellContents(name).ToString());
                            writer.WriteString(newLine);
                        }

                        writer.WriteEndElement();
                        writer.WriteString(newLine);
                    }
                    writer.WriteString(newLine);
                    writer.WriteEndDocument();
                }
                Changed = false;
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException();
            }
        }

        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the value of the cell
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object GetCellValue(string name)
        {
            if (name == null)
                throw new InvalidNameException();
            if (!validName(name))
                throw new InvalidNameException();

            Cell temp;
            if (sheet.TryGetValue(name, out temp))
                return temp.CellValue;
            else
                return "";
        }

        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetContentsOfCell(string a, string content)
        {
            string name = Normalize(a);

            if (content == null)
                throw new ArgumentNullException();
            if (name == null || !validName(name))
                throw new InvalidNameException();

            Changed = true;

            ISet<string> result;

            double doubleValue;
            if (Double.TryParse(content, out doubleValue))
            {
                SetCellValue(name, doubleValue);
                result = SetCellContents(name, doubleValue);
                return result;
            }

            if (!content.StartsWith("="))
            {
                SetCellValue(name, content);
                return SetCellContents(name, content);
            }

            Formula tempFormula = null;
            if (content.StartsWith("="))
            {
                string temp = content.Substring(1);
                tempFormula = new Formula(temp, IsValid, Normalize);
            }

            result = SetCellContents(name, tempFormula);

            foreach (string s in result)
            {
                string VarName = Normalize(s);
                Cell newCell;
                if (sheet.TryGetValue(VarName, out newCell))
                {
                    Formula newFormula = new Formula(newCell.CellContent.ToString(), IsValid, Normalize);

                    newCell.CellValue = newFormula.Evaluate(cellLookup, IsValid, Normalize);
                }
            }
            return SetCellContents(name, tempFormula);
        }

        /// <summary>
        /// Returns the value of a cell.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private double cellLookup(string s)
        {
            Cell temp = null;
            if (!sheet.TryGetValue(s, out temp))
            {
                
            }

            double d;
            if (Double.TryParse(temp.CellValue.ToString(), out d))
            {
                return (double)temp.CellValue;
            }

            else
                throw new FormulaFormatException("Cell value is not a double, or cannot be evaluated to a double.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool SetCellValue(string name, object value)
        {
            bool valueChanged = false;

            Cell temp;
            if (sheet.TryGetValue(name, out temp))
            {
                object oldValue = temp.CellValue;

                temp.CellValue = value;

                if (oldValue != value)
                {
                    valueChanged = true;
                }
            }
            return valueChanged;
        }

        /// <summary>
        /// Recalculates the value of a given cell.
        /// </summary>
        /// <param name="s"></param>
        private void Recalculate(string s)
        {
            Cell temp;
            if (sheet.TryGetValue(s, out temp))
            {
                if (temp.CellContent is Formula)
                {
                    Formula f = new Formula(temp.CellContent.ToString(), IsValid, Normalize);
                    temp.CellValue = f.Evaluate(cellLookup, IsValid, Normalize);
                }
            }
        }
    }
}
