# Swifter.Core

#### Swifter.Core 有助于您解除语言限制，编写您最优秀的 .Net 程序。

## Swifter.Core 实现的旗舰功能：
	(1): 几乎所有常用类型的超高性能对象映射工具。
	(2): 效率超高数学算法！超 .Net 自带算法 10+ 倍。
	(3): 开放的委托接口！创建您最实用，性能最好的委托吧。
	(4): 极致性能的缓存工具。线程安全的 亿/秒 读取性能，比线程不安全的 Dictionary 还要快两倍！
	(5): 开放指针工具！允许您获取对象的地址，字段偏移量，类型的大小等底层信息。
	(6): 高性能的类型转换工具 XConvert！允许您将任意类型转换为任意类型，只要它们本身支持转换。
	(7): 解决 .Net20 到 .Net471 的版本兼容问题。 引用 Swifter.Core 允许您在低版本中使用 元组，dynamic，LINQ 等。


## 已支持映射的对象或值类型有
	bool, byte, sbyte, char, shoft, ushoft, int, uint, long, ulong,
	float, double, decimal, string, enum DateTime, DateTimeOffset,
	Guid, TimeSpan, DBNull, Nullable<T>, Version, Type,
	Array, Multidimensional-Arrays, IList, IList<T>, ICollection,
	ICollection<T>, IDictionary, IDictionary<TKey, TValue>,
	IEnumerable, IEnumerable<T>, DataTable, DbDataReader ...
	其余类型将会被当作 Object，以 属性键/属性值 的形式映射。

## 高效的数学算法
	(1): 大数字加减乘除算法
	(2): 整型和浮点型 2-64 进制 ToString 和 Parse 算法
	(3): Guid 和 时间的 ToString 和 Parse 算法。

## 创新技术
	(1): Difference-Switch 字符串匹配算法，比 Hash 匹配快 3 倍！
	(2): 支持 ref 属性！现在允许您在实体中定义 ref 属性降低程序内存啦。
	(3): 内部三种实现创建委托，支持创建 99.9% 方法的委托！（仅当 TypedReference* 作为参数类型的方法不支持。）

## 数学算法性能对比

## 对象映射简单性能对比

## 缓存性能对比

## 委托动态执行的性能对比


## 高端玩法

#### (1): 将一个对象的复制到字典中 (反之亦然。)：

#### (2): 将一个对象转为结构地址，并设置它的私有字段的值：

#### (3): 将一个数字转换为 52 进制的字符串：
