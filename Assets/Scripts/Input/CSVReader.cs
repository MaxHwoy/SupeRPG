/*
 * IMTB (formatted and adjusted)
 * 
 * NReco CSV library (https://github.com/nreco/csv/)
 * Copyright 2017-2018 Vitaliy Fedorchenko
 * Distributed under the MIT license
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace SupeRPG.Input
{
    public class CsvReader
    {
        private readonly TextReader rdr;
        private readonly int delimLength;
        private char[] buffer = null;
        private int bufferLength;
        private int bufferLoadThreshold;
        private int lineStartPos = 0;
        private int actualBufferLen = 0;
        private List<Field> fields = null;
        private int fieldsCount = 0;
        private int linesRead = 0;

        public string Delimiter { get; }

        public int BufferSize { get; set; } = 32768;

        public bool TrimFields { get; set; } = true;

        public CsvReader(TextReader rdr) : this(rdr, ",")
        {
        }

        public CsvReader(TextReader rdr, string delimiter)
        {
            this.rdr = rdr;
            this.Delimiter = delimiter;
            this.delimLength = delimiter.Length;

            if (this.delimLength == 0)
            {
                throw new ArgumentException("Delimiter cannot be empty.");
            }
        }

        private int ReadBlockAndCheckEof(char[] buffer, int start, int len, ref bool eof)
        {
            if (len == 0)
            {
                return 0;
            }
            
            var read = this.rdr.ReadBlock(buffer, start, len);

            if (read < len)
            {
                eof = true;
            }

            return read;
        }

        private bool FillBuffer()
        {
            var eof = false;
            var toRead = this.bufferLength - this.actualBufferLen;

            if (toRead >= this.bufferLoadThreshold)
            {
                int freeStart = (this.lineStartPos + this.actualBufferLen) % this.buffer.Length;

                if (freeStart >= lineStartPos)
                {
                    this.actualBufferLen += this.ReadBlockAndCheckEof(this.buffer, freeStart, this.buffer.Length - freeStart, ref eof);

                    if (this.lineStartPos > 0)
                    {
                        this.actualBufferLen += this.ReadBlockAndCheckEof(this.buffer, 0, this.lineStartPos, ref eof);
                    }
                }
                else
                {
                    this.actualBufferLen += this.ReadBlockAndCheckEof(this.buffer, freeStart, toRead, ref eof);
                }
            }

            return eof;
        }

        private string GetLineTooLongMsg()
        {
            return String.Format("CSV line #{1} length exceedes buffer size ({0})", this.BufferSize, this.linesRead);
        }

        private int ReadQuotedFieldToEnd(int start, int maxPos, bool eof, ref int escapedQuotesCount)
        {
            int pos = start;
            int chIdx;
            char ch;

            for (/* empty */; pos < maxPos; pos++)
            {
                chIdx = pos < this.bufferLength ? pos : pos % this.bufferLength;
                
                ch = this.buffer[chIdx];

                if (ch == '\"')
                {
                    bool hasNextCh = (pos + 1) < maxPos;

                    if (hasNextCh && this.buffer[(pos + 1) % this.bufferLength] == '\"')
                    {
                        // double quote inside quote = just a content
                        pos++;
                        escapedQuotesCount++;
                    }
                    else
                    {
                        return pos;
                    }
                }
            }
            if (eof)
            {
                // this is incorrect CSV as quote is not closed
                // but in case of EOF lets ignore that
                return pos - 1;
            }

            throw new InvalidDataException(this.GetLineTooLongMsg());
        }

        private bool ReadDelimTail(int start, int maxPos, ref int end)
        {
            int pos;
            int idx;
            int offset = 1;

            for (; offset < this.delimLength; offset++)
            {
                pos = start + offset;
                idx = pos < this.bufferLength ? pos : pos % this.bufferLength;

                if (pos >= maxPos || this.buffer[idx] != this.Delimiter[offset])
                {
                    return false;
                }
            }

            end = start + offset - 1;
            
            return true;
        }

        private Field GetOrAddField(int startIdx)
        {
            this.fieldsCount++;
            
            while (this.fieldsCount > this.fields.Count)
            {
                this.fields.Add(new Field());
            }
            
            var f = this.fields[this.fieldsCount - 1];
            
            f.Reset(startIdx);
            
            return f;
        }

        public int FieldsCount
        {
            get
            {
                return this.fieldsCount;
            }
        }

        public string this[int idx]
        {
            get
            {
                if (idx < this.fieldsCount)
                {
                    return this.fields[idx].GetValue(this.buffer);
                }

                return null;
            }
        }

        public int GetValueLength(int idx)
        {
            if (idx < this.fieldsCount)
            {
                var f = this.fields[idx];

                return f.Quoted ? f.Length - f.EscapedQuotesCount : f.Length;
            }

            return -1;
        }

        public void ProcessValueInBuffer(int idx, Action<char[], int, int> handler)
        {
            if (idx < this.fieldsCount)
            {
                var f = this.fields[idx];

                if ((f.Quoted && f.EscapedQuotesCount > 0) || f.End >= this.bufferLength)
                {
                    var chArr = f.GetValue(this.buffer).ToCharArray();

                    handler(chArr, 0, chArr.Length);
                }
                else if (f.Quoted)
                {
                    handler(this.buffer, f.Start + 1, f.Length - 2);
                }
                else
                {
                    handler(this.buffer, f.Start, f.Length);
                }
            }
        }

        public bool Read()
        {
        Start:
            if (this.fields == null)
            {
                this.fields = new List<Field>();
                this.fieldsCount = 0;
            }
            if (this.buffer == null)
            {
                this.bufferLoadThreshold = Math.Min(this.BufferSize, 8192);
                this.bufferLength = this.BufferSize + this.bufferLoadThreshold;
                this.buffer = new char[this.bufferLength];
                this.lineStartPos = 0;
                this.actualBufferLen = 0;
            }

            var eof = this.FillBuffer();

            this.fieldsCount = 0;

            if (this.actualBufferLen <= 0)
            {
                return false; // no more data
            }
            
            this.linesRead++;

            int maxPos = this.lineStartPos + this.actualBufferLen;
            int charPos = this.lineStartPos;

            var currentField = this.GetOrAddField(charPos);
            bool ignoreQuote = false;
            char delimFirstChar = this.Delimiter[0];
            bool trimFields = this.TrimFields;

            int charBufIdx;
            char ch;

            for (/* empty */; charPos < maxPos; charPos++)
            {
                charBufIdx = charPos < this.bufferLength ? charPos : charPos % this.bufferLength;
                
                ch = this.buffer[charBufIdx];

                switch (ch)
                {
                    case '\"':
                        if (ignoreQuote)
                        {
                            currentField.End = charPos;
                        }
                        else if (currentField.Quoted || currentField.Length > 0)
                        {
                            // current field already is quoted = lets treat quotes as usual chars
                            currentField.End = charPos;
                            currentField.Quoted = false;
                            ignoreQuote = true;
                        }
                        else
                        {
                            var endQuotePos = this.ReadQuotedFieldToEnd(charPos + 1, maxPos, eof, ref currentField.EscapedQuotesCount);
                            currentField.Start = charPos;
                            currentField.End = endQuotePos;
                            currentField.Quoted = true;
                            charPos = endQuotePos;
                        }
                        break;

                    case '\r':
                        if ((charPos + 1) < maxPos && this.buffer[(charPos + 1) % this.bufferLength] == '\n')
                        {
                            // \r\n handling
                            charPos++;
                        }
                        // in some files only \r used as line separator - lets allow that
                        charPos++;
                        goto LineEnded;

                    case '\n':
                        charPos++;
                        goto LineEnded;

                    default:
                        if (ch == delimFirstChar && (this.delimLength == 1 || this.ReadDelimTail(charPos, maxPos, ref charPos)))
                        {
                            currentField = this.GetOrAddField(charPos + 1);
                            ignoreQuote = false;
                            continue;
                        }
                        // space
                        if (ch == ' ' && trimFields)
                        {
                            continue; // do nothing
                        }

                        // content char
                        if (currentField.Length == 0)
                        {
                            currentField.Start = charPos;
                        }

                        if (currentField.Quoted)
                        {
                            // non-space content after quote = treat quotes as part of content
                            currentField.Quoted = false;
                            ignoreQuote = true;
                        }
                        currentField.End = charPos;
                        break;
                }

            }
            if (!eof)
            {
                // line is not finished, but whole buffer was processed and not EOF
                throw new InvalidDataException(this.GetLineTooLongMsg());
            }
        LineEnded:
            this.actualBufferLen -= charPos - this.lineStartPos;
            this.lineStartPos = charPos % this.bufferLength;

            if (this.fieldsCount == 1 && this.fields[0].Length == 0)
            {
                // skip empty lines
                goto Start;
            }

            return true;
        }


        internal sealed class Field
        {
            internal int Start;
            internal int End;
            internal bool Quoted;
            internal int EscapedQuotesCount;
            internal string cachedValue = null;
            internal int Length => this.End - this.Start + 1;

            internal Field()
            {
            }

            internal Field Reset(int start)
            {
                this.Start = start;
                this.End = start - 1;
                this.Quoted = false;
                this.EscapedQuotesCount = 0;
                this.cachedValue = null;
                return this;
            }

            internal string GetValue(char[] buf)
            {
                return this.cachedValue ??= this.GetValueInternal(buf);
            }

            string GetValueInternal(char[] buf)
            {
                if (this.Quoted)
                {
                    var s = this.Start + 1;
                    var lenWithoutQuotes = this.Length - 2;
                    var val = lenWithoutQuotes > 0 ? this.GetString(buf, s, lenWithoutQuotes) : String.Empty;
                    
                    if (this.EscapedQuotesCount > 0)
                    {
                        val = val.Replace("\"\"", "\"");
                    }

                    return val;
                }

                var len = this.Length;

                return len > 0 ? this.GetString(buf, this.Start, len) : String.Empty;
            }

            private string GetString(char[] buf, int start, int len)
            {
                var bufLen = buf.Length;
                start = start < bufLen ? start : start % bufLen;
                var endIdx = start + len - 1;

                if (endIdx >= bufLen)
                {
                    var prefixLen = buf.Length - start;
                    var prefix = new string(buf, start, prefixLen);
                    var suffix = new string(buf, 0, len - prefixLen);
                    return prefix + suffix;
                }
                
                return new string(buf, start, len);
            }
        }
    }
}
