package com.aote.expression;

import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import com.aote.helper.StringHelper;


/**
 * 参数
 * 
 */
public class Param {

	/**
	 * 参数标记
	 * 
	 * @param str
	 */
	private String tag = "";

	private Map params = new HashMap();

	public Param(String str) {
		int index = str.indexOf("?");
		if (index != -1) {
			this.tag = str.substring(0, index);
		}
		this.params = getParams(str.substring(index + 1, str.length()));
	}

	private Map getParams(String params) {
		Map result = new HashMap();
		List list = StringHelper.stringToLinkedList(params, '&');
		Iterator iter = list.iterator();
		while (iter.hasNext()) {
			String s = (String) iter.next();
			int index = s.indexOf('=');
			String name = s.substring(0, index);
			String value = s.substring(index + 1);
			result.put(name, value);
		}
		return result;
	}

	public String getTag() {
		return this.tag;
	}

	public Map getParams() {
		return this.params;
	}
}
