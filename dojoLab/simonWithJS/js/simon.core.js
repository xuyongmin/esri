/*
  甘露模型实现代码：add by xuym on 20140709
  最后一个参数是JSON表示的类定义
  如果参数数量大于1个，则第一个参数是基类
  第一个和最后一个之间的参数，将来可标识类实现的接口
  返回值是类，类是一个构造函数
*/

function Class() {
	var aDefine = arguments[arguments.length-1];//最后一个参数是类定义
	if (!aDefine) {return;};
	var aBase = arguments.length>1?arguments[0]:object;//解析基类
	function prototype_(){}; //构造prototype的临时函数，用于挂接原型链
	prototype_.prototype = aBase.prototype;//准备传递prototype
	var aPrototype = new prototype_();//建立要用的prototype
	for(var member in aDefine){
		if(member != 'Create'){
			aPrototype[member] = aDefine[member];
		}
	}

	//根据是否继承特殊属性和性能情况，可分别注释掉下列的语句
	if (aDefine.toString != Object.prototype.toString) {
		aPrototype.toString = aDefine.toString;
	};

	if (aDefine.toLocaleString != Object.prototype.toLocaleString) {
		aPrototype.toLocaleString = aDefine.toLocaleString;
	};

	if (aDefine.valueOf != Object.prototype.valueOf) {
		aPrototype.valueOf = aDefine.valueOf;
	};

	var aType;
	if (aDefine.Create) { //如果有构造函数
		aType = aDefine.Create;//类型即为该构造函数
	}else{ //否则为默认构造函数
		aType = function() {
			this.base.apply(this,arguments);
		}
	};

	aType.prototype = aPrototype; //设置类（构造函数）的prototype
	aType.Base = aBase; //设置类型关系
	aType.prototype.Type = aType; //为本类型对象扩展一个Type属性
	return aType;//返回构造函数作为类
}

//根类object定义
function object(){} //定义小写的object根类，用于实现最基础的方法等
object.prototype.isA = function(aType){
	var self = this.Type;
	while(self){
		if(self == aType) { return true; }
		self = self.Base;
	};
	return false;
};
object.prototype.base = function() {
	// var Caller = object.prototype.base.caller;
	// Caller && Caller.Base && Caller.Base.apply(this,arguments);
	var Base = this.Type.Base; //获取当前对象的基类
	if(!Base.Base){ //若基类已经没有基类
		Base.apply(this,arguments);
	}else{
		this.base = MakeBase(Base); //先覆写this.base
		Base.apply(this,arguments);
		delete this.base; //删除覆写的base属性
	}

	function MakeBase(Type){ //包装基类构造函数
		var Base = Type.Base;
		if(!Base.Base){return Base;}//基类已无基类，就无需包装
		return function(){//包装为引用临时变量Base的闭包函数
			this.base = MakeBase(Base);//先覆写this.base
			Base.apply(this,arguments);//再调用基类的构造函数
		}
	}
}