namespace VT3CyberZone.Desktop;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var login = new Forms.LoginForm();
        if (login.ShowDialog() != DialogResult.OK || Api.ApiClient.Instance.CurrentUser is not { } user)
        {
            return;
        }

        if (user.Role is not ("ADMIN" or "STAFF"))
        {
            Api.ApiClient.Instance.Logout();
            MessageBox.Show(
                "Tài khoản hội viên không thể đăng nhập màn hình quản lý.",
                "Không đủ quyền",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        Application.Run(new Forms.MainForm(user.Username, user.Role));
    }
}
