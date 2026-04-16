import { Component } from '@angular/core';
import { CommentListComponent } from './components/comment-list/comment-list';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommentListComponent],
  template: '<app-comment-list></app-comment-list>',
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
      background: #f5f5f5;
    }
  `]
})
export class AppComponent {}