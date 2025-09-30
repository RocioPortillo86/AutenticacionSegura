<%@ Page Title="Inicio" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication1.Pages.Menu" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-wrap">
     
        <div class="page-header">
            <div>
                <h1>Bienvenido, <asp:Label ID="lblWelcome" runat="server" /></h1>
                <span class="role-badge">
                    <asp:Label ID="lblRole" runat="server" />
                </span>
            </div>
        </div>

    
        <div class="card mt-20">
            <p>Usa el menú superior para navegar por el sistema.</p>
        </div>

        <div class="card mt-20">
            <h2 class="card-title">Resumen rápido</h2>
            <div class="stats-grid">
                <div class="stat">
                    <div class="stat-label">Productos</div>
                    <div class="stat-number"><asp:Label ID="lblCountProducts" runat="server" Text="—" /></div>
                </div>
                <div class="stat">
                    <div class="stat-label">Ventas hoy</div>
                    <div class="stat-number"><asp:Label ID="lblSalesToday" runat="server" Text="—" /></div>
                </div>
                <div class="stat">
                    <div class="stat-label">Total hoy</div>
                    <div class="stat-number"><asp:Label ID="lblTotalToday" runat="server" Text="$0.00" /></div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

