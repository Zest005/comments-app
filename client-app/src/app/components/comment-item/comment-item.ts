import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Comment } from '../../models/comment.model';
import { CommentFormComponent } from '../comment-form/comment-form';

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
  lightboxUrl = '';

  toggleReplyForm(): void {
    this.showReplyForm = !this.showReplyForm;
  }

  onReplyCreated(): void {
    this.showReplyForm = false;
  }

  openLightbox(url: string): void {
    this.lightboxUrl = url;
    this.showLightbox = true;
  }

  closeLightbox(): void {
    this.showLightbox = false;
    this.lightboxUrl = '';
  }

  formatDate(dateStr: string): string {
    let utcDateStr = dateStr;
    if (!utcDateStr.endsWith('Z')) {
      utcDateStr += 'Z';
    }

    const date = new Date(utcDateStr);
    
    return date.toLocaleDateString('en-US', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  isImage(contentType: string): boolean {
    return contentType.startsWith('image/');
  }
}