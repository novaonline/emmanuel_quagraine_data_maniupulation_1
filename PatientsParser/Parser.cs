using PatientsParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PatientsParser
{
    class Parser
    {
        /// <summary>
        /// Returns a bulk size of 'maxEntries'.
        /// Traverses and determines words. For every 3rd word, a patient model is created and added to the result
        /// </summary>
        /// <returns></returns>
        public static List<Patients> FetchPatients(int maxEntries)
        {
            using (FileStream fs = new FileStream(Program.FILE_NAME, FileMode.Open))
            {
                Program.BEGIN_POSITION_ON_FAIL = Program.POSITION_IN_FILE; // let the last known position be the begining position if fail happens

                List<Patients> result = new List<Patients>();
                var alphaNumericRegex = new Regex("[a-zA-Z0-9]");
                List<string> bufferRow = new List<string>(); // temp storage for a row
                string bufferWord = String.Empty; // temp storage for words
                bool currentlyReadingWord = false; // determines if program is currently reading a word

                // can resume if need be
                fs.Seek(Program.POSITION_IN_FILE, SeekOrigin.Begin);
                while (fs.Position < fs.Length && result.Count < maxEntries)
                {

                    char character = Convert.ToChar(fs.ReadByte());
                    bool isAlphaNumeric = alphaNumericRegex.IsMatch(character.ToString());

                    if (isAlphaNumeric && currentlyReadingWord)
                    {
                        bufferWord += character;
                        currentlyReadingWord = true;
                    }
                    else if (isAlphaNumeric && !currentlyReadingWord)
                    {
                        // this is a start of a word
                        bufferWord += character;
                        currentlyReadingWord = true;
                    }
                    else
                    {
                        // is not alphaNumeric and if it was currently reading, this is the end of a word
                        if (currentlyReadingWord)
                        {
                            bufferRow.Add(bufferWord);
                            bufferWord = String.Empty;
                        }
                        currentlyReadingWord = false;
                    }
                    if (IsAtEndOfRow(bufferRow))
                    {

                        // at the end of row
                        var model = BufferRowToPatientModel(bufferRow);
                        result.Add(model);

                        bufferRow = new List<string>();
                    }
                }
                Program.POSITION_IN_FILE = fs.Position;
                return result;
            }
        }

        private static Patients BufferRowToPatientModel(List<string> bufferRow)
        {
            string firstName = bufferRow[0];
            string lastName = bufferRow[1];
            int? age = null;
            if (bufferRow.Count == 3)
            {
                if (!int.TryParse(bufferRow[2], out int ageFromParse)) // a little check just to make sure that the age is at least correct.
                {
                    throw new Exception($"The age for {firstName} {lastName} is invalid");
                }
                age = ageFromParse;
            }

            var model = new Patients
            {
                // id will be automatically set by the db
                FirstName = firstName,
                LastName = lastName,
                Age = age
            };
            return model;
        }

        private static bool IsAtEndOfRow(List<string> bufferRow)
        {
            /**
             * NOTE: 
             * Since Age can be null in database,
             * we might want to check if bufferRow.Count == 3 && bufferRow.Last() is numeric
             * or bufferRow.Count == 2 && bufferRow.Last() is not numeric 
             * just need to add the following below as part of the return; 
             * || bufferRow.Count == 2 && !numericRegex.IsMatch(bufferRow.Last())
            **/
            var numericRegex = new Regex("^[0-9]*$");
            return bufferRow.Count == 3 && numericRegex.IsMatch(bufferRow.Last());
        }
    }
}
