// $ANTLR 2.7.6 (2005-12-22): "stringExpression.g" -> "StringExpression.java"$

package com.aote.expression.antlr;

import com.aote.expression.ParamProcessorFactory;
import java.util.Map;
import java.util.List;
import java.util.LinkedList;
import org.w3c.dom.Element;

import antlr.TokenBuffer;
import antlr.TokenStreamException;
import antlr.TokenStreamIOException;
import antlr.ANTLRException;
import antlr.LLkParser;
import antlr.Token;
import antlr.TokenStream;
import antlr.RecognitionException;
import antlr.NoViableAltException;
import antlr.MismatchedTokenException;
import antlr.SemanticException;
import antlr.ParserSharedInputState;
import antlr.collections.impl.BitSet;

public class StringExpression extends antlr.LLkParser       implements StringExpressionTokenTypes
 {

	private StringLexer lexer;
	
	public void setLexer(StringLexer lexer) {
		this.lexer = lexer;
	}
	
    private String process(String param)
    {
        return ParamProcessorFactory.getInstance().process(param);
    }

	private String getParamString(String param) {
		int len = param.length();
		return param.substring(1, len - 1);
	}

protected StringExpression(TokenBuffer tokenBuf, int k) {
  super(tokenBuf,k);
  tokenNames = _tokenNames;
}

public StringExpression(TokenBuffer tokenBuf) {
  this(tokenBuf,1);
}

protected StringExpression(TokenStream lexer, int k) {
  super(lexer,k);
  tokenNames = _tokenNames;
}

public StringExpression(TokenStream lexer) {
  this(lexer,1);
}

public StringExpression(ParserSharedInputState state) {
  super(state,1);
  tokenNames = _tokenNames;
}

	public final String  expression() throws RecognitionException, TokenStreamException {
		String result="";
		
		
		String s;
		
		
		try {      // for error handling
			result=stringCut();
			{
			_loop3:
			do {
				if ((LA(1)==STRING||LA(1)==PARAMS)) {
					s=stringCut();
					result += s;
				}
				else {
					break _loop3;
				}
				
			} while (true);
			}
		}
		catch (RecognitionException ex) {
			reportError(ex);
			recover(ex,_tokenSet_0);
		}
		return result;
	}
	
	public final String  stringCut() throws RecognitionException, TokenStreamException {
		String result="";
		
		
			String s;
		
		
		try {      // for error handling
			{
			s=string();
			{
			_loop7:
			do {
				if ((LA(1)==DOT)) {
					match(DOT);
					this.lexer.inFunction=true;
					s=cutFunction(s);
					this.lexer.inFunction=false;
				}
				else {
					break _loop7;
				}
				
			} while (true);
			}
			}
			result=s;
		}
		catch (RecognitionException ex) {
			reportError(ex);
			recover(ex,_tokenSet_1);
		}
		return result;
	}
	
	public final String  string() throws RecognitionException, TokenStreamException {
		String result="";
		
		Token  s = null;
		Token  p = null;
		
		try {      // for error handling
			{
			int _cnt15=0;
			_loop15:
			do {
				if ((LA(1)==STRING)) {
					s = LT(1);
					match(STRING);
					result += s.getText();
				}
				else if ((LA(1)==PARAMS)) {
					p = LT(1);
					match(PARAMS);
					result += process(getParamString(p.getText()));
				}
				else {
					if ( _cnt15>=1 ) { break _loop15; } else {throw new NoViableAltException(LT(1), getFilename());}
				}
				
				_cnt15++;
			} while (true);
			}
		}
		catch (RecognitionException ex) {
			reportError(ex);
			recover(ex,_tokenSet_2);
		}
		return result;
	}
	
	public final String  cutFunction(
		String str
	) throws RecognitionException, TokenStreamException {
		String result="";
		
		Token  id = null;
		
		List pl;
		
		
		try {      // for error handling
			{
			id = LT(1);
			match(ID);
			match(LEFT);
			pl=paramList();
			match(RIGHT);
			}
			//result=StringFunctionFactory.getInstance().invoke(id.getText(), str, pl);
		}
		catch (RecognitionException ex) {
			reportError(ex);
			recover(ex,_tokenSet_2);
		}
		return result;
	}
	
	public final List  paramList() throws RecognitionException, TokenStreamException {
		List result=new LinkedList();
		
		Token  s = null;
		Token  s1 = null;
		
		try {      // for error handling
			switch ( LA(1)) {
			case RIGHT:
			{
				break;
			}
			case NUMBER:
			{
				s = LT(1);
				match(NUMBER);
				result.add(s.getText());
				{
				_loop12:
				do {
					if ((LA(1)==COMMA)) {
						match(COMMA);
						s1 = LT(1);
						match(NUMBER);
						result.add(s1.getText());
					}
					else {
						break _loop12;
					}
					
				} while (true);
				}
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			}
		}
		catch (RecognitionException ex) {
			reportError(ex);
			recover(ex,_tokenSet_3);
		}
		return result;
	}
	
	
	public static final String[] _tokenNames = {
		"<0>",
		"EOF",
		"<2>",
		"NULL_TREE_LOOKAHEAD",
		"DOT",
		"ID",
		"LEFT",
		"RIGHT",
		"NUMBER",
		"COMMA",
		"STRING",
		"PARAMS"
	};
	
	private static final long[] mk_tokenSet_0() {
		long[] data = { 2L, 0L};
		return data;
	}
	public static final BitSet _tokenSet_0 = new BitSet(mk_tokenSet_0());
	private static final long[] mk_tokenSet_1() {
		long[] data = { 3074L, 0L};
		return data;
	}
	public static final BitSet _tokenSet_1 = new BitSet(mk_tokenSet_1());
	private static final long[] mk_tokenSet_2() {
		long[] data = { 3090L, 0L};
		return data;
	}
	public static final BitSet _tokenSet_2 = new BitSet(mk_tokenSet_2());
	private static final long[] mk_tokenSet_3() {
		long[] data = { 128L, 0L};
		return data;
	}
	public static final BitSet _tokenSet_3 = new BitSet(mk_tokenSet_3());
	
	}
