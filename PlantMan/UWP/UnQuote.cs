using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using TextFileParsers;

namespace JKCo.Utility
{
    public class UnQuoter
    {
        static string Q = "\"";
        static string Q2 = Q + Q;
        static string Q3 = Q + Q + Q;
        static string Q4 = Q + Q + Q + Q;

        static string NullRep = "[{null}]";
        static string OpenQuoteRep = "[{openQ}]";
        static string CloseQuoteRep = "[{closeQ}]";
        static string SingleQuoteRep = "[{singleQ}]";
        static string DoubleQuoteRep = "[{doubleQ}]";
        static string Nothing = "";

        static string nl = Environment.NewLine;

        /// <summary>
        /// Parses a stream line by line, replacing quotation marks with corresponding
        /// symbols which can be turned back in to quotation marks using ReQuote().
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fieldDelimiter"></param>
        /// <returns></returns>
        /// <remarks>
        /// Some Delimited Text File parsers choke on fields which contain 
        /// quotation marks within the content of a field -ie, not as the first or last
        /// character "wrapping" the content between the delimiters. This is because many
        /// DTF emitters - like spreadsheets which emit comma separated files - place quotations
        /// marks at either end of a field whose content contains the same char/str as the delimiter
        /// (in this case, commas). This helps DTF parsers to distinguish between the delimiter char/str
        /// and actual content. However, many DTF parsers are then confused by quotation marks which
        /// appear within the content of a field, and they intrepret these as the start or end of a field.
        /// To avoid this, use the UnQuote method to replace all non-wrapping quotation marks within 
        /// a DTF with smart placeholders. After parsing the file into fields, you can call ReQuote on 
        /// each field to replace the smart placeholders with the appropriate quotation mark representations.
        /// Note that multiple quotation marks in a row within content will always be reduced to a single 
        /// quotation mark.
        /// </remarks>
        static public string UnQuote(Stream stream, String fieldDelimiter = ",")
        {
            string returnString = "";
            string Delim = fieldDelimiter;

            using (DelimitedFieldParser parser = new DelimitedFieldParser(stream))
            {
                while (!parser.EndOfFile)
                {
                    #region Documentation
                    // Legend:
                    // Nothing in these rules or instructions means apostrophe. Ever.
                    // The letter Q by itself means a single instance of the quote glyph (")

                    // Rules: 
                    // JOE: this is wrong, remove it.
                    // 1. we do not accept a final (post-parse) value of Quote+Quote. Final values of Q+Q are replaced with nothing.
                    // 2. corresponding to Rule 1, four quotes -> two quotes = nothing.

                    // Instructions:
                    // 1. Use Parser to read line from stream. In that line:
                    // 2. Replace any instance of four Qs in a row with nothing (delete); recursively.

                    // Beginning and end of line:
                    // 3. if first four chars in line are "QQQ," replace with OpenQuoteRepSymbol + SingleQuoteRepSymbol + CloseQuoteSymbol + Delim ( intended: [StartOfLine]", )
                    // 4. if last four chars in line are ",QQQ" replace with Delim + OpenQuoteRepSymbol + SingleQuoteRepSymbol + CloseQuoteSymbol ( intended: ,"[EOL] )
                    
                    // JOE: this can't happen, it would end up being wrapped in quotes, so """"
                    // 5. if first three chars in line are "QQ," replace with NullRepSymbol + Delim ( intended: [StartOfLine]"", )
                    // 6. if last three chars in line are ",QQ" replace with Delim + NullRepSymbol ( intended: ,""[EOL] )

                    // JOE: case seven will never be emitted. A single quote in a cell emits three quotes; same case as 3, in fact
                    // 7. If first two chars in line are "Q," replace with SingleQuoteRepSymbol + Delim ( intended: [SOL]", )
                    // JOE: wrong, same case as 4
                    // 8. If last two chars in line are ",Q" replace with Delim + SingleQuoteRepSymbol ( intended: ,"[EOL] )

                    // Open and Closing Quotes for entire field:
                    // JOE, two quotes in a field will be wrapped to four quotes. 
                    // 9. Replace any instance of ",QQ," with Delim + NullRepSymbol + Delim ( intended: null field )
                    // JOE: remove case 10, cannot happen
                    // 10. Replace every ",Q," with Delim + SingleQuoteRepSymbol + Delim ( intended: field = Q )
                    // JOE: case 11 is actually valid and should not be changed.Occurs when cell contains [ Hello,"Dolly ]
                    // 11. Replace every ",Q" with  Delim + OpenQuoteRepSymbol
                    // 12. Replace every "Q," with CloseQuoteRepSymbol + Delim

                    // Now we have no startLine or endLine cases, and no valid open or close quotes.
                    // All other quotes are intended to be in text of field. Any instance of QQ is an artifact
                    // of the CSV translator (or the user being weird, and we don't care), and we want that QQ
                    // to result in a Q in the end. Any single Q remaining after we replace QQ needs to be examined;
                    // I can't think of a case where this should be valid.

                    // 13. Replace any instance of two Q's in a row with DoubleQRepSymbol, recursively
                    // 14. Replace any instance of single Q with SingleQRepSymbol (should not need to recurse).

                    // Now we should have no Qs at all. However, we are about to parse the line again
                    // with ParserTwo, and it needs to see single quotes around any field which contains
                    // commas. So replace OpenQuoteRepSymbol and CloseQuoteRepSymbol with real quotes.
                    // Open and Close quotes will not end up in the resulting TextFields.

                    // 15. Replace any instance of OpenQuoteRepSymbol or CloseQuoteSymbol with Q

                    #endregion Documentation

                    // Prep
                    string target = "";
                    string repl = "";

                    // 1. Use Parser object ParserPrime to read line from file. In that line:
                    string wholeLine = parser.ReadLine();

                    // if the entire line was empty, throw exception.
                    // TODO: throw exception
                    if (wholeLine == null)
                    {
                        throw new Exception("Entire line was null.");
                    }

                    // 2. Replace any instance of four Qs in a row with nothing (delete); recursively.
                    target = Q4;
                    while (wholeLine.Contains(target))
                    {
                        string modLine = wholeLine.Replace(target, Nothing);
                    }

                    // Beginning and end of line:
                    // 3. if first four chars in line are "QQQ," replace with 
                    // OpenQuoteRepSymbol + SingleQuoteRepSymbol + CloseQuoteSymbol + Delim ( intended: [StartOfLine]", )
                    target = Q3 + Delim;
                    repl = OpenQuoteRep + SingleQuoteRep + CloseQuoteRep + Delim;
                    if (wholeLine.StartsWith(target))
                    {
                        string restOfLine = wholeLine.Substring(target.Length - 1);
                        wholeLine = repl + restOfLine;
                    }

                    // 4. if last four chars in line are ",QQQ" replace with 
                    // Delim + OpenQuoteRepSymbol + SingleQuoteRepSymbol + CloseQuoteSymbol ( intended: ,"[EOL] )
                    target = Delim + Q3;
                    repl = Delim + OpenQuoteRep + SingleQuoteRep + CloseQuoteRep;
                    if (wholeLine.EndsWith(target))
                    {
                        string beginOfLine = wholeLine.Substring(0, wholeLine.Length - target.Length);
                        wholeLine = beginOfLine + repl;
                    }

                    // 5. if first three chars in line are "QQ," replace with NullRepSymbol + Delim ( intended: [StartOfLine]"", )
                    target = Q2 + Delim;
                    repl = NullRep + Delim;
                    if (wholeLine.StartsWith(target))
                    {
                        string restOfLine = wholeLine.Substring(target.Length - 1);
                        wholeLine = repl + restOfLine;
                    }

                    // 6. if last three chars in line are ",QQ" replace with Delim + NullRepSymbol ( intended: ,""[EOL] )
                    target = Delim + Q2;
                    repl = Delim + NullRep;
                    if (wholeLine.EndsWith(target))
                    {
                        string beginOfLine = wholeLine.Substring(0, wholeLine.Length - target.Length);
                        wholeLine = beginOfLine + repl;
                    }

                    // 7. If first two chars in line are "Q," replace with SingleQuoteRepSymbol + Delim ( intended: [SOL]", )
                    target = Q + Delim;
                    repl = SingleQuoteRep + Delim;
                    if (wholeLine.StartsWith(target))
                    {
                        string restOfLine = wholeLine.Substring(target.Length - 1);
                        wholeLine = repl + restOfLine;
                    }

                    // 8. If last two chars in line are ",Q" replace with Delim + SingleQuoteRepSymbol ( intended: ,"[EOL] )
                    target = Delim + Q;
                    repl = Delim + SingleQuoteRep;
                    if (wholeLine.EndsWith(target))
                    {
                        string beginOfLine = wholeLine.Substring(0, wholeLine.Length - target.Length);
                        wholeLine = beginOfLine + repl;
                    }

                    // Open and Closing Quotes for entire field:
                    // 9. Replace any instance of ",QQ," with Delim + NullRepSymbol + Delim ( intended: null field )
                    target = Delim + Q2 + Delim;
                    repl = Delim + NullRep + Delim;
                    while (wholeLine.Contains(target))
                    {
                        wholeLine = wholeLine.Replace(target, repl);
                    }

                    // 10. Replace every ",Q," with Delim + SingleQuoteRepSymbol + Delim ( intended: field = Q )
                    target = Delim + Q + Delim;
                    repl = Delim + SingleQuoteRep + Delim;
                    while (wholeLine.Contains(target))
                    {
                        wholeLine = wholeLine.Replace(target, repl);
                    }

                    // 11. Replace every ",Q" with  Delim + OpenQuoteRepSymbol
                    target = Delim + Q;
                    repl = Delim + OpenQuoteRep;

                    while (wholeLine.Contains(target))
                    {
                        wholeLine = wholeLine.Replace(target, repl);
                    }

                    // 12. Replace every "Q," with CloseQuoteRepSymbol + Delim
                    target = Q + Delim;
                    repl = CloseQuoteRep + Delim;
                    while (wholeLine.Contains(target))
                    {
                        wholeLine = wholeLine.Replace(target, repl);
                    }

                    // 13. Replace any instance of two Q's in a row with DoubleQRepSymbol, recursively
                    target = Q2;
                    repl = DoubleQuoteRep;
                    while (wholeLine.Contains(target))
                    {
                        wholeLine = wholeLine.Replace(target, repl);
                    }

                    // 14. Replace any instance of single Q with SingleQRepSymbol (should not need to recurse).
                    target = Q2;
                    repl = SingleQuoteRep;
                    while (wholeLine.Contains(target))
                    {
                        wholeLine = wholeLine.Replace(target, repl);
                    }

                    // Now we should have no Qs at all. However, the caller is about to parse
                    // this data again, and it needs to see single quotes around any field which contains
                    // commas. So replace any OpenQuoteRepSymbol and CloseQuoteRepSymbol with real quotes.
                    // Open and Close quotes will not end up in the resulting TextFields.

                    // 15. Replace any instance of OpenQuoteRepSymbol or CloseQuoteSymbol with Q
                    target = OpenQuoteRep;
                    repl = Q;
                    wholeLine = wholeLine.Replace(target, repl);

                    target = CloseQuoteRep;
                    repl = Q;
                    wholeLine = wholeLine.Replace(target, repl);

                    // Now add the line to the string, and start over
                    returnString += wholeLine + nl;

                    //// DebugOut
                    //System.Diagnostics.Debug.WriteLine("PreParse Line " + parser.LineNumber.ToString() +
                    //    ", " + wholeLine);
                }
            }
            return returnString;
        }

        /// <summary>
        /// Takes a "field" as parsed by UnQuote and replaces any tokens in it with originally intended content.
        /// </summary>
        /// <param name="UnQuotedString"></param>
        /// <returns></returns>
        static public string ReQuoteField(String UnQuotedString)
        {
            // Now in each TextField, we undo our changes. For each text field:
            // 1. If the field contains ONLY the NullRepSymbol, return an Empty string.
            // 2. Assert that a field does not contain both NullRep AND content.
            // 3. If field contains ONLY the DoubleQRepSymbol, replace with Nothing (was ,"",  in the original)
            // 4. Replace all DoubleQRepSymbols with a single quote.
            // 5. Replace remaining SingleQRepSymbol with single quote.

            // If we were sent null or empty or whitespace only string, just return it unmodified
            if (String.IsNullOrEmpty(UnQuotedString) || String.IsNullOrWhiteSpace(UnQuotedString)) return UnQuotedString;

            // 1. If the field contains ONLY the NullRepSymbol, return an Empty string.
            if (UnQuotedString == NullRep) return String.Empty;

            // 2. Assert that a field does not contain both NullRep AND content.
            if (UnQuotedString.Contains(NullRep))
            {
                System.Diagnostics.Debug.Assert(false, "Field contains both NullReplacement symbol and content.");
                // remove the nullrep anyway
                UnQuotedString.Replace(NullRep, Nothing);
            }

            // 3. If field contains ONLY the DoubleQRepSymbol, replace with Nothing (was ,"",  in the original)
            if (UnQuotedString == DoubleQuoteRep) return String.Empty;

            // 4. Replace all DoubleQRepSymbols with a single quote.
            UnQuotedString = UnQuotedString.Replace(DoubleQuoteRep, Q);

            // 5. Replace remaining SingleQRepSymbol with single quote.
            UnQuotedString = UnQuotedString.Replace(SingleQuoteRep, Q);

            return UnQuotedString;
        }

        /// <summary>
        /// Utility Method for creating a Stream from a String.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


    }
}
