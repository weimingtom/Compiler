﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Sara
{
    /// <summary>
    /// Manage Id and reserved words
    /// </summary>
    public class WordTable : Dictionary<string, Word>
    {
        private readonly List<Word> _keyWords = new List<Word>
        {
            new Word("if", Tag.IF),
            new Word("else", Tag.ELSE),
            new Word("while", Tag.WHILE),
            new Word("do", Tag.DO),
            new Word("break", Tag.BREAK),

            Word.True,
            Word.False,
            Type.Int,
            Type.Char,
            Type.Bool,
            Type.Float
        };

        public List<Word> KeyWords
        {
            get { return _keyWords; }
        }

        public WordTable()
        {
            _keyWords.ForEach(w => { this[w.Lexeme] = w; });
        }
    }

    public class Lexer
    {
        private StreamReader _stream;
        public Dictionary<string, Word> Words { get; private set; }
        public long Line { get; private set; }
        public IList<Token> Result { get; private set; }

        public Lexer(StreamReader sr)
        {
            this._stream = sr;
            this.Words = new Sara.WordTable();
            this.Line = 1;
        }

        private IList<Token> Scan(StreamReader reader)
        {
            //delegates for later use
            Func<char> curr =
                () => { return (char)reader.Peek(); };

            Func<bool> notEof =
                () => { return reader.Peek() != -1; };

            Func<bool> isWS =
                () => { return char.IsWhiteSpace(curr()); };

            Func<bool> isWord =
                () => { return char.IsLetter(curr()) || '_' == curr(); };

            Func<bool> isNumber =
                () => { return char.IsDigit(curr()); };

            Action move =
                () => { reader.Read(); };

            Func<char, bool> matchNext =
                (char arg) => { move(); return arg == curr(); };

            var ret = new List<Token>();
            while (notEof())
            {
                //for whitespace
                if (isWS())
                {
                    while (isWS()) move();
                    continue;
                }

                //for operators like && !=, etc
                switch (curr())
                {
                    case '&':
                        ret.Add(matchNext('&') ? Word.and : new Token('&')); continue;
                    case '|':
                        ret.Add(matchNext('|') ? Word.or : new Token('|')); continue;
                    case '=':
                        ret.Add(matchNext('=') ? Word.eq : new Token('=')); continue;
                    case '!':
                        ret.Add(matchNext('=') ? Word.ne : new Token('!')); continue;
                    case '<':
                        ret.Add(matchNext('=') ? Word.le : new Token('<')); continue;
                    case '>':
                        ret.Add(matchNext('=') ? Word.ge : new Token('>')); continue;
                }

                if (isNumber())
                {
                    int v = 0;
                    do
                    {
                        v = 10 * v + (int)(curr() - '0');
                        move();
                    } while (isNumber());
                    if(curr() != '.')
                    {
                        ret.Add(new Num(v));
                        continue;
                    }

                    float f = v;
                    for (float d = 10; ; d *= 10)
                    {
                        move();
                        if (!isNumber()) break;
                        f += (int)(curr() - '0') / d;
                    }
                    ret.Add(new Real(f));
                    continue;
                }

                //for identifiers and reserved words
                if (isWord())
                {
                    var sb = new StringBuilder();
                    sb.Append(curr());
                    move();
                    for (; isWord() || char.IsDigit(curr()); move()) sb.Append(curr());
                    var w = sb.ToString();

                    if (this.Words.ContainsKey(w)) this.Result.Add(Words[w]);
                }
            }
            return ret;
        }
    }
}
