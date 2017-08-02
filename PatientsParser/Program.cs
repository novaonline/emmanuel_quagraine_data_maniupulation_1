using PatientsParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PatientsParser
{
    class Program
    {
        public static long BEGIN_POSITION_ON_FAIL { get; set; }
        public static long POSITION_IN_FILE { get; set; }
        public static string FILE_NAME { get; set; }
        public static int BULK_SIZE = 1;

        /// <summary>
        /// a bulk amount of of patients is returned, we attempt to insert the data into database.
        /// if successful, data should be there, else it'll rollback
        /// if program fails, and error is printed along with the last position in file in the database
        /// pass that last position in file as the second arguement and resume
        /// </summary>
        /// <param name="args">0 = file path (relative to project folder or absolute), 1 = position when data was last inserted succesfully (optional)</param>
        static void Main(string[] args)
        {
            BEGIN_POSITION_ON_FAIL = 0;
            POSITION_IN_FILE = 0;

            ValidateArguments(args);
            try
            {
                List<Patients> patients = new List<Patients>();
                do
                {
                    patients = Parser.FetchPatients(BULK_SIZE); // grabs a bulk of patients
                    if(!patients.Any())
                    {
                        break; // no patients were returned. must be at end of line
                    }

                    using (var patientsCtx = new DAL.PatientsContext())
                    {
                        using (var transaction = patientsCtx.Database.BeginTransaction())
                        {
                            try
                            {
                                foreach (var patient in patients)
                                {
                                    patientsCtx.Patients.Add(patient);
                                    var id = patientsCtx.SaveChanges();
                                    // do something with id if needed;
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine(ex.Message);
                                Console.Error.WriteLine(ex.StackTrace);
                                throw ex; // bubble
                            }

                        }
                    }
                } while (patients.Count > 0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                Console.WriteLine($"** Cursor is at {BEGIN_POSITION_ON_FAIL} **");
                Console.ReadLine();
                Environment.Exit(-1);
            }

        }

        private static void ValidateArguments(string[] args)
        {
            // determine if filepath specified
            if (!args.Any())
            {
                Console.Error.WriteLine("The file path must be specified");
            }
            else
            {
                FILE_NAME = args[0];
            }

            if (args.Count() >= 2)
            {
                try
                {
                    POSITION_IN_FILE = int.Parse(args[1]);
                }
                catch (Exception)
                {
                    Console.Error.WriteLine("Couldnt parse BYTE_CURSOR arguement");
                }
            }
        }
    }
}