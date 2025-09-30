using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication1.Class.Data;
using WebApplication1.Class.Models;

namespace WebApplication1.Pages
{
    public partial class Users : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                fvUser.Visible = false;                 // oculto al entrar
                fvUser.ChangeMode(FormViewMode.Insert); // modo por defecto
                LoadUsers();
            }
        }

        private void LoadUsers()
        {
            gvUsers.DataSource = UserData.GetAll(); // GetAll es static
            gvUsers.DataBind();
        }

        // =========================
        // Acciones de la grilla
        // =========================

        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsers.PageIndex = e.NewPageIndex;
            LoadUsers();
        }

        protected void gvUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*   var idObj = gvUsers.SelectedDataKey?.Value;
               if (idObj == null) return;

               var repo = new UserData();
               var user = repo.GetById((int)idObj);
               if (user == null) return;

               // Mostrar FormView en modo edición con el usuario seleccionado
               fvUser.Visible = true;
               fvUser.ChangeMode(FormViewMode.Edit);
               fvUser.DataSource = new[] { user };
               fvUser.DataBind();

               lblMessage.Text = string.Empty;*/
            int id = Convert.ToInt32(gvUsers.SelectedDataKey.Value);

            UserData userData = new UserData();
            var user = userData.GetById(id);

            fvUser.Visible = true;
            fvUser.ChangeMode(FormViewMode.Edit);
            fvUser.DataSource = new List<User> { user }; // <- Enlazamos SOLO el seleccionado
            fvUser.DataBind();
        }

        protected void gvUsers_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = (int)gvUsers.DataKeys[e.RowIndex].Value;

            var repo = new UserData();
            repo.Delete(id);

            e.Cancel = true;      // no usar DataSource automático
            gvUsers.EditIndex = -1;

            LoadUsers();
            lblMessage.Text = "Eliminado correctamente";
            lblMessage.Style["display"] = "block";
        }

        // =========================
        // Nuevo / Guardar
        // =========================

        protected void btnNew_Click(object sender, EventArgs e)
        {
            fvUser.Visible = true;
            fvUser.ChangeMode(FormViewMode.Insert);
            fvUser.DataSource = null;       // <- SIN DATOS
            fvUser.DataBind();

            /* gvUsers.SelectedIndex = -1; // deseleccionar cualquier fila

             fvUser.Visible = true;
             fvUser.ChangeMode(FormViewMode.Insert);

             // Renderizar Insert "vacío" y cortar cualquier binding previo
             fvUser.DataSource = new object[] { new object() };
             fvUser.DataBind();

             // Limpiar / defaults explícitos
             var txtEmail = (TextBox)fvUser.FindControl("txtEmail");
             var txtPassword = (TextBox)fvUser.FindControl("txtPassword");
             var ddlRole = (DropDownList)fvUser.FindControl("ddlRole");
             var chkActive = (CheckBox)fvUser.FindControl("chkActive");

             if (txtEmail != null) txtEmail.Text = string.Empty;
             if (txtPassword != null) txtPassword.Text = string.Empty;
             if (ddlRole != null) ddlRole.SelectedIndex = 0;
             if (chkActive != null) chkActive.Checked = true;

             lblMessage.Text = string.Empty;*/
        }

        protected void btnSave_Click1(object sender, EventArgs e)
        {
            try
            {
                UserData userData = new UserData();

                if (fvUser.CurrentMode == FormViewMode.Insert)
                {
                    var newUser = new User
                    {
                        Email = ((TextBox)fvUser.FindControl("txtEmail")).Text.Trim(),
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                            ((TextBox)fvUser.FindControl("txtPassword")).Text.Trim()),
                        Role = ((DropDownList)fvUser.FindControl("ddlRole")).SelectedValue,
                        Active = ((CheckBox)fvUser.FindControl("chkActive")).Checked
                    };

                    userData.Insert(newUser);
                    lblMessage.Text = "Usuario creado correctamente.";
                }
                else if (fvUser.CurrentMode == FormViewMode.Edit)
                {
                    var updatedUser = new User
                    {
                        Id = Convert.ToInt32(((HiddenField)fvUser.FindControl("hfUserId")).Value),
                        Email = ((TextBox)fvUser.FindControl("txtEmail")).Text.Trim(),
                        Role = ((DropDownList)fvUser.FindControl("ddlRole")).SelectedValue,
                        Active = ((CheckBox)fvUser.FindControl("chkActive")).Checked
                    };

                    userData.Update(updatedUser);
                    lblMessage.Text = "Usuario actualizado correctamente.";
                }

                lblMessage.CssClass = "alert alert-success";

                // Refrescar lista y ocultar el detalle
                LoadUsers();
                fvUser.Visible = false;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error al guardar: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger";
            }

            // Respetar validación de la página para el grupo
            /*if (!Page.IsValid)
            {
                fvUser.Visible = true; // mantener visible si falló validación
                return;
            }

            try
            {
                T C<T>(string id) where T : Control => fvUser.FindControl(id) as T;

                var txtEmail = C<TextBox>("txtEmail");
                var txtPassword = C<TextBox>("txtPassword");   // existe solo en Insert
                var ddlRole = C<DropDownList>("ddlRole");
                var chkActive = C<CheckBox>("chkActive");
                var hfUserId = C<HiddenField>("hfUserId");

                if (txtEmail == null || ddlRole == null || chkActive == null)
                    throw new InvalidOperationException("No se encontraron controles del formulario.");

                var repo = new UserData();

                if (fvUser.CurrentMode == FormViewMode.Insert)
                {
                    if (txtPassword == null || string.IsNullOrWhiteSpace(txtPassword.Text))
                        throw new InvalidOperationException("La contraseña es obligatoria.");

                    var u = new User
                    {
                        Email = txtEmail.Text.Trim(),
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text),
                        Role = ddlRole.SelectedValue,
                        Active = chkActive.Checked
                    };

                    repo.Insert(u);
                    lblMessage.Text = "Usuario creado.";
                }
                else if (fvUser.CurrentMode == FormViewMode.Edit)
                {
                    if (!int.TryParse(hfUserId?.Value, out var id))
                        throw new InvalidOperationException("Id inválido.");

                    var u = repo.GetById(id);
                    if (u == null) throw new InvalidOperationException("Usuario no encontrado.");

                    u.Email = txtEmail.Text.Trim();
                    u.Role = ddlRole.SelectedValue;
                    u.Active = chkActive.Checked;

                    // Cambiar contraseña SOLO si escribieron una nueva (si existe el control)
                    if (txtPassword != null && !string.IsNullOrWhiteSpace(txtPassword.Text))
                        u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text);
                    else
                        u.PasswordHash = null; // tu Update lo ignora si es null

                    repo.Update(u);
                    lblMessage.Text = "Usuario actualizado.";
                }

                lblMessage.Style["display"] = "block";

                // Ocultar y resetear el FormView tras guardar
                fvUser.Visible = false;
                fvUser.ChangeMode(FormViewMode.Insert);
                fvUser.DataSource = null;
                fvUser.DataBind();

                LoadUsers();
            }
            catch (Exception ex)
            {
                fvUser.Visible = true; // mantener visible para corregir
                lblMessage.Text = "Error al guardar: " + ex.Message;
                lblMessage.Style["display"] = "block";
            }*/
        }
    }
}
