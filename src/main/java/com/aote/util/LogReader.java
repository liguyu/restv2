package com.aote.util;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;

public class LogReader {
	// 从一个大的日志文件里读取给定行数，到结果文件中
	public static void main(String[] args) {
		try {
			// 打开结果文件
			FileWriter writer = new FileWriter(args[2]);
			BufferedWriter bw = new BufferedWriter(writer);
			FileReader reader = new FileReader(args[0]);
			BufferedReader br = new BufferedReader(reader);
			// 获得开始行，以及总行数
			String[] nums = args[1].split(",");
			int start = Integer.parseInt(nums[0]);
			int len = Integer.parseInt(nums[1]);
			// 读掉开始行
			for(int i = 0; i < start; i++) {
				br.readLine();
			}
			// 读取数据，放到目标文件中
			for(int i = 0; i < len; i++) {
				String str = br.readLine();
				bw.append(str);
				bw.newLine();
			}
			br.close();
			reader.close();
			bw.flush();
			writer.flush();
			bw.close();
			writer.close();
		} catch (IOException e) {
			System.out.println(e.getMessage());
		}
	}
}
