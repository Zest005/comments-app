import { Injectable } from '@angular/core';

export type Lang = 'en' | 'uk';

@Injectable({
  providedIn: 'root'
})
export class I18nService {
  private currentLang: Lang = 'en';

  private translations: Record<Lang, Record<string, string>> = {
    en: {
      'title': 'Comments',
      'leaveComment': 'Leave a Comment',
      'reply': 'Reply',
      'cancel': 'Cancel',
      'send': 'Send',
      'sending': 'Sending...',
      'preview': 'Preview',
      'hidePreview': 'Hide Preview',
      'userName': 'User Name',
      'email': 'E-mail',
      'homePage': 'Home page',
      'text': 'Text',
      'captcha': 'CAPTCHA',
      'attachFile': 'Attach file (JPG, PNG, GIF, TXT)',
      'chooseFile': 'Choose file',
      'noFile': 'No file selected',
      'totalComments': 'Total comments',
      'sortByDate': 'Sort by date',
      'newestFirst': 'Newest first ↓',
      'oldestFirst': 'Oldest first ↑',
      'loading': 'Loading...',
      'noComments': 'No comments yet. Be the first!',
      'previous': '← Previous',
      'next': 'Next →',
      'required': 'is required.',
      'userNamePlaceholder': 'Latin letters and digits only',
      'emailPlaceholder': 'user@example.com',
      'homePagePlaceholder': 'https://example.com',
      'textPlaceholder': 'Allowed HTML tags: <a>, <code>, <i>, <strong>',
      'captchaPlaceholder': 'Enter text from image',
      'newCaptcha': 'New CAPTCHA',
      'captchaRequiredFile': 'CAPTCHA is required when uploading a file.',
      'userNameRequired': 'User Name is required.',
      'userNameInvalid': 'Only Latin letters and digits allowed.',
      'emailRequired': 'E-mail is required.',
      'emailInvalid': 'Invalid email format.',
      'homePageInvalid': 'Must be a valid URL (starting with http:// or https://).',
      'textRequired': 'Comment text is required.',
      'generalError': 'An error occurred. Please try again.',
      'login': 'Login',
      'logout': 'Logout',
      'loginTitle': 'Enter your details',
      'welcomeBack': 'Welcome',
      'changeUser': 'Change user',
      'fileTypeError': 'Only JPG, PNG, GIF images and TXT files are allowed.',
      'fileSizeError': 'Text file must not exceed 100 KB.',
      'tagItalic': 'italic',
      'tagBold': 'bold',
      'tagCode': 'code',
      'tagLink': 'link',
      'enterUrl': 'Enter URL:',
      'linkText': 'link text',
      'invalidCaptcha': 'Invalid CAPTCHA.',
      'invalidHtmlTags': 'HTML tags are not properly closed. Check your markup.'
    },
    uk: {
      'title': 'Коментарі',
      'leaveComment': 'Залишити коментар',
      'reply': 'Відповісти',
      'cancel': 'Скасувати',
      'send': 'Надіслати',
      'sending': 'Надсилається...',
      'preview': 'Попередній перегляд',
      'hidePreview': 'Сховати перегляд',
      'userName': "Ім'я користувача",
      'email': 'E-mail',
      'homePage': 'Домашня сторінка',
      'text': 'Текст',
      'captcha': 'CAPTCHA',
      'attachFile': 'Прикріпити файл (JPG, PNG, GIF, TXT)',
      'chooseFile': 'Вибрати файл',
      'noFile': 'Файл не вибрано',
      'totalComments': 'Всього коментарів',
      'sortByDate': 'Сортувати за датою',
      'newestFirst': 'Спочатку нові ↓',
      'oldestFirst': 'Спочатку старі ↑',
      'loading': 'Завантаження...',
      'noComments': 'Коментарів ще немає. Будьте першим!',
      'previous': '← Попередня',
      'next': 'Наступна →',
      'required': "є обов'язковим.",
      'userNamePlaceholder': 'Тільки латинські літери та цифри',
      'emailPlaceholder': 'user@example.com',
      'homePagePlaceholder': 'https://example.com',
      'textPlaceholder': 'Дозволені HTML теги: <a>, <code>, <i>, <strong>',
      'captchaPlaceholder': 'Введіть текст з картинки',
      'newCaptcha': 'Нова CAPTCHA',
      'captchaRequiredFile': 'CAPTCHA потрібна при завантаженні файлу.',
      'userNameRequired': "Ім'я користувача є обов'язковим.",
      'userNameInvalid': 'Тільки латинські літери та цифри.',
      'emailRequired': "E-mail є обов'язковим.",
      'emailInvalid': 'Невірний формат e-mail.',
      'homePageInvalid': 'URL повинен починатися з http:// або https://.',
      'textRequired': "Текст коментаря є обов'язковим.",
      'generalError': 'Сталася помилка. Спробуйте ще раз.',
      'login': 'Увійти',
      'logout': 'Вийти',
      'loginTitle': 'Введіть ваші дані',
      'welcomeBack': 'Ласкаво просимо',
      'changeUser': 'Змінити користувача',
      'fileTypeError': 'Дозволені лише JPG, PNG, GIF зображення та TXT файли.',
      'fileSizeError': 'Текстовий файл не повинен перевищувати 100 КБ.',
      'tagItalic': 'курсив',
      'tagBold': 'жирний',
      'tagCode': 'код',
      'tagLink': 'посилання',
      'enterUrl': 'Введіть URL:',
      'linkText': 'текст посилання',
      'invalidCaptcha': 'Невірна CAPTCHA.',
      'invalidHtmlTags': 'HTML теги не закриті коректно. Перевірте розмітку.'
    }
  };

  constructor() {
    const saved = localStorage.getItem('lang') as Lang;
    if (saved && this.translations[saved]) {
      this.currentLang = saved;
    }
  }

  get lang(): Lang {
    return this.currentLang;
  }

  setLang(lang: Lang): void {
    this.currentLang = lang;
    localStorage.setItem('lang', lang);
  }

  t(key: string): string {
    return this.translations[this.currentLang][key] || key;
  }

  formatDate(dateStr: string): string {
    let utcDateStr = dateStr;
    if (!utcDateStr.endsWith('Z')) {
      utcDateStr += 'Z';
    }
    const date = new Date(utcDateStr);

    const locale = this.currentLang === 'uk' ? 'uk-UA' : 'en-US';

    return date.toLocaleString(locale, {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}