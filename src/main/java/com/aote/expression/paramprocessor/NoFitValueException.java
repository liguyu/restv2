package com.aote.expression.paramprocessor;

import com.aote.expression.Param;

public class NoFitValueException extends RuntimeException {

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	public NoFitValueException(Param param) {
		super("参数" + param.getTag() + "的没有合适参数值");
	}
}
