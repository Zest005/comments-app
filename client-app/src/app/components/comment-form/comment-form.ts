import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CommentService } from '../../services/comment.service';
import { I18nService } from '../../services/i18n.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-comment-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './comment-form.html',
  styleUrls: ['./comment-form.css']
})
export class CommentFormComponent implements OnInit {
  @Input() parentCommentId: number | null = null;

  @Output() cancelReply = new EventEmitter<void>();

  @Output() commentCreated = new EventEmitter<void>();

  userName = '';
  email = '';
  homePage = '';
  text = '';
  captchaText = '';
  captchaId = '';
  captchaImage = '';
  selectedFile: File | null = null;

  errors: { [key: string]: string[] } = {};
  isSubmitting = false;
  previewHtml = '';
  showPreview = false;

  constructor(private commentService: CommentService, public i18n: I18nService, public auth: AuthService) {}

  ngOnInit(): void {
    const user = this.auth.getUser();
    if (user) {
      this.userName = user.userName;
      this.email = user.email;
      this.homePage = user.homePage || '';
    }
  }

  loadCaptcha(): void {
    this.commentService.getCaptcha().subscribe({
      next: (response) => {
        this.captchaId = response.captchaId;
        this.captchaImage = response.imageBase64;
        this.captchaText = '';
      },
      error: (err) => console.error('Failed to load CAPTCHA:', err)
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'text/plain'];
      if (!allowedTypes.includes(file.type)) {
        alert('Only JPG, PNG, GIF images and TXT files are allowed.');
        input.value = '';
        return;
      }

      if (file.type === 'text/plain' && file.size > 100 * 1024) {
        alert(this.i18n.t('fileSizeError'));
        input.value = '';
        return;
      }

      this.selectedFile = file;
      this.loadCaptcha();
    } else {
      this.selectedFile = null;
      this.captchaText = '';
      this.captchaId = '';
      this.captchaImage = '';
    }
  }

  removeFile(fileInput: HTMLInputElement): void {
    this.selectedFile = null;
    this.captchaText = '';
    this.captchaId = '';
    this.captchaImage = '';
    fileInput.value = '';
  }

  insertTag(tag: string): void {
    const textarea = document.querySelector(
      this.parentCommentId ? `#reply-text-${this.parentCommentId}` : '#comment-text'
    ) as HTMLTextAreaElement;

    if (!textarea) return;

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selectedText = this.text.substring(start, end);

    let insertion: string;

    if (tag === 'a') {
      const url = prompt(this.i18n.t('enterUrl'), 'https://');
      if (!url) return;
      insertion = `<a href="${url}" title="">${selectedText || this.i18n.t('linkText')}</a>`;
    } else {
      insertion = `<${tag}>${selectedText}</${tag}>`;
    }

    this.text = this.text.substring(0, start) + insertion + this.text.substring(end);

    this.onTextChange();

    setTimeout(() => {
      textarea.focus();
      const newCursorPos = start + insertion.length;
      textarea.setSelectionRange(newCursorPos, newCursorPos);
    });
  }

  private translateServerError(msg: string): string {
    const errorMap: Record<string, string> = {
      'Invalid CAPTCHA.': this.i18n.t('invalidCaptcha'),
      'CAPTCHA is required when uploading a file.': this.i18n.t('captchaRequiredFile'),
    };
    if (msg.startsWith('Unclosed') || msg.startsWith('Unexpected')) {
      return this.i18n.t('invalidHtmlTags');
    }
    return errorMap[msg] || msg;
  }

  private sanitizePreview(input: string): string {
    if (!input) return '';

    const allowedTags = new Set(['a', 'code', 'i', 'strong']);

    let result = input.replace(/<(\/?)(\w+)([^>]*)>/g, (match, slash, tagName, attrs) => {
      const tag = tagName.toLowerCase();

      if (!allowedTags.has(tag)) {
        return match.replace(/</g, '&lt;').replace(/>/g, '&gt;');
      }

      if (tag === 'a' && slash !== '/') {
        const hrefMatch = attrs.match(/href\s*=\s*"([^"]*)"/);
        const titleMatch = attrs.match(/title\s*=\s*"([^"]*)"/);
        let cleanTag = '<a';
        if (hrefMatch) cleanTag += ` href="${hrefMatch[1]}"`;
        if (titleMatch) cleanTag += ` title="${titleMatch[1]}"`;
        cleanTag += ' target="_blank" rel="noopener noreferrer">';
        return cleanTag;
      }

      return match;
    });

    result = result.replace(/\n/g, '<br>');

    return result;
  }

  togglePreview(): void {
    this.showPreview = !this.showPreview;
    if (this.showPreview) {
      this.previewHtml = this.sanitizePreview(this.text);
    }
  }
  
  onTextChange(): void {
    if (this.showPreview) {
      this.previewHtml = this.sanitizePreview(this.text);
    }
  }

  validate(): boolean {
    this.errors = {};

    if (!this.userName.trim()) {
      this.errors['userName'] = [this.i18n.t('userNameRequired')];
    } else if (!/^[a-zA-Z0-9]+$/.test(this.userName)) {
      this.errors['userName'] = [this.i18n.t('userNameInvalid')];
    }

    if (!this.email.trim()) {
      this.errors['email'] = [this.i18n.t('emailRequired')];
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email)) {
      this.errors['email'] = [this.i18n.t('emailInvalid')];
    }

    if (this.homePage.trim() && !/^https?:\/\/.+/.test(this.homePage)) {
      this.errors['homePage'] = [this.i18n.t('homePageInvalid')];
    }

    if (!this.text.trim()) {
      this.errors['text'] = [this.i18n.t('textRequired')];
    }

    if (this.selectedFile && !this.captchaText.trim()) {
      this.errors['captchaText'] = [this.i18n.t('captchaRequiredFile')];
    }

    return Object.keys(this.errors).length === 0;
  }

  onSubmit(): void {
    if (!this.validate()) return;

    this.isSubmitting = true;

    this.commentService.createComment(
      {
        userName: this.userName.trim(),
        email: this.email.trim(),
        homePage: this.homePage.trim(),
        text: this.text,
        parentCommentId: this.parentCommentId,
        captchaText: this.captchaText.trim(),
        captchaId: this.captchaId
      },
      this.selectedFile
    ).subscribe({
      next: () => {
        this.text = '';
        this.captchaText = '';
        this.captchaId = '';
        this.captchaImage = '';
        this.selectedFile = null;
        this.showPreview = false;
        this.errors = {};
        this.commentCreated.emit();
      },
      error: (err) => {
        this.isSubmitting = false;
        const serverErrors = err.error?.Errors || err.error?.errors;
        if (serverErrors) {
          const errors: { [key: string]: string[] } = {};
          for (const key of Object.keys(serverErrors)) {
            const normalizedKey = key.charAt(0).toLowerCase() + key.slice(1);
            errors[normalizedKey] = serverErrors[key].map((msg: string) => this.translateServerError(msg));
          }
          this.errors = errors;
        } else if (err.error?.Error || err.error?.error) {
          const msg = err.error.Error || err.error.error;
          this.errors = { general: [this.translateServerError(msg)] };
        } else {
          this.errors = { general: [this.i18n.t('generalError')] };
        }
        this.loadCaptcha();
      },
      complete: () => {
        this.isSubmitting = false;
      }
    });
  }
}