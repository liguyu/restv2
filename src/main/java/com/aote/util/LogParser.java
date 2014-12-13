package com.aote.util;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;

/**
 * 分析Tomcat的日志，辅助找到报装中丢失的用户花名单
 */
public final class LogParser {

	/**
	 * 运行方式为：java LogParser log文件名   报建名   结果文件名
     *
	 */
	public static void main(String[] args) {
		try {
			// 打开结果文件
			String output = "e:\\logparse.txt";
			String fold = "e:\\logdatas";
			String key = "11005930";
			FileWriter writer = new FileWriter(output);
			BufferedWriter bw = new BufferedWriter(writer);
			//参数0为目录名，对目录中的所有文件进行循环处理
		
			File dir = new File(fold);
			
			for(File file : dir.listFiles()) {
				System.out.println("正在处理：" + file.getName());
				processOneFile(file,key, bw);	
			}
			bw.flush();
			writer.flush();
			bw.close();
			writer.close();
			System.out.println("处理End：" );

		} catch (IOException e) {
			System.out.println(e.getMessage());
		}
	}
	
	//判断source中是否含有关键字keys，关键字按逗号分割。如果source中包含
	//给定的所有关键字，返回真。否则，返回false。
	public static boolean contains(String source, String keys) {
		for(String key : keys.split(",")) {
			//只要有不包含的，就返回false
			if(!source.contains(key)) {
				return false;
			}
		}
		return true;
	}
	
	//处理一个日志文件，从日志文件里找到含所有关键字的行，写到目标文件中。
	public static void processOneFile(File logFile, String keys, BufferedWriter bw){
		try {
			// 打开日志文件以及结果文件
			FileReader reader = new FileReader(logFile);
			BufferedReader br = new BufferedReader(reader);
			// 从日志文件里读一行
			int i = 0;
			String str = br.readLine();
			while(str != null) {
				// 如果包括报建名，对这一行格式化后存入结果文件中
				if(contains(str, keys)) {
					bw.append(i + "--" + str);
					bw.newLine();
				}
				str = br.readLine();
				i++;
			}
			br.close();
			reader.close();
		} catch (IOException e) {
			System.out.println(e.getMessage());
		}
	}
}
