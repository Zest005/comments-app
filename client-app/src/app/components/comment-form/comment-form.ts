import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CommentService } from '../../services/comment.service';

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

  constructor(private commentService: CommentService) {}

  ngOnInit(): void {

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
        alert('Text file must not exceed 100 KB.');
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
      const url = prompt('Enter URL:', 'https://');
      if (!url) return;
      insertion = `<a href="${url}" title="">${selectedText || 'link text'}</a>`;
    } else {
      insertion = `<${tag}>${selectedText}</${tag}>`;
    }

    this.text = this.text.substring(0, start) + insertion + this.text.substring(end);

    setTimeout(() => {
      textarea.focus();
      const newCursorPos = start + insertion.length;
      textarea.setSelectionRange(newCursorPos, newCursorPos);
    });
  }

  togglePreview(): void {
    this.showPreview = !this.showPreview;
    if (this.showPreview) {
      this.previewHtml = this.text.replace(/\n/g, '<br>');
    }
  }

  validate(): boolean {
    this.errors = {};

    if (!this.userName.trim()) {
      this.errors['userName'] = ['User Name is required.'];
    } else if (!/^[a-zA-Z0-9]+$/.test(this.userName)) {
      this.errors['userName'] = ['Only Latin letters and digits allowed.'];
    }

    if (!this.email.trim()) {
      this.errors['email'] = ['E-mail is required.'];
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email)) {
      this.errors['email'] = ['Invalid email format.'];
    }

    if (this.homePage.trim() && !/^https?:\/\/.+/.test(this.homePage)) {
      this.errors['homePage'] = ['Must be a valid URL (starting with http:// or https://).'];
    }

    if (!this.text.trim()) {
      this.errors['text'] = ['Comment text is required.'];
    }

    if (this.selectedFile && !this.captchaText.trim()) {
      this.errors['captchaText'] = ['CAPTCHA is required when uploading a file.'];
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
            errors[normalizedKey] = serverErrors[key];
          }
          this.errors = errors;
        } else if (err.error?.Error || err.error?.error) {
          this.errors = { general: [err.error.Error || err.error.error] };
        } else {
          this.errors = { general: ['An error occurred. Please try again.'] };
        }
        this.loadCaptcha();
      },
      complete: () => {
        this.isSubmitting = false;
      }
    });
  }
}