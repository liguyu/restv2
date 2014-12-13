package com.aote.expression;

import javax.servlet.ServletContext;
import org.springframework.context.ApplicationContext;
import org.springframework.web.context.support.WebApplicationContextUtils;
import com.aote.expression.paramprocessor.NoFitValueException;
import com.aote.expression.paramprocessor.ParamProcessor;
import com.aote.listener.ContextListener;

/**
 * 参数处理器工厂,单例
 * 
 */
public class ParamProcessorFactory {

	private static ParamProcessorFactory self = new ParamProcessorFactory();


	private ParamProcessorFactory() {
	}

	public static ParamProcessorFactory getInstance() {
		return self;
	}

	/**
	 * 	// 创建参数对象,获取参数处理器，进行处理
	 * @param param
	 * @return
	 * @throws NoFitValueException
	 */
	public String process(String param) throws NoFitValueException {
		Param p = new Param(param);
		ServletContext sc = ContextListener.getContext();	 
    	ApplicationContext ctx= WebApplicationContextUtils.getWebApplicationContext(sc);
		ParamProcessor o = (ParamProcessor)ctx.getBean(p.getTag());
		return  o.process(p);
	}

	 
}
