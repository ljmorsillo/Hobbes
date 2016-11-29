<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TestLogin.aspx.cs" Inherits="TestLogin" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
        <p>
            <asp:Label ID="Label1" runat="server" Text="Username:   "></asp:Label>
            <asp:TextBox ID="UserTB" runat="server"></asp:TextBox>
        </p>
        <p>
            <asp:Label ID="Label2" runat="server" Text="Password:   "></asp:Label>
            <asp:TextBox ID="PsswrdTB" runat="server"></asp:TextBox>
        </p>
        <asp:Button ID="Button1" runat="server" Text="Login" />
    </form>
</body>
</html>
