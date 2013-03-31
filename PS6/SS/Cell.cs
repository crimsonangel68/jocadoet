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

namespace SS
{
    //============================================================================================= Cell
    /// <summary>
    /// Cell class.  Each cell has a name, content and value.
    /// Name is a string.
    /// Content is either a string, double or formula.
    /// Value is either a string, double, or formula error.
    /// </summary>
    public class Cell
    {
        private object cellContent;
        private object cellValue;

        /// <summary>
        /// Contents of the cell
        /// </summary>
        public object CellContent
        {
            get
            {
                return cellContent;
            }
            set
            {
                cellContent = value;
            }
        }

        /// <summary>
        /// Value of the cell
        /// </summary>
        public object CellValue
        {
            get
            {
                return cellValue;
            }
            set
            {
                cellValue = value;
            }
        }

        /// <summary>
        /// Create a new Cell containing a string.
        /// </summary>
        /// <param name="s"></param>
        public Cell(string s)
        {
            cellContent = s;
            cellValue = s;
        }

        /// <summary>
        /// Create a new Cell containing a double.
        /// </summary>
        /// <param name="d"></param>
        public Cell(double d)
        {
            cellContent = d.ToString();
            cellValue = d;
        }

        /// <summary>
        /// Create a new Cell containing a Formula.
        /// </summary>
        /// <param name="f"></param>
        public Cell(Formula f)
        {
            cellContent = f.ToString();
            
        }
    }
}
