using System;
using System.Linq;
using VkScriptAnalyzer.Lexer.Mashines;

namespace VkScriptAnalyzer.Lexer
{
    public class LexicalAnalyzer
    {
        /// <summary>
        /// Была ли проверка автоматов
        /// </summary>
        private bool _wasChecked = false;

        /// <summary>
        /// Была ли лексема-разделитель
        /// </summary>
        private bool _wasDividingLexeme = false;

        private string _input;

        /// <summary>
        /// Токен, содержащий лексему-разделитель
        /// </summary>
        private Token _fastToken = null;

        private readonly char[] _dividingChars =
        {
            '+',
            '-',
            '/',
            '*',
            ';',
            ',',
            '(',
            ')',
            '{',
            '}',
            '<',
            '>',
            '!',
            '=',
            '.',
            ':'
        };

        private readonly char[] _whitespaceChars =
        {
            ' ',
            '\t',
            '\n',
            '\r'
        };

        private readonly MaсhineNumber _maсhineNumber = new();
        private readonly MaсhineIdentifier _maсhineIdentifier = new();
        private readonly MaсhineString _maсhineString = new();


        private readonly string[] _keyWords =
        {
            "var",
            "if",
            "else",
            "while",
            "and",
            "or",
            "return",
        };

        private readonly string[] _boolDataTypes =
        {
            "true",
            "false"
        };

        private readonly Machine[] _parsers;

        public int PosNumber { get; private set; }

        public LexicalAnalyzer(string text)
        {
            _input = text.TrimStart().TrimEnd();

            _parsers = new Machine[]
            {
                _maсhineNumber,
                _maсhineIdentifier,
                _maсhineString
            };

            PosNumber = 1;
        }

        private char ParseSymbol()
        {
            var symbol = _input[0];
            _input = _input.Remove(0, 1);

            return symbol;
        }

        private char? CheckNextSymbol()
        {
            if (_input.Length == 0)
            {
                return null;
            }

            return _input[0];
        }

        private Token CheckParsers()
        {
            var token = new Token();
            var find = false;
            string value = null;

            var temp_parsers = _parsers
                .Where(p => !string.IsNullOrEmpty(p.LexValue))
                .OrderByDescending(p => p.LexValue.Length)
                .ToArray();

            foreach (var parser in temp_parsers)
            {
                value = parser.LexValue;

                if (!parser.IsEnd())
                {
                    continue;
                }

                token.Type = parser.Type switch
                {
                    TokenType.Identifier when _keyWords.Contains(value) => TokenType.KeyWord,
                    TokenType.Identifier when _boolDataTypes.Contains(value) => TokenType.BoolDataType,
                    _ => parser.Type
                };

                token.Value = value;
                token.Pos = PosNumber;

                find = true;

                break;
            }

            ResetParsers();

            if (!find)
            {
                token.Type = TokenType.Unknown;
                token.Value = value;
            }

            if (token.Type != TokenType.String)
            {
                return token;
            }

            token.Value = token.Value.Remove(0, 1);
            token.Value = token.Value.Remove(token.Value.Length - 1, 1);

            return token;
        }

        private Token CheckDoubleDividingChars(char firstDividingChar, bool parseNotDividingLexem)
        {
            var secondDividingChar = CheckNextSymbol();

            switch (firstDividingChar)
            {
                case '=' when secondDividingChar != '=':
                    return null;
                case '=':
                {
                    ParseSymbol();

                    var token = new Token()
                    {
                        Type = TokenType.Equal,
                        Value = "==",
                        Pos = PosNumber
                    };

                    if (!parseNotDividingLexem)
                    {
                        return token;
                    }

                    _fastToken = token;

                    return CheckParsers();
                }
                case '!' when secondDividingChar == '=':
                {
                    ParseSymbol();

                    var token = new Token()
                    {
                        Type = TokenType.NonEqual,
                        Value = "!=",
                        Pos = PosNumber
                    };

                    if (!parseNotDividingLexem)
                    {
                        return token;
                    }

                    _fastToken = token;

                    return CheckParsers();
                }
                default:
                    return null;
            }
        }

        private void ResetParsers()
        {
            foreach (var parser in _parsers)
            {
                parser.Reset();
            }
        }

        public Token GetToken()
        {
            if (_fastToken != null)
            {
                var tmp = _fastToken;
                _fastToken = null;

                return tmp;
            }

            if (_input.Length == 0)
            {
                return _fastToken;
            }

            if (_wasChecked)
            {
                _wasChecked = false;
            }

            var parseNotDividingLexeme = false;
            var isWhiteSpace = false;

            while (_input.Length > 0)
            {
                var symbol = ParseSymbol();

                if (_whitespaceChars.Contains(symbol))
                {
                    if (symbol == '\n')
                    {
                        PosNumber++;
                    }

                    isWhiteSpace = true;
                }

                var dividingLexeme = false;
                var type = TokenType.Unknown;

                if (_dividingChars.Contains(symbol))
                {
                    var doubleDividingToken = CheckDoubleDividingChars(symbol, parseNotDividingLexeme);

                    if (doubleDividingToken != null)
                    {
                        return doubleDividingToken;
                    }

                    dividingLexeme = true;

                    type = (TokenType) symbol;
                }

                if (dividingLexeme)
                {
                    if (symbol == '.')
                    {
                        // если начали разбирать число, то точка не будет символом-"разделителем"
                        var is_error = _maсhineNumber.InError();

                        if (!is_error)
                        {
                            foreach (var parser in _parsers)
                            {
                                parser.Parse(symbol);
                            }

                            continue;
                        }
                    }

                    if (_wasDividingLexeme || !parseNotDividingLexeme)
                    {
                        return new()
                        {
                            Type = type,
                            Value = Convert.ToString(symbol),
                            Pos = PosNumber
                        };
                    }

                    _wasDividingLexeme = true;

                    _fastToken = new()
                    {
                        Type = type,
                        Value = Convert.ToString(symbol),
                        Pos = PosNumber
                    };

                    if (parseNotDividingLexeme)
                    {
                        return CheckParsers();
                    }
                } else if (isWhiteSpace)
                {
                    isWhiteSpace = false;

                    if (!parseNotDividingLexeme)
                    {
                        continue;
                    }

                    _wasChecked = true;

                    return CheckParsers();
                } else
                {
                    _wasDividingLexeme = false;
                    parseNotDividingLexeme = true;

                    foreach (var parser in _parsers)
                    {
                        parser.Parse(symbol);
                    }
                }
            }

            return !isWhiteSpace ? CheckParsers() : null;
        }
    }
}