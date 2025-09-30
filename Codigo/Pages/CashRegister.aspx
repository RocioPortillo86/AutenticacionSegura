<%@ Page Title="Caja" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CashRegister.aspx.cs" Inherits="WebApplication1.Pages.CashRegister" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-wrap">
        <div class="page-header">
            <h1>Caja</h1>
            <div class="actions">
                <asp:Button ID="btnCheckout" runat="server" Text="Registrar Venta"
                    CssClass="btn btn-success" OnClick="btnCheckout_Click" />
            </div>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert" EnableViewState="false" />

   
        <div class="card">
            <h2 class="card-title">Agregar producto</h2>
            <div class="form-grid">
                <div class="form-field">
                    <label>Producto</label>
                    <asp:DropDownList ID="ddlProducts" runat="server" CssClass="input"></asp:DropDownList>
                </div>
                <div class="form-field">
                    <label>Cantidad</label>
                    <asp:TextBox ID="txtQty" runat="server" CssClass="input" TextMode="Number" min="1" />
                </div>
                <div class="form-field" style="align-self:end">
                    <asp:Button ID="btnAddItem" runat="server" Text="Agregar" CssClass="btn btn-primary" OnClick="btnAddItem_Click" />
                </div>
            </div>
        </div>

   
        <div class="card mt-20">
            <h2 class="card-title">Carrito</h2>
            <asp:GridView ID="gvCart" runat="server"
                CssClass="table"
                HeaderStyle-CssClass="table-head"
                RowStyle-CssClass="table-row"
                AlternatingRowStyle-CssClass="table-row alt"
                PagerStyle-CssClass="table-pager"
                SelectedRowStyle-CssClass="row-selected"
                GridLines="None" BorderStyle="None" />
        </div>

        
        <div class="card mt-20">
            <h2 class="card-title">Totales</h2>
            <div class="totals">
                <div class="total">
                    <div class="label">Subtotal</div>
                    <div class="number"><asp:Label ID="lblSubtotal" runat="server" Text="$0.00" /></div>
                </div>
                <div class="total">
                    <div class="label">Impuesto</div>
                    <div class="number"><asp:Label ID="lblTax" runat="server" Text="$0.00" /></div>
                </div>
                <div class="total">
                    <div class="label">Total</div>
                    <div class="number"><asp:Label ID="lblTotal" runat="server" Text="$0.00" /></div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
