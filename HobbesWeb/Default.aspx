<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Test Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Panel ID="Panel3" runat="server" Height="38px">
            <asp:Label ID="Label1" runat="server" Text="Test Stuff" Font-Size="Larger"></asp:Label>
        </asp:Panel>
        <asp:Panel ID="Panel2" runat="server" Height="103px">
            <asp:LinkButton ID="LinkButton1" runat="server" PostBackUrl="~/TestLogin.aspx">Go To Login Page</asp:LinkButton>
            <br />
            <asp:Button ID="MakeButton" runat="server" Text="Make Cookie" OnClick="SetCookiesInResponse_Click" />
            <asp:Button ID="Button1" runat="server" OnClick="ShowCookiesInRequest_Click" Text="Show Cookies" />
        </asp:Panel>
    
    </div>
        <asp:Panel ID="Panel1" runat="server" Height="343px" style="margin-top: 96px" EnableTheming="False">
            <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="Button" />
            <asp:TextBox ID="TextOut" runat="server" Height="208px" OnTextChanged="TextBox1_TextChanged" TextMode="MultiLine" Width="731px">This is a test</asp:TextBox>
        </asp:Panel>
    </form>
</body>
</html>
