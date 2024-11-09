using KeyAuth;
using Perf_sharp.Properties;
using System.Management;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Perf_sharp
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }
        private readonly Shaker shaker = new();
        private readonly ImageHandler imageHandler = new();
        #region API
        private readonly api KeyAuthApp = new(
        name: "ToXFree",
        ownerid: "RogzjVRny8",
        version: "1.0"
        );
        #endregion
        private void Form1_Load(object sender, EventArgs e)
        {
            Invalidate();
            guna2ShadowForm1.SetShadowForm(this);
            signin.Cursor = Cursors.Hand;
            signup.Cursor = Cursors.Hand;
            //
            KeyAuthApp.init();
            if (KeyAuthApp.response.success)
                members.Text = string.Concat("+", KeyAuthApp.app_data.numUsers, " MEMBER");
            //
            imageHandler.SetImageFromResource(Resources.start_up__1_);
            miniTweakIco.Image = imageHandler.Image;
            imageHandler.SetImageFromResource(Resources.sort_down_24px);
            triangle.Image = imageHandler.Image;
            imageHandler.SetImageFromResource(Resources.closed_eye_16px);
            visiblePass.Image = imageHandler.Image;
            imageHandler.SetImageFromResource(Resources.start_up);
            tweakIco.Image = imageHandler.Image;
        }
        #region Drag
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        private void guna2GradientPanel2_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = Location;
        }
        private void guna2GradientPanel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Opacity = 0.98;
                Cursor = Cursors.Hand;
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }
        private void guna2GradientPanel2_MouseUp(object sender, MouseEventArgs e)
        {
            Opacity = 1;
            Cursor = Cursors.Default;
            dragging = false;
        }
        #endregion
        #region Animation
        private int moveDistance = 6;
        private int moveSpeed = 50;
        private readonly bool stopMoving = false;
        private bool isDown = false;
        private async Task MovetriangleDownAsync()
        {
            while (!stopMoving)
            {
                if (!isDown)
                {
                    for (int i = 0; i < moveDistance; i++)
                    {
                        tweakIco.Location = new Point(tweakIco.Location.X, tweakIco.Location.Y + 1);
                        triangle.Location = new Point(triangle.Location.X, triangle.Location.Y + 1);
                        await Task.Delay(moveSpeed);
                    }
                    isDown = true;
                }
                else
                {
                    for (int i = 0; i < moveDistance; i++)
                    {
                        tweakIco.Location = new Point(tweakIco.Location.X, tweakIco.Location.Y - 1);
                        triangle.Location = new Point(triangle.Location.X, triangle.Location.Y - 1);
                        await Task.Delay(moveSpeed);
                    }
                    isDown = false;
                }
            }
        }
        private async void Form1_Shown(object sender, EventArgs e)
        {
            Invalidate();
            await shaker.Shake(this);
            await MovetriangleDownAsync();
        }
        private bool isVisible = false;
        #endregion
        #region Password visibility
        private void visiblePass_Click(object sender, EventArgs e)
        {
            if (!isVisible)
            {
                isVisible = true;
                imageHandler.SetImageFromResource(Resources.eye_16px);
                visiblePass.Image = imageHandler.Image;
                passBox.PasswordChar = '\0';
                passBox.UseSystemPasswordChar = false;
            }
            else
            {
                isVisible = false;
                imageHandler.SetImageFromResource(Resources.closed_eye_16px);
                visiblePass.Image = imageHandler.Image;
                passBox.PasswordChar = '●';
                passBox.UseSystemPasswordChar = true;
            }
        }
        #endregion
        private void moveButton(Control control, int x, int y)
        {
            loginButton.Location = new Point(x, y);
        }
        private void signin_Click(object sender, EventArgs e)
        {
            moveButton(loginButton, 85, 299);
            guna2Separator2.Location = new Point(85, 124);
            signin.ForeColor = Color.FromArgb(176, 157, 255);
            signup.ForeColor = Color.White;
            guna2HtmlLabel5.Text = "LOGIN TO TOX UTILITY TO CONTINUE.";
            loginButton.Text = "Login";
        }
        private void signup_Click(object sender, EventArgs e)
        {
            moveButton(loginButton, 85, 343);
            guna2Separator2.Location = new Point(231, 124);
            signup.ForeColor = Color.FromArgb(176, 157, 255);
            signin.ForeColor = Color.White;
            guna2HtmlLabel5.Text = "SIGN UP IN TOX UTILITY AND BE A MEMBER!";
            loginButton.Text = "Sign up";
        }
        private void loginButton_Click(object sender, EventArgs e)
        {
            switch (loginButton.Text)
            {
                case "Login":
                    KeyAuthApp.login(userBox.Text, passBox.Text);
                    if (KeyAuthApp.response.success)
                    {
                        // Here do what you wish!
                        MessageBox.Show(KeyAuthApp.response.message);
                    }
                    else
                    {
                        MessageBox.Show(KeyAuthApp.response.message);
                    }
                    break;
                case "Sign up":
                    KeyAuthApp.register(userBox.Text, passBox.Text, passBox.Text);
                    if (KeyAuthApp.response.success)
                    {
                        // Here do what you wish!
                        MessageBox.Show(KeyAuthApp.response.message);
                    }
                    else
                    {
                        MessageBox.Show(KeyAuthApp.response.message);
                    }
                    break;
            }
        }

        private void guna2HtmlLabel4_Click(object sender, EventArgs e)
        {

        }
    }
}
