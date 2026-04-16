import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { Comment, PagedResult } from '../../models/comment.model';
import { CommentService } from '../../services/comment.service';
import { SignalRService } from '../../services/signalr.service';
import { CommentFormComponent } from '../comment-form/comment-form';
import { CommentItemComponent } from '../comment-item/comment-item';

@Component({
  selector: 'app-comment-list',
  standalone: true,
  imports: [CommonModule, CommentFormComponent, CommentItemComponent],
  templateUrl: './comment-list.html',
  styleUrls: ['./comment-list.css']
})
export class CommentListComponent implements OnInit, OnDestroy {
  comments: Comment[] = [];
  currentPage = 1;
  pageSize = 25;
  totalPages = 0;
  totalCount = 0;

  sortBy = 'createdAt';
  sortDescending = true;

  isLoading = false;

  private signalRSub!: Subscription;

  constructor(
    private commentService: CommentService,
    private signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    this.loadComments();

    this.signalRService.startConnection();

    this.signalRSub = this.signalRService.newComment$.subscribe(newComment => {
      if (newComment.parentCommentId === null) {
        if (this.sortBy === 'createdAt' && this.sortDescending && this.currentPage === 1) {
          this.comments.unshift(newComment);
          this.totalCount++;
          if (this.comments.length > this.pageSize) {
            this.comments.pop();
          }
        } else {
          this.loadComments();
        }
      } else {
        const parent = this.comments.find(c => c.id === newComment.parentCommentId);
        if (parent) {
          if (!parent.replies) {
            parent.replies = [];
          }
          parent.replies.push(newComment);
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.signalRService.stopConnection();
    if (this.signalRSub) {
      this.signalRSub.unsubscribe();
    }
  }

  loadComments(): void {
    this.isLoading = true;

    this.commentService.getComments(
      this.currentPage,
      this.pageSize,
      this.sortBy,
      this.sortDescending
    ).subscribe({
      next: (result: PagedResult<Comment>) => {
        this.comments = result.items;
        this.totalPages = result.totalPages;
        this.totalCount = result.totalCount;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load comments:', err);
        this.isLoading = false;
      }
    });
  }

  onSort(field: string): void {
    if (this.sortBy === field) {
      this.sortDescending = !this.sortDescending;
    } else {
      this.sortBy = field;
      this.sortDescending = true;
    }
    this.currentPage = 1;
    this.loadComments();
  }

  getSortIcon(field: string): string {
    if (this.sortBy !== field) return '↕';
    return this.sortDescending ? '↓' : '↑';
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadComments();
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    for (let i = 1; i <= this.totalPages; i++) {
      pages.push(i);
    }
    return pages;
  }

  onCommentCreated(): void {
    this.currentPage = 1;
    this.sortBy = 'createdAt';
    this.sortDescending = true;
    this.loadComments();
  }

  private addReplyToTree(comments: Comment[], reply: Comment): boolean {
    for (const comment of comments) {
      if (comment.id === reply.parentCommentId) {
        if (!comment.replies) {
          comment.replies = [];
        }
        comment.replies.push(reply);
        return true;
      }
      if (comment.replies && comment.replies.length > 0) {
        if (this.addReplyToTree(comment.replies, reply)) {
          return true;
        }
      }
    }
    return false;
  }
}