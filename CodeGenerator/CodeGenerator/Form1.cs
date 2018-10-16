using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


// Created By Varun 11-10-2018
namespace CodeGenerator
{
    public partial class Form1 : Form
    {
        string ConnectionString = "server=.;initial catalog=AnPost;integrated security=true";
        private StringBuilder _Service = new StringBuilder();
        private StringBuilder _ServiceInterface = new StringBuilder();
        private StringBuilder _ViewModel = new StringBuilder();
        private StringBuilder _Controller = new StringBuilder();
        private StringBuilder _ListPage = new StringBuilder();
        private StringBuilder _EditableList = new StringBuilder();
        private StringBuilder _FullController = new StringBuilder();
        private StringBuilder _EditableController = new StringBuilder();
        private StringBuilder _FullViewPage = new StringBuilder();

        public Form1()
        {

            InitializeComponent();
        }

        #region Get Model
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            CreateModel();
            MessageBox.Show("Model Created Sucessfully");
        }
        #endregion Get Model

        #region Service
        public void CreateService()
        {
            // Service
            _Service.AppendLine($"using ABS.Core;");
            _Service.AppendLine($"using ABS.Core.Caching;");
            _Service.AppendLine($"using ABS.Core.{txttablefoldername.Text}.{txtPageName.Text};");
            _Service.AppendLine($"namespace ABS.Services.{txtPageName.Text}");
            _Service.AppendLine("{");
            _Service.AppendLine($"public class {txtPageName.Text}Service : SeriveBase<{txtPageName.Text}>, I{txtPageName.Text}Service");
            _Service.AppendLine("{");
            _Service.AppendLine($"public {txtPageName.Text}Service(ICacheManager cacheManager, IRepository<{txtPageName.Text}> repository) : base(cacheManager, repository)");
            _Service.AppendLine("{");
            _Service.AppendLine("}");
            _Service.AppendLine("}");
            _Service.AppendLine("}");
            // Service

            // Service Interface
            _ServiceInterface.AppendLine($"using ABS.Core.{txttablefoldername.Text}.{txtPageName.Text};");
            _ServiceInterface.AppendLine($"namespace ABS.Services.{txtPageName.Text}Service");
            _ServiceInterface.AppendLine("{");
            _ServiceInterface.AppendLine($"public interface I{txtPageName.Text}Service : IServiceBasee<{txtPageName.Text}>");
            _ServiceInterface.AppendLine("{");
            _ServiceInterface.AppendLine("}");
            _ServiceInterface.AppendLine("}");
            // Service Interface

            //Save
            string ServicePath = txtfilepath.Text + "\\" + txtPageName.Text + "\\" + txtPageName.Text + "Service.cs";
            string IServicePath = txtfilepath.Text + "\\" + txtPageName.Text + "\\I" + txtPageName.Text + "Service.cs";
            FileSave(ServicePath, _Service);
            FileSave(IServicePath, _ServiceInterface);
            //Save
        }
        #endregion Service

        #region Model
        public void CreateModel()
        {
            var itemlist = (List<TableList>)dataGridView1.DataSource;
            List<string> ExtraColumn = new List<string>();
            SqlConnection myConnection = new SqlConnection();
            myConnection.ConnectionString = ConnectionString;
            string sqlQry = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{txtPageName.Text.ToLower()}' ORDER BY ORDINAL_POSITION";
            SqlDataAdapter myAdapter = new SqlDataAdapter(sqlQry, myConnection);
            DataSet myDataSet = new DataSet("Columns");
            myAdapter.Fill(myDataSet, "Columns");
            var empList = myDataSet.Tables[0].AsEnumerable().Select(o => o.ItemArray[0].ToString()).ToList();
            List<string> InnerModel = new List<string>();
            ExtraColumn.Add("Id");
            ExtraColumn.Add("CompanyId");
            ExtraColumn.Add("Status");
            ExtraColumn.Add("CreatedBy");
            ExtraColumn.Add("CreatedDate");
            ExtraColumn.Add("UpdatedBy");
            ExtraColumn.Add("UpdatedDate");
            foreach (var item in empList.Where(o => !ExtraColumn.Contains(o) && o.EndsWith("Id")))
            {
                InnerModel.Add(item.ToString().Replace("Id", "Model"));
            }
            empList.AddRange(InnerModel);
            _ViewModel.AppendLine("using ABS.Web.Framework;");
            _ViewModel.AppendLine("using ABS.Web.Framework.Mvc;");
            _ViewModel.AppendLine("using ABS.Web.Validation;");
            _ViewModel.AppendLine("using FluentValidation.Attributes;");
            _ViewModel.AppendLine("using System;");
            _ViewModel.AppendLine("using System.Collections.Generic;");
            _ViewModel.AppendLine("using System.Linq;");
            _ViewModel.AppendLine("using System.Web;");
            _ViewModel.AppendLine("using System.Web.Mvc;");
            _ViewModel.AppendLine("namespace ABS.Web.Models");
            _ViewModel.AppendLine("{");
            _ViewModel.AppendLine($"public partial class {txtPageName.Text}Model : BaseABSEntityModel");
            _ViewModel.AppendLine("{");
            _ViewModel.AppendLine($"public {txtPageName.Text}Model()");
            _ViewModel.AppendLine("{");
            foreach (var item in itemlist.Where(o => o.ColumnType == "D"))
            {
                _ViewModel.AppendLine(item.ColumnName.Replace("Id", "") + "List = new ist<SelectListItem>();");
            }
            _ViewModel.AppendLine("}");
            foreach (var item in empList.Where(o => !ExtraColumn.Contains(o) && !o.EndsWith("Model")))
            {
                SqlDataAdapter myAdapter2 = new SqlDataAdapter($"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{txtPageName.Text.ToLower()}' AND COLUMN_NAME = '{item}' ORDER BY ORDINAL_POSITION", myConnection);
                DataSet ColumnDataType = new DataSet("ColumnDataType");
                myAdapter2.Fill(ColumnDataType, "ColumnDataType");

                SqlDataAdapter myAdapter3 = new SqlDataAdapter($"SELECT IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{txtPageName.Text.ToLower()}' AND COLUMN_NAME = '{item}' ORDER BY ORDINAL_POSITION", myConnection);
                DataSet ColumnDataTypeType = new DataSet("ColumnDataType");
                myAdapter3.Fill(ColumnDataTypeType, "ColumnDataTypeType");

                _ViewModel.AppendLine($"[ABSResourceDisplayName(\"{item}\")]");
                if (ColumnDataType.Tables[0].AsEnumerable().FirstOrDefault().ItemArray[0].ToString().ToLower() == "yes")
                {
                    _ViewModel.AppendLine("public " + GetClrType(ColumnDataType.Tables[0].AsEnumerable().FirstOrDefault().ItemArray[0].ToString()) + "? " + item + " { get; set; }");
                }
                else
                {
                    _ViewModel.AppendLine("public " + GetClrType(ColumnDataType.Tables[0].AsEnumerable().FirstOrDefault().ItemArray[0].ToString()) + " " + item + " { get; set; }");
                }
            }

            foreach (var item in empList.Where(o => !ExtraColumn.Contains(o) && o.EndsWith("Model")))
            {
                _ViewModel.AppendLine("public " + item + " " + item.Replace("Model", "") + "{ get; set; }");
            }
            foreach (var item in itemlist.Where(o => o.ColumnType == "D"))
            {
                _ViewModel.AppendLine("public List<SelectListItem> " + item.ColumnName.Replace("Id", "") + "List { get; set; }");
                _ViewModel.AppendLine("public string" + item.ColumnName.Replace("Id", "") + "Name { get; set; }");
            }
            _ViewModel.AppendLine("}");
            _ViewModel.AppendLine("}");

            //Save
            string ModelPath = txtfilepath.Text + "\\" + txtPageName.Text + "\\" + txtPageName.Text + "Model.cs";
            FileSave(ModelPath, _ViewModel);
            //Save
        }

        #endregion Model

        #region FillData
        public void FillData()
        {
            SqlConnection myConnection = new SqlConnection();
            myConnection.ConnectionString = ConnectionString;
            string sqlQry = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{txtPageName.Text.ToLower()}' ORDER BY ORDINAL_POSITION";
            SqlDataAdapter myAdapter = new SqlDataAdapter(sqlQry, myConnection);
            DataSet ColumnDataTypeType = new DataSet("ColumnDataType");
            myAdapter.Fill(ColumnDataTypeType, "ColumnDataTypeType");
            List<TableList> tablelist = new List<TableList>();
            List<string> ExtraColumn = new List<string>();
            ExtraColumn.Add("Id");
            ExtraColumn.Add("CompanyId");
            ExtraColumn.Add("Status");
            ExtraColumn.Add("CreatedBy");
            ExtraColumn.Add("CreatedDate");
            ExtraColumn.Add("UpdatedBy");
            ExtraColumn.Add("UpdatedDate");
            var empList = ColumnDataTypeType.Tables[0].AsEnumerable().Select(o => o.ItemArray[0].ToString()).ToList();
            foreach (var item in empList.Where(o => !ExtraColumn.Contains(o)))
            {
                tablelist.Add(new TableList()
                {
                    ColumnName = item
                });
            }
            dataGridView1.DataSource = tablelist;
            //dataGridView1.Columns[3].Width = 150;
        }
        #endregion GetList

        #region Get List
        private void button1_Click(object sender, EventArgs e)
        {
            FillData();

        }
        #endregion

        #region Get Service
        private void button2_Click_1(object sender, EventArgs e)
        {
            CreateService();
            MessageBox.Show("Service Created Sucessfully");
        }
        #endregion Get Service

        #region Get List Page
        private void button3_Click(object sender, EventArgs e)
        {
            CreateService();
            CreateModel();
            CreateListController();
            ListViewPage();
            MessageBox.Show("List Page Created Sucessfully");
        }
        #endregion Get List Page

        #region Generate ALl
        private void button4_Click(object sender, EventArgs e)
        {
            CreateModel();
            CreateService();
            CreateFullController();
            ListViewPage();
            CreateFullViewPage();
            MessageBox.Show("Full Page Created Sucessfully");
        }
        #endregion Generate ALl

        #region Controller
        public void CreateEditableListController()
        {
            _EditableController.AppendLine("using ABS.Core;");
            var itemlist = (List<TableList>)dataGridView1.DataSource;
            _EditableController.AppendLine($"using ABS.Core.Domain.{txttablefoldername.Text};");
            _EditableController.AppendLine($"using ABS.Services.{txtPageName.Text};");
            foreach (var item in itemlist.Where(o => o.ColumnType == "D"))
            {
                _EditableController.AppendLine($"using ABS.Core.Domain.{item.ColumnName.Replace("Id", "")};");
                _EditableController.AppendLine($"using ABS.Services.{item.ColumnName.Replace("Id", "")};");
            }
            _EditableController.AppendLine("using ABS.Web.Framework.Controllers;");
            _EditableController.AppendLine("using ABS.Web.Models;");
            _EditableController.AppendLine("using System;");
            _EditableController.AppendLine("using System.Collections.Generic;");
            _EditableController.AppendLine("using System.Linq;");
            _EditableController.AppendLine("using System.Web;");
            _EditableController.AppendLine("using System.Web.Mvc;");
            _EditableController.AppendLine("namespace ABS.Web.Controllers");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine($"public class {txtPageName.Text}Controller : BaseAdminController");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine($"private readonly I{txtPageName.Text}Service _{txtPageName.Text}Service;");
            foreach (var item in itemlist)
            {
                if (item.ColumnType == "D")
                {
                    _EditableController.AppendLine($"private readonly I{item.ColumnName.Replace("Id", "")}Service _{item.ColumnName.Replace("Id", "")}Service;");
                }
            }
            string services = "";
            foreach (var item in itemlist)
            {
                if (item.ColumnType == "D")
                {
                    services = $",I{item.ColumnName.Replace("Id", "")}Service {item.ColumnName.Replace("Id", "")}Service";
                }
            }
            _EditableController.AppendLine($"public {txtPageName.Text}Controller(I{txtPageName.Text}Service {txtPageName.Text}Service" + services + ")");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine($"this._{txtPageName.Text}Service = {txtPageName.Text}Service;");
            foreach (var item in itemlist.Where(o => o.ColumnType == "D"))
            {
                _EditableController.AppendLine($"this._{item.ColumnName.Replace("Id", "")}Service = {item.ColumnName.Replace("Id", "")}Service;");
            }
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("public virtual ActionResult List()");
            _EditableController.AppendLine("{");

            foreach (var item in itemlist.Where(o => o.ColumnType == "D"))
            {
                _EditableController.AppendLine($"{txtPageName.Text}Model _{txtPageName.Text.ToLower()} = new {txtPageName.Text}Model();");
                _EditableController.AppendLine($"_{txtPageName.Text.ToLower()}." + item.ColumnName.Replace("Id", "") + $"List = _{item.ColumnName.Replace("Id", "")}..GetAllEntities().ToABSSelectListItems(o=>o.Name,o=>o.Id.ToString());");
            }
            if (itemlist.Any(o => o.ColumnType == "D"))
            {
                _EditableController.AppendLine($"return View(_{txtPageName.Text.ToLower()});");
            }
            else
            {
                _EditableController.AppendLine("return View();");
            }
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("public virtual ActionResult GetList(DataSourceRequest request)");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine($"return ABSJson(_{txtPageName.Text}Service.GetAllEntities().Select(o => o.ToModel()).ToDataSourceResult(request));");
            _EditableController.AppendLine("}");

            _EditableController.AppendLine("[HttpPost]");
            _EditableController.AppendLine($"public virtual ActionResult CreateOrUpdate([DataSourceRequest] DataSourceRequest request, {txtPageName.Text}Model model)");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine("if (model != null && ModelState.IsValid)");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine("try");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine("if (model.Id > 0)");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine($"var tablemodel = _{txtPageName.Text}Service.GetEntityById(model.Id);");
            _EditableController.AppendLine("tablemodel = model.ToEntity(tablemodel);");
            _EditableController.AppendLine($"_{txtPageName.Text}Service.UpdateEntity(tablemodel);");
            _EditableController.AppendLine("return ABSJson(new { Type = \"S\", Message = \"" + txtPageName.Text + " updated sucessfully\" });");
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("else");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine($"_{txtPageName.Text}Service.AddEntity(model.ToEntity());");
            _EditableController.AppendLine("return ABSJson(new { Type = \"S\", Message = \"" + txtPageName.Text + " added sucessfully\" });");
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("catch (Exception e)");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine("return ABSJson(new { Type = \"E\", Message = e.Message });");
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("return ABSJson(new { Type = \"E\", Message = ValidationErrorMessage() });");
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("public virtual ActionResult Delete(long Id = 0)");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine("try");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine($"var tablemodel = _{txtPageName.Text}Service.GetEntityById(Id);");
            _EditableController.AppendLine($"_{txtPageName.Text}Service.DeleteEntity(tablemodel);");
            _EditableController.AppendLine("return ABSJson(new { Type = \"S\", Message = \"" + txtPageName.Text + " deleted sucessfully\" });");
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("catch (Exception e)");
            _EditableController.AppendLine("{");
            _EditableController.AppendLine("return ABSJson(new { Type = \"E\", Message = e.Message });");
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("}");
            _EditableController.AppendLine("}");

            //Save
            string ModelPath = txtfilepath.Text + "\\" + txtPageName.Text + "\\" + txtPageName.Text + "Controller.cs";
            FileSave(ModelPath, _EditableController);
            //Save
        }
        public void CreateListController()
        {
            _Controller.AppendLine("using ABS.Core;");
            var itemlist = (List<TableList>)dataGridView1.DataSource;
            _Controller.AppendLine($"using ABS.Core.Domain.{txttablefoldername.Text};");
            _Controller.AppendLine($"using ABS.Services.{txtPageName.Text};");
            foreach (var item in itemlist)
            {
                if (item.ColumnType == "D")
                {
                    _Controller.AppendLine($"using ABS.Core.Domain.{item.ColumnName.Replace("Id", "")};");
                    _Controller.AppendLine($"using ABS.Services.{item.ColumnName.Replace("Id", "")};");
                }
            }
            _Controller.AppendLine("using ABS.Web.Framework.Controllers;");
            _Controller.AppendLine("using ABS.Web.Models;");
            _Controller.AppendLine("using System;");
            _Controller.AppendLine("using System.Collections.Generic;");
            _Controller.AppendLine("using System.Linq;");
            _Controller.AppendLine("using System.Web;");
            _Controller.AppendLine("using System.Web.Mvc;");
            _Controller.AppendLine("namespace ABS.Web.Controllers");
            _Controller.AppendLine("{");
            _Controller.AppendLine($"public class {txtPageName.Text}Controller : BaseAdminController");
            _Controller.AppendLine("{");
            _Controller.AppendLine($"private readonly I{txtPageName.Text}Service _{txtPageName.Text}Service;");
            foreach (var item in itemlist)
            {
                if (item.ColumnType == "D")
                {
                    _Controller.AppendLine($"private readonly I{item.ColumnName.Replace("Id", "")}Service _{item.ColumnName.Replace("Id", "")}Service;");
                }
            }
            string services = "";
            foreach (var item in itemlist)
            {
                if (item.ColumnType == "D")
                {
                    services = $",I{item.ColumnName.Replace("Id", "")}Service {item.ColumnName.Replace("Id", "")}Service";
                }
            }
            _Controller.AppendLine($"public {txtPageName.Text}Controller(I{txtPageName.Text}Service {txtPageName.Text}Service" + services + ")");
            _Controller.AppendLine("{");
            _Controller.AppendLine($"this._{txtPageName.Text}Service = {txtPageName.Text}Service;");
            foreach (var item in itemlist)
            {
                if (item.ColumnType == "D")
                {
                    _Controller.AppendLine($"this._{item.ColumnName.Replace("Id", "")}Service = {item.ColumnName.Replace("Id", "")}Service;");
                }
            }
            _Controller.AppendLine("}");
            _Controller.AppendLine("public virtual ActionResult List()");
            _Controller.AppendLine("{");
            _Controller.AppendLine("return View();");
            _Controller.AppendLine("}");
            _Controller.AppendLine("public virtual ActionResult GetList(DataSourceRequest request)");
            _Controller.AppendLine("{");
            _Controller.AppendLine($"return ABSJson(_{txtPageName.Text}Service.GetAllEntities().Select(o => o.ToModel()).ToDataSourceResult(request));");
            _Controller.AppendLine("}");
            _Controller.AppendLine("}");
            _Controller.AppendLine("}");

            //Save
            string ModelPath = txtfilepath.Text + "\\" + txtPageName.Text + "\\" + txtPageName.Text + "Controller.cs";
            FileSave(ModelPath, _Controller);
            //Save
        }
        public void CreateFullController()
        {
            _FullController.AppendLine("using ABS.Core;");
            var itemlist = (List<TableList>)dataGridView1.DataSource;
            _FullController.AppendLine($"using ABS.Core.Domain.{txttablefoldername.Text};");
            _FullController.AppendLine($"using ABS.Services.{txtPageName.Text};");
            foreach (var item in itemlist)
            {
                if (item.ColumnType == "D")
                {
                    _FullController.AppendLine($"using ABS.Core.Domain.{item.ColumnName.Replace("Id", "")};");
                    _FullController.AppendLine($"using ABS.Services.{item.ColumnName.Replace("Id", "")};");
                }
            }
            _FullController.AppendLine("using ABS.Web.Framework.Controllers;");
            _FullController.AppendLine("using ABS.Web.Models;");
            _FullController.AppendLine("using System;");
            _FullController.AppendLine("using System.Collections.Generic;");
            _FullController.AppendLine("using ABS.Services.Security;");
            _FullController.AppendLine("using System.Linq;");
            _FullController.AppendLine("using System.Web;");
            _FullController.AppendLine("using System.Web.Mvc;");
            _FullController.AppendLine("namespace ABS.Web.Controllers");
            _FullController.AppendLine("{");
            _FullController.AppendLine($"public class {txtPageName.Text}Controller : BaseAdminController");
            _FullController.AppendLine("{");
            _FullController.AppendLine($"private readonly I{txtPageName.Text}Service _{txtPageName.Text}Service;");
            foreach (var item in itemlist)
            {
                if (item.ColumnType == "D")
                {
                    _FullController.AppendLine($"private readonly I{item.ColumnName.Replace("Id", "")}Service _{item.ColumnName.Replace("Id", "")}Service;");
                }
            }
            string services = "";
            foreach (var item in itemlist)
            {
                if (item.ColumnType == "D")
                {
                    services = $",I{item.ColumnName.Replace("Id", "")}Service {item.ColumnName.Replace("Id", "")}Service";
                }
            }
            _FullController.AppendLine($"public {txtPageName.Text}Controller(I{txtPageName.Text}Service {txtPageName.Text}Service" + services + ")");
            _FullController.AppendLine("{");
            _FullController.AppendLine($"this._{txtPageName.Text}Service = {txtPageName.Text}Service;");
            foreach (var item in itemlist)
            {
                if (item.ColumnType == "D")
                {
                    _FullController.AppendLine($"this._{item.ColumnName.Replace("Id", "")}Service = {item.ColumnName.Replace("Id", "")}Service;");
                }
            }
            _FullController.AppendLine("{");
            _FullController.AppendLine("public virtual ActionResult List()");
            _FullController.AppendLine("{");
            _FullController.AppendLine("return View();");
            _FullController.AppendLine("}");
            _FullController.AppendLine("public virtual ActionResult GetList(DataSourceRequest request)");
            _FullController.AppendLine("{");
            _FullController.AppendLine($"return ABSJson(_{txtPageName.Text}Service.GetAllEntities().Select(o => o.ToModel()).ToDataSourceResult(request));");
            _FullController.AppendLine("}");

            _FullController.AppendLine("[ParameterConverterAttribute(\"EncryptId\")]");
            _FullController.AppendLine("public virtual ActionResult CreateOrUpdate(long EncryptId = 0)");
            _FullController.AppendLine("{");
            _FullController.AppendLine($"var model = new {txtPageName.Text}Model();");
            _FullController.AppendLine("if (EncryptId > 0)");
            _FullController.AppendLine("{");
            _FullController.AppendLine($"model = _{txtPageName.Text}Service.GetEntityById(EncryptId).ToModel();");
            _FullController.AppendLine("}");
            foreach (var item in itemlist.Where(o => o.ColumnType == "D"))
            {
                _FullController.AppendLine($"model." + item.ColumnName.Replace("Id", "") + $"List = _{item.ColumnName.Replace("Id", "")}..GetAllEntities().ToABSSelectListItems(o=>o.Name,o=>o.Id.ToString()); ");
            }
            _FullController.AppendLine("return View(model);");
            _FullController.AppendLine("}");

            _FullController.AppendLine("[HttpPost]");
            _FullController.AppendLine("[ValidateAntiForgeryToken]");
            _FullController.AppendLine($"public virtual ActionResult CreateOrUpdate({txtPageName.Text}Model model)");
            _FullController.AppendLine("{");
            _FullController.AppendLine("if (model != null && ModelState.IsValid)");
            _FullController.AppendLine("{");
            _FullController.AppendLine("try");
            _FullController.AppendLine("{");
            _FullController.AppendLine("if (model.Id > 0)");
            _FullController.AppendLine("{");
            _FullController.AppendLine($"var tablemodel = _{txtPageName.Text}Service.GetEntityById(model.Id);");
            _FullController.AppendLine("tablemodel = model.ToEntity(tablemodel);");
            _FullController.AppendLine($"_{txtPageName.Text}Service.UpdateEntity(tablemodel);");
            _FullController.AppendLine("SuccessNotification(\"" + txtPageName.Text + " updated sucessfully\");");
            _FullController.AppendLine("}");
            _FullController.AppendLine("else");
            _FullController.AppendLine("{");
            _FullController.AppendLine($"_{txtPageName.Text}Service.AddEntity(model.ToEntity());");
            _FullController.AppendLine("SuccessNotification(\"" + txtPageName.Text + " Added sucessfully\");");
            _FullController.AppendLine("}");
            _FullController.AppendLine("return RedirectToAction(\"CreateOrUpdate\", new { EncryptId = CSecurityModel.SecurityModelEncryption(model.Id.ToString()) });");
            _FullController.AppendLine("}");
            _FullController.AppendLine("catch (Exception e)");
            _FullController.AppendLine("{");
            _FullController.AppendLine("ErrorNotification(e.Message);");
            _FullController.AppendLine("}");
            _FullController.AppendLine("}");
            _FullController.AppendLine("return View(model);");
            _FullController.AppendLine("}");
            _FullController.AppendLine("[ParameterConverterAttribute(\"EncryptId\")]");
            _FullController.AppendLine("public virtual ActionResult Delete(long EncryptId = 0)");
            _FullController.AppendLine("{");
            _FullController.AppendLine("try");
            _FullController.AppendLine("{");
            _FullController.AppendLine($"var tablemodel = _{txtPageName.Text}Service.GetEntityById(EncryptId);");
            _FullController.AppendLine($"_{txtPageName.Text}Service.DeleteEntity(tablemodel);");
            _FullController.AppendLine("return ABSJson(new { Type = \"S\", Message = \"" + txtPageName.Text + " deleted sucessfully\" });");
            _FullController.AppendLine("}");
            _FullController.AppendLine("catch (Exception e)");
            _FullController.AppendLine("{");
            _FullController.AppendLine("return ABSJson(new { Type = \"E\", Message = e.Message });");
            _FullController.AppendLine("}");
            _FullController.AppendLine("}");


            _FullController.AppendLine("}");
            _FullController.AppendLine("}");

            //Save
            string ModelPath = txtfilepath.Text + "\\" + txtPageName.Text + "\\" + txtPageName.Text + "Controller.cs";
            FileSave(ModelPath, _FullController);
            //Save
        }
        #endregion Controller

        #region ListView
        public void ListViewPage()
        {
            _ListPage.AppendLine("@Html.AntiForgeryToken()");
            _ListPage.AppendLine("<div class=\"container - fluid container - fullw bg - white\">");
            _ListPage.AppendLine("<fieldset>");
            _ListPage.AppendLine("<legend>List</legend>");
            _ListPage.AppendLine($"@(Html.Kendo().Grid<{txtPageName.Text}Model>()");
            _ListPage.AppendLine($" .Name(\"{txtPageName.Text}Grid\")");
            _ListPage.AppendLine(".Columns(columns =>{");
            var itemlist = (List<TableList>)dataGridView1.DataSource;
            foreach (var item in itemlist)
            {
                if (item.IsGrid)
                {
                    _ListPage.AppendLine($"columns.Bound(c => c.{item.ColumnName}).Title(\"{(item.GridTitle == null ? item.ColumnName : item.GridTitle)}\");");
                }
            }
            _ListPage.AppendLine("columns.Template(c => { }).Width(80).ClientTemplate(\"<div align='center'><a class='k-button k-button-icontext k-grid-edit'  onclick='ABSEdit(\\\"/" + txtPageName.Text + "/CreateOrUpdate?EncryptId=#= EncrytedId #\\\")'><i class='k-icon k-edit'></i>Edit</a></div>\").Title(\"Edit\");");
            _ListPage.AppendLine("columns.Template(c => { }).Width(100).ClientTemplate(\"<div align='center'><a class='k-button k-button-icontext k-grid-delete' onclick='ABSDelete(\\\"/" + txtPageName.Text + "/Delete?EncryptId=#= EncrytedId #\\\", \\\"" + txtPageName.Text + "Grid\\\")'><i class='k-icon k-delete'></i>Delete</a></div>\").Title(\"Delete\");");
            _ListPage.AppendLine("})");
            _ListPage.AppendLine(".Scrollable()");
            _ListPage.AppendLine(".Filterable()");
            _ListPage.AppendLine(".HtmlAttributes(new { style = \"height: 500px\" })");
            _ListPage.AppendLine(".Sortable()");
            _ListPage.AppendLine(".Pageable(pageable => pageable");
            _ListPage.AppendLine(".Refresh(true)");
            _ListPage.AppendLine(".PageSizes(true)");
            _ListPage.AppendLine(".ButtonCount(5))");
            _ListPage.AppendLine(".DataSource(dataSource => dataSource");
            _ListPage.AppendLine(".Ajax()");
            _ListPage.AppendLine(".ServerOperation(false)");
            _ListPage.AppendLine(".Model(model =>");
            _ListPage.AppendLine("{");
            _ListPage.AppendLine("model.Id(p => p.Id);");
            _ListPage.AppendLine("})");
            _ListPage.AppendLine(".Read(read => { read.Action(\"GetList\", \"" + txtPageName.Text + "\"); })");
            _ListPage.AppendLine(".PageSize(10)");
            _ListPage.AppendLine(")");
            _ListPage.AppendLine(")");
            _ListPage.AppendLine("</fieldset>");
            _ListPage.AppendLine("</div>");
            //Save
            string ModelPath = txtfilepath.Text + "\\" + txtPageName.Text + "\\ List.cshtml";
            FileSave(ModelPath, _ListPage);
            //Save
        }

        public void CreateFullViewPage()
        {
            _FullViewPage.AppendLine($"@model {txtPageName.Text}Model");
            _FullViewPage.AppendLine($"@using(Html.BeginForm())");
            _FullViewPage.AppendLine("{");
            _FullViewPage.AppendLine("@Html.HiddenFor(o => o.Id)");
            _FullViewPage.AppendLine("<section id = \"page-title\" >");
            _FullViewPage.AppendLine($"<h1 class=\"mainTitle\">Manage {txtPageName.Text}</h1>");
            _FullViewPage.AppendLine("<br />");
            _FullViewPage.AppendLine("<br />");
            _FullViewPage.AppendLine("<div class=\"pull-left\">");
            _FullViewPage.AppendLine($"<h1><i class=\"fa fa-arrow-circle-left\"></i>@Html.ActionLink(\"{txtPageName.Text}List\", \"List\")</h1>");
            _FullViewPage.AppendLine("</div>");
            _FullViewPage.AppendLine("<div class=\"pull-right\">");
            _FullViewPage.AppendLine("@if (Model.Id > 0)");
            _FullViewPage.AppendLine("{");
            _FullViewPage.AppendLine("<a href=\"@Url.Action(\"CreateOrUpdate\")\" class=\"btn btn-primary btn-wide btn-scroll btn-scroll-top fa fa-plus\">");
            _FullViewPage.AppendLine("<span>Add</span>");
            _FullViewPage.AppendLine("</a>");
            _FullViewPage.AppendLine("}");
            _FullViewPage.AppendLine("<button type=\"submit\" class=\"btn btn-success btn-wide btn-scroll btn-scroll-top fa fa-floppy-o\">");
            _FullViewPage.AppendLine("@if (Model.Id > 0)");
            _FullViewPage.AppendLine("{");
            _FullViewPage.AppendLine("<span>Update</span>");
            _FullViewPage.AppendLine("}");
            _FullViewPage.AppendLine("else");
            _FullViewPage.AppendLine("{");
            _FullViewPage.AppendLine("<span>Save</span>");
            _FullViewPage.AppendLine("}");
            _FullViewPage.AppendLine("</button>");
            _FullViewPage.AppendLine("</div>");
            _FullViewPage.AppendLine("</section>");
            _FullViewPage.AppendLine("<div class=\"container-fluid container-fullw bg-white\">");
            _FullViewPage.AppendLine("<fieldset>");
            _FullViewPage.AppendLine($"<legend>{txtPageName.Text}</legend>");
            _FullViewPage.AppendLine("@Html.AntiForgeryToken()");

            int i = 1;
            var itemlist = (List<TableList>)dataGridView1.DataSource;
            foreach (var item in itemlist.Where(x => x.IsForm))
            {

                if (i == 1)
                {
                    i = 2;
                    _FullViewPage.AppendLine($"<div class=\"row\">");
                    _FullViewPage.AppendLine("<div class=\"col-md-6\">");
                    _FullViewPage.AppendLine("<div class=\"form-group\">");
                    _FullViewPage.AppendLine($"@Html.ABSLabelFor(model => model.{item.ColumnName})");
                    Generatcontrols(item, _FullViewPage);
                    _FullViewPage.AppendLine("</div>");
                    _FullViewPage.AppendLine("</div>");
                }

                else if(i == 2)
                {
                    i = 1;
                    _FullViewPage.AppendLine("<div class=\"col-md-6\">");
                    _FullViewPage.AppendLine("<div class=\"form-group\">");
                    _FullViewPage.AppendLine($"@Html.ABSLabelFor(model => model.{item.ColumnName})");
                    Generatcontrols(item, _FullViewPage);
                    _FullViewPage.AppendLine("</div>");
                    _FullViewPage.AppendLine("</div>");
                    _FullViewPage.AppendLine("</div>");
                }
                else
                {
                    _FullViewPage.AppendLine("</div>");
                }

               

            }


            _FullViewPage.AppendLine("</fieldset>");
            _FullViewPage.AppendLine("</div>");
            _FullViewPage.AppendLine("}");


            

            //Save
            string ModelPath = txtfilepath.Text + "\\" + txtPageName.Text + "\\ CreateOrUpdate.cshtml";
            FileSave(ModelPath, _FullViewPage);
            //Save
        }

        public void EditableListView()
        {
            _EditableList.AppendLine($"@model {txtPageName.Text}Model");
            _EditableList.AppendLine("@Html.AntiForgeryToken()");
            _EditableList.AppendLine("<div class=\"container - fluid container - fullw bg - white\">");
            _EditableList.AppendLine("<fieldset>");
            _EditableList.AppendLine("<legend>List</legend>");
            _EditableList.AppendLine($"@(Html.Kendo().Grid<{txtPageName.Text}Model>()");
            _EditableList.AppendLine($" .Name(\"{txtPageName.Text}Grid\")");
            _EditableList.AppendLine(".Columns(columns =>{");
            var itemlist = (List<TableList>)dataGridView1.DataSource;
            foreach (var item in itemlist)
            {
                if (item.IsGrid)
                {
                    //DateType
                    if (item.ColumnType == "DT")
                    {
                        _EditableList.AppendLine("columns.Bound(p => p." + item.ColumnName + ").Format(\\" + (item.ColumnFormat == null ? "{0:dd-MMM-yyyy}" : item.ColumnFormat) + "\\).Title(\\" + (item.GridTitle == null ? item.ColumnName : item.GridTitle) + "\\).EditorTemplateName(\"DateFilter\").Filterable(f => f.UI(\"DateFilter\"));");
                    }
                    //DropDown
                    else if (item.ColumnType == "D")
                    {
                        _EditableList.AppendLine($"columns.ForeignKey(c => c.{item.ColumnName}, Model.{item.ColumnName.Replace("Id", "")}List, \"Value\", \"Text\").Title(\\" + (item.GridTitle == null ? item.ColumnName : item.GridTitle) + "\\).Width(200).EditorTemplateName(\"ForeignKeyDorpDown\").Filterable(f => f.Multi(true));");
                    }
                    //Time Control
                    else if (item.ColumnType == "TM")
                    {
                        _EditableList.AppendLine("columns.Bound(p => p." + item.ColumnName + ").Format(\\" + (item.ColumnFormat == null ? "{0:HH:mm}" : item.ColumnFormat) + "\\).Title(\\" + (item.GridTitle == null ? item.ColumnName : item.GridTitle) + "\\).EditorTemplateName(\"TimePicker\").Filterable(f => f.UI(\"TimeFilter24\"));");
                    }
                    //CheckBox
                    else if (item.ColumnType == "C")
                    {
                        _EditableList.AppendLine("");
                    }
                    //Numeric Textbox
                    else if (item.ColumnType == "N")
                    {
                        _EditableList.AppendLine("columns.Bound(c => c." + item.ColumnName + ").Title(\\" + (item.GridTitle == null ? item.ColumnName : item.GridTitle) + "\\).EditorTemplateName(\"NumberTextboxWithoutDecimal\").Format(\\" + (item.ColumnFormat == null ? "{0:0.#}" : item.ColumnFormat) + "\\);");
                    }
                    else
                    {
                        _EditableList.AppendLine($"columns.Bound(c => c.{item.ColumnName}).Title(\"{(item.GridTitle == null ? item.ColumnName : item.GridTitle)}\");");
                    }

                }
            }
            _EditableList.AppendLine("columns.Command(command => { command.Edit(); }).Width(100).Title(\"Edit\");");
            _EditableList.AppendLine("columns.Template(c => { }).Width(100).ClientTemplate(\"<div align='center'><a class='k-button k-button-icontext k-grid-delete' onclick='ABSDelete(\\\"/" + txtPageName.Text + "/Delete?EncryptId=#= EncrytedId #\\\", \\\"" + txtPageName.Text + "Grid\\\")'><i class='k-icon k-delete'></i>Delete</a></div>\").Title(\"Delete\");");
            _EditableList.AppendLine("})");
            _EditableList.AppendLine(".ToolBar(toolbar => toolbar.Create())");
            _EditableList.AppendLine(".Editable(editable => editable.Mode(GridEditMode.InLine).DisplayDeleteConfirmation(true))");
            _EditableList.AppendLine(".Scrollable()");
            _EditableList.AppendLine(".Filterable()");
            _EditableList.AppendLine(".HtmlAttributes(new { style = \"height: 500px\" })");
            _EditableList.AppendLine(".Sortable()");
            _EditableList.AppendLine(".Pageable(pageable => pageable");
            _EditableList.AppendLine(".Refresh(true)");
            _EditableList.AppendLine(".PageSizes(true)");
            _EditableList.AppendLine(".ButtonCount(5))");
            _EditableList.AppendLine(".DataSource(dataSource => dataSource");
            _EditableList.AppendLine(".Ajax()");
            _EditableList.AppendLine(".ServerOperation(false)");
            _EditableList.AppendLine(".Model(model =>");
            _EditableList.AppendLine("{");
            _EditableList.AppendLine("model.Id(p => p.Id);");
            _EditableList.AppendLine("})");
            _EditableList.AppendLine(".Events(events => events.Error(\"error_handler\").Sync(\"sync_handler\").RequestEnd(\"request_handler\"))");
            _EditableList.AppendLine(".Read(read => { read.Action(\"GetList\", \"" + txtPageName.Text + "\"); })");
            _EditableList.AppendLine(".Create(Create => Create.Action(\"CreateOrUpdate\", \"" + txtPageName.Text + "\"))");
            _EditableList.AppendLine(".Update(update => update.Action(\"CreateOrUpdate\", \"" + txtPageName.Text + "\"))");
            _EditableList.AppendLine(".Destroy(Destroy => Destroy.Action(\"Delete\", \"" + txtPageName.Text + "\"))");
            _EditableList.AppendLine(".PageSize(10)");
            _EditableList.AppendLine(")");
            _EditableList.AppendLine(")");
            _EditableList.AppendLine("</fieldset>");
            _EditableList.AppendLine("</div>");
            //Save
            string ModelPath = txtfilepath.Text + "\\" + txtPageName.Text + "\\ List.cshtml";
            FileSave(ModelPath, _EditableList);
        }
        #endregion ListView

        #region EditableListView
        private void button5_Click(object sender, EventArgs e)
        {
            CreateModel();
            CreateService();
            CreateEditableListController();
            EditableListView();
            MessageBox.Show("Edit List Page Created Sucessfully");
        }
        #endregion EditableListView

        #region CommonLogic 

        public void FileSave(string Path, StringBuilder _Service)
        {
            bool exists = System.IO.Directory.Exists(txtfilepath.Text + "\\" + txtPageName.Text);
            if (!exists)
            {
                System.IO.Directory.CreateDirectory(txtfilepath.Text + "\\" + txtPageName.Text);
            }

            if (!File.Exists(Path))
            {
                using (StreamWriter sw = File.CreateText(Path))
                {
                    sw.WriteLine(_Service.ToString());
                }
            }
        }

        public static string GetClrType(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "bigint":
                    return "long";

                case "binary":
                case "image":
                case "timestamp":
                case "varbinary":
                    return "byte[]";

                case "bit":
                    return "bool";

                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return "string";

                case "datetime":
                case "smalldatetime":
                case "date":
                case "time":
                case "datetime2":
                    return "DateTime";

                case "decimal":
                case "money":
                case "smallmoney":
                    return "decimal";

                case "float":
                    return "double";

                case "int":
                    return "int";

                case "uniqueidentifier":
                    return "Guid";

                case "smallint":
                    return "short";

                case "tinyint":
                    return "byte";
                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }
        }

        public void Generatcontrols(TableList item,StringBuilder _FullViewPage)
        {
            if (item.ColumnType == "T")
            {
                _FullViewPage.AppendLine("@Html.Kendo().TextBoxFor(m => m." + item.ColumnName + ").HtmlAttributes(new { placeholder = \"Enter " + (item.GridTitle == null ? item.ColumnName : item.GridTitle) + "\",  style = \"width:100%;\" })");
            }
            else if (item.ColumnType == "D")
            {
                _FullViewPage.AppendLine("@(Html.Kendo().DropDownListFor(model => model." + item.ColumnName + ")");
                _FullViewPage.AppendLine(".HtmlAttributes(new { style = \"width: 100%;\" })");
                _FullViewPage.AppendLine(".DataTextField(\"Text\")");
                _FullViewPage.AppendLine(".DataValueField(\"Value\")");
                _FullViewPage.AppendLine(".Filter(\"contains\")");
                _FullViewPage.AppendLine(".BindTo(Model." + item.ColumnName.Replace("Id", "") + "List)");
                _FullViewPage.AppendLine(".DataSource(source =>");
                _FullViewPage.AppendLine("{");
                _FullViewPage.AppendLine("source.ServerFiltering(false);");
                _FullViewPage.AppendLine("})");
                _FullViewPage.AppendLine(")");
            }
            else if (item.ColumnType == "C")
            {
                _FullViewPage.AppendLine("@Html.Kendo().CheckBoxFor(m => m." + item.ColumnName + ").HtmlAttributes(new { style = \"width: 100%;\" })");
            }
        }
        public class TableList
        {
            public string ColumnName { get; set; }
            public string ColumnType { get; set; }
            public bool IsGrid { get; set; }
            public string GridTitle { get; set; }
            public string ColumnFormat { get; set; }
            public bool IsForm { get; set; }
        }

        #endregion


    }
}
