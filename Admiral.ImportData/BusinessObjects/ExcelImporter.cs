using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Spreadsheet;
using DevExpress.Xpo;

namespace Admiral.ImportData
{
    public class ExcelImporter
    {
        XafApplication _application;
        IImportOption option;
        public void Setup(XafApplication app)
        {
            _application = app;

        }

        public ExcelImporter()
        {

        }
        public void ProcessImportAction(IWorkbook document)
        {
            var os = _application.CreateObjectSpace();
            bool rst = true;
            foreach (var item in document.Worksheets)
            {
                var typeName = item.Cells[0, 1].DisplayText;
                var t = option.MainTypeInfo.Application.BOModel.GetClass(ReflectionHelper.FindType(typeName));
                var success = StartImport(item, t, os);

                item.Cells[0, 3].SetValue(success ? "导入成功" : "导入失败");

                rst &= success;

            }

            if (rst)
            {
                try
                {
                    os.CommitChanges();
                }
                catch (Exception ex)
                {
                    os.Rollback();
                }
            }
            else {
                os.Rollback();
            }
        }

        public static Action DoApplicationEvent { get; set; }

        private bool StartImport(Worksheet ws, IModelClass bo, IObjectSpace os)
        {
            //开始导入:
            //1.先使用表头的标题找到属性名称
            Dictionary<int, IModelMember> fields = new Dictionary<int, IModelMember>();
            List<SheetRowObject> objs = new List<SheetRowObject>();
            //var ws = _spreadsheet.Document.Worksheets[0];
            var columnCount = ws.Columns.LastUsedIndex;

            var updateImport = bo.TypeInfo.FindAttribute<UpdateImportAttribute>();
            var isUpdateImport = updateImport != null;
            var keyColumn = 0;
            IModelMember keyField = null;
            for (int c = 1; c <= columnCount; c++)
            {
                var fieldCaption = ws.Cells[1, c].DisplayText;
                var fieldName = bo.AllMembers.SingleOrDefault(x => x.Caption == fieldCaption);
                fields.Add(c, fieldName);
                if (isUpdateImport && fieldName.Name == updateImport.KeyMember)
                {
                    keyColumn = c;
                    keyField = fieldName;
                }
            }

            var sheetContext = new SheetContext(ws);
            var rowCount = ws.Rows.LastUsedIndex;

            for (int r = 2; r <= rowCount; r++)
            {
                //ws.Cells[r, 0].ClearContents();

                for (int c = 1; c <= columnCount; c++)
                {
                    var cel = ws.Cells[r, c];
                    if (cel.FillColor != Color.Empty)
                        cel.FillColor = Color.Empty;

                    if (cel.Font.Color != Color.Empty)
                        cel.Font.Color = Color.Empty;
                }
            }


            for (int r = 2; r <= rowCount; r++)
            {
                XPBaseObject obj;
                if (isUpdateImport)
                {
                    var cdvalue = Convert.ChangeType(ws.Cells[r, keyColumn].Value.ToObject(), keyField.Type);
                    var cri = new BinaryOperator(updateImport.KeyMember, cdvalue);
                    obj = os.FindObject(bo.TypeInfo.Type, cri) as XPBaseObject;
                    if (obj == null)
                    {
                        obj = os.CreateObject(bo.TypeInfo.Type) as XPBaseObject;
                    }
                }
                else
                {
                    obj = os.CreateObject(bo.TypeInfo.Type) as XPBaseObject;
                }

                var result = new SheetRowObject(sheetContext) {Object = obj, Row = r, RowObject = ws.Rows[r]};
                //var vle = ws.Cells[r, c];
                for (int c = 1; c <= columnCount; c++)
                {
                    var field = fields[c];
                    var cell = ws.Cells[r, c];

                    if (!cell.Value.IsEmpty)
                    {
                        object value = null;
                        //引用类型
                        if (typeof (XPBaseObject).IsAssignableFrom(field.MemberInfo.MemberType))
                        {
                            var conditionValue = cell.Value.ToObject();
                            //如果指定了查找条件，就直接使用
                            var idf = field.MemberInfo.FindAttribute<ImportDefaultFilterCriteria>();
                            var condition = idf == null ? "" : idf.Criteria;

                            #region 查找条件

                            if (string.IsNullOrEmpty(condition))
                            {
                                //没指定查找条件，主键不是自动生成的，必定为手工输入
                                if (!field.MemberInfo.MemberTypeInfo.KeyMember.IsAutoGenerate)
                                {
                                    condition = field.MemberInfo.MemberTypeInfo.KeyMember.Name + " = ?";
                                }
                            }

                            if (string.IsNullOrEmpty(condition))
                            {
                                //还是没有，找设置了唯一规则的
                                var ufield =
                                    field.MemberInfo.MemberTypeInfo.Members.FirstOrDefault(
                                        x => x.FindAttribute<RuleUniqueValueAttribute>() != null
                                        );
                                if (ufield != null)
                                    condition = ufield.Name + " = ? ";
                            }

                            if (string.IsNullOrEmpty(condition))
                            {
                                //还是没有，用defaultproperty指定的
                                var ufield = field.MemberInfo.MemberTypeInfo.DefaultMember;
                                if (ufield != null)
                                {
                                    condition = ufield.Name + " = ? ";
                                }
                            }

                            #endregion

                            #region p

                            if (string.IsNullOrEmpty(condition))
                            {
                                result.AddErrorMessage(
                                    string.Format(
                                        "错误，没有为引用属性{}设置查找条件，查询过程中出现了错误，请修改查询询条!",
                                        field.MemberInfo.Name), cell);
                            }
                            else
                            {
                                try
                                {
                                    var @operator = CriteriaOperator.Parse(condition, new object[] {conditionValue});
                                    var list = os.GetObjects(field.MemberInfo.MemberType, @operator, true);
                                    if (list.Count != 1)
                                    {
                                        result.AddErrorMessage(
                                            string.Format(
                                                "错误，在查找“{0}”时，使用查找条件“{1}”，输入值是：“{3}”，查询过程中出现了错误，请修改查询询条!错误详情:{2}",
                                                field.MemberInfo.MemberType.FullName, condition,
                                                "找到了" + list.Count + "条记录", conditionValue), cell);
                                    }
                                    else
                                    {
                                        value = list[0];
                                    }
                                }
                                catch (Exception exception1)
                                {
                                    result.AddErrorMessage(
                                        string.Format("错误，在查找“{0}”时，使用查找条件“{1}”，查询过程中出现了错误，请修改查询询条!错误详情:{2}",
                                            field.MemberInfo.MemberType.FullName, condition, exception1.Message),
                                        cell);
                                }
                            }

                            #endregion

                        }
                        else if (field.MemberInfo.MemberType == typeof (DateTime))
                        {
                            if (!cell.Value.IsDateTime)
                            {
                                result.AddErrorMessage(string.Format("字段:{0},要求输入日期!", field.Name), cell);
                            }
                            else
                            {
                                value = cell.Value.DateTimeValue;
                            }
                        }
                        else if (field.MemberInfo.MemberType == typeof (decimal) ||
                                 field.MemberInfo.MemberType == typeof (int) ||
                                 field.MemberInfo.MemberType == typeof (long) ||
                                 field.MemberInfo.MemberType == typeof (short)
                            )
                        {
                            if (!cell.Value.IsNumeric)
                            {
                                result.AddErrorMessage(string.Format("字段:{0},要求输入数字!", field.Name), cell);
                            }
                            else
                            {
                                value = Convert.ChangeType(cell.Value.NumericValue, field.MemberInfo.MemberType);
                            }
                        }
                        else if (field.MemberInfo.MemberType == typeof (bool))
                        {
                            if (!cell.Value.IsNumeric)
                            {
                                result.AddErrorMessage(string.Format("字段:{0},要求输入布尔值!", field.Name), cell);
                            }
                            else
                            {
                                value = cell.Value.BooleanValue;
                            }
                        }
                        else if (field.MemberInfo.MemberType == typeof (string))
                        {
                            var v = cell.Value.ToObject();
                            if (v != null)
                                value = v.ToString();
                        }
                        else if (field.MemberInfo.MemberType.IsEnum)
                        {
                            var names = field.MemberInfo.MemberType.GetEnumNames();
                            if (names.Contains(cell.Value.TextValue))
                            {
                                value = Enum.Parse(field.MemberInfo.MemberType, cell.Value.TextValue);
                            }
                            else
                            {
                                result.AddErrorMessage(string.Format("字段:{0},所填写的枚举值，没在定义中出现!", field.Name), cell);
                            }
                        }
                        obj.SetMemberValue(field.Name, value);
                    }
                }
                objs.Add(result);

                if (DoApplicationEvent != null)
                {
                    DoApplicationEvent();

                    this.option.Progress = ((r/(decimal)rowCount));
                    //Debug.WriteLine(this.option.Progress);
                    //var progress = ws.Cells[r, 0];
                    //progress.SetValue("完成");
                }
            }

            if (objs.All(x => !x.HasError)){
                try
                {
                    Validator.RuleSet.ValidateAll(os, objs.Select(x => x.Object), "Save");
                    return true;
                }
                catch (ValidationException msgs)
                {
                    var rst = true;
                    foreach (var item in msgs.Result.Results)
                    {
                        if (item.Rule.Properties.ResultType == ValidationResultType.Error && item.State == ValidationState.Invalid)
                        {
                            var r = objs.FirstOrDefault(x => x.Object == item.Target);
                            if (r != null)
                            {
                                r.AddErrorMessage(item.ErrorMessage, item.Rule.UsedProperties);
                            }
                            rst &= false;
                        }
                    }
                    return rst;
                }
            }
            return false;
        }

        IModelMember[] GetMembers(IModelClass cls)
        {
            return cls.AllMembers.Where(x =>
                !x.MemberInfo.IsAutoGenerate &&
                !x.IsCalculated &&
                !x.MemberInfo.IsReadOnly &&
                !x.MemberInfo.IsList
                ).ToArray().Except(cls.AllMembers.Where((x) =>
                {
                    var ia = x.MemberInfo.FindAttribute<ImportOptionsAttribute>();
                    return ia != null && !ia.NeedImport;
                }
                    )
                ).ToArray();
        }

        public void InitializeExcelSheet(IWorkbook book, IImportOption option)
        {
            this.option = option;
            CreateSheet(book.Worksheets[0], option.MainTypeInfo);
            if (book.Worksheets.Count == 1)
            {
                var listProperties = option.MainTypeInfo.AllMembers.Where(x => x.MemberInfo.IsList && x.MemberInfo.ListElementTypeInfo.IsPersistent);
                foreach (var item in listProperties)
                {
                    var cls = option.MainTypeInfo.Application.BOModel.GetClass(item.MemberInfo.ListElementTypeInfo.Type);
                    var b = book.Worksheets.Add(cls.Caption);
                    CreateSheet(b, cls);
                }
            }
        }

        private void CreateSheet(Worksheet book, IModelClass boInfo)
        {
            book.Name = boInfo.Caption;
            book.Cells[0, 0].Value = "系统类型";
            book.Cells[0, 1].Value = boInfo.TypeInfo.FullName;
            book.Cells[0, 2].Value = "本行信息为导入时对应系统业务信息，请勿删除!";
            //1.第一行，用于显示消息.
            //2.第一列，用于显示录入错误信息。
            var i = 1;
            #region main
            var cells = book.Cells;
            var members = GetMembers(boInfo);
            foreach (var item in members)
            {
                var c = cells[1, i];
                c.Value = item.Caption;
                c.FillColor = Color.FromArgb(255, 153, 0);
                c.Font.Color = Color.White;
                var isRequiredField = IsRequiredField(item);

                var range = book.Range.FromLTRB(i, 2, i, 20000);

                //DataValidation dv = null;

                if (isRequiredField)
                {
                    c.Font.Bold = true;
                }
                i++;
            }
            #endregion
        }

        IRule[] _rules;
        IRule[] Rules
        {
            get
            {
                if (_rules == null)
                {
                    _rules = Validator.RuleSet.GetRules().ToArray();
                }
                return _rules;
            }
        }

        public bool IsRequiredField(IModelMember member)
        {
            //Rules.Where(x=>x.t)
            return Rules.Any(x => x.Properties is IRuleRequiredFieldProperties && x.Properties.TargetType == member.ModelClass.TypeInfo.Type && x.UsedProperties.IndexOf(member.Name) > -1);

        }
    }
}