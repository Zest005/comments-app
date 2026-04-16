using System;
using System.Collections.Generic;
using System.Text;

namespace CommentsApp.Application.Common.Interfaces
{
    public interface ICaptchaService
    {
        (string CaptchaId, byte[] CaptchaImage) GenerateCaptcha();

        bool ValidateCaptcha(string captchaId, string userInput);
    }
}
