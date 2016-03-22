using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Threading.Tasks;

namespace Admiral.ImportData
{
    //数据导入模块实现方法：
    //1.业务逻辑类中实现了IImportData接口后，即认为将启用数据导入功能

    //2.定义一个数据导入界面业务的逻辑
    public interface IImportOption
    {
        IModelClass MainTypeInfo { get; set; }
        decimal Progress { get; set; }
        Action<decimal> UpdateProgress { get; set; }
    }

    public class ImportOption : IImportOption
    {
        public IModelClass MainTypeInfo
        {
            get; set;
        }

        private decimal progress;

        public decimal Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                if (UpdateProgress != null)
                    UpdateProgress(value);
            }
        }

        public Action<decimal> UpdateProgress { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class UpdateImportAttribute : Attribute
    {
        public string KeyMember { get; set; }

        public UpdateImportAttribute(string keyMember)
        {
            this.KeyMember = keyMember;
        }
    }

    public interface IImportData { }

    [XafDisplayName("数据导入")]
    [NonPersistent]
    [ImageName("ImportData")]
    public class ImportData : BaseObject
    {
        public ImportData(Session s) : base(s)
        {

        }

        public IImportOption Option { get; set; }

        private decimal _Progress;
        [XafDisplayName("进度")][ModelDefault("AllowEdit","False")]
        public decimal Progress
        {
            get { return _Progress; }
            set { SetPropertyValue("Progress", ref _Progress, value); }
        }

    }

    //弹出窗口后，根据元数据信息列举出字段名
    //如果是简单类型，直接列举
    //如果是子列表，则新建一个sheet

    //确认后，根据BO中书写的验证规则验证数据，不满足的数据，给出错误提示，交需要将错误与Excel行做上对应

    /// <summary>
    /// 在导入数据时，指定引用型属性如何去查找值
    /// 如何定的是：“Name=?”则使用Excel中单元格的值去替换？号的值。
    /// 有结构：客户（姓名、年龄、联系人｛联系人姓名，手机｝），在导入客户信息时，联系人是引用型属性，指定了 “联系人姓名”后则可以导入。
    /// 指定的条件为 在联系人属性上设置[ImportDefaultFilterCriteria("联系人姓名=?")]即可。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ImportDefaultFilterCriteria : Attribute
    {
        // Methods
        public ImportDefaultFilterCriteria(string criteria)
        {
            this.Criteria = criteria;
        }

        // Properties
        public string Criteria { get; set; }
    }

    /// <summary>
    /// 指示属性、字段是否需要导入
    /// </summary>
    [AttributeUsage(AttributeTargets.Property| AttributeTargets.Field )]
    public class ImportOptionsAttribute : Attribute
    {
        public bool NeedImport { get;private set; } 

        public ImportOptionsAttribute(bool needImport)
        {
            this.NeedImport = needImport;
        }
    }
}
