  namespace web_project_BE.Helper
{
    public class EmailBody
    {
        public static string EmailStringBody(string email, string emailToken)
        {
            return $@"<html>
              <head></head>
              <body style=""margin:0; padding: 0;font-family: Arial, Helvetica, sans-serif;"">
                <div style=""height:auto;background: linear-gradient(to top, #e3f099 50%, #d9eb1a 90%) no-repeat;width: 400px;padding: 30px;"">
                  <div>
                    <div>
                      <h1>Reset Your Password</h1>
                      <hr>
                      <p>You're receiving this e-mail because you requested a password reset for your HD SHOE - STORE WEB account.</p>
                      <p>Please tap the button below to choose a new password.</p>
                      <a href=""http://localhost:4200/reset?email={email}&code={emailToken}"" target="""" _blank"" style=""background:black;padding:10px;border:none;
                        color:white;border-radius:4px;display:block;margin:0 auto;width:50%;text-align:center;text-decoration:none"">Reset Password</a><br>
            
                        <p>Kind Regards,<br><br>
                        HD SHOE - STORE WEB</P>
                    </div>
                  </div>
                </div>

              </body>
            </html>";
        }


        public static string ProductNotifyEmail(string performer, string message)
        {
            return $@"<html>
            <head></head>
            <body style=""margin:0; padding: 0;font-family: Arial, Helvetica, sans-serif;"">
                <div
                    style=""height:auto;background: linear-gradient(to top, #e3f099 50%, #d9eb1a 90%) no-repeat;width: 400px;padding: 30px;"">
                    <div>
                        <div>
                            <h1>The Product Notify Email</h1>
                            <hr>
                            <p>{message} {performer}</p>
                           
                            <p>Kind Regards,<br><br>
                                HD SHOE - STORE WEB</P>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string CategoryNotifyEmail(string performer, string message)
        {
            return $@"<html>
            <head></head>
            <body style=""margin:0; padding: 0;font-family: Arial, Helvetica, sans-serif;"">
                <div
                    style=""height:auto;background: linear-gradient(to top, #e3f099 50%, #d9eb1a 90%) no-repeat;width: 400px;padding: 30px;"">
                    <div>
                        <div>
                            <h1>The Category Notify Email</h1>
                            <hr>
                            <p>{message} {performer}</p>
                           
                            <p>Kind Regards,<br><br>
                                HD SHOE - STORE WEB</P>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string OrderNotifyEmail(string performer, string message)
        {
            return $@"<html>
            <head></head>
            <body style=""margin:0; padding: 0;font-family: Arial, Helvetica, sans-serif;"">
                <div
                    style=""height:auto;background: linear-gradient(to top, #e3f099 50%, #d9eb1a 90%) no-repeat;width: 400px;padding: 30px;"">
                    <div>
                        <div>
                            <h1>The Order Notify Email</h1>
                            <hr>
                            <p>{message} {performer}</p>
                           
                            <p>Kind Regards,<br><br>
                                HD SHOE - STORE WEB</P>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}
