using System;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using Admiral.ImportData;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

namespace Admiral.DataImport.Sample.Module.BusinessObjects
{
    [DefaultClassOptions]
    public class Order : BaseObject,IImportData
    { 
        public Order(Session session)
            : base(session)
        {
        }

        private string _Code;
        public string Code
        {
            get { return _Code; }
            set { SetPropertyValue("Code", ref _Code, value); }
        }

        private DateTime _Date;
        public DateTime Date
        {
            get { return _Date; }
            set { SetPropertyValue("Date", ref _Date, value); }
        }

        private bool _Active;
        public bool Active
        {
            get { return _Active; }
            set { SetPropertyValue("Active", ref _Active, value); }
        }

        private int _Level;
        public int Level
        {
            get { return _Level; }
            set { SetPropertyValue("Level", ref _Level, value); }
        }

        private decimal _SumPrice;
        public decimal SumPrice
        {
            get { return _SumPrice; }
            set { SetPropertyValue("SumPrice", ref _SumPrice, value); }
        }

        private string _ImportHidden;
        [ImportOptions(false)]
        public string ImportHidden
        {
            get { return _ImportHidden; }
            set { SetPropertyValue("ImportHidden", ref _ImportHidden, value); }
        }




        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }
        
    }

    public class OrderItem : BaseObject
    {
        public OrderItem(Session s):base(s)
        {
            
        }

        private string _Code;
        public string Code
        {
            get { return _Code; }
            set { SetPropertyValue("Code", ref _Code, value); }
        }

        private string _Memo;
        public string Memo
        {
            get { return _Memo; }
            set { SetPropertyValue("Memo", ref _Memo, value); }
        }

    }
}
