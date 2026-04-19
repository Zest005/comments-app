import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { Comment } from '../../models/comment.model';
import { CommentFormComponent } from '../comment-form/comment-form';
import { CommentService } from '../../services/comment.service';
import { SignalRService } from '../../services/signalr.service';
import { I18nService } from '../../services/i18n.service';

@Component({
  selector: 'app-comment-item',
  standalone: true,
  imports: [CommonModule, CommentFormComponent],
  templateUrl: './comment-item.html',
  styleUrls: ['./comment-item.css']
})
export class CommentItemComponent implements OnInit, OnDestroy {
  @Input() comment!: Comment;

  showReplyForm = false;
  showLightbox = false;
  closingLightbox = false;
  lightboxUrl = '';

  repliesLoaded = false;
  repliesVisible = false;
  repliesAnimating = false;
  loadedReplies: Comment[] = [];
  totalReplies = 0;
  isLoadingReplies = false;

  private replySub!: Subscription;

  constructor(public i18n: I18nService, private commentService: CommentService, private signalRService: SignalRService) {}

  ngOnInit(): void {
    this.replySub = this.signalRService.newReply$.subscribe(notification => {
      if (notification.parentCommentId === this.comment.id) {
        this.comment.replyCount++;
        this.totalReplies++;

        if (this.repliesVisible) {
          this.loadMoreReplies();
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.replySub?.unsubscribe();
  }

  toggleReplyForm(): void {
    this.showReplyForm = !this.showReplyForm;
  }

  onReplyCreated(): void {
    this.showReplyForm = false;
  }

  toggleReplies(): void {
    if (!this.repliesLoaded) {
      this.loadInitialReplies();
    } else if (this.repliesVisible) {
      this.repliesAnimating = true;
      setTimeout(() => {
        this.repliesVisible = false;
        this.repliesAnimating = false;
      }, 300);
    } else {
      this.repliesVisible = true;
    }
  }

  loadInitialReplies(): void {
    this.isLoadingReplies = true;
    this.commentService.getReplies(this.comment.id, 0, 3).subscribe({
      next: (result) => {
        this.loadedReplies = result.items;
        this.totalReplies = result.totalCount;
        this.repliesLoaded = true;
        this.repliesVisible = true;
        this.isLoadingReplies = false;
      },
      error: () => {
        this.isLoadingReplies = false;
      }
    });
  }

  loadMoreReplies(): void {
    this.isLoadingReplies = true;
    const skip = this.loadedReplies.length;
    this.commentService.getReplies(this.comment.id, skip, 3).subscribe({
      next: (result) => {
        this.loadedReplies.push(...result.items);
        this.totalReplies = result.totalCount;
        this.isLoadingReplies = false;
      },
      error: () => {
        this.isLoadingReplies = false;
      }
    });
  }

  get remainingReplies(): number {
    return this.totalReplies - this.loadedReplies.length;
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