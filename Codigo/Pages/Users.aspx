<%@ Page Title="Usuarios" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="WebApplication1.Pages.Users" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-wrap">
        <div class="page-header">
            <h1>Usuarios</h1>
            <div class="actions">
                <asp:Button ID="btnNew"    runat="server" Text="Nuevo"   CssClass="btn btn-primary" OnClick="btnNew_Click" />
                <asp:Button ID="btnSave"   runat="server" Text="Guardar" CssClass="btn btn-success" OnClick="btnSave_Click1" />

            </div>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert" EnableViewState="false" />

        <div class="card">

        </div>
       <asp:GridView ID="gvUsers" runat="server"
    CssClass="table"
    HeaderStyle-CssClass="table-head"
    RowStyle-CssClass="table-row"
    AlternatingRowStyle-CssClass="table-row alt"
    PagerStyle-CssClass="table-pager"
    SelectedRowStyle-CssClass="row-selected"
    GridLines="None" BorderStyle="None"

    AutoGenerateColumns="False"
    DataKeyNames="Id"

    AllowPaging="true" PageSize="10"
    OnPageIndexChanging="gvUsers_PageIndexChanging"
    OnSelectedIndexChanged="gvUsers_SelectedIndexChanged"
    OnRowDeleting="gvUsers_RowDeleting">

    <Columns>
       
        <asp:CommandField ShowSelectButton="true" SelectText="Editar" />

        
        <asp:BoundField DataField="Email" HeaderText="Email" />
        <asp:BoundField DataField="Role" HeaderText="Rol" />
        <asp:CheckBoxField DataField="Active" HeaderText="Activo" />


        <asp:CommandField ShowDeleteButton="true" DeleteText="Eliminar" />
    </Columns>
</asp:GridView>


   
        <div class="card mt-20">
            <h2 class="card-title">Detalle / Edición</h2>
         <asp:FormView ID="fvUser" runat="server" CssClass="form" Visible="false">
    <InsertItemTemplate>
        <asp:HiddenField ID="hfUserId" runat="server" />
        <div class="form-grid">
            <div class="form-field">
                <label>Email</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="input" />
            </div>
            <div class="form-field">
                <label>Contraseña</label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="input" TextMode="Password" />
            </div>
            <div class="form-field">
                <label>Rol</label>
                <asp:DropDownList ID="ddlRole" runat="server" CssClass="input">
                    <asp:ListItem Value="Admin">Admin</asp:ListItem>
                    <asp:ListItem Value="Cashier">Cashier</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="form-field">
                <label>Activo</label><br />
                <asp:CheckBox ID="chkActive" runat="server" Checked="true" CssClass="checkbox" />
            </div>
        </div>
    </InsertItemTemplate>

    <EditItemTemplate>
        <asp:HiddenField ID="hfUserId" runat="server" Value='<%# Bind("Id") %>' />
        <div class="form-grid">
            <div class="form-field">
                <label>Email</label>
                <asp:TextBox ID="txtEmail" runat="server" Text='<%# Bind("Email") %>' CssClass="input" />
            </div>
            <div class="form-field">
                <label>Rol</label>
                <asp:DropDownList ID="ddlRole" runat="server" CssClass="input" SelectedValue='<%# Bind("Role") %>'>
                    <asp:ListItem Value="Admin">Admin</asp:ListItem>
                    <asp:ListItem Value="Cashier">Cashier</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="form-field">
                <label>Activo</label><br />
                <asp:CheckBox ID="chkActive" runat="server" Checked='<%# Bind("Active") %>' CssClass="checkbox" />
            </div>
        </div>
    </EditItemTemplate>
</asp:FormView>

        </div>
    </div>
</asp:Content>
