<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WebApplication1.Pages.Login" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Iniciar sesión</title>
    <link href="~/Styles/Site.css" rel="stylesheet" type="text/css" runat="server" />
</head>
<body>
<form id="form1" runat="server">
  <div class="auth-wrap">
    <div class="auth-card card">
      <h1 class="card-title" style="text-align:center">Iniciar sesión</h1>

      <!-- Control nativo de ASP.NET Login -->
      <asp:Login ID="LoginForm" runat="server"
                 OnAuthenticate="LoginForm_Authenticate"
                 FailureText="Credenciales inválidas."
                 DisplayRememberMe="false"
                 TitleText="">
        <LayoutTemplate>
          <div class="form">
            <div class="form-grid">
              <!-- Usuario (Email) -->
              <div class="form-field" style="grid-column:1 / -1">
                <label for="UserName">Email</label>
                <asp:TextBox ID="UserName" runat="server" CssClass="input" TextMode="Email" />
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                    ControlToValidate="UserName" CssClass="field-error" Display="Dynamic"
                    ErrorMessage="El email es obligatorio." />
                <asp:RegularExpressionValidator ID="revEmail" runat="server"
                    ControlToValidate="UserName" CssClass="field-error" Display="Dynamic"
                    ValidationExpression="^\S+@\S+\.\S+$"
                    ErrorMessage="Ingresa un email válido." />
              </div>

              <!-- Contraseña -->
              <div class="form-field" style="grid-column:1 / -1">
                <label for="Password">Contraseña</label>
                <asp:TextBox ID="Password" runat="server" CssClass="input" TextMode="Password" />
                <asp:RequiredFieldValidator ID="rfvPassword" runat="server"
                    ControlToValidate="Password" CssClass="field-error" Display="Dynamic"
                    ErrorMessage="La contraseña es obligatoria." />
              </div>
            </div>

            <div class="auth-actions">
              <asp:Button ID="LoginButton" runat="server" CommandName="Login"
                          CssClass="btn btn-primary" Text="Iniciar sesión" />
            </div>

            <asp:ValidationSummary ID="ValidationSummary1" runat="server"
               CssClass="alert" DisplayMode="BulletList" ShowSummary="true" />
            <asp:Literal ID="FailureText" runat="server" EnableViewState="False" />
          </div>
        </LayoutTemplate>
      </asp:Login>
    </div>
  </div>
</form>
</body>
</html>
