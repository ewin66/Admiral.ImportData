// Developer Express Code Central Example:
// XAF Project Management Application - Stage 8
// 
// This example contains the source code for the XAF Project Management Application
// - Stage 8. The complete description can be found in the XAF – Project Management
// Application #10
// (http://community.devexpress.com/blogs/garyshort/archive/2009/09/30/xaf-project-management-application-10.aspx)
// blog post.
// 
// You can find sample updates and versions for different programming languages here:
// http://www.devexpress.com/example=E1750

using System;
using System.Collections.Generic;
using System.Text;
using Admiral.ImportData;
using DevExpress.ExpressApp.Editors;
using DevExpress.XtraEditors;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;

namespace Admiral.DataImport.Editors.Win
{
    [PropertyEditor(typeof(decimal), false)]
    public class WinProgressEdit : DXPropertyEditor
    {
        public WinProgressEdit(Type objectType, IModelMemberViewItem model) : base(objectType, model) { }
        protected override object CreateControlCore()
        {
            return new TaskProgressBarControl();
        }
        protected override RepositoryItem CreateRepositoryItem()
        {
            return new RepositoryItemTaskProgressBarControl();
        }
        

        protected override void SetupRepositoryItem(RepositoryItem item)
        {
            RepositoryItemTaskProgressBarControl repositoryItem = (RepositoryItemTaskProgressBarControl)item;
            repositoryItem.Maximum = 100;
            repositoryItem.Minimum = 0;
            base.SetupRepositoryItem(item);
        }
    }
    public class TaskProgressBarControl : ProgressBarControl
    {
        static TaskProgressBarControl()
        {
            
            RepositoryItemTaskProgressBarControl.Register();
        }
        public override string EditorTypeName { get { return RepositoryItemTaskProgressBarControl.EditorName; } }
        protected override object ConvertCheckValue(object val)
        {
            return val;
        }

        public TaskProgressBarControl()
        {
            base.Properties.Maximum = 100;
        }
    }
    public class RepositoryItemTaskProgressBarControl : RepositoryItemProgressBar
    {
        protected internal const string EditorName = "TaskProgressBarControl";
        protected internal static void Register()
        {
            if (!EditorRegistrationInfo.Default.Editors.Contains(EditorName))
            {
                EditorRegistrationInfo.Default.Editors.Add(new EditorClassInfo(EditorName, typeof(TaskProgressBarControl),
                    typeof(RepositoryItemTaskProgressBarControl), typeof(ProgressBarViewInfo),
                    new ProgressBarPainter(), true, EditImageIndexes.ProgressBarControl, typeof(DevExpress.Accessibility.ProgressBarAccessible)));
            }
        }
        static RepositoryItemTaskProgressBarControl()
        {
            Register();
        }
        public RepositoryItemTaskProgressBarControl()
        {
            Maximum = 100;
            Minimum = 0;
            ShowTitle = true;
            Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        }
        protected override int ConvertValue(object val)
        {
            try
            {
                float number = Convert.ToSingle(val);
                return (int)(Minimum + number * Maximum);
            }
            catch { }
            return Minimum;
        }

        public override string EditorTypeName { get { return EditorName; } }
    }

}
