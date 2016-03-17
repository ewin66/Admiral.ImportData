# Admiral.ImportData
请看这里，有图：
http://www.cnblogs.com/foreachlife/p/xafimportexcel.html

我实现了XAF项目中Excel数据的导入，使用Devexpress 新出的spreadsheet控件，可能也不新了吧:D

好，先看一下效果图：下图是Web版本的。



下面是win版：



 

功能说明：

支持从Excel任意版本导入数据，可以使用 打开文件功能选择现有的文件，没有模板时，请来到上图界面中，另存为Excel到本地，往模板上填加数据。

导入时使用了显示名称进行匹配字段，所以字段部分不要修改。

导入时会使用你在写好的验证规则。

支持Win+Web两个版本。

使用方法：

 



第一步：将Admiral.ImportData模块拖到你的项目的模块中去，上图为例，我将把Admiral.ImportData拖到图中A项目中，即MFBI.Module中去。

第二步：将Admiral.ImportData.Web 拖到B中。

第三步：将Admiral.ImportData.Win拖到C中。

你不知道模块在哪里？请看下图：



先打开solution 中的Module.cs, 然后从toolbox拖动ImportDataModule到Required Modules中。

当然这是把源码直接放到项目中去的方法，如果想直接使用DLL,可以编译好后，在toolbox中填加选择项，选择路径后，再进行拖动。

 

再来看看代码中的设置：

以下代码中有两处标红的，第一必须实现IImportData接口才可能导入，这个接口是空的，不用实现。DomainComponent也可以这样使用。

对于普通的字段，没有其他设置。

对于引用型字段，需要[ImportDefaultFilterCriteria("编码=?")]这样，来设置将来查找引用类型的值时，用什么条件进行查找，当然问号会被替换为Excel中真实的值。

复制代码
    [XafDisplayName("销量明细")]
    [NavigationItem("销售模块")]
    public class 销量明细 : BaseObject,IImportData
    {
        public 销量明细(Session s) : base(s)
        {

        }

        private 订单 _订单;
        [Association]

        [ImportDefaultFilterCriteria("编码=?")]
        public 订单 订单
        {
            get { return _订单; }
            set { SetPropertyValue("订单", ref _订单, value); }
        }

        ......
    }
复制代码
 

当前模块还比较简单 ，以后会慢慢完善。

 

 20160128已经更新了支持win下面除了ribbon以外其他的界面类型。

 

2016-3-17 将源码发布到github.

地址：https://github.com/tylike/Admiral.ImportData

修复了几个小BUG。

增加了更新导入功能，在业务对象上面写[UpdateImport("属性名称")]，其中属性名称是指，在导入时，使用哪个属性的值 Excel->库中的 进行比较，来确定此对象是否存在。

增加了[ImportOptions(false)]属性，可以写在属性或字段上面，设置为false时，即在导入时不显示此字段。

增加给Winform增加了图标。

导入完成后，在Excel中给出了提示信息，成功或不成功。

在查找引用属性时，如果没有使用ImportDefaultFilterCriteria属性进行设置，则按如下优先级进行：

1.看主键是否是非自动生成的，是，则使用主键查找。

2.看引用类上面是否有[RuleUniqueValue]标记的属性，有则使用。

3.看DefaultProperty是否有设置，有则使用。
