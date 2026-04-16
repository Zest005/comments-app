using System;
using System.Collections.Generic;
using System.Text;

namespace CommentsApp.Application.Common.Interfaces
{
    public interface ICaptchaService
    {
        (string CaptchaId, byte[] ImageBytes) GenerateCaptcha();

        bool ValidateCaptcha(string captchaId, string userInput);
    }
}
