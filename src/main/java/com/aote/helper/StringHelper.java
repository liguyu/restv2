package com.aote.helper;

import java.util.LinkedList;

/**
 * 字符串工具类
 * @author Administrator
 *
 */
public class StringHelper {

	
	/**
	 * 把以给定字符分割的字符串分解成字符串向量
	 */
	public static LinkedList stringToLinkedList(String source, char ch) {
		LinkedList v = new LinkedList();
		if (source == null || source.equals("")) {
			return v;
		}
		int start = 0;
		int end = source.indexOf(ch);
		while (end != -1) {
			String str = source.substring(start, end);
			v.add(str);
			start = end + 1;
			end = source.indexOf(ch, start);
		}
		String str = source.substring(start);
		v.add(str);
		return v;
	}
}
