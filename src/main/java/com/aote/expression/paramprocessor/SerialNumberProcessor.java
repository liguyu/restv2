package com.aote.expression.paramprocessor;

import java.util.HashMap;
import java.util.Map;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.orm.hibernate3.HibernateTemplate;
import com.aote.expression.Param;

/**
 * 编号默认值处理器
 * 
 */
public class SerialNumberProcessor implements ParamProcessor {
    
	@Autowired  
	private HibernateTemplate hibernateTemplate;
 
 

	//根据name取数据， 如果没有编号，创建初始值为1的编号
	public String process(Param param) {
		String result ;
		Map paramData = param.getParams();
		String serialName = (String) paramData.get("name");
		String length = (String) paramData.get("length");
		
		Map obj = (Map) this.hibernateTemplate.get("serial", serialName);
		int temp;
		if (obj == null) {
			obj = new HashMap();
			obj.put("id", serialName);
			temp = 1;
		} else {
			temp = Integer.parseInt(obj.get("value") + "");
		}
		obj.put("value", temp + 1);
		this.hibernateTemplate.saveOrUpdate("serial", obj);
		//根据设定的长度补零
		result = temp+"";
		if (length == null || length.equals("")) {
			return result;
		}
		// 长度不足得载编号前加0
		int defLength = Integer.parseInt(length);
		int numLength = result.length();
		for (int i = 0; i < (defLength - numLength); i++) {
			result = "0" + result;
		}
		return result;
	}
}
