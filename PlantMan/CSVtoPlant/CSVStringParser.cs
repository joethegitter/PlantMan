using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;
using System.Reflection;

using PlantMan;
using PlantMan.Plants;

namespace CSVtoPlant
{

    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// Provides methods and properties for parsing structured text files.
    /// </summary>
    public abstract class StructuredTextParser : IDisposable
    {
        private TextReader reader;
        private string[] commentTokens;
        private long lineNumber;
        private bool ownsReader;

        /// <summary>
        /// Initializes a new instance of StructuredTextParser for reading from
        /// the specified stream.
        /// </summary>
        /// <param name="fs">The file stream to read from.</param>
        protected StructuredTextParser(Stream fs)
        {
            this.reader = new StreamReader(fs);
            this.ownsReader = true;
        }

        /// <summary>
        /// Initializes a new instance of StructuredTextParser for reading from
        /// the specified text reader.
        /// </summary>
        /// <param name="reader">The reader used as source.</param>
        protected StructuredTextParser(TextReader reader)
        {
            this.reader = reader;
            this.ownsReader = false;
        }

        /// <summary>
        /// Gets a value indicating wether the end of file has been reached.
        /// </summary>
        public bool EndOfFile
        {
            get
            {
                return (Peek() == -1);
            }
        }

        /// <summary>
        /// Indicates wether blank lines should be ignored.
        /// 
        /// The default value for this property is false.
        /// </summary>
        public bool IgnoreBlankLines { get; set; }

        /// <summary>
        /// Gets the current line number or -1 if there are no character left to read.
        /// </summary>
        public long LineNumber
        {
            get
            {
                return this.lineNumber;
            }
        }

        /// <summary>
        /// Indicates whether leading and trailing white space should be 
        /// trimmed from field values.
        /// 
        /// The default value for this property is false.
        /// </summary>
        public bool TrimWhiteSpace { get; set; }

        /// <summary>
        /// Gets the comment tokens.
        /// 
        /// When a comment token is found at the begining of the line, it indicates
        /// that the line is a comment and should be skipped.
        /// </summary>
        public string[] GetCommentTokens()
        {
            if (commentTokens == null)
                return commentTokens;

            return (string[])commentTokens.Clone();
        }

        /// <summary>
        /// Sets the comment tokens.
        /// 
        /// When a comment token is found at the begining of the line, it indicates
        /// that the line is a comment and should be skipped.
        /// </summary>
        /// <param name="commentTokens">Array of strings indicating the comment tokens.</param>
        public void SetCommentTokens(params string[] commentTokens)
        {
            if (commentTokens == null)
                this.commentTokens = null;

            this.commentTokens = (string[])commentTokens.Clone();
        }

        /// <summary>
        /// Closes the stream reader and file stream as they are no longer 
        /// needed.
        /// </summary>
        public void Close()
        {
            this.lineNumber = -1;
            if (reader != null)
            {
                reader.Dispose();
            }

            if (ownsReader && reader != null)
                reader.Dispose();

            reader = null;
        }

        /// <summary>
        /// Reads the next character without advancing the file cursor.
        /// </summary>
        /// <returns>The code of the read character.</returns>
        public int Peek()
        {
            int c = reader.Peek();

            if (c == -1)
                this.lineNumber = -1;

            return c;
        }

        /// <summary>
        /// Reads the next character and advances the file cursor.
        /// </summary>
        /// <returns>The code of the read character.</returns>
        public int Read()
        {
            int c = reader.Read();

            if (c == -1)
                this.lineNumber = -1;

            return c;
        }

        /// <summary>
        /// When overriden by a derived class, reads the next text line, 
        /// parse it and returns the resulting fields as an array of strings.
        /// </summary>
        /// <returns>All the fields of the current line as an array of strings.</returns>
        public abstract TextFields ReadFields();

        /// <summary>
        /// Reads the next line without parsing for fields.
        /// </summary>
        /// <returns>The next whole line.</returns>
        public string ReadLine()
        {
            if (EndOfFile == true)
                return null;

            string line = reader.ReadLine();
            this.lineNumber += 1;

            if (IgnoreLine(line))
                return ReadLine();
            // bug, you already trimmed white space in IgnoreLine test. Or did you?
            if (TrimWhiteSpace == true)
                return line.Trim();

            return line.TrimEnd(new char[] { '\n', '\r' });
        }

        /// <summary>
        /// Reads until the end of the file stream without parsing for fields.
        /// </summary>
        /// <returns>The whole contents from the current position as 
        /// one string.
        /// </returns>
        public string ReadToEnd()
        {
            this.lineNumber = -1;
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Skips the next line.
        /// </summary>
        public void SkipLine()
        {
            ReadLine();
        }

        /// <summary>
        /// Skips the next given number of lines.
        /// </summary>
        /// <param name="lines">The number of lines to skip.</param>
        public void SkipLines(int lines)
        {
            for (int i = 0; i < lines; i++)
            {
                if (EndOfFile)
                    break;

                SkipLine();
            }
        }

        /// <summary>
        /// Determines wether the given line should be ignored by the reader.
        /// </summary>
        /// <param name="line">The source text line.</param>
        /// <returns>True if the line should be ignored or false otherwise.</returns>
        protected virtual bool IgnoreLine(string line)
        {
            if (line == null)
                return false;

            string str = line.Trim();

            if (IgnoreBlankLines && str.Length == 0)
                return true;

            if (commentTokens != null)
            {
                foreach (string commentToken in this.commentTokens)
                {
                    if (str.StartsWith(commentToken, StringComparison.Ordinal))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from each 
        /// field of the given array.
        /// </summary>
        /// <param name="fields">The array of fields to be trimmed.</param>
        /// <returns>A trimmed version of the array of fields.</returns>
        protected static string[] TrimFields(string[] fields)
        {
            int elems = fields.Length;
            string[] trimmedFields = new string[elems];

            for (int i = 0; i < elems; i++)
            {
                trimmedFields[i] = fields[i].Trim();
            }

            return trimmedFields;
        }

        #region IDisposable implementation

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Close();
            }

            disposed = true;
        }

        ~StructuredTextParser()
        {
            Dispose(false);
        }

        #endregion

    }

    /// <summary>
    /// Provides methods and properties for parsing delimited text files.
    /// </summary>
    /// <example>
    /// <code>
    /// using (DelimitedFieldParser parser = new DelimitedFieldParser("contacts.csv"))
    /// {
    ///     parser.SetDelimiters(',');
    ///     
    ///     while (!parser.EndOfFile)
    ///     {
    ///         TextFields fields = parser.ReadFields();
    ///         // Process fields here using TextFields.GetXXX methods
    ///     }
    /// }
    /// </code>
    /// </example>
    public class DelimitedFieldParser : StructuredTextParser
    {
        private enum DelimitedFieldParserState
        {
            InDelimiter,
            InTextData,
            InQuotedText,
            InClosingQuotes
        }

        private char[] delimiters = { ',' };
        private StringBuilder currentField;
        private List<string> fields = new List<string>();
        private DelimitedFieldParserState state = DelimitedFieldParserState.InDelimiter;
        private CultureInfo culture;

        ///// <summary>
        ///// Initializes a new instance of DelimitedFieldParser for reading from
        ///// the file specified by file name using the default field delimiter.
        ///// </summary>
        ///// <param name="fileName">The name of the file to read from.</param>
        //public DelimitedFieldParser(string fileName)
        //    : this(fileName, CultureInfo.InvariantCulture)
        //{
        //}

        ///// <summary>
        ///// Initializes a new instance of DelimitedFieldParser for reading from
        ///// the file specified by file name using the default field delimiter.
        ///// </summary>
        ///// <param name="fileName">The name of the file to read from.</param>
        ///// <param name="culture">The culture-specific information to use for parsing the fields.</param>
        //public DelimitedFieldParser(string fileName, CultureInfo culture)
        //    : base(fileName)
        //{
        //    this.culture = culture;
        //}

        /// <summary>
        /// Initializes a new instance of DelimitedFieldParser for reading from
        /// the specified stream using the default field delimiter.
        /// </summary>
        /// <param name="fs">The file stream to read from.</param>
        public DelimitedFieldParser(Stream fs)
            : this(fs, CultureInfo.InvariantCulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of DelimitedFieldParser for reading from
        /// the specified stream using the default field delimiter.
        /// </summary>
        /// <param name="fs">The file stream to read from.</param>
        /// <param name="culture">The culture-specific information to use for parsing the fields.</param>
        public DelimitedFieldParser(Stream fs, CultureInfo culture)
            : base(fs)
        {
            this.culture = culture;
        }

        /// <summary>
        /// Initializes a new instance of DelimitedFieldParser for reading from
        /// the specified text reader using the default field delimiter.
        /// </summary>
        /// <param name="reader">The reader used as source.</param>
        public DelimitedFieldParser(TextReader reader)
            : this(reader, CultureInfo.InvariantCulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of DelimitedFieldParser for reading from
        /// the specified text reader using the default field delimiter.
        /// </summary>
        /// <param name="reader">The reader used as source.</param>
        /// <param name="culture">The culture-specific information to use for parsing the fields.</param>
        public DelimitedFieldParser(TextReader reader, CultureInfo culture)
            : base(reader)
        {
            this.culture = culture;
        }

        /// <summary>
        /// If true, parser will assume one or more fields are enclosed in quotation marks.
        /// Quotation marks appearing within the body of a field will cause problems.
        /// If false, parser will choke on quotation marks. Serious limitation.
        /// entire field is enclosed in quotes. Quotes within a field will break that field into multiple fields. 
        /// a delimited file.
        /// 
        /// The default value for this property is false.
        /// </summary>
        public bool HasFieldsEnclosedInQuotes { get; set; }

        /// <summary>
        /// Indicates wether consecutive delimiters are treated as one.
        /// 
        /// The default value for this property is false.
        /// </summary>
        public bool SqueezeDelimiters { get; set; }

        /// <summary>
        /// Gets the delimiters for the parser.
        /// </summary>
        /// <returns></returns>
        public char[] GetDelimiters()
        {
            return (char[])this.delimiters.Clone();
        }

        /// <summary>
        /// Sets the field delimiters for the parser to the specified values.
        /// </summary>
        /// <param name="delimiters">The set of fields delimiters.</param>
        /// <exception cref="ArgumentNullException">Raised if delimiters argument is null.</exception>
        public void SetDelimiters(params char[] delimiters)
        {
            if (delimiters == null)
                throw new ArgumentNullException("delimiters");

            this.delimiters = (char[])delimiters.Clone();

            ValidateDelimiters();
        }

        private void ValidateDelimiters()
        {
            foreach (char delimiter in delimiters)
            {
                if (delimiter == '\r' || delimiter == '\n')
                    throw new ArgumentException("Invalid delimiter.");
            }
        }

        /// <summary>
        /// Reads the next line, parse it and returns the resulting fields 
        /// as an array of strings.
        /// </summary>
        /// <returns>All the fields of the current line as an array of strings.</returns>
        /// <exception cref="MalformedLineException">
        /// Raised when a line cannot be parsed using the specified format.
        /// </exception>
        public override TextFields ReadFields()
        {
            string line = ReadLine();

            string[] fields = ParseLine(line);

            if (TrimWhiteSpace)
                return new TextFields(TrimFields(fields), culture);

            return new TextFields(fields, culture);
        }

        private string[] ParseLine(string line)
        {
            Initialize();

            foreach (char c in line)
                ParseChar(c);

            EndOfLineEvent();

            return ((string[])fields.ToArray());
        }

        private void Initialize()
        {
            NewField();
            fields.Clear();
            state = DelimitedFieldParserState.InDelimiter;
        }

        private void ParseChar(char c)
        {
            if (IsDelimiter(c))
                DelimiterCharEvent(c);
            else if (IsQuote(c))
                QuoteCharEvent(c);
            else
                DefaultCharEvent(c);
        }

        private static bool IsQuote(char c)
        {
            return c == '"';
        }

        private bool IsDelimiter(char c)
        {
            foreach (char delimiter in delimiters)
            {
                if (c == delimiter)
                    return true;
            }
            return false;
        }

        private void DelimiterCharEvent(char c)
        {
            switch (state)
            {
                case DelimitedFieldParserState.InDelimiter:
                    if (!SqueezeDelimiters)
                        AddField();
                    break;

                case DelimitedFieldParserState.InTextData:
                    AddField();
                    NewField();
                    state = DelimitedFieldParserState.InDelimiter;
                    break;

                case DelimitedFieldParserState.InQuotedText:
                    AppendChar(c);
                    break;

                case DelimitedFieldParserState.InClosingQuotes:
                    AddField();
                    NewField();
                    state = DelimitedFieldParserState.InDelimiter;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void QuoteCharEvent(char c)
        {
            if (!HasFieldsEnclosedInQuotes)
            {
                DefaultCharEvent(c);
                return;
            }

            switch (state)
            {
                case DelimitedFieldParserState.InDelimiter:
                    state = DelimitedFieldParserState.InQuotedText;
                    break;

                case DelimitedFieldParserState.InTextData:
                    throw new Exception("Unexpected quote found in line: " + LineNumber.ToString());

                case DelimitedFieldParserState.InQuotedText:
                    state = DelimitedFieldParserState.InClosingQuotes;
                    break;

                case DelimitedFieldParserState.InClosingQuotes:
                    AddField();
                    NewField();
                    state = DelimitedFieldParserState.InQuotedText;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void DefaultCharEvent(char c)
        {
            switch (state)
            {
                case DelimitedFieldParserState.InDelimiter:
                    AppendChar(c);
                    state = DelimitedFieldParserState.InTextData;
                    break;

                case DelimitedFieldParserState.InTextData:
                    AppendChar(c);
                    break;

                case DelimitedFieldParserState.InQuotedText:
                    AppendChar(c);
                    break;

                case DelimitedFieldParserState.InClosingQuotes:
                    throw new Exception("Expected delimiter not found in line: " + LineNumber.ToString());

                default:
                    throw new InvalidOperationException();
            }
        }

        private void EndOfLineEvent()
        {
            switch (state)
            {
                case DelimitedFieldParserState.InDelimiter:
                    AddField();
                    break;

                case DelimitedFieldParserState.InTextData:
                    AddField();
                    break;

                case DelimitedFieldParserState.InQuotedText:
                    throw new Exception("Closing quote was expected in line: " + LineNumber.ToString());

                case DelimitedFieldParserState.InClosingQuotes:
                    AddField();
                    NewField();
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void AppendChar(char c)
        {
            currentField.Append(c);
        }

        private void AddField()
        {
            fields.Add(currentField.ToString());
        }

        private void NewField()
        {
            currentField = new StringBuilder();
        }
    }

    /// <summary>
    /// Provides access to the field values within each line for a StructuredTextParser
    /// or a derivative class.
    /// </summary>
    public class TextFields
    {
        private string[] items;
        private CultureInfo culture;

        /// <summary>
        /// Initializes a new instance of TextFields from an array of strings.
        /// </summary>
        /// <param name="values">The array of string containing the values for each field.</param>
        public TextFields(string[] values)
            : this(values, CultureInfo.InvariantCulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of TextFields from an array of strings indicating
        /// the culture-specific information to use for parsing the fields.
        /// </summary>
        /// <param name="values">The array of string containing the values for each field.</param>
        /// <param name="cultureInfo">The culture-specific information to use for parsing the fields.</param>
        public TextFields(string[] values, CultureInfo cultureInfo)
        {
            this.items = (string[])values.Clone();
            this.culture = cultureInfo;
        }

        /// <summary>
        /// Gets the number of fields in the current record.
        /// </summary>
        public int Count
        {
            get { return items.Length; }
        }

        /// <summary>
        /// Gets an array containing each field as read from the input stream.
        /// </summary>
        /// <returns>An array of strings.</returns>
        public string[] ToArray()
        {
            return (string[])items.Clone();
        }

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation
        /// using the underlying culture-specific list separator to delimit each field.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString()
        {
            return ToString(culture.TextInfo.ListSeparator);
        }

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation
        /// using the supplied separator to delimit each field.
        /// </summary>
        /// <param name="separator"></param>
        /// <returns>The string representation of this instance.</returns>
        public string ToString(string separator)
        {
            return string.Join(separator, items);
        }
    }

    public class Mapper
    {
        /// <summary>
        /// Returns NULL if invalid name passed, Plant could not be created.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        private static Plant PlantFromListOfFields(List<string> list, int lineNumber)
        {
            // we cannot create an actual Plant object yet
            // because we have not yet read the name from the csv
            Plant pl;
            string strVal = "";
            string UNASSIGNED = "<Value has not been assigned.>";
            string start = "Line " + lineNumber.ToString() + " Field ";


            if (list == null) { throw new ArgumentNullException("list"); }

            #region String Properties

            // Name
            strVal = GetStringValue(list, IndexOfFieldInSource.Name, lineNumber);
            if (strVal == "")
            {
                Debug.WriteLine(start + "Name - Name field empty, record will be skipped.");
                try
                {
                    throw new InvalidOperationException(start + "Name - INVALID NAME, record will be skipped.");
                }
                catch
                {
                    // we threw the exception for visibility during testing, we can proceed
                    return null;
                }
            }

            try
            {
                pl = new Plant(strVal);
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.WriteLine(start + "Name? - could not create Plant, record will be skipped.");
                    throw new InvalidOperationException(start + "Name? - could not create Plant, record will be skipped; name = {" + strVal + "}", ex);
                }
                catch
                {
                    // we only threw exception for visibility, we can proceed
                    return null;
                }
            }

            // URL
            strVal = GetStringValue(list, IndexOfFieldInSource.URL, lineNumber);
            if (strVal == "")
            {
                Debug.WriteLine(start + "URL - was empty, will become Unassigned.");
                pl.URL = UNASSIGNED;
            }
            else
            {
                pl.URL = strVal;
            }

            // ScientificName
            strVal = GetStringValue(list, IndexOfFieldInSource.ScientificName, lineNumber);
            if (strVal == "")
            {
                Debug.WriteLine(start + "ScientificName - was empty, will become Unassigned.");
                pl.ScientificName = UNASSIGNED;
            }
            else
            {
                pl.ScientificName = strVal;
            }

            // NotableVisuals
            strVal = GetStringValue(list, IndexOfFieldInSource.NotableVisuals, lineNumber);
            pl.NotableVisuals = strVal;

            // Notes
            strVal = GetStringValue(list, IndexOfFieldInSource.Notes, lineNumber);
            pl.Notes = strVal;

            // CNPS_Soil
            strVal = GetStringValue(list, IndexOfFieldInSource.CNPS_Soil, lineNumber);
            if (strVal == "")
            {
                Debug.WriteLine(start + "ScientificName - was empty, will become Unassigned.");
                pl.CNPS_Soil = UNASSIGNED;
            }
            else
            {
                pl.CNPS_Soil = strVal;
            }

            #endregion String Properties

            #region Numeric Fields
            // numerics

            // If GetDecimalValue finds a number it returns true and decVal holds that number.
            // If it finds UnivUnknown it returns true, and decVal holds Decimal.MinVal.
            // If it finds null/empty/whitespace/not_a_number, it returns false, and decVal holds zero.
            Decimal decVal;
            Plant.DecimalLike adl = new Plant.DecimalLike();

            if (GetDecimalValue(list, IndexOfFieldInSource.MaxHeight, out decVal, lineNumber))
            {
                // succes was true, so field held a real value or UnivUnknown
                // if field was UnivUnknown, decVal will contain decimal.MinValue
                if (decVal == Decimal.MinValue)
                {
                    adl = new Plant.DecimalLike(true);
                }
                else
                {
                    adl = new Plant.DecimalLike(decVal);
                }
            }
            else
            {
                // success was false, so field held an empty/null/whitespace value
                Debug.WriteLine(start + "MaxHeight - not a meaningful val, will become Unassigned.");
                adl = new Plant.DecimalLike();
            }
            pl.MaxHeight = adl;


            if (GetDecimalValue(list, IndexOfFieldInSource.MinHeight, out decVal, lineNumber))
            {
                // succes was true, so field held a real value or UnivUnknown
                // if field was UnivUnknown, decVal will contain decimal.MinValue
                if (decVal == Decimal.MinValue)
                {
                    adl = new Plant.DecimalLike(true);
                }
                else
                {
                    adl = new Plant.DecimalLike(decVal);
                }
            }
            else
            {
                // success was false, so field held an empty/null/whitespace value
                Debug.WriteLine(start + "MaxHeight - not a meaningful val, will become Unassigned.");
                adl = new Plant.DecimalLike();
            }
            pl.MinHeight = adl;


            if (GetDecimalValue(list, IndexOfFieldInSource.MaxWidth, out decVal, lineNumber))
            {
                // succes was true, so field held a real value or UnivUnknown
                // if field was UnivUnknown, decVal will contain decimal.MinValue
                if (decVal == Decimal.MinValue)
                {
                    adl = new Plant.DecimalLike(true);
                }
                else
                {
                    adl = new Plant.DecimalLike(decVal);
                }
            }
            else
            {
                // success was false, so field held an empty/null/whitespace value
                Debug.WriteLine(start + "MaxWidth - not a meaningful val, will become Unassigned.");
                adl = new Plant.DecimalLike();
            }
            pl.MaxWidth = adl;


            if (GetDecimalValue(list, IndexOfFieldInSource.MinWidth, out decVal, lineNumber))
            {
                // succes was true, so field held a real value or UnivUnknown
                // if field was UnivUnknown, decVal will contain decimal.MinValue
                if (decVal == Decimal.MinValue)
                {
                    adl = new Plant.DecimalLike(true);
                }
                else
                {
                    adl = new Plant.DecimalLike(decVal);
                }
            }
            else
            {
                // success was false, so field held an empty/null/whitespace value
                Debug.WriteLine(start + "MinWidth - not a meaningful val, will become Unassigned.");
                adl = new Plant.DecimalLike();
            }
            pl.MinWidth = adl;


            if (GetDecimalValue(list, IndexOfFieldInSource.MaxRainfallInches, out decVal, lineNumber))
            {
                // succes was true, so field held a real value or UnivUnknown
                // if field was UnivUnknown, decVal will contain decimal.MinValue
                if (decVal == Decimal.MinValue)
                {
                    adl = new Plant.DecimalLike(true);
                }
                else
                {
                    adl = new Plant.DecimalLike(decVal);
                }
            }
            else
            {
                // success was false, so field held an empty/null/whitespace value
                Debug.WriteLine(start + "MaxRainfallInches - not a meaningful val, will become Unassigned.");
                adl = new Plant.DecimalLike();
            }
            pl.MaxRainfallInches = adl;


            if (GetDecimalValue(list, IndexOfFieldInSource.MaxSoilpH, out decVal, lineNumber))
            {
                // succes was true, so field held a real value or UnivUnknown
                // if field was UnivUnknown, decVal will contain decimal.MinValue
                if (decVal == Decimal.MinValue)
                {
                    adl = new Plant.DecimalLike(true);
                }
                else
                {
                    adl = new Plant.DecimalLike(decVal);
                }
            }
            else
            {
                // success was false, so field held an empty/null/whitespace value
                Debug.WriteLine(start + "MaxSoilpH - not a meaningful val, will become Unassigned.");
                adl = new Plant.DecimalLike();
            }
            pl.MaxSoilpH = adl;


            if (GetDecimalValue(list, IndexOfFieldInSource.MinSoilpH, out decVal, lineNumber))
            {
                // succes was true, so field held a real value or UnivUnknown
                // if field was UnivUnknown, decVal will contain decimal.MinValue
                if (decVal == Decimal.MinValue)
                {
                    adl = new Plant.DecimalLike(true);
                }
                else
                {
                    adl = new Plant.DecimalLike(decVal);
                }
            }
            else
            {
                // success was false, so field held an empty/null/whitespace value
                Debug.WriteLine(start + "MinSoilpH - not a meaningful val, will become Unassigned.");
                adl = new Plant.DecimalLike();
            }
            pl.MinSoilpH = adl;


            if (GetDecimalValue(list, IndexOfFieldInSource.MinRainfallInches, out decVal, lineNumber))
            {
                // succes was true, so field held a real value or UnivUnknown
                // if field was UnivUnknown, decVal will contain decimal.MinValue
                if (decVal == Decimal.MinValue)
                {
                    adl = new Plant.DecimalLike(true);
                }
                else
                {
                    adl = new Plant.DecimalLike(decVal);
                }
            }
            else
            {
                // success was false, so field held an empty/null/whitespace value
                Debug.WriteLine(start + "MinRainfallInches - not a meaningful val, will become Unassigned.");
                adl = new Plant.DecimalLike();
            }
            pl.MinRainfallInches = adl;


            if (GetDecimalValue(list, IndexOfFieldInSource.MinWinterTempF, out decVal, lineNumber))
            {
                // succes was true, so field held a real value or UnivUnknown
                // if field was UnivUnknown, decVal will contain decimal.MinValue
                if (decVal == Decimal.MinValue)
                {
                    adl = new Plant.DecimalLike(true);
                }
                else
                {
                    adl = new Plant.DecimalLike(decVal);
                }
            }
            else
            {
                // success was false, so field held an empty/null/whitespace value
                Debug.WriteLine(start + "MinWinterTempF - not a meaningful val, will become Unassigned.");
                adl = new Plant.DecimalLike();
            }
            pl.MinWinterTempF = adl;


            #endregion Numeric Fields

            #region Single Enum Properties

            // Single Enum values
            // indexOfCanonicalValue is the index into the specific array of strings
            // containing the valid canonical values for this field. If the value in 
            // the field could not be mapped to a canonical value, the result will be
            // this field's version of Unassigned.
            int indexOfCanonicalValue;
            string canonicalVal = "";

            // Type
            PlantType tt = PlantType.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.Type, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.PlantType[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "Annual_herb", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Annual_herb;
            }
            else if (string.Equals(canonicalVal, "Bush", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Bush;
            }
            else if (string.Equals(canonicalVal, "Shade", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Fern;
            }
            else if (string.Equals(canonicalVal, "Grass", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Grass;
            }
            else if (string.Equals(canonicalVal, "Perennial_herb", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Perennial_herb;
            }
            else if (string.Equals(canonicalVal, "Tree", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Tree;
            }
            else if (string.Equals(canonicalVal, "Vine", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Vine;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.Unknown;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                tt = PlantType.NotApplicable;
            }
            pl.Type = tt;

            // WateringRequirement
            WateringType wt = WateringType.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.WateringRequirements, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.WateringType[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "Drought_tolerant", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Drought_tolerant;
            }
            else if (string.Equals(canonicalVal, "Infrequent", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Infrequent;
            }
            else if (string.Equals(canonicalVal, "Moderate", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Moderate;
            }
            else if (string.Equals(canonicalVal, "Occasional", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Occasional;
            }
            else if (string.Equals(canonicalVal, "Regular", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Regular;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.Unknown;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                wt = WateringType.NotApplicable;
            }
            pl.WateringRequirement = wt;

            // CNPS_Drainage
            CNPS_Drainage ct = CNPS_Drainage.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.CNPS_Drainage, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.CNPS_Drainage[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "Fast", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Fast;
            }
            else if (string.Equals(canonicalVal, "Medium", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Medium;
            }
            else if (string.Equals(canonicalVal, "Slow", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Slow;
            }
            else if (string.Equals(canonicalVal, "Standing", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Standing;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.Unknown;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                ct = CNPS_Drainage.NotApplicable;
            }
            pl.CNPS_Drainage = ct;

            // Yes / No

            // AttractsBirds
            YesNoMaybe yn = YesNoMaybe.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.AttractsBirds, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.YesNoMaybe[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "No", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.No;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.NotApplicable;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unknown;
            }
            else if (string.Equals(canonicalVal, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Yes;
            }
            pl.AttractsBirds = yn;

            // AttractsButterflies
            yn = YesNoMaybe.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.AttractsButterflies, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.YesNoMaybe[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "No", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.No;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.NotApplicable;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unknown;
            }
            else if (string.Equals(canonicalVal, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Yes;
            }
            pl.AttractsButterflies = yn;


            // AttractsHummingbirds
            yn = YesNoMaybe.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.AttractsHummingbirds, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.YesNoMaybe[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "No", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.No;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.NotApplicable;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unknown;
            }
            else if (string.Equals(canonicalVal, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Yes;
            }
            pl.AttractsHummingbirds = yn;

            // AttractsNativeBees
            yn = YesNoMaybe.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.AttractsNativeBees, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.YesNoMaybe[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "No", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.No;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.NotApplicable;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unknown;
            }
            else if (string.Equals(canonicalVal, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Yes;
            }
            pl.AttractsNativeBees = yn;

            // DocumentedAsGoodInContainers
            yn = YesNoMaybe.Unassigned;
            indexOfCanonicalValue = GetIndexOfSingleEnumValue(list, IndexOfFieldInSource.DocumentedAsGoodInContainers, lineNumber);
            if (indexOfCanonicalValue == -1) { indexOfCanonicalValue = 0; }  // HACK: we're assuming unassigned = 0
            canonicalVal = ValueOfFieldInSource.YesNoMaybe[indexOfCanonicalValue];
            if (string.Equals(canonicalVal, "No", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.No;
            }
            else if (string.Equals(canonicalVal, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.NotApplicable;
            }
            else if (string.Equals(canonicalVal, "Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unassigned;
            }
            else if (string.Equals(canonicalVal, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Unknown;
            }
            else if (string.Equals(canonicalVal, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                yn = YesNoMaybe.Yes;
            }
            pl.DocumentedAsGoodInContainers = yn;

            #endregion Single Enum Properties

            #region Multiple-Value Enum Properties

            // Multiple Value Properties
            strVal = GetStringValue(list, IndexOfFieldInSource.FloweringMonths, lineNumber);
            if (strVal == ValueOfFieldInSource.UnivUnassigned)
            {
                pl.FloweringMonths = FloweringMonth.Unassigned;
            }
            else if (strVal == ValueOfFieldInSource.UnivUnknown)
            {
                pl.FloweringMonths = FloweringMonth.Unknown;
            }
            else if (strVal == ValueOfFieldInSource.UnivNotApplicable)
            {
                pl.FloweringMonths = FloweringMonth.NotApplicable;
            }
            else
            {
                FloweringMonth myVal;
                if (Enum.TryParse<FloweringMonth>(strVal, out myVal))
                {
                    pl.FloweringMonths = myVal;
                }
                else
                {
                    pl.FloweringMonths = FloweringMonth.Unassigned;
                    Debug.Assert(false, start + "Could not parse FloweringMonths string: " + strVal);
                }

                // SunTypes
                // JOE
            }

            #endregion Multiple-Value Enum Properties

            return pl;

        }

        public static Dictionary<string, Plant> ParseDataIntoStore(object currentApp, string resourceName, bool dataHasHeaderLine = false)
        {
            // create a dictionary of KeyValuePairs, where KVP.Key is name of plant,
            // and KVP.Value is the actual Plant object. Well, intialize one.
            Dictionary<string, Plant> PlantDic = new Dictionary<string, Plant>();

            Assembly assembly = currentApp.GetType().GetTypeInfo().Assembly;
            string resource = resourceName;
            string assName = assembly.GetName().ToString();

            // joe, if this code isn't working, make it work

            string[] resNames = assembly.GetManifestResourceNames();

            // TODO:
            // HACK:
            // fuck it, i boned this code a bit. Just insert the correct rsource name now
            // resource = "UWP_TestBed.CSVData.PlantsFixed.csv";


            // Read stream and remove quotes                       
            string theUnQuotedVersion = "";
            using (Stream streamCSV = assembly.GetManifestResourceStream(resource))
            {
                // TODO: joe go get that code which verified the resource existed
                if (streamCSV == null)
                {
                    throw new ArgumentException("Could not open a stream from that resource. Misspelled?");
                }

                theUnQuotedVersion = UnQuoter.UnQuote(streamCSV);
            }

            using (Stream stream = UnQuoter.GenerateStreamFromString(theUnQuotedVersion))
            {
                using (DelimitedFieldParser parser = new DelimitedFieldParser(stream))
                {
                    // set the properties of the DelimitedFieldParser object
                    parser.SetDelimiters(new char[] { ',' });   // NEW added
                    parser.HasFieldsEnclosedInQuotes = true;    // NEW addeds

                    bool headerConsumed = false;

                    int lineCounter = 0;

                    while (!parser.EndOfFile)
                    {
                        // Read the line into "TextFields" (specific to this parser)
                        TextFields tfs = parser.ReadFields();
                        lineCounter++;

                        // if useHeader, ignore over first line
                        if (dataHasHeaderLine && lineCounter == 1) { headerConsumed = true; continue; }

                        // convert TextFields into List of strings
                        string[] tfa = tfs.ToArray();
                        List<string> temp = new List<string>(tfa);
                        //List<string> temp = tfa.ToList<string>();

                        // Put the quotes back into each field
                        List <string> theFields = new List<string>();
                        foreach (string s in temp)
                        {
                            string y = UnQuoter.ReQuoteField(s);
                            Debug.WriteLine("Line " + lineCounter.ToString() + ": " + y);
                            theFields.Add(y);
                        }

                        Plant pl = PlantFromListOfFields(theFields, lineCounter);

                        if (pl == null)
                        {
                            throw new InvalidOperationException("PlantFromTextFields returned a null Plant.");
                        }

                        if (pl.Name == null)
                        {
                            throw new InvalidOperationException("PlantFromTextFields returned a Plant with a null name.");
                        }

                        try
                        {
                            PlantDic.Add(pl.Name, pl);
                        }
                        catch (ArgumentException ae)
                        {
                            throw new InvalidOperationException("Tried to add a Plant already in the Dictionary. See Inner.", ae);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("Exception thrown adding Plant to Dictionary. See Inner.", ex);
                        }
                    }
                }
            }
            return PlantDic;
        }

        /// <summary>
        /// If the value in the field cannot resolves to empty string, returns Decimal.Null, 
        /// indicating "Unassigned". If string resolves to UnivUnknown, returns Decimal.MinVal.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="outVal"></param>
        /// <param name="lineNumber"></param>
        /// <param name="suppressException"></param>
        /// <returns>If value was null/empty/whitespace/not recognized as meaningful, returns Decimal.Null, indicating "Unassigned". 
        /// If val was UnivUnknown, returns Decimal.MinVal</returns>
        private static bool GetDecimalValue(List<string> list, int index, out decimal outVal, int lineNumber, bool suppressException = false)
        {
            string start = "ParseError: Line " + lineNumber.ToString() + " Field " + index.ToString() + " - ";
            decimal retDec = decimal.Zero;
            bool retBool = false;

            if (list[index] == null)
            {
                Debug.Assert(false, start + "value in list == null");
                outVal = decimal.Zero;
                return false;
            }

            if (string.IsNullOrWhiteSpace(list[index]))
            {
                Debug.Assert(false, start + "value in list == whitespace");
                outVal = decimal.Zero;
                return false;
            }

            if (string.IsNullOrEmpty(list[index]))
            {
                Debug.Assert(false, start + "value in list != null && !IsNullOrEmpty(value)");
                outVal = decimal.Zero;
                return false;
            }

            // if we are here, we have chars of some type. let's see if they are numbers
            bool success = false;
            decimal outie;
            string str = list[index].Trim();

            if (str == ValueOfFieldInSource.UnivUnknown)
            {
                outVal = Decimal.MinValue;
                return true;
            }

            success = decimal.TryParse(str, out outie);
            if (!success)
            {
                Debug.Assert(false, start + "not a number, value in list = " + str);
                retDec = decimal.Zero;
                retBool = false;
            }
            else
            {
                retDec = outie;
                retBool = true;
            }
            outVal = retDec;
            return retBool;
        }

        private static string GetStringValue(List<string> list, int index, int lineNumber, bool suppressException = false)
        {
            string start = "ParseError: Line " + lineNumber.ToString() + " Field " + index.ToString() + " - ";

            string retVal = "";

            if (list[index] == null)
            {
                Debug.WriteLine(start + "field == null.");
            }
            else if (string.IsNullOrWhiteSpace(list[index]))
            {
                Debug.WriteLine(start + "field == whitespace only.");
            }
            else if (string.IsNullOrEmpty(list[index]))
            {
                Debug.WriteLine(start + "field == string.Empty().");
            }

            string ARealString = list[index].Trim();
            if (string.IsNullOrEmpty(ARealString))
            {
                Debug.WriteLine(start + "is Empty after Trim().");
                retVal = "";
            }
            else
            {
                retVal = ARealString;
            }
            return retVal;
        }

        /// <summary>
        /// Reads the text value of a field representing a value from an enjm... JOE write
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="suppressException"></param>
        /// <returns>-1 for Unassigned </returns>
        private static int GetIndexOfSingleEnumValue(List<string> list, int index, int lineNumber, bool suppressException = false)
        {
            string start = "ParseNotation: Line " + lineNumber.ToString() + " Field " + index.ToString() + " - ";

            int indexIntoArray = -1;
            string trimmed = "";

            // Translate all variants of null or empty or whitespace to "";
            // but articulate what the actual value was here.
            if (list[index] == null)
            {
                Debug.Assert(false, start + "value in list == null");
                trimmed = "";
            }
            else if (string.IsNullOrWhiteSpace(list[index]))
            {
                Debug.Assert(false, start + "value in list == whitespace");
                trimmed = "";
            }
            else if (string.IsNullOrEmpty(list[index]))
            {
                Debug.Assert(false, start + "value in list == not null but IsNullOrEmpty() fails.");
                trimmed = "";
            }
            else
            {
                trimmed = list[index].Trim();
            }

            // Let each enum type decide how to handle empty value.

            // Determine which static array we should look in to find the value stored in list[index].
            // Once you determine which array to look in, return the index of that array where the field
            // value was found. If not found, return -1.
            switch (index)
            {
                case IndexOfFieldInSource.Type:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.PlantType, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.CNPS_Drainage:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.CNPS_Drainage, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        // that was a valid value for this field, so return the
                        // index into array of valid values for this field.
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.SunRequirement:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.SunType, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.WateringRequirements:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.SunType, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.AttractsBirds:
                case IndexOfFieldInSource.AttractsButterflies:
                case IndexOfFieldInSource.AttractsHummingbirds:
                case IndexOfFieldInSource.AttractsNativeBees:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.SunType, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.Alameda:
                case IndexOfFieldInSource.Contra_Costa:
                case IndexOfFieldInSource.Marin:
                case IndexOfFieldInSource.Napa:
                case IndexOfFieldInSource.Santa_Clara:
                case IndexOfFieldInSource.San_Francisco:
                case IndexOfFieldInSource.San_Mateo:
                case IndexOfFieldInSource.Solano:
                case IndexOfFieldInSource.Sonoma:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.SunType, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                case IndexOfFieldInSource.DocumentedAsGoodInContainers:
                    indexIntoArray = Array.BinarySearch<string>(ValueOfFieldInSource.YesNoMaybe, trimmed, StringComparer.OrdinalIgnoreCase);
                    if (indexIntoArray > -1)
                    {
                        return indexIntoArray;
                    }
                    else
                    {
                        indexIntoArray = -1;
                    }
                    break;

                default:
                    {
                        Debug.WriteLine("Umappable value at Field Index: " + index.ToString());
                        if (!suppressException) throw new InvalidOperationException("Umappable value at Field Index: " + index.ToString());
                        return indexIntoArray; // should still be -1
                    }
            }
            return indexIntoArray;
        }

    }
}

