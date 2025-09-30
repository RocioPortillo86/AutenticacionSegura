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

                <asp:Label ID="lblMessage" runat="server" CssClass="alert" EnableViewState="false" />

                <div class="form">
                    <div class="form-grid">
                        <div class="form-field" style="grid-column:1 / -1">
                            <label>Email</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="input" TextMode="Email" />
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                                ControlToValidate="txtEmail" Display="Dynamic"
                                CssClass="field-error" ErrorMessage="El email es obligatorio." />
                            <asp:RegularExpressionValidator ID="revEmail" runat="server"
                                ControlToValidate="txtEmail" Display="Dynamic"
                                CssClass="field-error" ValidationExpression="^\S+@\S+\.\S+$"
                                ErrorMessage="Ingresa un email válido." />
                        </div>

                        <div class="form-field" style="grid-column:1 / -1">
                            <label>Contraseña</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="input" TextMode="Password" />
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server"
                                ControlToValidate="txtPassword" Display="Dynamic"
                                CssClass="field-error" ErrorMessage="La contraseña es obligatoria." />
                        </div>
                    </div>

                    <div class="auth-actions">
                        <asp:Button ID="btnLogin" runat="server" Text="Iniciar sesión"
                            CssClass="btn btn-primary" OnClick="btnLogin_Click" />
                    </div>

                    <asp:ValidationSummary ID="ValidationSummary1" runat="server"
                        CssClass="alert" DisplayMode="BulletList" ShowSummary="true" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>
