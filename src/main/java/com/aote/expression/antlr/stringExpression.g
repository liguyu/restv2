header {
package com.aote.expression.antlr;
import com.browsesoft.htmlcomponent.HTMLBasicComponent;
import com.aote.expression.ParamProcessorFactory;
import java.util.Map;
import com.browsesoft.user.User;
import java.util.List;
import java.util.LinkedList;
import com.aote.expression.function.StringFunctionFactory;
import org.w3c.dom.Element;
}

class StringExpression extends Parser ;
{
	private StringLexer lexer;
	
	public void setLexer(StringLexer lexer) {
		this.lexer = lexer;
	}
	
    private String process(String param,Map attrs,User loginUser,HTMLBasicComponent component,Element config)
    {
        return ParamProcessorFactory.getInstance().process(param,attrs,loginUser,component,config);
    }

	private String getParamString(String param) {
		int len = param.length();
		return param.substring(1, len - 1);
	}
}

expression [Map attrs,User loginUser,HTMLBasicComponent component,Element config]
returns [String result=""] {
    String s;
}
	: result=stringCut[attrs,loginUser,component,config] (s=stringCut[attrs,loginUser,component,config] {result += s;})*
	;
	
stringCut [Map attrs,User loginUser,HTMLBasicComponent component,Element config]
returns [String result=""] {
	String s;
}
	: (s=string[attrs,loginUser,component,config] (DOT {this.lexer.inFunction=true;} s=cutFunction[s] {this.lexer.inFunction=false;})*) {result=s;}
	;
	
cutFunction [String str] 
returns [String result=""] {
    List pl;
}
	: (id:ID LEFT pl=paramList RIGHT) {result=StringFunctionFactory.getInstance().invoke(id.getText(), str, pl);}
	;

paramList
returns [List result=new LinkedList()]
	:
	| s:NUMBER {result.add(s.getText());} (COMMA s1:NUMBER {result.add(s1.getText());})*
	;
	
string [Map attrs,User loginUser,HTMLBasicComponent component,Element config]
returns [String result=""]
	: (s:STRING {result += s.getText();} | p:PARAMS {result += process(getParamString(p.getText()),attrs,loginUser,component,config);})+
	;

class StringLexer extends Lexer; 
{
	boolean inFunction = false;
}

PARAMS
	: '#' (~'#')+ '#'
	;
DOT
	: '.'
	;
COMMA
	: {this.inFunction}? ','
	;
LEFT
	: {this.inFunction}? '('
	;
RIGHT
	: {this.inFunction}? ')'
	;
NUMBER
	: {this.inFunction}? ('0'..'9')+
	;
ID
	: {this.inFunction}? ('a'..'z'|'A'..'Z')('a'..'z'|'A'..'Z'|'0'..'9'|'_')*
	;
STRING
	: (~('#' | '.' | '\u0080'..'\ufffe') | '\u0080'..'\ufffe')+
	;
