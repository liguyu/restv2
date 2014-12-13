package com.aote.rs;

import java.util.Collection;
import java.util.Date;
import java.util.Enumeration;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.Map;

import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.WebApplicationException;

import net.sf.json.JSONArray;
import net.sf.json.JSONObject;

import org.apache.log4j.Logger;

import com.browsesoft.Entity;
import com.browsesoft.EntityManager;
import com.browsesoft.dbtools.DBTools;
import com.browsesoft.resource.BasicResource;
import com.browsesoft.resource.LicensePolicyTool;
import com.browsesoft.user.User;

@Path("user")
public class UserService {
	static Logger log = Logger.getLogger(UserService.class);

	@GET
	@Path("systime")
	public String getTime() {
		long time = new Date().getTime();
		return "" + time;
	}

	@GET
	@Path("{username}")
	public String getUser(@PathParam("username") String name) {
		try {
			User user = (User) EntityManager.getInstance().getUserForLoginName(
					name);
			String result = getUserString(user);
			log.debug(result);
			return result;
		} catch (Exception e) {
			throw new WebApplicationException(e);
		}
	}

	@GET
	@Path("{username}/{password}")
	// 根据用户名密码获取用户信息
	public String getUserWithPass(@PathParam("username") String name,
			@PathParam("password") String password) {
		try {
			User user = (User) EntityManager.getInstance().getUserForLoginName(
					name);
			String userpass = (String) user.attributes.get("password");
			log.debug("系统中登记的用户密码: " + userpass);
			// 密码不同，抛出密码错误异常
			if (!password.equals(userpass)) {
				throw new WebApplicationException(401);
			}
			String result = getUserString(user);
			log.debug(result);
			return result;
		} catch (WebApplicationException e) {
			throw e;
		} catch (Exception e) {
			throw new WebApplicationException(e);
		}
	}

	@GET
	@Path("{username}/{password}/{module}")
	// 获取某个登录用户，指定模块的数据
	public String getUserWithPass(@PathParam("username") String name,
			@PathParam("password") String password,
			@PathParam("module") String module) {
		try {

			User user = (User) EntityManager.getInstance().getUser(name,
					password, isCase());
			// 密码不同，抛出密码错误异常
			if (user == null) {
				throw new WebApplicationException(401);
			}
			//如果用户为不允许使用，返回401
			String f_using = (String) user.getAttributes().get("f_using");
			if(f_using !=null && f_using.equals("否"))
			{
				throw new WebApplicationException(401);
			}
			String result = getUserString(user, module);
			log.debug(result);
			return result;
		} catch (WebApplicationException e) {
			throw e;
		} catch (Exception e) {
			throw new WebApplicationException(e);
		}
	}

	@GET
	@Path("/{module}")
	// 获取某个登录用户，指定模块的数据
	public String getUserWithPass(@PathParam("module") String module) {
		try {
			User  user = (User) request.getSession().getAttribute("loginUser");
			String result = getUserString(user, module);
			log.debug(result);
			return result;
		} catch (WebApplicationException e) {
			throw e;
		} catch (Exception e) {
			throw new WebApplicationException(e);
		}
	}

	@POST
	@Path("/entity")
	public String save(String data) {
		log.debug(data);
		try {
			// 解析数据，根据id得到对象，更新属性
			JSONArray array = JSONArray.fromObject(data);
			JSONObject obj = (JSONObject) array.iterator().next();
			JSONObject dataObj = obj.getJSONObject("data");
			Hashtable newAttrs = this.jsonToHash(dataObj);
			String id = (String) newAttrs.get("id");
			Entity entity = (Entity) EntityManager.getInstance().getObject(id);
			if (entity == null) {
				throw new WebApplicationException(500);
			}
			entity.getAttributes().putAll(newAttrs);
			entity.update();
			return obj.toString();
		} catch (Exception e) {
			throw new WebApplicationException(500);
		}
	}
	
	@POST
	@Path("/entityMemUpdate")
	public String saveMem(String data) {
		log.debug(data);
		try {
			// 解析数据，根据id得到对象，更新属性
			JSONArray array = JSONArray.fromObject(data);
			JSONObject obj = (JSONObject) array.iterator().next();
			JSONObject dataObj = obj.getJSONObject("data");
			Hashtable newAttrs = this.jsonToHash(dataObj);
			String id = (String) newAttrs.get("id");
			Entity entity = (Entity) EntityManager.getInstance().getObject(id);
			if (entity == null) {
				throw new WebApplicationException(500);
			}
			entity.getAttributes().putAll(newAttrs);
		//	entity.update();
			return obj.toString();
		} catch (Exception e) {
			throw new WebApplicationException(500);
		}
	}


	/**
	 * json数据转map
	 */
	private Hashtable jsonToHash(JSONObject obj) {
		Hashtable result = new Hashtable();
		Iterator iter = obj.keySet().iterator();
		while (iter.hasNext()) {
			String key = iter.next().toString();
			String value = obj.getString(key);
			if (value != null & !value.equals("null")) {
				key = key.toLowerCase();
				result.put(key, value);
			}
		}
		return result;
	}

	/**
	 * 看是否区分大小写
	 * 
	 * @param match
	 * @return
	 */
	private boolean isCase() {
		try {
			// 取出设置的是否区分大小写属性
			String sql = "select f_matchcase from t_setpassrule";
			String[][] temp = DBTools.executeQueryWithTableHead(sql);
			return temp[1][0].equals("区分大小写");
		} catch (Exception ex) {
			return false;
		}
	}

	// 获取用户信息，只获取指定模块的信息
	public String getUserString(User user, String module) {
		// 获得用户权限，包括用户所属组
		Collection rs = LicensePolicyTool.getRights(user, "function");
		// 对获得的权限进行处理，去掉父关系，保留子关系，以便Json转换，集合中保留最上层的父
		Map<String, Object> root = null;
		for (BasicResource func : getRoot(rs)) {
			// 如果与给定模块名称相同
			if (func.attributes.get("name").equals(module)) {
				root = getMap(func, rs);
				break;
			}
		}
		if (root == null) {
			throw new WebApplicationException(500);
		}
		// 从根元素中
		Map<String, Object> map = new HashMap<String, Object>(user.attributes);
		map.put("functions", root.get("children"));
		// 循环设置父对象
		BasicResource parent = (BasicResource) user.getParent();
		Map ht = map;
		while (parent != null) {
			Map parentMap = getMap(parent.attributes);
			ht.put("parent", parentMap);
			ht = parentMap;
			parent = (BasicResource) parent.getParent();
		}
		String result = JSONObject.fromObject(map).toString();
		return result;
	}

	// 获取资源的一般属性，不包括对象关系属性
	private Map getMap(Hashtable attrs) {
		Map result = new HashMap();
		Enumeration en = attrs.keys();
		while (en.hasMoreElements()) {
			Object key = en.nextElement();
			Object value = attrs.get(key);
			if (value.getClass().isPrimitive() || value instanceof String) {
				result.put(key, value);
			}
		}
		return result;
	}

	// 获取用户信息
	public String getUserString(User user) {
		// 获得用户权限，包括用户所属组
		Collection rs = LicensePolicyTool.getRights(user, "function");
		// 对获得的权限进行处理，去掉父关系，保留子关系，以便Json转换，集合中保留最上层的父
		LinkedList funcs = new LinkedList();
		for (BasicResource func : getRoot(rs)) {
			funcs.add(getMap(func, rs));
		}
		Map<String, Object> map = new HashMap<String, Object>(user.attributes);
		map.put("functions", funcs);
		String result = JSONObject.fromObject(map).toString();
		return result;
	}

	// 获得第一层功能列表
	public LinkedList<BasicResource> getRoot(Collection rs) {
		LinkedList<BasicResource> result = new LinkedList<BasicResource>();
		for (Object obj : rs) {
			BasicResource func = (BasicResource) obj;
			// 父不在这个集合中，就是根
			if (!rs.contains(func.getParent())) {
				result.add(func);
			}
		}
		return result;
	}

	// 处理一个功能项，把子添加到自己的属性子中，并且把所有子删除掉，要递归调用
	public Map<String, Object> getMap(BasicResource func, Collection rs) {
		Map<String, Object> result = new HashMap<String, Object>(
				func.attributes);
		// 获得该功能项有权访问的子
		LinkedList list = new LinkedList(func.getChildrenByType("function"));
		list.retainAll(rs);
		// 只有属性的子列表
		LinkedList attrList = new LinkedList();
		for (Object obj : list) {
			BasicResource f = (BasicResource) obj;
			Map<String, Object> child = getMap(f, rs);
			attrList.add(child);
		}
		// 设置子
		if (!attrList.isEmpty()) {
			result.put("children", attrList);
		}
		return result;
	}
}
