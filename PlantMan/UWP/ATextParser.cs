// StructuredTextParser.cs
// 
// Author: Mauricio Trícoli <mtricoli@live.com.ar>
// Heavily Modified for PCL by: Mike's Friend Joe
// Modified for radio by putting it on a large board 
// and hammering a nail through it.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

namespace TextFileParsers
{
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
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based field ordinal.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="IndexOutOfRange">
        /// The index passed was outside the range of 0 through FieldCount.
        /// </exception>
        public string this[int ordinal]
        {
            get { return items[ordinal]; }
        }

        /// <summary>
        /// Gets the value of the specified field as a Boolean.
        /// </summary>
        /// <param name="i">The zero-based field ordinal.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="IndexOutOfRange">
        /// The index passed was outside the range of 0 through FieldCount.
        /// </exception>
        public bool GetBoolean(int ordinal)
        {
            return Boolean.Parse(items[ordinal]);
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based field ordinal.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="IndexOutOfRange">
        /// The index passed was outside the range of 0 through FieldCount.
        /// </exception>
        public byte GetByte(int ordinal)
        {
            return Byte.Parse(items[ordinal], culture);
        }

        /// <summary>
        /// Gets the character value of the specified field.
        /// </summary>
        public char GetChar(int ordinal)
        {
            char chr;
            if (ordinal < items.GetLowerBound(0)) throw new IndexOutOfRangeException("Passed value less than lower bound of array/collection.");
            if (ordinal > items.GetUpperBound(0)) throw new IndexOutOfRangeException("Passed value greater than upper bound of array/collection.");
            if (!Char.TryParse(items[ordinal], out chr))
            {
                throw new InvalidOperationException("Field at position " + ordinal.ToString() +
                    "was null, or greater than a single character.");
            }
            else
            {
                return chr;
            }
        }

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based field ordinal.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="IndexOutOfRange">
        /// The index passed was outside the range of 0 through FieldCount.
        /// </exception>
        public DateTime GetDateTime(int ordinal)
        {
            return DateTime.Parse(items[ordinal], culture);
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based field ordinal.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="IndexOutOfRange">
        /// The index passed was outside the range of 0 through FieldCount.
        /// </exception>
        public decimal GetDecimal(int ordinal)
        {
            return Decimal.Parse(items[ordinal], culture);
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i">The zero-based field ordinal.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="IndexOutOfRange">
        /// The index passed was outside the range of 0 through FieldCount.
        /// </exception>
        public double GetDouble(int ordinal)
        {
            return Double.Parse(items[ordinal], culture);
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based field ordinal.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="IndexOutOfRange">
        /// The index passed was outside the range of 0 through FieldCount.
        /// </exception>
        public short GetInt16(int ordinal)
        {
            return Int16.Parse(items[ordinal], culture);
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based field ordinal.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="IndexOutOfRange">
        /// The index passed was outside the range of 0 through FieldCount.
        /// </exception>
        public int GetInt32(int ordinal)
        {
            return Int32.Parse(items[ordinal], culture);
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based field ordinal.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="IndexOutOfRange">
        /// The index passed was outside the range of 0 through FieldCount.
        /// </exception>
        public long GetInt64(int ordinal)
        {
            return Int64.Parse(items[ordinal], culture);
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i">The zero-based field ordinal.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="IndexOutOfRange">
        /// The index passed was outside the range of 0 through FieldCount.
        /// </exception>
        public float GetSingle(int ordinal)
        {
            return Single.Parse(items[ordinal], culture);
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based field ordinal.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="IndexOutOfRange">
        /// The index passed was outside the range of 0 through FieldCount.
        /// </exception>
        public string GetString(int ordinal)
        {
            return items[ordinal];
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

    // TODO: Joe go get FixedWidth thing, add it here
}

