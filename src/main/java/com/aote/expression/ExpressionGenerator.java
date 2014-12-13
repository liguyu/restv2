package com.aote.expression;

import java.io.Reader;
import java.io.StringReader;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import javax.servlet.http.HttpServletRequest;

import com.aote.expression.antlr.StringExpression;
import com.aote.expression.antlr.StringLexer;

 

 

/**
 * 表达式值产生器 从页面中查找表达式配置并进行处理
 * 
 */
public class ExpressionGenerator {

	/**
	 * 单例
	 */
	private static ExpressionGenerator self = new ExpressionGenerator();

	private ExpressionGenerator() {
	}

	public static ExpressionGenerator getInstance() {
		return self;
	}

	/**
	 * 创建表达式
	 */
	public static Object getExpressionValue(String exp) {
		try {
			Object result = null;
			Reader r = new StringReader(exp);
		    StringLexer lexer = new StringLexer(r);
			StringExpression parser = new StringExpression(lexer);
			parser.setLexer(lexer);
			result = parser.expression();
			return result;
		} catch (RuntimeException e) {
			throw e;
		} catch (Exception e) {
			throw new RuntimeException(e);
		}
	}

 
}