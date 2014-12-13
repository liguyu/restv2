package com.aote.expression.paramprocessor;

 

import com.aote.expression.Param;

/**
 * 参数处理器
 * 
 */
public interface ParamProcessor {

	/**
	 * 根据参数和属性集合进行默认值处理
	 * 
	 * @param attribures
	 *            属性集合
	 * @param param
	 *            参数
	 * @param loginUser
	 *            登录用户
	 * @return
	 */
	public String process(Param param) throws NoFitValueException;
}
