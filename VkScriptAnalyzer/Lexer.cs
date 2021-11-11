using System;
using System.Collections.Generic;
using System.Linq;

namespace VkScriptAnalyzer
{
    public enum Lex_type
    {
        Unknown,
        Identifier,
        Number,
        KeyWord,
        DataType,
        Assign,
        String,
        OneSymbol,
        Plus_Op,
        Minus_Op
    }
    enum Input_signal
    {
        Digit,
        Letter,
        Dot,
        Comma,
        Minus,
        Letter_i,
        Letter_f,
        Colon,
        Equals,
        Quote,
        Other,
        End
    }
    enum State
    {
        S0,
        S1,
        S2,
        S3,
        S4,
        S5,
        S_error
    }

    abstract class Mashine
    {
        public Lex_type type { get; set; }
        public State state { get; set; }
        public string lex_value { get; set; }

        private readonly Dictionary<Input_signal, Dictionary<State, State>> next_state;
        private State[] finished_states;

        public abstract Input_signal DefineSignal(char symbol);

        public Mashine(Dictionary<Input_signal, Dictionary<State, State>> next_state, Lex_type type, State[] finished_states)
        {
            this.next_state = next_state;
            this.type = type;
            this.finished_states = finished_states;
            state = State.S0;
            lex_value = string.Empty;
        }

        public void Parse(char symbol)
        {
            Input_signal signal = DefineSignal(symbol);

            if (signal != Input_signal.End)
            {
                if (!next_state.ContainsKey(signal))
                {
                    state = State.S_error;
                }
                else if (state != State.S_error)
                {
                    state = next_state[signal][state];
                }

                if (signal != Input_signal.Other)
                    lex_value += symbol;
                else
                    state = State.S0;
            }
        }

        public bool IsEnd()
        {
            return state != State.S_error && finished_states.Contains(state);
        }

        public void Reset()
        {
            state = State.S0;
            lex_value = null;
        }
    }

    class MashineNumber : Mashine
    {
        public MashineNumber(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
            base(next_state, Lex_type.Number, new State[] { State.S2, State.S4 })
        { }

        public override Input_signal DefineSignal(char symbol)
        {
            switch (symbol)
            {
                case '-': return Input_signal.Minus;
                case '.': return Input_signal.Dot;
                case ',': return Input_signal.Comma;
            }

            if (symbol >= '0' && symbol <= '9')
                return Input_signal.Digit;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }

    class MashineIdentifier : Mashine
    {
        public MashineIdentifier(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
            base(next_state, Lex_type.Identifier, new State[] { State.S1 })
        { }

        public override Input_signal DefineSignal(char symbol)
        {
            if (symbol >= 'a' && symbol <= 'z' || symbol >= 'A' && symbol <= 'Z')
                return Input_signal.Letter;
            else if (symbol >= '0' && symbol <= '9')
                return Input_signal.Digit;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }

    class MashineAssign : Mashine
    {
        public MashineAssign(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
            base(next_state, Lex_type.Assign, new State[] { State.S2 })
        { }

        public override Input_signal DefineSignal(char symbol)
        {
            if (symbol == ':')
                return Input_signal.Colon;
            else if (symbol == '=')
                return Input_signal.Equals;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }

    class MashineString : Mashine
    {
        public MashineString(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
            base(next_state, Lex_type.String, new State[] { State.S2 })
        { }

        public override Input_signal DefineSignal(char symbol)
        {
            if (symbol == '\'')
                return Input_signal.Quote;
            else if (symbol >= 'a' && symbol <= 'z' || symbol >= 'A' && symbol <= 'Z' || symbol >= '0' && symbol <= '9')
                return Input_signal.Letter;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }

    /*class MashineOneSymbol : Mashine
	{
		private const string enable_one_symbols = "+-=<>()";
		public MashineOneSymbol(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
			base(next_state, Lex_type.OneSymbol, new State[] { State.S1 })
		{ }

		public override Input_signal DefineSignal(char symbol)
		{
			if (enable_one_symbols.Contains(symbol))
				return Input_signal.Letter;
			else if (symbol == ' ')
				return Input_signal.End;
			else return Input_signal.Other;
		}
	}*/

    public class Token
    {
        public string value = null;
        public Lex_type type;
    }

    public class Lexer
    {
        /// <summary>
        /// Была ли проверка автоматов
        /// </summary>
        private bool was_checked = false;
        /// <summary>
        /// Была ли лексема-разделитель
        /// </summary>
        private bool was_dividing_lexem = false;

        private string input;
        /// <summary>
        /// Токен, содержащий лексему-разделитель
        /// </summary>
        private Token fast_token = null;

        public Lexer(string text)
        {
            input = text;
        }

        string[] keyWords =
        {
            "If",
            "Then",
            "Else",
            "var",
            "begin",
            "end",
            "read",
            "write",
            "round",
        };

        string[] dataTypes =
        {
            "integer",
            "double",
        };

        Mashine[] parsers = {
            new MashineNumber(
                new Dictionary<Input_signal, Dictionary<State, State>>()
                {
                    { Input_signal.Digit,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S2 },
                        {  State.S1, State.S2 },
                        {  State.S2, State.S2 },
                        {  State.S3, State.S4 },
                        {  State.S4, State.S4 }
                    } },
                    { Input_signal.Dot,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S_error },
                        {  State.S1, State.S_error },
                        {  State.S2, State.S3 },
                        {  State.S3, State.S_error },
                        {  State.S4, State.S_error },
                    } },
                    { Input_signal.Comma,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S_error },
                        {  State.S1, State.S_error },
                        {  State.S2, State.S3 },
                        {  State.S3, State.S_error },
                        {  State.S4, State.S_error },
                    } },
                    { Input_signal.Minus,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S1 },
                        {  State.S1, State.S_error },
                        {  State.S2, State.S_error },
                        {  State.S3, State.S_error },
                        {  State.S4, State.S_error },
                    } },
                    { Input_signal.Other,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S_error },
                        {  State.S1, State.S_error },
                        {  State.S2, State.S_error },
                        {  State.S3, State.S_error },
                        {  State.S4, State.S_error }
                    }
                },
            }),

            new MashineIdentifier(
                new Dictionary<Input_signal, Dictionary<State, State>>()
                {
                    { Input_signal.Digit,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S_error },
                        {  State.S1, State.S1 }
                    } },
                    { Input_signal.Letter,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S1 },
                        {  State.S1, State.S1 }
                    } },
                    { Input_signal.Other,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S_error },
                        {  State.S1, State.S_error }
                    } },
                }),
			/*new MashineAssign(
				new Dictionary<Input_signal, Dictionary<State, State>>()
				{
					{ Input_signal.Colon,
						new Dictionary<State, State>() {
						{  State.S0, State.S1 },
						{  State.S1, State.S_error },
						{  State.S2, State.S_error }
					} },
					{ Input_signal.Equals,
						new Dictionary<State, State>() {
						{  State.S0, State.S_error },
						{  State.S1, State.S2 },
						{  State.S2, State.S_error }
					} },
					{ Input_signal.Other,
						new Dictionary<State, State>() {
						{  State.S0, State.S_error },
						{  State.S1, State.S_error },
						{  State.S2, State.S_error }
					} },
				}
				),
			new MashineString(
				new Dictionary<Input_signal, Dictionary<State, State>>()
				{
					{ Input_signal.Quote,
						new Dictionary<State, State>() {
							{ State.S0, State.S1 },
							{ State.S1, State.S2 },
							{ State.S2, State.S_error }
					} },
					{ Input_signal.Letter,
						new Dictionary<State, State>() {
							{ State.S0, State.S_error },
							{ State.S1, State.S1 },
							{ State.S2, State.S_error }
					} },
					{ Input_signal.Other,
						new Dictionary<State, State>() {
							{ State.S0, State.S_error },
							{ State.S1, State.S_error },
							{ State.S2, State.S_error }
					} },
				}),*/
			/*new MashineOneSymbol(
				new Dictionary<Input_signal, Dictionary<State, State>>()
				{
					{ Input_signal.Letter,
						new Dictionary<State, State>() {
						{  State.S0, State.S1 },
						{  State.S1, State.S1 },
						{  State.S_error, State.S1 }
					} },
					{ Input_signal.Other,
						new Dictionary<State, State>() {
						{  State.S0, State.S_error },
						{  State.S1, State.S_error }
					} }
				})*/
		};

        char Parse_Symbol()
        {
            char symbol = input[0];
            input = input.Remove(0, 1);

            return symbol;
        }

        Token Check_Parsers()
        {
            Token token = new Token();
            bool find = false;
            string value = null;

            var temp_parsers = parsers
                        .Where(p => p.lex_value != string.Empty && p.lex_value != null)
                        .OrderByDescending(p => p.lex_value.Length)
                        .ToArray();

            foreach (Mashine parser in temp_parsers)
            {
                value = parser.lex_value;

                if (parser.IsEnd())
                {
                    if (parser.type == Lex_type.Identifier && keyWords.Contains(value))
                        token.type = Lex_type.KeyWord;
                    else if (parser.type == Lex_type.Identifier && dataTypes.Contains(value))
                        token.type = Lex_type.DataType;
                    else
                        token.type = parser.type;
                    token.value = value;
                    find = true;
                    break;
                }
            }

            ResetParsers();

            if (!find)
            {
                token.type = Lex_type.Unknown;
                token.value = value;
            }

            return token;
        }

        private void ResetParsers()
        {
            foreach (Mashine parser in parsers)
            {
                parser.Reset();
            }
        }

        public Token GetToken()
        {
            if (fast_token != null)
            {
                var tmp = fast_token;//РАЗРЕШИТЬ ЭТОТ КОСТЫЛЬ!
                fast_token = null;
                return tmp;
            }

            if (input.Length == 0)
            {
                if (fast_token != null)
                    return fast_token;
                else
                    return null;
            }

            if (was_checked)
                was_checked = false;

            bool parse_not_dividing_lexem = false;
            bool is_white_space = false;
            while (input.Length > 0)
            {
                char symbol = Parse_Symbol();

                if (symbol == ' '
                    || symbol == '\t'
                    || symbol == '\n')
                {
                    if (was_checked)
                        continue;

                    is_white_space = true;
                }

                bool dividing_lexem = false;
                Lex_type type = Lex_type.Unknown;

                if (symbol == '+')
                {
                    dividing_lexem = true;

                    type = Lex_type.Plus_Op;
                }
                else if (symbol == '-')
                {
                    dividing_lexem = true;

                    type = Lex_type.Minus_Op;
                }

                if (dividing_lexem)
                {
                    if (was_dividing_lexem)
                    {
                        return new Token()
                        {
                            type = type,
                            value = Convert.ToString(symbol)
                        };
                    }

                    was_dividing_lexem = true;

                    fast_token = new Token()
                    {
                        type = type,
                        value = Convert.ToString(symbol)
                    };

                    if (parse_not_dividing_lexem)
                    {
                        parse_not_dividing_lexem = false;
                        return Check_Parsers();
                    }
                }
                else if (is_white_space)
                {
                    is_white_space = false;

                    if (!was_checked)
                    {
                        was_checked = true;
                        return Check_Parsers();
                    }
                }
                else
                {
                    was_dividing_lexem = false;
                    parse_not_dividing_lexem = true;

                    foreach (Mashine parser in parsers)
                    {
                        parser.Parse(symbol);
                    }
                }
            }

            return Check_Parsers();
        }
    }
}