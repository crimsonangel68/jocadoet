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
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    class SpreadsheetApplicationContext : ApplicationContext
    {
        //The number of open forms
        private int formCount = 0;

        //Singleton ApplicationContext
        private static SpreadsheetApplicationContext appContext;

        /// <summary>
        /// Private constructor for singleton pattern.
        /// </summary>
        private SpreadsheetApplicationContext()
        {
            
        }

        /// <summary>
        /// Returns the one SpreadsheetApplicationContext
        /// </summary>
        /// <returns></returns>
        public static SpreadsheetApplicationContext getAppContext()
        {
            if (appContext == null)
                appContext = new SpreadsheetApplicationContext();
            return appContext;
        }

        /// <summary>
        /// Runs the form.
        /// </summary>
        /// <param name="form"></param>
        public void RunForm(Form form)
        {
            //One more form is running
            formCount++;

            //We want to know when this form closes
            form.FormClosed += (o, e) => { if (--formCount <= 0) ExitThread(); };

            //Run the form
            form.Show();
        }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // New way of opening files..................................
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SpreadsheetApplicationContext appContext = SpreadsheetApplicationContext.getAppContext();
            appContext.RunForm(new Startup());
            Application.Run(appContext);
            //Application.Run(new Startup());

            while (Application.OpenForms.Count > 0)
            {

            }

            // end of new opening method................................  */
             
             
            
            // commenting out old way of opening file-------------------
            /*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Console.Write(args[0]);
            //Console.Read();


            if (args.Length == 0)
            {
                //Start an application context and run one form inside it
                SpreadsheetApplicationContext appContext = SpreadsheetApplicationContext.getAppContext();
                appContext.RunForm(new Form1());
                Application.Run(appContext);
            }
            else
            {
                //Start an application context and run one form inside it
                SpreadsheetApplicationContext appContext = SpreadsheetApplicationContext.getAppContext();
                appContext.RunForm(new Form1(args[0]));
                Application.Run(appContext);
            }
            //end comment------------------------------------------------ */
        }
    }
}
