//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 3.3.1.7705
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// $ANTLR 3.3.1.7705 D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g 2014-07-04 18:37:26

// The variable 'variable' is assigned but its value is never used.
#pragma warning disable 219
// Unreachable code detected.
#pragma warning disable 162


using System.Collections.Generic;
using Antlr.Runtime;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "3.3.1.7705")]
[System.CLSCompliant(false)]
public partial class SimpleBooleanLexer : Antlr.Runtime.Lexer
{
	public const int EOF=-1;
	public const int AND=4;
	public const int LPAREN=5;
	public const int OR=6;
	public const int RPAREN=7;
	public const int WORD=8;
	public const int WS=9;

    // delegates
    // delegators

	public SimpleBooleanLexer()
	{
		OnCreated();
	}

	public SimpleBooleanLexer(ICharStream input )
		: this(input, new RecognizerSharedState())
	{
	}

	public SimpleBooleanLexer(ICharStream input, RecognizerSharedState state)
		: base(input, state)
	{


		OnCreated();
	}
	public override string GrammarFileName { get { return "D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g"; } }


	partial void OnCreated();
	partial void EnterRule(string ruleName, int ruleIndex);
	partial void LeaveRule(string ruleName, int ruleIndex);

	partial void EnterRule_LPAREN();
	partial void LeaveRule_LPAREN();

	// $ANTLR start "LPAREN"
	[GrammarRule("LPAREN")]
	private void mLPAREN()
	{
		EnterRule_LPAREN();
		EnterRule("LPAREN", 1);
		TraceIn("LPAREN", 1);
		try
		{
			int _type = LPAREN;
			int _channel = DefaultTokenChannel;
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:10:8: ( '(' )
			DebugEnterAlt(1);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:10:10: '('
			{
			DebugLocation(10, 10);
			Match('('); 

			}

			state.type = _type;
			state.channel = _channel;
		}
		finally
		{
			TraceOut("LPAREN", 1);
			LeaveRule("LPAREN", 1);
			LeaveRule_LPAREN();
		}
	}
	// $ANTLR end "LPAREN"

	partial void EnterRule_RPAREN();
	partial void LeaveRule_RPAREN();

	// $ANTLR start "RPAREN"
	[GrammarRule("RPAREN")]
	private void mRPAREN()
	{
		EnterRule_RPAREN();
		EnterRule("RPAREN", 2);
		TraceIn("RPAREN", 2);
		try
		{
			int _type = RPAREN;
			int _channel = DefaultTokenChannel;
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:11:8: ( ')' )
			DebugEnterAlt(1);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:11:10: ')'
			{
			DebugLocation(11, 10);
			Match(')'); 

			}

			state.type = _type;
			state.channel = _channel;
		}
		finally
		{
			TraceOut("RPAREN", 2);
			LeaveRule("RPAREN", 2);
			LeaveRule_RPAREN();
		}
	}
	// $ANTLR end "RPAREN"

	partial void EnterRule_AND();
	partial void LeaveRule_AND();

	// $ANTLR start "AND"
	[GrammarRule("AND")]
	private void mAND()
	{
		EnterRule_AND();
		EnterRule("AND", 3);
		TraceIn("AND", 3);
		try
		{
			int _type = AND;
			int _channel = DefaultTokenChannel;
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:12:5: ( 'AND' )
			DebugEnterAlt(1);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:12:7: 'AND'
			{
			DebugLocation(12, 7);
			Match("AND"); 


			}

			state.type = _type;
			state.channel = _channel;
		}
		finally
		{
			TraceOut("AND", 3);
			LeaveRule("AND", 3);
			LeaveRule_AND();
		}
	}
	// $ANTLR end "AND"

	partial void EnterRule_OR();
	partial void LeaveRule_OR();

	// $ANTLR start "OR"
	[GrammarRule("OR")]
	private void mOR()
	{
		EnterRule_OR();
		EnterRule("OR", 4);
		TraceIn("OR", 4);
		try
		{
			int _type = OR;
			int _channel = DefaultTokenChannel;
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:13:4: ( 'OR' )
			DebugEnterAlt(1);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:13:6: 'OR'
			{
			DebugLocation(13, 6);
			Match("OR"); 


			}

			state.type = _type;
			state.channel = _channel;
		}
		finally
		{
			TraceOut("OR", 4);
			LeaveRule("OR", 4);
			LeaveRule_OR();
		}
	}
	// $ANTLR end "OR"

	partial void EnterRule_WS();
	partial void LeaveRule_WS();

	// $ANTLR start "WS"
	[GrammarRule("WS")]
	private void mWS()
	{
		EnterRule_WS();
		EnterRule("WS", 5);
		TraceIn("WS", 5);
		try
		{
			int _type = WS;
			int _channel = DefaultTokenChannel;
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:14:4: ( ( ' ' | '\\t' | '\\r' | '\\n' ) )
			DebugEnterAlt(1);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:14:7: ( ' ' | '\\t' | '\\r' | '\\n' )
			{
			DebugLocation(14, 7);
			if ((input.LA(1)>='\t' && input.LA(1)<='\n')||input.LA(1)=='\r'||input.LA(1)==' ')
			{
				input.Consume();

			}
			else
			{
				MismatchedSetException mse = new MismatchedSetException(null,input);
				DebugRecognitionException(mse);
				Recover(mse);
				throw mse;}

			DebugLocation(14, 35);
			_channel=Hidden;

			}

			state.type = _type;
			state.channel = _channel;
		}
		finally
		{
			TraceOut("WS", 5);
			LeaveRule("WS", 5);
			LeaveRule_WS();
		}
	}
	// $ANTLR end "WS"

	partial void EnterRule_WORD();
	partial void LeaveRule_WORD();

	// $ANTLR start "WORD"
	[GrammarRule("WORD")]
	private void mWORD()
	{
		EnterRule_WORD();
		EnterRule("WORD", 6);
		TraceIn("WORD", 6);
		try
		{
			int _type = WORD;
			int _channel = DefaultTokenChannel;
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:15:6: ( (~ ( ' ' | '\\t' | '\\r' | '\\n' | '(' | ')' ) )* )
			DebugEnterAlt(1);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:15:9: (~ ( ' ' | '\\t' | '\\r' | '\\n' | '(' | ')' ) )*
			{
			DebugLocation(15, 9);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:15:9: (~ ( ' ' | '\\t' | '\\r' | '\\n' | '(' | ')' ) )*
			try { DebugEnterSubRule(1);
			while (true)
			{
				int alt1=2;
				try { DebugEnterDecision(1, false);
				int LA1_0 = input.LA(1);

				if (((LA1_0>='\u0000' && LA1_0<='\b')||(LA1_0>='\u000B' && LA1_0<='\f')||(LA1_0>='\u000E' && LA1_0<='\u001F')||(LA1_0>='!' && LA1_0<='\'')||(LA1_0>='*' && LA1_0<='\uFFFF')))
				{
					alt1 = 1;
				}


				} finally { DebugExitDecision(1); }
				switch ( alt1 )
				{
				case 1:
					DebugEnterAlt(1);
					// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:
					{
					DebugLocation(15, 9);
					input.Consume();


					}
					break;

				default:
					goto loop1;
				}
			}

			loop1:
				;

			} finally { DebugExitSubRule(1); }


			}

			state.type = _type;
			state.channel = _channel;
		}
		finally
		{
			TraceOut("WORD", 6);
			LeaveRule("WORD", 6);
			LeaveRule_WORD();
		}
	}
	// $ANTLR end "WORD"

	public override void mTokens()
	{
		// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:1:8: ( LPAREN | RPAREN | AND | OR | WS | WORD )
		int alt2=6;
		try { DebugEnterDecision(2, false);
		try
		{
			alt2 = dfa2.Predict(input);
		}
		catch (NoViableAltException nvae)
		{
			DebugRecognitionException(nvae);
			throw;
		}
		} finally { DebugExitDecision(2); }
		switch (alt2)
		{
		case 1:
			DebugEnterAlt(1);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:1:10: LPAREN
			{
			DebugLocation(1, 10);
			mLPAREN(); 

			}
			break;
		case 2:
			DebugEnterAlt(2);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:1:17: RPAREN
			{
			DebugLocation(1, 17);
			mRPAREN(); 

			}
			break;
		case 3:
			DebugEnterAlt(3);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:1:24: AND
			{
			DebugLocation(1, 24);
			mAND(); 

			}
			break;
		case 4:
			DebugEnterAlt(4);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:1:28: OR
			{
			DebugLocation(1, 28);
			mOR(); 

			}
			break;
		case 5:
			DebugEnterAlt(5);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:1:31: WS
			{
			DebugLocation(1, 31);
			mWS(); 

			}
			break;
		case 6:
			DebugEnterAlt(6);
			// D:\\projects\\common\\Antlr\\simpleboolean-parser\\SimpleBoolean.g:1:34: WORD
			{
			DebugLocation(1, 34);
			mWORD(); 

			}
			break;

		}

	}


	#region DFA
	DFA2 dfa2;

	protected override void InitDFAs()
	{
		base.InitDFAs();
		dfa2 = new DFA2(this, SpecialStateTransition2);
	}

	private class DFA2 : DFA
	{
		private const string DFA2_eotS =
			"\x1\x6\x2\xFFFF\x2\x6\x2\xFFFF\x1\x6\x1\xA\x1\xB\x2\xFFFF";
		private const string DFA2_eofS =
			"\xC\xFFFF";
		private const string DFA2_minS =
			"\x1\x9\x2\xFFFF\x1\x4E\x1\x52\x2\xFFFF\x1\x44\x2\x0\x2\xFFFF";
		private const string DFA2_maxS =
			"\x1\x4F\x2\xFFFF\x1\x4E\x1\x52\x2\xFFFF\x1\x44\x2\xFFFF\x2\xFFFF";
		private const string DFA2_acceptS =
			"\x1\xFFFF\x1\x1\x1\x2\x2\xFFFF\x1\x5\x1\x6\x3\xFFFF\x1\x4\x1\x3";
		private const string DFA2_specialS =
			"\x8\xFFFF\x1\x0\x1\x1\x2\xFFFF}>";
		private static readonly string[] DFA2_transitionS =
			{
				"\x2\x5\x2\xFFFF\x1\x5\x12\xFFFF\x1\x5\x7\xFFFF\x1\x1\x1\x2\x17\xFFFF"+
				"\x1\x3\xD\xFFFF\x1\x4",
				"",
				"",
				"\x1\x7",
				"\x1\x8",
				"",
				"",
				"\x1\x9",
				"\x9\x6\x2\xFFFF\x2\x6\x1\xFFFF\x12\x6\x1\xFFFF\x7\x6\x2\xFFFF\xFFD6"+
				"\x6",
				"\x9\x6\x2\xFFFF\x2\x6\x1\xFFFF\x12\x6\x1\xFFFF\x7\x6\x2\xFFFF\xFFD6"+
				"\x6",
				"",
				""
			};

		private static readonly short[] DFA2_eot = DFA.UnpackEncodedString(DFA2_eotS);
		private static readonly short[] DFA2_eof = DFA.UnpackEncodedString(DFA2_eofS);
		private static readonly char[] DFA2_min = DFA.UnpackEncodedStringToUnsignedChars(DFA2_minS);
		private static readonly char[] DFA2_max = DFA.UnpackEncodedStringToUnsignedChars(DFA2_maxS);
		private static readonly short[] DFA2_accept = DFA.UnpackEncodedString(DFA2_acceptS);
		private static readonly short[] DFA2_special = DFA.UnpackEncodedString(DFA2_specialS);
		private static readonly short[][] DFA2_transition;

		static DFA2()
		{
			int numStates = DFA2_transitionS.Length;
			DFA2_transition = new short[numStates][];
			for ( int i=0; i < numStates; i++ )
			{
				DFA2_transition[i] = DFA.UnpackEncodedString(DFA2_transitionS[i]);
			}
		}

		public DFA2( BaseRecognizer recognizer, SpecialStateTransitionHandler specialStateTransition )
			: base(specialStateTransition)
		{
			this.recognizer = recognizer;
			this.decisionNumber = 2;
			this.eot = DFA2_eot;
			this.eof = DFA2_eof;
			this.min = DFA2_min;
			this.max = DFA2_max;
			this.accept = DFA2_accept;
			this.special = DFA2_special;
			this.transition = DFA2_transition;
		}

		public override string Description { get { return "1:1: Tokens : ( LPAREN | RPAREN | AND | OR | WS | WORD );"; } }

		public override void Error(NoViableAltException nvae)
		{
			DebugRecognitionException(nvae);
		}
	}

	private int SpecialStateTransition2(DFA dfa, int s, IIntStream _input)
	{
		IIntStream input = _input;
		int _s = s;
		switch (s)
		{
			case 0:
				int LA2_8 = input.LA(1);

				s = -1;
				if (((LA2_8>='\u0000' && LA2_8<='\b')||(LA2_8>='\u000B' && LA2_8<='\f')||(LA2_8>='\u000E' && LA2_8<='\u001F')||(LA2_8>='!' && LA2_8<='\'')||(LA2_8>='*' && LA2_8<='\uFFFF'))) {s = 6;}

				else s = 10;

				if (s >= 0) return s;
				break;
			case 1:
				int LA2_9 = input.LA(1);

				s = -1;
				if (((LA2_9>='\u0000' && LA2_9<='\b')||(LA2_9>='\u000B' && LA2_9<='\f')||(LA2_9>='\u000E' && LA2_9<='\u001F')||(LA2_9>='!' && LA2_9<='\'')||(LA2_9>='*' && LA2_9<='\uFFFF'))) {s = 6;}

				else s = 11;

				if (s >= 0) return s;
				break;
		}
		NoViableAltException nvae = new NoViableAltException(dfa.Description, 2, _s, input);
		dfa.Error(nvae);
		throw nvae;
	}
 
	#endregion

}