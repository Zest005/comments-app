import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Comment } from '../../models/comment.model';
import { CommentFormComponent } from '../comment-form/comment-form';
import { I18nService } from '../../services/i18n.service';

@Component({
  selector: 'app-comment-item',
  standalone: true,
  imports: [CommonModule, CommentFormComponent],
  templateUrl: './comment-item.html',
  styleUrls: ['./comment-item.css']
})
export class CommentItemComponent {
  @Input() comment!: Comment;

  showReplyForm = false;
  showLightbox = false;
  closingLightbox = false;
  lightboxUrl = '';

  constructor(public i18n: I18nService) {}

  toggleReplyForm(): void {
    this.showReplyForm = !this.showReplyForm;
  }

  onReplyCreated(): void {
    this.showReplyForm = false;
  }

  openLightbox(url: string): void {
    this.lightboxUrl = url;
    this.showLightbox = true;
    this.closingLightbox = false;
  }

  closeLightbox(): void {
    this.closingLightbox = true;
    setTimeout(() => {
      this.showLightbox = false;
      this.closingLightbox = false;
      this.lightboxUrl = '';
    }, 300);
  }

  formatDate(dateStr: string): string {
    return this.i18n.formatDate(dateStr);
  }

  isImage(contentType: string): boolean {
    return contentType.startsWith('image/');
  }
}