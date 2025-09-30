<%@ Page Title="Reporte de Ventas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SalesReport.aspx.cs" Inherits="WebApplication1.Pages.SalesReport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-wrap">
        <div class="page-header">
            <h1>Reporte de Ventas</h1>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert" EnableViewState="false" />

        
        <div class="card">
            <h2 class="card-title">Filtro por fechas</h2>
            <div class="form-grid">
                <div class="form-field">
                    <label>Desde</label>
                    <asp:TextBox ID="txtFrom" runat="server" CssClass="input" TextMode="Date" />
                </div>
                <div class="form-field">
                    <label>Hasta</label>
                    <asp:TextBox ID="txtTo" runat="server" CssClass="input" TextMode="Date" />
                </div>
                <div class="form-field" style="align-self:end">
                    <asp:Button ID="btnFilter" runat="server" Text="Filtrar" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
                </div>
            </div>
        </div>

        
        <div class="card mt-20">
            <h2 class="card-title">Resultados</h2>
            <asp:GridView ID="gvSales" runat="server"
                CssClass="table"
                HeaderStyle-CssClass="table-head"
                RowStyle-CssClass="table-row"
                AlternatingRowStyle-CssClass="table-row alt"
                PagerStyle-CssClass="table-pager"
                SelectedRowStyle-CssClass="row-selected"
                GridLines="None" BorderStyle="None" />
        </div>

        
        <div class="card mt-20">
            <div class="totals">
                <div class="total">
                    <div class="label">Total General</div>
                    <div class="number">
                        <asp:Label ID="lblTotalGeneral" runat="server" Text="$0.00" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
