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
		String
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
		}

		public void Parse(char symbol)
		{
			Input_signal signal = DefineSignal(symbol);

			if (signal != Input_signal.End)
			{
				lex_value += symbol;

				if (!next_state.ContainsKey(signal))
					state = State.S_error;
				else if (state != State.S_error)
					state = next_state[signal][state];
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

	class MashineOneSymbol : Mashine
	{

		private const string enable_one_symbols = "+-=<>()";
		public MashineOneSymbol(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
			base(next_state, Lex_type.Assign, new State[] { State.S1 })
		{ }

		public override Input_signal DefineSignal(char symbol)
		{
			if (enable_one_symbols.Contains(symbol))
				return Input_signal.Letter;
			else if (symbol == ' ')
				return Input_signal.End;
			else return Input_signal.Other;
		}
	}

	public class Token
	{
		public string Value = null;
		public Lex_type Type;
	}

	public static class Lexer
	{
		public static string input { get; set; }

		static string[] keyWords =
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
		static string[] dataTypes =
		{
			"integer",
			"double",
		};

		static Mashine[] parsers = {
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
			new MashineAssign(
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
				}),
			new MashineOneSymbol(
				new Dictionary<Input_signal, Dictionary<State, State>>()
				{
					{ Input_signal.Letter,
						new Dictionary<State, State>() {
						{  State.S0, State.S1 },
						{  State.S1, State.S_error }
					} },
					{ Input_signal.Other,
						new Dictionary<State, State>() {
						{  State.S0, State.S_error },
						{  State.S1, State.S_error }
					} },
				})
		};

		static char Parse_Symbol()
		{
			char symbol = input[0];
			input = input.Remove(0, 1);

			return symbol;
		}

		static Token Check_Parsers()
		{
			Token token = new Token();
			bool find = false;
			string value = null;
			foreach (Mashine parser in parsers)
			{
				value = parser.lex_value;

				if (parser.IsEnd())
				{
					if (parser.type == Lex_type.Identifier && keyWords.Contains(value))
						token.Type = Lex_type.KeyWord;
					else if (parser.type == Lex_type.Identifier && dataTypes.Contains(value))
						token.Type = Lex_type.DataType;
					else
						token.Type = parser.type;
					token.Value = value;
					find = true;
					break;
				}
			}

			ResetParsers();

			if (!find)
			{
				token.Type = Lex_type.Unknown;
				token.Value = value;
				//Console.WriteLine("Лексема '" + value + "' не определена!");
				//throw new ParserException(value);
			}

			return token;
		}

		private static void ResetParsers()
		{
			foreach (Mashine parser in parsers)
			{
				parser.Reset();
			}
		}

		public static Token GetToken()
		{
			if (input.Length == 0)
				return new Token() { Type = Lex_type.Unknown };

			while (input.Length > 0)
			{
				char symbol = Parse_Symbol();

				foreach (Mashine parser in parsers)
				{
					parser.Parse(symbol);
				}

				if (symbol == ' ')
				{
					return Check_Parsers();
				}
			}

			return Check_Parsers();
		}
	}
}