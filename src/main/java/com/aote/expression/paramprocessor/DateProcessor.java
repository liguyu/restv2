package com.aote.expression.paramprocessor;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;

import com.aote.expression.Param;

public class DateProcessor implements ParamProcessor {

	public String process(Param param) {
		Date d = new Date();
		String format = (String) param.getParams().get("format");
		if (format == null) {
			return d.toString();
		}
		else {
			SimpleDateFormat sdf = new SimpleDateFormat(format);
			return sdf.format(d);		
		}
	}
}
