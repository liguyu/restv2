package com.aote.rs;

import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.WebApplicationException;
import javax.ws.rs.core.MediaType;

import org.apache.http.HttpEntity;
import org.apache.http.HttpHost;
import org.apache.http.HttpResponse;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.util.EntityUtils;
import org.apache.log4j.Logger;

import org.springframework.stereotype.Component;

/**
 * JSON service 调用隧道
 * 
 * @author lgy
 *
 */
@Path("tunnel")
@Component
public class Tunnel {
	static Logger log = Logger.getLogger(DBService.class);

	@GET
	@Produces(MediaType.APPLICATION_JSON)
	public String test() {
		return "Hello Tunnel";
	}
	/**
	 * 只处理get方式请求
	 * @return
	 */
	@GET
	@Path("{url}")
	@Produces(MediaType.APPLICATION_JSON)
	public String urlFallThrough(@PathParam("url") String url) {
	    DefaultHttpClient httpclient = new DefaultHttpClient();
	    try {
	      HttpGet getRequest = new HttpGet(url.replace("|", "/"));
	      HttpResponse httpResponse = httpclient.execute(getRequest);
	      HttpEntity entity = httpResponse.getEntity();
	      if(entity != null)
	    	  return EntityUtils.toString(entity);
	      else
	    	  return "";

	    } catch (Exception e) {
	    	throw new WebApplicationException(400);
	    } finally {
	      httpclient.getConnectionManager().shutdown();
	    }
	}

}
