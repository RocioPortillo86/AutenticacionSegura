<%@ Page Title="Productos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Products.aspx.cs" Inherits="WebApplication1.Pages.Products" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-wrap">
        <div class="page-header">
            <h1>Productos</h1>
            <div class="actions">
                <asp:Button ID="btnNew"    runat="server" Text="Nuevo"    CssClass="btn btn-primary" OnClick="btnNew_Click" />
                <asp:Button ID="btnSave"   runat="server" Text="Guardar"  CssClass="btn btn-success" OnClick="btnSave_Click" />
                <asp:Button ID="btnDelete" runat="server" Text="Eliminar" CssClass="btn btn-danger"  OnClick="btnDelete_Click" />
            </div>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert" EnableViewState="false" />

        
        <div class="card">
            <asp:GridView ID="gvProducts" runat="server"
                CssClass="table"
                HeaderStyle-CssClass="table-head"
                RowStyle-CssClass="table-row"
                AlternatingRowStyle-CssClass="table-row alt"
                PagerStyle-CssClass="table-pager"
                SelectedRowStyle-CssClass="row-selected"
                GridLines="None" BorderStyle="None" />
        </div>

    
        <div class="card mt-20">
            <h2 class="card-title">Detalle / Edición</h2>
            <asp:FormView ID="fvProduct" runat="server" CssClass="form">
              
                <ItemTemplate>
                    <div class="form-grid">
                        <div class="form-field">
                            <label>ID</label>
                            <span><%# Eval("Id") %></span>
                        </div>
                        <div class="form-field">
                            <label>Nombre</label>
                            <span><%# Eval("Name") %></span>
                        </div>
                        <div class="form-field">
                            <label>Precio</label>
                            <span><%# String.Format("{0:C}", Eval("Price")) %></span>
                        </div>
                        <div class="form-field">
                            <label>Stock</label>
                            <span><%# Eval("Stock") %></span>
                        </div>
                        <div class="form-field">
                            <label>Activo</label>
                            <span><%# (bool)Eval("Active") ? "Sí" : "No" %></span>
                        </div>
                        
                    </div>
                </ItemTemplate>

               
                <EditItemTemplate>
                    <div class="form-grid">
                        <div class="form-field">
                            <label>Nombre</label>
                            <asp:TextBox ID="txtName" runat="server" Text='<%# Bind("Name") %>' CssClass="input" />
                        </div>
                        <div class="form-field">
                            <label>Precio</label>
                            <asp:TextBox ID="txtPrice" runat="server" Text='<%# Bind("Price", "{0:F2}") %>' CssClass="input" />
                        </div>
                        <div class="form-field">
                            <label>Stock</label>
                            <asp:TextBox ID="txtStock" runat="server" Text='<%# Bind("Stock") %>' CssClass="input" />
                        </div>
                        <div class="form-field">
                            <label>Activo</label>
                            <asp:CheckBox ID="chkActive" runat="server" Checked='<%# Bind("Active") %>' CssClass="checkbox" />
                        </div>
                    </div>
                </EditItemTemplate>
            </asp:FormView>
        </div>
    </div>
</asp:Content>
